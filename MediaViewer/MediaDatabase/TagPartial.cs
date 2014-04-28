using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MediaViewer.MediaDatabase
{
  
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
                return (false);
            }

            return (other.Name.Equals(Name));
        }

        
    }


}
