using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MediaViewer.Utils
{
    class Misc
    {
        public static void DebugOut(string msg)
        {

            System.Diagnostics.Debug.Print(msg);
        }

        public static void DebugOut(Object obj)
        {

            System.Diagnostics.Debug.Print(obj.ToString());
        }


        public static string formatTimeSeconds(int totalSeconds)
        {

            int seconds = (int)(totalSeconds % 60);
            int minutes = (int)((totalSeconds / 60) % 60);
            int hours = (int)(totalSeconds / 3600);

            string hoursStr = "";

            if (hours != 0)
            {

                hoursStr = hours.ToString() + ":";
            }

            string output = hoursStr +
                minutes.ToString("00") + ":" +
                seconds.ToString("00");

            return (output);

        }

        public static string formatSizeBytes(long sizeBytes)
        {

            long GB = 1073741824;
            long MB = 1048576;
            long KB = 1024;
            string output;

            if (sizeBytes > GB)
            {

                output = (sizeBytes / (double)GB).ToString("0.00") + " GB";

            }
            else if (sizeBytes > MB)
            {

                output = (sizeBytes / (double)MB).ToString("0.00") + " MB";

            }
            else if (sizeBytes > KB)
            {

                output = (sizeBytes / (double)KB).ToString("0") + " KB";

            }
            else
            {

                output = sizeBytes.ToString() + " Bytes";
            }

            return (output);
        }
        /*
            template<class T>
            static T clamp(T val, T min, T max) {

                if(val < min) val = min;
                else if(val > max) val = max;

                return(val);
            }

            template<class T>
            static T lerp(double val, T min, T max) {

                val = clamp<double>(val, 0, 1);
		
                return(T((1-val) * min + val * max));
            }

            template<class T>
            static double invlerp(T val, T min, T max) {

                double result = (val - min) / double(max - min);
			
                return(result);
            }
        */

        public static bool listSortAndCompare<T>(List<T> a, List<T> b)
        {

            if (a.Count != b.Count) return (false);

            a.Sort();
            b.Sort();

            for (int i = 0; i < a.Count; i++)
            {

                if (!a[i].Equals(b[i]))
                {

                    return (false);
                }
            }

            return (true);
        }




    }
}
