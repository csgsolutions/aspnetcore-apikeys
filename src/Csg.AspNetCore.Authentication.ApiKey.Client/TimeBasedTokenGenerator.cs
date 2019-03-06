using System;
using System.Collections.Generic;
using System.Text;

namespace Csg.ApiKeyGenerator
{
    public enum HashType
    {
        HMACSHA1,
        HMACSHA256,
        HMACSHA512
    }

    public class TimeBasedTokenGenerator
    {
        private static readonly DateTimeOffset _epoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

        public HashType HashType { get; set; } = HashType.HMACSHA256;
        
        /// <summary>
        /// The number of seconds in an interval of time where the same key will be generated.
        /// </summary>
        public int IntervalSeconds { get; set; } = 60;

        public int AllowedNumberOfDriftIntervals { get; set; } = 1;

        protected System.Security.Cryptography.HMAC GetHashMethod(byte[] key)
        {
            switch (this.HashType)
            {
                case HashType.HMACSHA1: return new System.Security.Cryptography.HMACSHA1(key);
                case HashType.HMACSHA256: return new System.Security.Cryptography.HMACSHA256(key);
                case HashType.HMACSHA512: return new System.Security.Cryptography.HMACSHA512(key);
                default: throw new NotSupportedException($"An unsupported hash type {this.HashType} was specified.");
            }
        }

        protected long GetCounter(DateTimeOffset now)
        {
            long epochSeconds = (long)now.Subtract(_epoch).TotalSeconds;

            return epochSeconds / this.IntervalSeconds;
        }

        /// <summary>
        /// Generates a time-based token based using the current
        /// </summary>
        /// <param name="clientID"></param>
        /// <param name="secret"></param>
        /// <param name="utcNow"></param>
        /// <returns></returns>
        public byte[] ComputeToken(string clientID, string secret, DateTimeOffset now)
        {
            if (string.IsNullOrWhiteSpace(clientID)) throw new ArgumentNullException(nameof(clientID));
            if (string.IsNullOrWhiteSpace(secret)) throw new ArgumentNullException(nameof(secret));

            if (now < _epoch)
            {
                throw new ArgumentOutOfRangeException(nameof(now));
            }

            return ComputeTokenInternal(clientID, secret, this.GetCounter(now));
        }

        protected byte[] ComputeTokenInternal(string clientID, string secret, long counter)
        {
            if (string.IsNullOrWhiteSpace(clientID)) throw new ArgumentNullException(nameof(clientID));
            if (string.IsNullOrWhiteSpace(secret)) throw new ArgumentNullException(nameof(secret));

            if (counter < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(counter));
            }

            var key = new System.Security.Cryptography.Rfc2898DeriveBytes(secret, BitConverter.GetBytes(counter)).GetBytes(32);
            var message = System.Text.UTF8Encoding.UTF8.GetBytes(clientID.Trim().ToUpperInvariant());

            return this.GetHashMethod(key).ComputeHash(message);
        }
        
        public bool ValidateToken(string clientID, string secret, byte[] token, DateTimeOffset now)
        {
            if (string.IsNullOrWhiteSpace(clientID)) throw new ArgumentNullException(nameof(clientID));
            if (string.IsNullOrWhiteSpace(secret)) throw new ArgumentNullException(nameof(secret));
            if (token == null) throw new ArgumentNullException(nameof(token));

            long counter = this.GetCounter(now);

            // get epoch seconds rounded to the nearest interval
            var systemToken = ComputeTokenInternal(clientID, secret, counter);
            
            // slow compare the hashes
            if (TokenCompareHelper.AreTokensEqual(systemToken, token))
            {
                return true;
            }

            // attempt to match tokens for the number of intervals of tolerance forward or backward
            long altCounter;

            for (int i = 1; i <= this.AllowedNumberOfDriftIntervals; i++)
            {
                altCounter = counter + i;
                // try a match where the caller's clock was ahead of the system time
                systemToken = ComputeTokenInternal(clientID, secret, altCounter);

                if (TokenCompareHelper.AreTokensEqual(systemToken, token))
                {
                    return true;
                }

                altCounter = counter - i;
                // try a match where the caller's clock was behind the system time
                systemToken = ComputeTokenInternal(clientID, secret, altCounter);

                if (TokenCompareHelper.AreTokensEqual(systemToken, token))
                {
                    return true;
                }
            }


            return false;
        }
    }
}
