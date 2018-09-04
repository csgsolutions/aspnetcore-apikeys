using System;
using System.Collections.Generic;
using System.Text;

namespace Csg.AspNetCore.Authentication.ApiKey
{
    internal class TimeBasedHmac
    {
        private string _algorithmName;
        
        /// <summary>
        /// Initializes and instance with the given algorithm name.
        /// </summary>
        /// <param name="algorithmName">Accepts any algorithm name accepted by <see cref="System.Security.Cryptography.HMAC.Create"/>.</param>
        public TimeBasedHmac(string algorithmName)
        {
            _algorithmName = algorithmName;
            this.IntervalSeconds = 60;
            this.AllowDriftSteps = 5;
        }
                    
        /// <summary>
        /// Gets or sets the time interval in seconds for which a signature will be valid.
        /// </summary>
        public int IntervalSeconds { get; set; }

        /// <summary>
        /// Gets or sets the number of intervals backwards or forwards in time the validator will allow when validating the signature.
        /// </summary>
        public int AllowDriftSteps { get; set; }

        private System.Security.Cryptography.HMAC GetHmac()
        {
            if (_algorithmName.Equals("HMACSHA1", StringComparison.OrdinalIgnoreCase))
            {
                return new System.Security.Cryptography.HMACSHA1();
            }
            else if (_algorithmName.Equals("HMACSHA256", StringComparison.OrdinalIgnoreCase))
            {
                return new System.Security.Cryptography.HMACSHA256();
            }
            else if (_algorithmName.Equals("HMACSHA512", StringComparison.OrdinalIgnoreCase))
            {
                return new System.Security.Cryptography.HMACSHA512();
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Computes an authenticator for a given message and password.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="password"></param>
        /// <param name="now"></param>
        /// <returns></returns>
        public byte[] ComputeAuthenticator(byte[] message, string password, DateTime now)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message", "The message argument cannot be null.");
            }

            if (password == null)
            {
                throw new ArgumentNullException("password", "The password argument cannot be null.");
            }
            
            var key = new System.Security.Cryptography.Rfc2898DeriveBytes(password, BitConverter.GetBytes(RoundDate(now, this.IntervalSeconds).Ticks));


            using (var hmac = GetHmac())
            {
                var hash = new byte[(hmac.HashSize/8)+1];
                hmac.Key = key.GetBytes(32);
                hash[0] = 0;
                hmac.ComputeHash(message).CopyTo(hash, 1);
                return hash;
            }
        }

        /// <summary>
        /// Validates an authenticator for a given message and password.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="authenticator"></param>
        /// <param name="password"></param>
        /// <param name="now"></param>
        /// <returns></returns>
        public bool ValidateAuthenticator(byte[] message, byte[] authenticator, string password, DateTime now)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message", "The message argument cannot be null.");
            }

            if (authenticator == null)
            {
                throw new ArgumentNullException("authenticator", "The authenticator argument cannot be null.");
            }

            if (password == null)
            {
                throw new ArgumentNullException("password", "The password argument cannot be null.");
            }

            var salt = new byte[8];
            byte[] hash;

            if (authenticator.Length <= 0)
            {
                return false;
            }

            // The first byte is a version indicator
            if (authenticator[0] != 0)
            {
                return false;
            }

            hash = ComputeAuthenticator(message, password, now);

            if (ByteHelper.AreArraysEqual(hash, authenticator))
            {
                return true;
            }

            now = RoundDate(now, this.IntervalSeconds);

            for (var i = -this.AllowDriftSteps; i <= this.AllowDriftSteps; i++)
            {
                if (i == 0)
                {
                    continue;
                }

                hash = ComputeAuthenticator(message, password, now.AddSeconds(this.IntervalSeconds * i));

                if (ByteHelper.AreArraysEqual(hash, authenticator))
                {
                    return true;
                }
            }

            return false;
        }

        private static DateTime RoundDate(DateTime date, int intervalSeconds)
        {
            var epoch = new DateTime(1970, 01, 01, 0, 0, 0, DateTimeKind.Utc);
            long seconds = (long)Math.Floor(date.Subtract(epoch).TotalSeconds);

            seconds = seconds - (seconds % intervalSeconds);

            return epoch.AddSeconds(seconds);
        }
    }
}
