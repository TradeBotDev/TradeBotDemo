using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TradeMarket.Model
{
    public class Balance : IComparable<Balance>
    {
        #region Static Members
        
        //TODO возмоно это надо запихать в бд
        public static Dictionary<string, string> AllCurrencies;

        public static IOrderedEnumerable<Balance> AllCurrenciesBalance;
        #endregion

        public string Currency { get; set; }

        public double Value { get; set; }

        public Balance(string currency, double value)
        {
            Currency = currency;
            Value = value;
        }



        /// <summary>
        /// Компоратор пока работает по именам только
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(Balance other)
        {
            return Currency.CompareTo(other.Currency);
        }
    }
}
