using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MediaDatabase
{
    class Tag
    {
        public Tag()
        {               
            LinkedTags = new HashSet<Tag>();    
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public String Name { get; set; }
        public int? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public virtual ICollection<Tag> LinkedTags { get; set; }

        public int? TagCategoryId { get; set; }
        public virtual TagCategory TagCategory { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

}
