using History.DataBase;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using TradeBot.Common.v1;
using Newtonsoft.Json;

namespace History
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DataContext db = new();

            Thread.Sleep(3000);
        }
    }
}
