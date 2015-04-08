using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Model.Media.Metadata
{
    public class GeoTagCoordinate 
    {
        public enum CoordinateType
        {
            LATITUDE,
            LONGITUDE
        }

        Char direction;

        public CoordinateType CoordType { get; private set; }

        public GeoTagCoordinate(CoordinateType type)
        {
            CoordType = type;

            if (CoordType == CoordinateType.LATITUDE)
            {
                direction = 'S';
            }
            else
            {
                direction = 'N';
            }

            Coord = null;
            Decimal = null;
        }

        int degrees;
        int minutes;
        int seconds;
        int secondsFraction;

        public string Coord
        {

            set
            {
                if (value == null)
                {
                    Decimal = null;                            
                    return;
                }

                degrees = 0;
                minutes = 0;
                seconds = 0;
                secondsFraction = 0;
                direction = '0';
                dec = 0;

                int s1 = value.IndexOf(",");
                int s2 = value.LastIndexOf(",");
                int s3 = value.IndexOf(".");

                int s4 = (s2 == -1 || s1 == s2) ? s3 : s2;

                degrees = Convert.ToInt32(value.Substring(0, s1));
                minutes = Convert.ToInt32(value.Substring(s1 + 1, s4 - s1 - 1));

                int fractLength = value.Length - s4 - 2;
                int temp = Convert.ToInt32(value.Substring(s4 + 1, fractLength));

                if (s2 == -1 || s1 == s2)
                {

                    secondsFraction = temp;

                    double d = Math.Pow(10, fractLength);

                    seconds = (int)((secondsFraction / d) * 60);

                    dec = degrees + ((minutes + secondsFraction / d) / 60);

                }
                else
                {
                    seconds = temp;
                    secondsFraction = 0;

                    dec = degrees + (minutes / 60.0) + (seconds / 3600.0);
                }

                direction = Char.ToUpper(value[value.Length - 1]);

                if (direction == 'W' || direction == 'S')
                {

                    dec *= -1;
                }
                       
            }

            get
            {
                if (Decimal == null) return (null);

                string result = string.Format("{0},{1}.{2}{3}",
                    Convert.ToString(degrees),
                    Convert.ToString(minutes),
                    Convert.ToString(secondsFraction),
                    direction);

                return (result);
            }
        }

        private double? dec;

        public double? Decimal
        {
            get
            {
                return (dec);
            }

            set
            {
   
                this.dec = value;

                if (value == null)
                {                                   
                    return;
                }

                degrees = (int)Math.Truncate(Math.Abs(dec.Value));

                minutes = ((int)Math.Truncate(Math.Abs(dec.Value) * 60)) % 60;

                double fract = (Math.Abs(dec.Value) * 3600) / 60;
                fract = fract - Math.Floor(fract);

                seconds = (int)(fract * 60);
                secondsFraction = (int)(fract * 10000);

                if (dec < 0)
                {
                    if (CoordType == CoordinateType.LATITUDE)
                    {
                        direction = 'S';
                    }
                    else
                    {
                        direction = 'W';
                    }
                }
                else
                {
                    if (CoordType == CoordinateType.LATITUDE)
                    {
                        direction = 'N';
                    }
                    else
                    {
                        direction = 'E';
                    }
                }
        
            }
        }

    }
}
