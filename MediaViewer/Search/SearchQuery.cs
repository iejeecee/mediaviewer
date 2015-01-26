using MediaViewer.MediaDatabase;
using MediaViewer.MediaDatabase.DbCommands;
using MediaViewer.UserControls.Relation;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Search
{
    public enum MediaType
    {
        All,
        Video,
        Images
    }

    class SearchQuery : BindableBase
    {

        public SearchQuery()
        {
            Text = "Search";
            SearchType = MediaType.All;
            Tags = new ObservableCollection<Tag>();

            VideoWidthStart = null;
            VideoWidthEnd = null;

            VideoHeightStart = null;
            VideoHeightEnd = null;

            FramesPerSecondStart = null;
            framesPerSecondEnd = null;

            NrChannelsStart = null;
            NrChannelsEnd = null;

            DurationSecondsStart = null;
            DurationSecondsEnd = null;

            CreationStart = null;
            CreationEnd = null;
      
        }

        public bool IsEmpty
        {
            get
            {
                if (!String.IsNullOrEmpty(Text) && !String.IsNullOrWhiteSpace(Text))
                {
                    return (false);
                }

                if (tags.Count > 0)
                {
                    return (false);
                }

                if (this.CreationEnd != null || this.CreationStart != null)
                {
                    return (false);
                }

                if (this.DurationSecondsEnd != null || this.DurationSecondsStart != null)
                {
                    return (false);
                }

                if (this.FramesPerSecondEnd != null || this.FramesPerSecondStart != null)
                {
                    return (false);
                }

                if (this.NrChannelsEnd != null || this.NrChannelsStart!= null)
                {
                    return (false);
                }

                if (this.ImageHeightEnd != null || this.ImageHeightStart != null)
                {
                    return (false);
                }

                if (this.ImageWidthEnd != null || this.ImageWidthStart != null)
                {
                    return (false);
                }

                if (this.RatingEnd != null || this.RatingStart != null)
                {
                    return (false);
                }

                if (this.VideoHeightEnd != null || this.VideoHeightStart != null)
                {
                    return (false);
                }

                if (this.VideoWidthEnd != null || this.VideoWidthStart != null)
                {
                    return (false);
                }

                return (true);
            }
        }

        ObservableCollection<Tag> tags;

        public ObservableCollection<Tag> Tags
        {
            get { return tags; }
            set { tags = value; }
        }

        MediaType searchType;

        public MediaType SearchType
        {
            get { return searchType; }
            set
            {           
                SetProperty(ref searchType, value);
            }
        }

        String text;

        public String Text
        {
            get { return text; }
            set
            {                
                SetProperty(ref text, value);
            }
        }

        public void parseQuery()
        {
            /*TagDbCommands tagCommands = new TagDbCommands();

            tags = new List<Tag>();

            foreach (String tagName in Text.Split(','))
            {
                String tagNameTrimmed = tagName.Trim();

                Tag dbTag = tagCommands.getTagByName(tagNameTrimmed);

                if (dbTag == null)
                {
                    Tags.Add(new Tag() { Name = tagNameTrimmed });
                }
                else
                {
                    Tags.Add(dbTag);
                }
            }*/
        }

        Nullable<int> videoWidthStart;

        public Nullable<int> VideoWidthStart
        {
            get { return videoWidthStart; }
            set
            {               
                SetProperty(ref videoWidthStart, value);
            }
        }

        Nullable<int> videoWidthEnd;

        public Nullable<int> VideoWidthEnd
        {
            get { return videoWidthEnd; }
            set
            {               
                SetProperty(ref videoWidthEnd, value);
            }
        }

        Nullable<int> videoHeightStart;

        public Nullable<int> VideoHeightStart
        {
            get { return videoHeightStart; }
            set
            {              
                SetProperty(ref videoHeightStart, value);
            }
        }

        Nullable<int> videoHeightEnd;

        public Nullable<int> VideoHeightEnd
        {
            get { return videoHeightEnd; }
            set
            {              
                SetProperty(ref videoHeightEnd, value);
            }
        }

        Nullable<float> framesPerSecondStart;

        public Nullable<float> FramesPerSecondStart
        {
            get { return framesPerSecondStart; }
            set
            {               
                SetProperty(ref framesPerSecondStart, value);
            }
        }

        Nullable<float> framesPerSecondEnd;

        public Nullable<float> FramesPerSecondEnd
        {
            get { return framesPerSecondEnd; }
            set
            {               
                SetProperty(ref framesPerSecondEnd, value);
            }
        }

        Nullable<int> nrChannelsStart;

        public Nullable<int> NrChannelsStart
        {
            get { return nrChannelsStart; }
            set
            {
                SetProperty(ref nrChannelsStart, value);
            }
        }

        Nullable<int> nrChannelsEnd;

        public Nullable<int> NrChannelsEnd
        {
            get { return nrChannelsEnd; }
            set
            {
                SetProperty(ref nrChannelsEnd, value);
            }
        }


        Nullable<long> durationSecondsStart;

        public Nullable<long> DurationSecondsStart
        {
            get { return durationSecondsStart; }
            set
            {              
                SetProperty(ref durationSecondsStart, value);
            }
        }

        Nullable<long> durationSecondsEnd;

        public Nullable<long> DurationSecondsEnd
        {
            get { return durationSecondsEnd; }
            set
            {              
                SetProperty(ref durationSecondsEnd, value);
            }
        }

        Nullable<DateTime> creationStart;

        public Nullable<DateTime> CreationStart
        {
            get { return creationStart; }
            set
            {              
                SetProperty(ref creationStart, value);
            }
        }
        Nullable<DateTime> creationEnd;

        public Nullable<DateTime> CreationEnd
        {
            get { return creationEnd; }
            set
            {               
                SetProperty(ref creationEnd, value);
            }
        }

        Nullable<int> imageWidthStart;

        public Nullable<int> ImageWidthStart
        {
            get { return imageWidthStart; }
            set
            {                
                SetProperty(ref imageWidthStart, value);
            }
        }

        Nullable<int> imageWidthEnd;

        public Nullable<int> ImageWidthEnd
        {
            get { return imageWidthEnd; }
            set
            {            
                SetProperty(ref  imageWidthEnd, value);
            }
        }

        Nullable<int> imageHeightStart;

        public Nullable<int> ImageHeightStart
        {
            get { return imageHeightStart; }
            set
            {               
                SetProperty(ref imageHeightStart, value);
            }
        }

        Nullable<int> imageHeightEnd;

        public Nullable<int> ImageHeightEnd
        {
            get { return imageHeightEnd; }
            set
            {                
                SetProperty(ref imageHeightEnd, value);
            }
        }

        Nullable<double> ratingStart;

        public Nullable<double> RatingStart
        {
            get { return ratingStart; }
            set {  
                SetProperty(ref ratingStart, value);
            }
        }
        Nullable<double> ratingEnd;

        public Nullable<double> RatingEnd
        {
            get { return ratingEnd; }
            set {  
                SetProperty(ref ratingEnd, value);
            }
        }
    }
}
