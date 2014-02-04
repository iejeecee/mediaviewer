using MediaViewer.MediaDatabase;
using MediaViewer.MediaDatabase.DbCommands;
using MediaViewer.UserControls.Relation;
using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
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
            Tags = new List<Tag>();
                      
            VideoWidth = null;
            VideoWidthRelation = RelationEnum.EQUAL;

            VideoHeight = null;
            VideoHeightRelation = RelationEnum.EQUAL;

            FramesPerSecond = null;
            FramesPerSecondRelation = RelationEnum.EQUAL;

            DurationSeconds = null;
            DurationSecondsRelation = RelationEnum.EQUAL;
        }

        List<Tag> tags;

        public List<Tag> Tags
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
            TagDbCommands tagCommands = new TagDbCommands();

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
            }
        }

        Nullable<int> videoWidth;

        public Nullable<int> VideoWidth
        {
            get { return videoWidth; }
            set { videoWidth = value;
            NotifyPropertyChanged();
            }
        }

        RelationEnum videoWidthRelation;

        public RelationEnum VideoWidthRelation
        {
            get { return videoWidthRelation; }
            set { videoWidthRelation = value;
            NotifyPropertyChanged();
            }
        }

        Nullable<int> videoHeight;

        public Nullable<int> VideoHeight
        {
            get { return videoHeight; }
            set
            {
                videoHeight = value;
                NotifyPropertyChanged();
            }
        }

        RelationEnum videoHeightRelation;

        public RelationEnum VideoHeightRelation
        {
            get { return videoHeightRelation; }
            set { videoHeightRelation = value;
            NotifyPropertyChanged();
            }
        }

        Nullable<float> framesPerSecond;

        public Nullable<float> FramesPerSecond
        {
            get { return framesPerSecond; }
            set { framesPerSecond = value;
            NotifyPropertyChanged();
            }
        }

        RelationEnum framesPerSecondRelation;

        public RelationEnum FramesPerSecondRelation
        {
            get { return framesPerSecondRelation; }
            set { framesPerSecondRelation = value;
            NotifyPropertyChanged();
            }
        }

        Nullable<long> durationSeconds;

        public Nullable<long> DurationSeconds
        {
            get { return durationSeconds; }
            set { durationSeconds = value;
            NotifyPropertyChanged();
            }
        }

        RelationEnum durationSecondsRelation;

        public RelationEnum DurationSecondsRelation
        {
            get { return durationSecondsRelation; }
            set { durationSecondsRelation = value;
            NotifyPropertyChanged();
            }
        }
    }
}
