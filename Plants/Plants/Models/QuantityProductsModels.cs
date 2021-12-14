using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Plants.Models
{
    public class QuantityProducts
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public int ProductId { get; set; }
        public int? PurchaseId { get; set; }
        public Purchases Purchase { get; set; }
    }
}