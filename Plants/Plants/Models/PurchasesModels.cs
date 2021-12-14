using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Plants.Models
{
    public class Purchases
    {
        public int Id { get; set; }
        public int Amount { get; set; }
        public decimal Qost { get; set; }
        public DateTime DateTime { get; set; }
        public byte[] Check { get; set; }
        public bool Show { get; set; }
        public virtual ICollection<ApplicationUser> Users { get; set; }
        public virtual ICollection<Products> Products { get; set; }
        public ICollection<QuantityProducts> QuantityProducts { get; set; }
        public Purchases()
        {
            Users = new List<ApplicationUser>();
            Products = new List<Products>();
            QuantityProducts = new List<QuantityProducts>();
        }
    }
}