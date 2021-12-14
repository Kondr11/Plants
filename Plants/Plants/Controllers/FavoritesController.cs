using Plants.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Plants.Controllers
{
    [RequireHttps]
    public class FavoritesController : Controller
    {
        private ApplicationDbContext _db = new ApplicationDbContext();
        public string FavoritesId { get; set; }

        public const string FavoritesSessionKey = "Id";
        // GET: Favorites
        public ActionResult Index(string returnUrl)
        {
            IEnumerable<FavoritesItem> favorites = _db.FavoritesItems;
            ViewBag.FavoritesItems = favorites;

            return View(new FavoritesIndexViewModel
            {
                Id = GetFavoritesId(),
                ReturnUrl = returnUrl
            });
        }

        public void AddToFavorites(int id)
        {
            // Retrieve the product from the database.           
            FavoritesId = GetFavoritesId();

            var favoritesItem = _db.FavoritesItems.SingleOrDefault(
                f => f.FavoritesId == FavoritesId
                && f.ProductId == id);
            if (favoritesItem == null)
            {
                // Create a new favorites item if no favorites item exists.                 
                favoritesItem = new FavoritesItem
                {
                    Id = Guid.NewGuid().ToString(),
                    ProductId = id,
                    FavoritesId = FavoritesId,
                    Product = _db.Products.SingleOrDefault(
                   p => p.Id == id),
                    Quantity = 1,
                    DateCreated = DateTime.Now
                };

                _db.FavoritesItems.Add(favoritesItem);
            }
            _db.SaveChanges();
        }

        public EmptyResult EmptyResultAddToFavorites(int id)
        {
            AddToFavorites(id);
            return new EmptyResult();
        }

        public string GetFavoritesId()
        {
            if (Session[FavoritesSessionKey] == null)
            {
                if (!string.IsNullOrWhiteSpace(User.Identity.Name))
                {
                    Session[FavoritesSessionKey] = User.Identity.Name;
                }
                else
                {
                    // Generate a new random GUID using System.Guid class.     
                    Guid tempFavoritesId = Guid.NewGuid();
                    Session[FavoritesSessionKey] = tempFavoritesId.ToString();
                }
            }
            return Session[FavoritesSessionKey].ToString();
        }

        public List<FavoritesItem> GetFavotitesItems()
        {
            FavoritesId = GetFavoritesId();

            return _db.FavoritesItems.Where(
                f => f.FavoritesId == FavoritesId).ToList();
        }

        public FavoritesController GetFavorites(HttpContext context)
        {
            using (var favorites = new FavoritesController())
            {
                favorites.FavoritesId = favorites.GetFavoritesId();
                return favorites;
            }
        }

        public void RemoveItem(string removeFavoritesID, int removeProductID)
        {
            using (var _db = new Plants.Models.ApplicationDbContext())
            {
                try
                {
                    var myItem = (from f in _db.FavoritesItems where f.FavoritesId== removeFavoritesID && f.Product.Id == removeProductID select f).FirstOrDefault();
                    if (myItem != null)
                    {
                        // Remove Item.
                        _db.FavoritesItems.Remove(myItem);
                        _db.SaveChanges();
                    }
                }
                catch (Exception exp)
                {
                    throw new Exception("ERROR: Unable to Remove Favorites Item - " + exp.Message.ToString(), exp);
                }
            }
        }

        public EmptyResult EmptyResultRemoveItem(string removeFavoritesID, int removeProductID)
        {
            RemoveItem(removeFavoritesID, removeProductID);
            return new EmptyResult();
        }

        public PartialViewResult ViewFavoritesItem(string favoritesId)
        {
            var favoritesItem = _db.FavoritesItems.FirstOrDefault(c => c.Id == favoritesId);
            return PartialView(favoritesItem);
        }

        public void EmptyFavorites()
        {
            FavoritesId = GetFavoritesId();
            var favoritesItems = _db.FavoritesItems.Where(
                f => f.FavoritesId == FavoritesId);
            foreach (var favoritesItem in favoritesItems)
            {
                _db.FavoritesItems.Remove(favoritesItem);
            }
            // Save changes.             
            _db.SaveChanges();
        }

        public int GetCount()
        {
            FavoritesId = GetFavoritesId();

            // Get the count of each item in the favorites and sum them up          
            int? count = (from favoritesItems in _db.FavoritesItems
                          where favoritesItems.FavoritesId == FavoritesId
                          select (int?)favoritesItems.Quantity).Sum();
            // Return 0 if all entries are null         
            return count ?? 0;
        }
    }
}