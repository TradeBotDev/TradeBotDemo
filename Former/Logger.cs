using System;
using System.Threading.Tasks;
using TradeBot.Common.v1;

namespace Former
{
    public class Logger
    {
        public event Func<string, LogLevel, DateTimeOffset, Task> NewLog;

        public void WriteToLog(string message, LogLevel level, DateTimeOffset time)
        {
            NewLog?.Invoke(message, level, time);
        }
    }
}
