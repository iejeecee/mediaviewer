//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MediaViewer.MediaDatabase
{
    using System;
    using System.Collections.Generic;
    
    public partial class PresetMetadata
    {
        public PresetMetadata()
        {
            this.Tags = new HashSet<Tag>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public Nullable<double> Rating { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string Copyright { get; set; }
    
        public virtual ICollection<Tag> Tags { get; set; }
    }
}
