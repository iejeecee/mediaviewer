using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MediaViewer.Model.Utils
{
    class MiscUtils
    {
        public static void DebugOut(string msg)
        {

            System.Diagnostics.Debug.Print(msg);
        }

        public static void DebugOut(Object obj)
        {

            System.Diagnostics.Debug.Print(obj.ToString());
        }

        public static void parseTimeSeconds(long totalSeconds, out int seconds, out int minutes, out int hours)
        {
            seconds = (int)(totalSeconds % 60);
            minutes = (int)((totalSeconds / 60) % 60);
            hours = (int)(totalSeconds / 3600);
        }

        public static string formatTimeSeconds(long totalSeconds)
        {
            int seconds, minutes, hours;       

            parseTimeSeconds(totalSeconds, out seconds, out minutes, out hours);

            string hoursStr = "";

            if (hours != 0)
            {

                hoursStr = hours.ToString() + ":";
            }

            string output = hoursStr +
                minutes.ToString("00") + ":" +
                seconds.ToString("00");

            return (output);

        }

        public static string formatSizeBytes(long sizeBytes)
        {
            long TB = 1099511627776;
            long GB = 1073741824;
            long MB = 1048576;
            long KB = 1024;
            string output;

            if (sizeBytes > TB)
            {
                output = (sizeBytes / (double)TB).ToString("0.00") + " TB";
            }
            else if (sizeBytes > GB)
            {
                output = (sizeBytes / (double)GB).ToString("0.00") + " GB";
            }
            else if (sizeBytes > MB)
            {
                output = (sizeBytes / (double)MB).ToString("0.00") + " MB";
            }
            else if (sizeBytes > KB)
            {
                output = (sizeBytes / (double)KB).ToString("0") + " KB";
            }
            else
            {
                output = sizeBytes.ToString() + " Bytes";
            }

            return (output);
        }


        public static T clamp<T>(T val, T min, T max) where T : IComparable
        {

            if (val.CompareTo(min) < 0) val = min;
            else if (val.CompareTo(max) > 0) val = max;

            return (val);
        }


        public static double lerp(double val, double min, double max)
        {

            val = clamp<double>(val, 0, 1);

            return ((1 - val) * min + val * max);
        }


        public static double invlerp(double val, double min, double max) {

            double result = (val - min) / (max - min);
			
            return(result);
        }


        public static bool listSortAndCompare<T>(List<T> a, List<T> b)
        {

            if (a.Count != b.Count) return (false);

            a.Sort();
            b.Sort();

            for (int i = 0; i < a.Count; i++)
            {

                if (!a[i].Equals(b[i]))
                {

                    return (false);
                }
            }

            return (true);
        }

        const int maxHistorySize = 16;

        public static void insertIntoHistoryCollection(ObservableCollection<String> history, String element)
        {
            if(String.IsNullOrEmpty(element) || String.IsNullOrWhiteSpace(element)) return;

            if (history.Contains(element))
            {
                history.Remove(element);               
            }

            history.Insert(0, element);

            if (history.Count > maxHistorySize)
            {
                history.RemoveAt(history.Count - 1);
            }
        }

   

       
    }
}
