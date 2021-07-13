using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeBot.Common;

namespace TradeMarket.Model
{
    /// <summary>
    /// Этот класс для внутренней модели биржи. будет содержать больше информации чем нужно передавать 
    /// </summary>
    public class FullOrder
    {
        public String Id { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public double RemovePrice { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public DateTime RemoveDate { get; set; }
        public OrderSignature Signature { get; set; }
       
    }
}
