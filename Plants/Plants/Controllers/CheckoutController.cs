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
using System.Net.Mail;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Kernel.Font;
using iText.IO.Font;

namespace Plants.Controllers
{
    public class CheckoutController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Сheckout
        public async Task<ActionResult> Index(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (db.Users.Find(id) != null)
            {
                string phoneNumber = db.Users.FirstAsync(u => u.Id == id).Result.PhoneNumber;
                if (!string.IsNullOrEmpty(phoneNumber))
                {
                    Checkout checkout = await db.Checkouts.FindAsync(id);
                    if (checkout == null)
                    {

                        checkout = new Checkout()
                        {
                            Id = id,
                            FirstName = string.Empty,
                            SecondName = string.Empty,
                            PhoneNumber = phoneNumber
                        };
                    }
                    return View(new CheckoutAddressViewModels() { Checkout = checkout, Address = new Address() });
                }
                else
                    return RedirectToAction("AddPhoneNumber", "Manage", new { returnUrl = HttpContext.Request.RawUrl });
            }
            else
                return RedirectToAction("Login", "Account", new { returnUrl = "Checkout/Index/" + id, cartId = id });
        }

        // POST: Сheckout/Index
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в разделе https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<ActionResult> Index(CheckoutAddressViewModels checkoutAddress)
        {
            if (ModelState.IsValid)
            {
                var checkout = checkoutAddress.Checkout;
                checkoutAddress.Address.FullAddress = string.Format("{0} г.{1} ул.{2} д.{3} к.{4} кв.{5} э.{6} дф.{7} под.{8}",
                    checkoutAddress.Address.Country, checkoutAddress.Address.City, checkoutAddress.Address.Street, checkoutAddress.Address.House, 
                    checkoutAddress.Address.Korpus, checkoutAddress.Address.Flat, checkoutAddress.Address.Floor, checkoutAddress.Address.Intercom, 
                    checkoutAddress.Address.Entrance);
                if (!db.Checkouts.Where(c => c.Id == checkoutAddress.Checkout.Id).Any())
                {
                    db.Checkouts.Add(checkout).Addresses.Add(checkoutAddress.Address);
                }
                else
                {
                    var address = db.Checkouts.First(c => c.Id == checkoutAddress.Checkout.Id).Addresses.Where(a
                 => a.City == checkoutAddress.Address.City && a.Street == checkoutAddress.Address.Street &&
                 a.House == checkoutAddress.Address.House && a.Flat == checkoutAddress.Address.Flat);
                    if (!address.Any())
                        db.Checkouts.First(c => c.Id == checkoutAddress.Checkout.Id).Addresses.Add(checkoutAddress.Address);
                    else if (!address.Any(a => a.FullAddress.Length >= checkoutAddress.Address.FullAddress.Length))
                    {
                        db.Address.RemoveRange(address);
                        db.Checkouts.First(c => c.Id == checkoutAddress.Checkout.Id).Addresses.Add(checkoutAddress.Address);
                    }
                }
                await db.SaveChangesAsync();
                await Buy(checkoutAddress.Checkout.Id, checkoutAddress);
                return RedirectToAction("Completed");
            }

            return View(checkoutAddress);
        }

        public async Task Buy(string id, CheckoutAddressViewModels checkoutAddress)
        {
            int amount = 0;
            decimal qost = 0;
            DateTime dateTime = DateTime.Now;
            List<Products> products = new List<Products>();
            List<ApplicationUser> users = new List<ApplicationUser>() { await db.Users.FirstAsync(u => u.Id == id) };
            List<QuantityProducts> quantityProducts = new List<QuantityProducts>();
            string pr = string.Empty;
            foreach (var item in db.ShoppingCartItems.Where(c => c.CartId == id))
            {
                quantityProducts.Add(new QuantityProducts(){Quantity = item.Quantity, ProductId = item.Product.Id });
                pr += item.Product.Name + " в количестве: " + item.Quantity + " ед." + ", стоимсость за единицу: " + item.Product.Qost + " руб." + "\r\n";
                amount += item.Quantity;
                item.Product.QuantityInStock -= item.Quantity;// с этим аккуратно
                qost += item.Product.Qost * item.Quantity;
                products.Add(item.Product);
            }
            var purchase = new string[] { checkoutAddress.Checkout.SecondName + " " + checkoutAddress.Checkout.FirstName, dateTime.ToString(),
                                        checkoutAddress.Address.FullAddress, pr, qost.ToString() + " руб." };
            var data = new string[] { "Покупатель:", "Дата покупки:", "Адрес доставки:", "Товары:", "Стоимость покупки:" };
            Purchases purchases = new Purchases() {Amount = amount, Qost = qost, DateTime = dateTime, Check = CreateCheck(purchase, data) ,Show = true,
                                                   QuantityProducts = quantityProducts, Products = products, Users = users};
            db.Users.FirstOrDefault(u => u.Id == id).Purchases.Add(purchases);

            await SendEmil(db.Users.FirstOrDefaultAsync(u => u.Id == id).Result.Email, purchases.Check);

            db.ShoppingCartItems.RemoveRange(db.ShoppingCartItems.Where(c => c.CartId == id));
            await db.SaveChangesAsync();
        }

        public async Task SendEmil(string to, byte[] check)
        {
            var from = "plantsmarket.official@gmail.com";
            var pass = "yuchdumahjoiacqx";

            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);

            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(from, pass);
            client.EnableSsl = true;

            var mail = new MailMessage(from, to);

            mail.Subject = "noreply";
            mail.Body = "<h2>Чек покупки</h2>";
            mail.IsBodyHtml = true;
            using (MemoryStream memStream = new MemoryStream(check))
            {
                memStream.Position = 0;
                var contentType = new System.Net.Mime.ContentType(System.Net.Mime.MediaTypeNames.Application.Pdf);
                Attachment attach = new Attachment(memStream, contentType);
                attach.ContentDisposition.FileName = "Check.pdf";
                mail.Attachments.Add(attach);
                await client.SendMailAsync(mail);
            }
        }

        public byte[] CreateCheck(string[] purchase, string[] data)
        {
            var stream = new MemoryStream();
            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);
            PdfFont font = PdfFontFactory.CreateFont("c:/windows/fonts/times.ttf", "cp1251", PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
            Paragraph header = new Paragraph("ЧЕК от " + purchase[1]).SetFont(font)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetFontSize(24);
            document.Add(header);
            LineSeparator ls = new LineSeparator(new SolidLine());
            document.Add(ls);
            Table table = new Table(2, false).SetHorizontalAlignment(HorizontalAlignment.CENTER);


            for (int i = 0; i < purchase.Length; ++i)
            {
                var cell1 = new Cell(1, 1).SetTextAlignment(TextAlignment.LEFT)
                    .SetFont(font).SetFontSize(18)
                  .Add(new Paragraph(data[i]));
                var cell2 = new Cell(1, 1).SetTextAlignment(TextAlignment.LEFT)
                    .SetFont(font).SetFontSize(18).SetItalic()
                  .Add(new Paragraph(purchase[i]));
                table.AddCell(cell1);
                table.AddCell(cell2);
            }
            document.Add(table);
            document.Close();
            return stream.ToArray();

        }
        public ActionResult ToCart(string returnUrl)
        {
            return RedirectToAction("Index", "Cart", new { returnUrl = returnUrl });
        }

        public ActionResult Completed()
        {
            return View();
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
