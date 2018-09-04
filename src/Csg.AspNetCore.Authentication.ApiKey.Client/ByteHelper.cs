using System;
using System.Collections.Generic;
using System.Text;

namespace Csg.AspNetCore.Authentication.ApiKey
{
    internal static class ByteHelper
    {
        public static bool AreArraysEqual(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;

            if (a.Length == 0)
                return true;

            var result = true;

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    result = false;
                }
            }

            return result;
        }    
    }
}
