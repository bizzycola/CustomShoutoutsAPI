using System.Security.Cryptography;
using System.Text;

namespace CustomShoutoutsAPI.Helpers
{
    /// <summary>
    /// Used to generate random strings for certain resources(such as invite codes)
    /// </summary>
    public class KeyGenerator
    {
        internal static readonly char[] chars =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

        /// <summary>
        /// Returns a randomised key of the specified size
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static string GetUniqueKey(int size)
        {
            byte[] data = RandomNumberGenerator.GetBytes(4 * size);

            StringBuilder result = new(size);
            for (int i = 0; i < size; i++)
            {
                var rnd = BitConverter.ToUInt32(data, i * 4);
                var idx = rnd % chars.Length;

                result.Append(chars[idx]);
            }

            return result.ToString();
        }

        public static string GetUniqueKeyOriginal_BIASED(int size)
        {
            char[] chars =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            byte[] data = RandomNumberGenerator.GetBytes(size);

            StringBuilder result = new(size);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }
    }
}
