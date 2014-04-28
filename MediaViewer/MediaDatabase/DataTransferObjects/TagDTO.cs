using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MediaDatabase.DataTransferObjects
{
    [DataContract(IsReference = true)]
    class TagDTO 
    {
        public TagDTO()
        {                     
            this.ChildTags = new HashSet<TagDTO>();
            this.ParentTags = new HashSet<TagDTO>();
        }

        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public virtual TagCategoryDTO TagCategory { get; set; }
        [DataMember]
        public virtual ICollection<TagDTO> ChildTags { get; set; }
        [DataMember]
        public virtual ICollection<TagDTO> ParentTags { get; set; }
    }
}
