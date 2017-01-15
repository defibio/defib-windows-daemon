using Defib.Security;
using System;

namespace Defib
{
    public static class Utils
    {

        #region TOKENS

        public static bool IsTokenValid(string token)
        {
            if (!Context.Tokens.ContainsKey(token))
            {
                return false;
            }

            Token localToken = Context.Tokens[token];

            if (localToken.Expires > GetCurrentTimestamp())
            {
                return true;
            }

            return false;
        }

        #endregion

        #region CRYPTO

        public static string HashPassword(string password, string hash, string username)
        {
            return EasyEncryption.SHA.ComputeSHA256Hash(string.Format("{0}{1}{2}", password, hash, username));
        }

        public static string GenerateToken(string username)
        {
            return EasyEncryption.SHA.ComputeSHA256Hash(string.Format("{0}{1}", username, GetCurrentTimestamp()));
        }

        #endregion

        #region TIMESTAMPS

        // This is a problem for someone else (probably me) to figure out in 2038
        public static int GetCurrentTimestamp()
        {
            long ticks = DateTime.UtcNow.Ticks - DateTime.Parse("01/01/1970 00:00:00").Ticks;
            ticks /= 10000000;
            return Int32.Parse(ticks.ToString());
        }

        public static int GetTimeStamp(DateTime date)
        {
            long ticks = date.Ticks - DateTime.Parse("01/01/1970 00:00:00").Ticks;
            ticks /= 10000000;
            return Int32.Parse(ticks.ToString());
        }

        public static DateTime GetDateTime(int timestamp)
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(timestamp);
        }

        #endregion
    }
}
