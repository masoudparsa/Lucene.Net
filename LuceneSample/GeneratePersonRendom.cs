using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LuceneSample
{
    public static class GeneratePersonRendom
    {

        public static string GetBarcode()
        {
            Random rnd = new Random();
            string barcode = string.Empty;
            int counter = 0;
            while(counter<4)
            {
                barcode += rnd.Next(1, 9).ToString();
                counter++;
            }
            return barcode;
            
        }

        public static string GetName()
        {
            Random rndname = new Random();
            Random rndLengthName = new Random();
            int lengthName = rndLengthName.Next(3, 10);
            string name = string.Empty;
            int counter = 0;
            while (counter < lengthName)
            {
                name +=((char)Convert.ToByte(rndname.Next(97, 122))).ToString();
                counter++;
            }
            return name;
        }
        public static string GetFamily()
        {
            Random rndFamily = new Random();
            Random rndLengthFamily = new Random();
            int lengthFamily = rndLengthFamily.Next(3, 10);
            string family = string.Empty;
            int counter = 0;
            while (counter < lengthFamily)
            {
                family += ((char)Convert.ToByte(rndFamily.Next(97, 122))).ToString();
                counter++;
            }
            return family;
        }
    }
}