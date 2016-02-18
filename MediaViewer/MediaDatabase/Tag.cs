using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MediaViewer.MediaDatabase
{
    public class Tag : IEquatable<Tag>
    {
        public Tag()
        {
            this.BaseMetadatas = new List<BaseMetadata>();
            this.PresetMetadatas = new List<PresetMetadata>();
            this.ParentTags = new List<Tag>();
            this.ChildTags = new List<Tag>();
        }

        public override string ToString()
        {
            return Name;
        }

        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public long Used { get; set; }

        [Timestamp]
        public byte[] TimeStamp { get; set; }

        public virtual ICollection<BaseMetadata> BaseMetadatas { get; set; }
        public virtual ICollection<PresetMetadata> PresetMetadatas { get; set; }
        public virtual ICollection<Tag> ParentTags { get; set; }
        public virtual ICollection<Tag> ChildTags { get; set; }

        public bool Equals(Tag other)
        {
            if (other == null) return (false);

            return (Name.Equals(other.Name));
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Tag);
        }
    }
}
