using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MediaDatabase
{
    [Serializable]
    partial class Tag : IComparable<Tag>, IEquatable<Tag>
    {   
        public override string ToString()
        {
            return Name;
        }
     
        public int CompareTo(Tag other)
        {
            if (other == null)
            {
                throw new ArgumentException();
            }

            return (other.Name.CompareTo(Name));
        }

        public bool Equals(Tag other)
        {
            if (other == null)
            {
                throw new ArgumentException();
            }

            return (other.Name.Equals(Name));
        }
    }
}
