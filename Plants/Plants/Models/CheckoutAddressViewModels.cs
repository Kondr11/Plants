using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Plants.Models
{
    public class CheckoutAddressViewModels
    {
        public Checkout Checkout { get; set; }
        public Address Address { get; set; }
    }
}