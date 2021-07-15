using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TradeMarket.Model
{
    //TODO это должен быть легковес
    public class Currency
    {

        public string Name { get; set; }

        public Currency(string name)
        {
            Name = name;
        }
    }
}
