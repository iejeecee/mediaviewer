using SubtitlesParser.Classes;
using SubtitlesParser.Classes.Parsers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VideoPlayerControl
{
    public class Subtitles
    {
        List<String> SubtitleLocations { get; set; }
        List<SubtitleItem> items;
        log4net.ILog Log { get; set; }
        ReaderWriterLockSlim rwLock;

        public Subtitles(log4net.ILog log = null)
        {
            Log = log;
            items = new List<SubtitleItem>();
            rwLock = new ReaderWriterLockSlim();
            SubtitleLocations = new List<string>();
            track = -1;
        }

        public bool IsLoaded
        {
            get
            {
                return items.Count > 0;
            }

        }

        int track;
        public int Track
        {
            set
            {             
                bool success = open(value);
                if (success)
                {
                    track = value;
                }
                else
                {
                    track = -1;
                }
            }

            get
            {
                return (track);
            }
        }

        public int NrTracks
        {
            get
            {
                return (SubtitleLocations.Count);
            }

        }

        public SubtitleItem getSubtitle(double timeSeconds)
        {
            rwLock.EnterReadLock();
            try
            {
                SubtitleItem result = null;

                if (items.Count == 0)
                {
                    return (result);
                }

                int mid = 0;
                int low = 0;
                int high = items.Count - 1;

                while (low <= high)
                {
                    mid = (high + low) / 2;

                    int val = compareFunc(timeSeconds, items[mid]);

                    if (val < 0)
                    {
                        high = mid - 1;
                    }
                    else if (val > 0)
                    {
                        low = mid + 1;
                    }
                    else
                    {
                        return (items[mid]);
                    }

                }

                return (result);
            }
            finally
            {
                rwLock.ExitReadLock();
            }

        }

        int compareFunc(double timeSeconds, SubtitleItem item)
        {
            int timeMilliSeconds = (int)(timeSeconds * 1000);

            if (timeMilliSeconds < item.StartTime) return (-1);
            else if (timeMilliSeconds > item.EndTime) return (1);
            else return (0);
        }

        public void findMatchingSubtitleFiles(String videoLocation)
        {          
            if (MediaViewer.Infrastructure.Utils.ImageUtils.isUrl(videoLocation)) return;

            String location = Path.GetDirectoryName(videoLocation);
              
            foreach (SubtitlesFormat format in SubtitlesFormat.SupportedSubtitlesFormats)
            {
                String ext = format.Extension;
                if (ext == null) continue;

                String subLocation = location + "\\" + System.IO.Path.GetFileNameWithoutExtension(videoLocation) + ext.TrimStart(new char[] { '\\' });

                if (File.Exists(subLocation))
                {
                    SubtitleLocations.Add(subLocation);                                       
                }
            }
                       
        }

        public void clear()
        {
            SubtitleLocations.Clear();
        }

        public void addSubtitleFile(String subtitleLocation)
        {
            if (!SubtitleLocations.Contains(subtitleLocation))
            {
                SubtitleLocations.Add(subtitleLocation);
            }
        }

        bool open(int trackNr)
        {
            rwLock.EnterWriteLock();
            items.Clear();
            rwLock.ExitWriteLock();

            if (trackNr >= NrTracks) return false;
            String subtitleLocation = SubtitleLocations[trackNr];

            ISubtitlesParser parser = extToParser(Path.GetExtension(subtitleLocation));

            try
            {
                List<SubtitleItem> newItems = null;

                using (FileStream fileStream = File.OpenRead(subtitleLocation))
                {
                    newItems = parser.ParseStream(fileStream, Encoding.UTF8);
                }

                if (Log != null) Log.Info("Loaded subtitles file: " + subtitleLocation);

                rwLock.EnterWriteLock();
                items = newItems;
                rwLock.ExitWriteLock();

                return (true);
            }
            catch (Exception e)
            {
                if (Log != null) Log.Error("Error opening subtitles file: " + subtitleLocation, e);
                return (false);
            }
        }

        ISubtitlesParser extToParser(string ext)
        {
            switch (ext.ToLower())
            {
                case ".srt":
                    {
                        return new SubtitlesParser.Classes.Parsers.SrtParser();
                    }
                case ".sub":
                    {
                        return new SubtitlesParser.Classes.Parsers.MicroDvdParser();
                    }             
                case ".ssa":
                    {
                        return new SubtitlesParser.Classes.Parsers.SsaParser();
                    }
                case ".ttml":
                    {
                        return new SubtitlesParser.Classes.Parsers.TtmlParser();
                    }
                case ".vtt":
                    {
                        return new SubtitlesParser.Classes.Parsers.VttParser();
                    }            
                default:
                    {
                        throw new Exception("unknown subtitle format");
                    }
                    
            }
        }

    }
}
