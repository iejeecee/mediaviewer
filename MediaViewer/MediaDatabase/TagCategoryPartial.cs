using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MediaDatabase
{
    partial class TagCategory : IEquatable<TagCategory>
    {

        public bool Equals(TagCategory other)
        {
            if (other == null) return false;

            if (this.Name.Equals(other.Name)) return (true);
            else return (false);
        }
    }
}
