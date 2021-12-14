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
using System.IO;
using ClosedXML.Excel;

namespace Plants.Controllers
{
    [Authorize(Roles = "operator")]
    public class ManagingProductsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: ManagingProducts
        public async Task<ActionResult> Index()
        {
            var products = db.Products.Include(p => p.Category);
            return View(await products.ToListAsync());
        }

        // GET: ManagingProducts/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Products products = await db.Products.FindAsync(id);
            if (products == null)
            {
                return HttpNotFound();
            }
            return View(products);
        }

        // GET: ManagingProducts/Create
        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(db.Categories, "Id", "CategoryName");
            return View();
        }

        // POST: ManagingProducts/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в разделе https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,Description,QuantityInStock,Qost,Image,ProductImg,CreationDate,CategoryID")] Products products)
        {
            if (ModelState.IsValid)
            {
                if (products.ProductImg != null)
                {
                    byte[] image = new byte[products.ProductImg.ContentLength];
                    products.ProductImg.InputStream.Read(image, 0, image.Length);
                    products.Image = image;
                }
                products.Quatity = 1;
                products.CreationDate = DateTime.Now;
                db.Products.Add(products);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryID = new SelectList(db.Categories, "Id", "CategoryName", products.CategoryID);
            return View(products);
        }

        // GET: ManagingProducts/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Products products = await db.Products.FindAsync(id);
            if (products == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "Id", "CategoryName", products.CategoryID);
            return View(products);
        }

        // POST: ManagingProducts/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в разделе https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Description,QuantityInStock,Qost,Image,ProductImg,CategoryID")] Products products)
        {
            if (ModelState.IsValid)
            {
                if (products.ProductImg != null)
                {
                    byte[] image = new byte[products.ProductImg.ContentLength];
                    products.ProductImg.InputStream.Read(image, 0, image.Length);
                    products.Image = image;
                }
                products.Quatity = 1;
                db.Entry(products).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "Id", "CategoryName", products.CategoryID);
            return View(products);
        }

        // GET: ManagingProducts/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Products products = await db.Products.FindAsync(id);
            if (products == null)
            {
                return HttpNotFound();
            }
            return View(products);
        }

        // POST: ManagingProducts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Products products = await db.Products.FindAsync(id);
            db.Products.Remove(products);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public ActionResult ToCategories()
        {
            return RedirectToAction("Index", "ManagingCategories");
        }

        public ActionResult Import()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Import(HttpPostedFileBase fileExcel)
        {
            if (ModelState.IsValid)
            {
                int errorsCount = 0;
                using (XLWorkbook workBook = new XLWorkbook(fileExcel.InputStream, XLEventTracking.Disabled))
                {
                    foreach (IXLWorksheet worksheet in workBook.Worksheets)
                    {
                        if (worksheet.Name == "Категории")
                            errorsCount = CategoriesToDb(worksheet);
                        else if (worksheet.Name == "Продукты")
                            errorsCount = ProductsToDb(worksheet).Result;
                        ViewBag.ErrorsCount = errorsCount;
                    }
                }
                await db.SaveChangesAsync();
                return RedirectToAction("Index", "ManagingProducts");
            }
            return RedirectToAction("Index", "ManagingProducts");
        }

        public int CategoriesToDb(IXLWorksheet worksheet)
        {
            int errorsCount = 0;
            foreach (IXLRow row in worksheet.RowsUsed().Skip(1))
            {
                try
                {
                    Category category = new Category();
                    category.CategoryName = row.Cell(1).Value.ToString();
                    category.Description = row.Cell(2).Value.ToString();
                    if (!db.Categories.Where(c => c.CategoryName == category.CategoryName).Any())
                        db.Categories.Add(category);
                    else
                    {
                        db.Categories.Where(c => c.CategoryName == category.CategoryName).FirstAsync().Result.Description = category.Description;
                    }
                }
                catch (Exception e)
                {
                    errorsCount++;
                }
            }
            return errorsCount;
        }

        public async Task<int> ProductsToDb(IXLWorksheet worksheet)
        {
            int errorsCount = 0;
            Dictionary<int, ClosedXML.Excel.Drawings.IXLPicture> PicturesByCellAddress
                         = new Dictionary<int, ClosedXML.Excel.Drawings.IXLPicture>();
            foreach (ClosedXML.Excel.Drawings.IXLPicture pic in worksheet.Pictures)
            {
                try { PicturesByCellAddress.Add(pic.TopLeftCell.Address.RowNumber, pic); }
                catch { }
            }
            foreach (IXLRow row in worksheet.RowsUsed().Skip(1))
            {
                try
                {
                    Products product = new Products();
                    product.Name = row.Cell(1).Value.ToString();
                    product.Description = row.Cell(2).Value.ToString();
                    product.QuantityInStock = int.Parse(row.Cell(3).Value.ToString());
                    product.Qost = decimal.Parse(row.Cell(4).Value.ToString());
                    byte[] image = new byte[PicturesByCellAddress[row.RowNumber()].ImageStream.Length];
                    await PicturesByCellAddress[row.RowNumber()].ImageStream.ReadAsync(image, 0, image.Length);
                    product.Image = image;
                    var name = row.Cell(6).Value.ToString();
                    product.Category = db.Categories.Where(c => c.CategoryName == name).First();
                    product.CategoryID = product.Category.Id;
                    product.CreationDate = DateTime.Now;
                    if (!db.Products.Where(p=> p.Name == product.Name).Any())
                        db.Products.Add(product);
                    else
                    {
                        var removeItem = db.Products.Where(p => p.Name == product.Name);
                        product.CreationDate = removeItem.First().CreationDate;
                        db.Products.RemoveRange(removeItem);
                        db.Products.Add(product);
                    }
                }
                catch (Exception e)
                {
                    errorsCount++;
                }
            }
            return errorsCount;
        }

        public async Task<ActionResult> Export()
        {
            using (XLWorkbook workbook = new XLWorkbook(XLEventTracking.Disabled))
            {
                await DbToCategories(workbook);
                await DbToProductstegories(workbook);

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Flush();

                    return new FileContentResult(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        FileDownloadName = $"products_{DateTime.UtcNow.ToShortDateString()}.xlsx"
                    };
                }
            }
        }

        public async Task DbToCategories(XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("Категории");

            worksheet.Cell("A1").Value = "Название";
            worksheet.Cell("B1").Value = "Описание";
            worksheet.Row(1).Style.Font.Bold = true;

            List<Category> categories = await db.Categories.ToListAsync();

            for (int i = 0; i < categories.Count; i++)
            {
                worksheet.Cell(i + 2, 1).Value = categories[i].CategoryName;
                worksheet.Cell(i + 2, 2).Value = categories[i].Description;
            }
            worksheet.Columns().AdjustToContents();
        }

        public async Task DbToProductstegories(XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("Продукты");

            worksheet.Cell("A1").Value = "Название";
            worksheet.Cell("B1").Value = "Описание";
            worksheet.Cell("C1").Value = "Количество на складе";
            worksheet.Cell("D1").Value = "Цена, руб.";
            worksheet.Cell("E1").Value = "Изображение";
            worksheet.Cell("F1").Value = "Дата добавления";
            worksheet.Cell("G1").Value = "Категория";
            worksheet.Row(1).Style.Font.Bold = true;

            List<Products> products = await db.Products.ToListAsync();

            for (int i = 0; i < products.Count; i++)
            {
                worksheet.Cell(i + 2, 1).Value = products[i].Name;
                worksheet.Cell(i + 2, 2).Value = products[i].Description;
                worksheet.Cell(i + 2, 3).Value = products[i].QuantityInStock;
                worksheet.Cell(i + 2, 4).Value = products[i].Qost;
                using (MemoryStream memStream = new MemoryStream())
                {
                    await memStream.WriteAsync(products[i].Image, 0, products[i].Image.Length);
                    worksheet.AddPicture(memStream).MoveTo(worksheet.Cell(i + 2, 5)).WithSize(64, 64);
                }
                worksheet.Cell(i + 2, 6).SetDataType(XLDataType.Text).SetValue(products[i].CreationDate.ToString());
                worksheet.Cell(i + 2, 7).Value = products[i].Category.CategoryName;
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
