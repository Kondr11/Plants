using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Plants.Models
{
    public class Products
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Укажите наименование")]
        public string Name { get; set; }
        [NotMapped]
        public HttpPostedFileBase ProductImg { get; set; }

        [Required(ErrorMessage = "Добавьте описание")]
        public string Description { get; set; }
        public int Quatity { get; set; }

        [Required(ErrorMessage = "Укажите количество на складе")]
        public int QuantityInStock { get; set; }

        [Required(ErrorMessage = "Укажите цену")]
        public decimal Qost { get; set; }

        [Required(ErrorMessage = "Загрузите изображение")]
        public byte[] Image { get; set; }

        public DateTime CreationDate { get; set; }
        public int? CategoryID { get; set; }
        public virtual Category Category { get; set; }
        public virtual ICollection<Purchases> Purchases { get; set; }
        public Products()
        {
            Purchases = new List<Purchases>();
        }
    }
}