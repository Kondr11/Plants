using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Plants.Models
{
    public class Checkout
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Укажите имя")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Укажите фамилию")]
        public string SecondName { get; set; }

        [Required(ErrorMessage = "Укажите номер телефон")]
        public string PhoneNumber { get; set; }
        public virtual ICollection<ApplicationUser> Users { get; set; }
        public virtual ICollection<Address> Addresses { get; set; }
        public Checkout()
        {
            Users = new List<ApplicationUser>();
            Addresses = new List<Address>();
        }
    }
}