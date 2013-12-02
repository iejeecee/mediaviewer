using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MediaDatabase
{
    class TagCategory : IEquatable<TagCategory>
    {

        [Key]
        public int Id { get; set; }

        [Required]
        public String Name { get; set; }

        public virtual ICollection<Tag> Tags { get; set; }

        public int GetHashCode(object obj)
        {
            return (this.GetHashCode());
        }

        public bool Equals(TagCategory other)
        {
            if (other == null) return (false);
            else if (other.Name.Equals(Name)) return (true);
            else return (false);
        }
    }
}
