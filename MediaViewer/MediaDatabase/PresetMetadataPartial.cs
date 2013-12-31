using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MediaDatabase
{
    partial class PresetMetadata : IComparable<PresetMetadata>
    {
        public int CompareTo(PresetMetadata other)
        {
            if (other == null)
            {
                throw new ArgumentException();
            }

            return (other.Name.CompareTo(Name));
        }
    }
}
