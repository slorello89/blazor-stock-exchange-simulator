using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MarketMechanicsDemo.Shared
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }
        public BuySell BuyOrSell { get; set; }
        public LimitMarket LimitOrMarket { get; set; }
        public string Symbol { get; set; }
        public int NumSharesRemaining { get; set; }
        public int NumSharesInital { get; set; }
        public double Price { get; set; }
        public string TimePlaced { get; set; }
        public double FillPrice { get; set; }
    }
}
