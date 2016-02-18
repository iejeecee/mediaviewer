using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MediaViewer.MediaDatabase
{
    public class PresetMetadata
    {
        public PresetMetadata()
        {
            this.Tags = new List<Tag>();
        }

        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Title { get; set; }
        public Nullable<double> Rating { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string Copyright { get; set; }
        public bool IsNameEnabled { get; set; }
        public bool IsTitleEnabled { get; set; }
        public bool IsRatingEnabled { get; set; }
        public bool IsDescriptionEnabled { get; set; }
        public bool IsAuthorEnabled { get; set; }
        public bool IsCopyrightEnabled { get; set; }
        public Nullable<System.DateTime> CreationDate { get; set; }
        public bool IsCreationDateEnabled { get; set; }

        [Timestamp]
        public byte[] TimeStamp { get; set; }

        public virtual ICollection<Tag> Tags { get; set; }
    }
}
