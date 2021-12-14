using Plants.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;

namespace Plants.Controllers
{
    [RequireHttps]
    public class CartController : Controller
    {
        private ApplicationDbContext _db = new ApplicationDbContext();
        public string ShoppingCartId { get; set; }

        public const string CartSessionKey = "Id";
        // GET: Cart
        public ActionResult Index(string returnUrl)
        {
            IEnumerable<CartItem> cartItems = _db.ShoppingCartItems;
            ViewBag.ShoppingCartItems = cartItems;

            return View(new CartIndexViewModel
            {
                Id = GetCartId(),
                ReturnUrl = returnUrl
            });
        }

        public void AddToCart(int id, int quantity = 1)
        {
            ViewBag.Deficit = string.Empty;
            // Retrieve the product from the database.           
            ShoppingCartId = GetCartId();

            var cartItem = _db.ShoppingCartItems.SingleOrDefault(
                c => c.CartId == ShoppingCartId
                && c.ProductId == id);
            if (cartItem == null)
            {
                // Create a new cart item if no cart item exists.                 
                cartItem = new CartItem
                {
                    Id = Guid.NewGuid().ToString(),
                    ProductId = id,
                    CartId = ShoppingCartId,
                    Product = _db.Products.SingleOrDefault(
                   p => p.Id == id),
                    Quantity = quantity,
                    DateCreated = DateTime.Now
                };

                _db.ShoppingCartItems.Add(cartItem);
            }
            else
            {
                if (cartItem.Quantity + quantity <= cartItem.Product.QuantityInStock)
                    cartItem.Quantity += quantity;
                /*else
                    ViewBag.Deficit = string.Format("На складе на {0} меньше товара, чем вы хотите приобрести",
                        cartItem.Quantity + quantity - cartItem.Product.QuantityInStock);*/
            }
            _db.Products.First(p => p.Id == id).QuantityInStock -= quantity;
            _db.SaveChanges();
        }

        public EmptyResult EmptyResultAddToCart(int id, int quantity = 1)
        {
            if (quantity > _db.Products.First(p => p.Id == id).QuantityInStock)
            {
                ModelState.AddModelError("Quatity", 
                    string.Format("Количество добавляемого товара превышает его количество на складе на {0} ед.",
                    quantity - _db.Products.First(p => p.Id == id).QuantityInStock));
                return new EmptyResult();
            }
            if (quantity < 0)
            {
                ModelState.AddModelError("Quatity", "Количество добавляемого товара должно быть положительным");
                return new EmptyResult();
            }
            AddToCart(id, quantity);
            return new EmptyResult();
        }

        public string GetCartId()
        {
            string s = (string)Session[CartSessionKey];
            bool flag = _db.Users.Where(u => u.Id == s).Any();
            if (s != User.Identity.Name)
            {
                if (!string.IsNullOrWhiteSpace(User.Identity.Name) && _db.Users.Count() != 0)
                {
                    Session[CartSessionKey] = _db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault().Id;
                }
                else if (s == null || flag)
                {
                    Guid tempCartId = Guid.NewGuid();
                    Session[CartSessionKey] = tempCartId.ToString();
                }
            }
            return Session[CartSessionKey].ToString();
        }

        public List<CartItem> GetCartItems()
        {
            ShoppingCartId = GetCartId();

            return _db.ShoppingCartItems.Where(
                c => c.CartId == ShoppingCartId).ToList();
        }

        public decimal GetTotal()
        {
            ShoppingCartId = GetCartId();
            // Multiply product price by quantity of that product to get        
            // the current price for each of those products in the cart.  
            // Sum all product price totals to get the cart total.   
            decimal? total = decimal.Zero;
            total = (decimal?)(from cartItems in _db.ShoppingCartItems
                               where cartItems.CartId == ShoppingCartId
                               select (int?)cartItems.Quantity *
                               cartItems.Product.Qost).Sum();
            return total ?? decimal.Zero;
        }

        public CartController GetCart(HttpContext context)
        {
            using (var cart = new CartController())
            {
                cart.ShoppingCartId = cart.GetCartId();
                return cart;
            }
        }

        public void RemoveItem(string removeCartID, int removeProductID, int quantity)
        {
            using (var _db = new ApplicationDbContext())
            {
                try
                {
                    var myItem = (from c in _db.ShoppingCartItems where c.CartId == removeCartID && c.Product.Id == removeProductID select c).FirstOrDefault();
                    if (myItem != null)
                    {
                        if (myItem.Quantity == quantity)
                            _db.ShoppingCartItems.Remove(myItem);
                        else
                        {
                            myItem.Quantity -= quantity;
                        }
                        _db.Products.First(p => p.Id == removeProductID).QuantityInStock += quantity;
                        // Remove Item.
                        _db.SaveChanges();
                    }
                }
                catch (Exception exp)
                {
                    throw new Exception("ERROR: Unable to Remove Cart Item - " + exp.Message.ToString(), exp);
                }
            }
        }

        public EmptyResult EmptyResultRemoveItem(string removeCartID, int removeProductID, int quantity = 1)
        {
            
            RemoveItem(removeCartID, removeProductID, quantity);
            return new EmptyResult();
        }
        public void EmptyCart()
        {
            ShoppingCartId = GetCartId();
            var cartItems = _db.ShoppingCartItems.Where(
                c => c.CartId == ShoppingCartId);
            foreach (var cartItem in cartItems)
            {
                _db.ShoppingCartItems.Remove(cartItem);
            }
            // Save changes.             
            _db.SaveChanges();
        }

        /*public PartialViewResult TotalQost(CartItem cartItem)
        {
            var total = GetTotal();
            ViewBag.TotalQost = total != decimal.Zero ? total.ToString("# руб") : "0 руб";
            return PartialView(cartItem);
        }*/

        public PartialViewResult Sum(CartItem cartItem)
        {
            ViewBag.Count = GetCount();
            var total = GetTotal();
            ViewBag.TotalQost = total != decimal.Zero ? total.ToString("# руб") : "0 руб";
            return PartialView(cartItem);
        }

        public PartialViewResult ViewCartItem(string cartId)
        {
            var cartItem = _db.ShoppingCartItems.FirstOrDefault(c => c.Id == cartId);
            return PartialView(cartItem);
        }
        public int GetCount()
        {
            ShoppingCartId = GetCartId();
            int? count = (from cartItems in _db.ShoppingCartItems
                          where cartItems.CartId == ShoppingCartId
                          select (int?)cartItems.Quantity).Sum();
            return count ?? 0;
        }

        public ActionResult ToCheckout(string id)
        {
            return RedirectToAction("Index", "Checkout", new {id = id });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

                if (_db != null)
                {
                    _db.Dispose();
                    _db = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}