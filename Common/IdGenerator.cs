using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class IdGenerator
    {
        private static readonly Random Random = new Random();
        public static string CreateNewOrderId()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 4)
              .Select(s => s[Random.Next(s.Length)]).ToArray());
        }

        public static string CreateNewDealId()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 4)
              .Select(s => s[Random.Next(s.Length)]).ToArray());
        }
    }
}
