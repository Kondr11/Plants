using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Plants.Models;
using ClosedXML.Excel;
using System.IO;

namespace Plants.Controllers
{
    public class HistoryPurchasesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: HistoryPurchases
        public async Task<ActionResult> Index(string userId)
        {
            return View(await db.Purchases.Where(p => p.Users.Where(u => u.Id == userId).Any()).OrderByDescending(p => p.DateTime).ToListAsync());
        }

        // GET: HistoryPurchases/Details/5
        public async Task<ActionResult> Details(string userId, int purchaseId)
        {
            if (userId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var purchases = new List<Purchases>();
            purchases = await db.Purchases.Where(p => p.Users.FirstOrDefault().Id == userId && p.Id == purchaseId).ToListAsync();
            ViewBag.UserId = userId;
            if (purchases == null)
            {
                return HttpNotFound();
            }
            return View(purchases);
        }

        // GET: HistoryPurchases/Delete/5
        public async Task<ActionResult> Delete(string userId, int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Purchases purchases = await db.Purchases.FindAsync(id);
            ViewBag.UserId = userId;
            if (purchases == null)
            {
                return HttpNotFound();
            }
            return View(purchases);
        }

        // POST: HistoryPurchases/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id, string userId)
        {
            db.Purchases.FindAsync(id).Result.Show = false;
            await db.SaveChangesAsync();
            return RedirectToAction("Index", new { userId = userId });
        }

        public async Task<ActionResult> Export()
        {
            using (XLWorkbook workbook = new XLWorkbook(XLEventTracking.Disabled))
            {
                await DbToPurchases(workbook);

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Flush();

                    return new FileContentResult(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        FileDownloadName = $"purchases_{DateTime.UtcNow.ToShortDateString()}.xlsx"
                    };
                }
            }
        }

        public async Task DbToPurchases(XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("Покупки");

            worksheet.Cell("A1").Value = "Время покупки";
            worksheet.Cell("B1").Value = "ФИ";
            worksheet.Cell("C1").Value = "Телефон";
            worksheet.Cell("D1").Value = "Адрес доставки";
            worksheet.Cell("E1").Value = "Количество товаров";
            worksheet.Cell("F1").Value = "Итоговая стоимость, руб.";
            worksheet.Cell("G1").Value = "Название товара";
            worksheet.Cell("H1").Value = "Количество ед.";
            worksheet.Cell("I1").Value = "Цена за ед., руб.";

            worksheet.Row(1).Style.Font.Bold = true;

            List<Purchases> purchases  = await db.Purchases.ToListAsync();
            List<QuantityProducts> quantities = await db.QuantityProducts.ToListAsync();
            List<Checkout> checkouts = await db.Checkouts.ToListAsync();
            int k = 0;
            for (int i = 0; i < purchases.Count; i++)
            {
                worksheet.Cell(2 + k, 1).SetDataType(XLDataType.Text).SetValue(purchases[i].DateTime.ToString());
                string userId = purchases[i].Users.First().Id;
                worksheet.Cell(2 + k, 2).Value = checkouts.Find(c => c.Id == userId).SecondName + " "
                    + checkouts.Find(c => c.Id == userId).FirstName;
                worksheet.Cell(2 + k, 3).SetValue(checkouts.Find(c => c.Id == userId).PhoneNumber.ToString());
                worksheet.Cell(2 + k, 4).Value = checkouts.Find(c => c.Id == userId).Addresses.Last().FullAddress;
                worksheet.Cell(2 + k, 5).Value = purchases[i].Amount;
                worksheet.Cell(2 + k, 6).Value = purchases[i].Qost;
                int j = 0;
                List<Products> products = purchases[i].Products.ToList();
                foreach (var item in quantities.Where(q => q.PurchaseId == purchases[i].Id))
                {
                    worksheet.Cell(2 + k + j, 7).Value = products[j].Name;
                    worksheet.Cell(2 + k + j, 8).Value = item.Quantity;
                    worksheet.Cell(2 + k + j, 9).Value = products[j].Qost;
                    j++;
                }
                k += products.Count();
            }
            worksheet.Columns().AdjustToContents();
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
