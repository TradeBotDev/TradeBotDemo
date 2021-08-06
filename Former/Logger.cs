using System;
using System.Threading.Tasks;
using TradeBot.Common.v1;

namespace Former
{
    public class Logger
    {
        public event Func<string, LogLevel, DateTimeOffset, Task> NewLog;

        public async Task WriteToLog(string message, LogLevel level, DateTimeOffset time)
        {
            await NewLog?.Invoke(message, level, time);
        }
    }
}
