using Plants.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Plants.Controllers
{
    public class ProductsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Products
        public async Task<ActionResult> Index()
        {
            return View(await db.Products.ToListAsync());
        }

        public async Task<PartialViewResult> NewProducts()
        {
            return PartialView(await db.Products.Where(p => p.CreationDate > DateTime.Now.AddMonths(-1)).ToListAsync());
        }

        public PartialViewResult PopularProducts()
        {
            var dic = new Dictionary<Products, int>();
            foreach (var purchase in db.Purchases)
            {
                foreach (var product in purchase.Products)
                {
                    if (dic.ContainsKey(product))
                    {
                        dic[product]++;
                    }
                    else
                    {
                        dic.Add(product, 1);
                    }
                }
            }
            dic.OrderBy(p => p.Value);
            return PartialView(dic.Keys.ToList());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}