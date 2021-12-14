using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Plants.Models
{
    public class Address
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Укажите страну")]
        public string Country { get; set; }

        [Required(ErrorMessage = "Укажите город")]
        public string City { get; set; }

        [Required(ErrorMessage = "Укажите улицу")]
        public string Street { get; set; }

        [Required(ErrorMessage = "Укажите дом")]
        public string House { get; set; }
        public string Korpus { get; set; }

        [Required(ErrorMessage = "Укажите квартиру")]
        public string Flat { get; set; }
        public string Floor { get; set; }
        public string Intercom { get; set; }    //домофон
        public string Entrance { get; set; }    //подъезд
        public string FullAddress { get; set; }
        public virtual ICollection<Checkout> Checkouts { get; set; }

        public Address()
        {
            Checkouts = new List<Checkout>();
        }
    }
}