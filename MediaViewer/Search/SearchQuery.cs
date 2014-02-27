using MediaViewer.MediaDatabase;
using MediaViewer.MediaDatabase.DbCommands;
using MediaViewer.UserControls.Relation;
using MvvmFoundation.Wpf;
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

    class SearchQuery : ObservableObject
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

            DurationSecondsStart = null;
            DurationSecondsEnd = null;

            CreationStart = null;
            CreationEnd = null;
      
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
                searchType = value;
                NotifyPropertyChanged();
            }
        }

        String text;

        public String Text
        {
            get { return text; }
            set
            {
                text = value;
                NotifyPropertyChanged();
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
                videoWidthStart = value;
                NotifyPropertyChanged();
            }
        }

        Nullable<int> videoWidthEnd;

        public Nullable<int> VideoWidthEnd
        {
            get { return videoWidthEnd; }
            set
            {
                videoWidthEnd = value;
                NotifyPropertyChanged();
            }
        }

        Nullable<int> videoHeightStart;

        public Nullable<int> VideoHeightStart
        {
            get { return videoHeightStart; }
            set
            {
                videoHeightStart = value;
                NotifyPropertyChanged();
            }
        }

        Nullable<int> videoHeightEnd;

        public Nullable<int> VideoHeightEnd
        {
            get { return videoHeightEnd; }
            set
            {
                videoHeightEnd = value;
                NotifyPropertyChanged();
            }
        }

        Nullable<float> framesPerSecondStart;

        public Nullable<float> FramesPerSecondStart
        {
            get { return framesPerSecondStart; }
            set
            {
                framesPerSecondStart = value;
                NotifyPropertyChanged();
            }
        }

        Nullable<float> framesPerSecondEnd;

        public Nullable<float> FramesPerSecondEnd
        {
            get { return framesPerSecondEnd; }
            set
            {
                framesPerSecondEnd = value;
                NotifyPropertyChanged();
            }
        }

        Nullable<long> durationSecondsStart;

        public Nullable<long> DurationSecondsStart
        {
            get { return durationSecondsStart; }
            set
            {
                durationSecondsStart = value;
                NotifyPropertyChanged();
            }
        }

        Nullable<long> durationSecondsEnd;

        public Nullable<long> DurationSecondsEnd
        {
            get { return durationSecondsEnd; }
            set
            {
                durationSecondsEnd = value;
                NotifyPropertyChanged();
            }
        }

        Nullable<DateTime> creationStart;

        public Nullable<DateTime> CreationStart
        {
            get { return creationStart; }
            set
            {
                creationStart = value;
                NotifyPropertyChanged();
            }
        }
        Nullable<DateTime> creationEnd;

        public Nullable<DateTime> CreationEnd
        {
            get { return creationEnd; }
            set
            {
                creationEnd = value;
                NotifyPropertyChanged();
            }
        }

        Nullable<int> imageWidthStart;

        public Nullable<int> ImageWidthStart
        {
            get { return imageWidthStart; }
            set
            {
                imageWidthStart = value;
                NotifyPropertyChanged();
            }
        }

        Nullable<int> imageWidthEnd;

        public Nullable<int> ImageWidthEnd
        {
            get { return imageWidthEnd; }
            set
            {
                imageWidthEnd = value;
                NotifyPropertyChanged();
            }
        }

        Nullable<int> imageHeightStart;

        public Nullable<int> ImageHeightStart
        {
            get { return imageHeightStart; }
            set
            {
                imageHeightStart = value;
                NotifyPropertyChanged();
            }
        }

        Nullable<int> imageHeightEnd;

        public Nullable<int> ImageHeightEnd
        {
            get { return imageHeightEnd; }
            set
            {
                imageHeightEnd = value;
                NotifyPropertyChanged();
            }
        }

        Nullable<double> ratingStart;

        public Nullable<double> RatingStart
        {
            get { return ratingStart; }
            set { ratingStart = value;
            NotifyPropertyChanged();
            }
        }
        Nullable<double> ratingEnd;

        public Nullable<double> RatingEnd
        {
            get { return ratingEnd; }
            set { ratingEnd = value;
            NotifyPropertyChanged();
            }
        }
    }
}
