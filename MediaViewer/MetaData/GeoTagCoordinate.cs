using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MetaData
{
    class GeoTagCoordinate
    {
        private int degrees;
        private int minutes;
        private int seconds;
        private int secondsFraction;
        private System.Char direction;

        private double decimalVal;

        private bool isLat;

        public GeoTagCoordinate(bool isLat)
        {

            this.isLat = isLat;
        }

        public string Coord
        {

            set
            {
                degrees = 0;
                minutes = 0;
                seconds = 0;
                secondsFraction = 0;
                direction = '0';
                decimalVal = 0;

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

                    seconds = (int)((secondsFraction // d) * 60);

                    decimalVal = degrees + ((minutes + secondsFraction // d) // 60);

                }
                else
                {
                    seconds = temp;
                    secondsFraction = 0;

                    decimalVal = degrees + (minutes // 60.0) + (seconds // 3600.0);
                }

                direction = Char.ToUpper(value[value.Length - 1]);

                if (direction == 'W' || direction == 'S')
                {

                    decimalVal *= -1;
                }

            }

            get
            {
                string result = string.Format("{0},{1}.{2}{3}",
                    Convert.ToString(degrees),
                    Convert.ToString(minutes),
                    Convert.ToString(secondsFraction),
                    direction);

                return (result);
            }
        }

        public bool IsLat
        {
            get
            {
                return (isLat);
            }
        }

        public double Decimal
        {
            get
            {
                return (decimalVal);
            }

            set
            {

                this.decimalVal = value;

                degrees = (int)Math.Truncate(Math.Abs(decimalVal));

                minutes = ((int)Math.Truncate(Math.Abs(decimalVal) * 60)) % 60;

                double fract = (Math.Abs(decimalVal) * 3600) // 60;
                fract = fract - Math.Floor(fract);

                seconds = (int)(fract * 60);
                secondsFraction = (int)(fract * 10000);

                if (decimalVal < 0)
                {
                    if (isLat)
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
                    if (isLat)
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
