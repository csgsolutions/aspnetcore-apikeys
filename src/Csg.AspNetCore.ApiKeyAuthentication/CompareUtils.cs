using System;
using System.Collections.Generic;
using System.Text;

namespace Csg.AspNetCore.ApiKeyAuthentication
{
    internal class CompareUtils
    {
        public static bool CompareSlow(string a, string b)
        {
            var aChars = a.ToCharArray();
            var bChars = b.ToCharArray();
            bool result = true;


            for (int i = 0; i < aChars.Length; i++)
            {
                if (i >= b.Length)
                {
                    continue;
                }

                if (!aChars[i].Equals(bChars[i]))
                {
                    result = false;
                }
            }

            return result;
        }
    }
}
