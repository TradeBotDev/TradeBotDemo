using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Former.Models
{
    public static class Constance
    {
        public static readonly Dictionary<string, double> SlotsMultipliers = new ()
        {
            {"XBTUSD", 1},
            {"ETHUSD", 0.000001},
            {"DOGEUSD", 0.0010},
            {"LTCUSD", 0.000002},
            {"ADAUSDT", 0.0100},
            {"XRPUSD", 0.0002},
            {"SOLUSDT", 0.00001}
        };
    }

}
