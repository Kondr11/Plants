using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Plants.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Укажите наименование категории")]
        public string CategoryName { get; set; }

        [Required(ErrorMessage = "Добавьте описание")]
        public string Description { get; set; }
        public virtual ICollection<Products> Products { get; set; }
    }
}