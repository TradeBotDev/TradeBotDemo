using History.DataBase;
using System;
using System.Threading;
using History.Cache;

namespace History
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DataContext postgres = new();
            Console.ReadKey();
            RedisReader rr = new();
            rr.ShowKeys();
            Console.ReadKey();
        }
    }
}
