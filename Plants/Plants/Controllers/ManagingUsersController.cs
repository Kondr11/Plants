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
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;

namespace Plants.Controllers
{
    [Authorize(Roles = "superadmin,admin")]
    public class ManagingUsersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: ManagingUsers
        public async Task<ActionResult> Index()
        {
            return View(await db.Users.ToListAsync());
        }

        // GET: ManagingUsers/Roles
        public ActionResult Roles()
        {
            return View(db);
        }

        // GET: ManagingUsers/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser applicationUser = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (applicationUser == null)
            {
                return HttpNotFound();
            }
            return View(applicationUser);
        }

        // GET: ManagingUsers/RolesDetails
        public async Task<ActionResult> RolesDetails(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            switch (id)
            {
                case "6c0bd9a8-1280-4a53-8530-f877871e5571":
                    ViewBag.Description = "суперадмин"; //добавить описание для роли суперадмина
                    break;
                case "7d1874b6-5237-48f1-a5f6-34a42b088fbe":
                    ViewBag.Description = "админ";//добавить описание для роли админа
                    break;
                case "3f117bef-d708-43ba-a730-db75b5c8bbae":
                    ViewBag.Description = "юзверь";//добавить описание для роли пользователя
                    break;
                case "049b36c7-a919-440b-b9f1-50436def362f":
                    ViewBag.Description = "оператор";//добавить описание для роли оператора
                    break;
            }
            IdentityRole role = await db.Roles.FirstOrDefaultAsync(r => r.Id == id);
            if (role == null)
            {
                return HttpNotFound();
            }
            return View(role);
        }

        // GET: ManagingUsers/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ManagingUsers/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в разделе https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Email,PasswordHash,UserName")] ApplicationUser applicationUser)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = applicationUser.UserName, Email = applicationUser.Email };
                var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(db));
                var result = await userManager.CreateAsync(user, applicationUser.PasswordHash);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user.Id, "user");
                    return RedirectToAction(string.Format("Edit/{0}", user.Id));

                }
                AddErrors(result);
                return View(applicationUser);
            }

            return View(applicationUser);
        }

        // GET: ManagingUsers/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser applicationUser = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (applicationUser == null)
            {
                return HttpNotFound();
            }
            return View(applicationUser);
        }

        // POST: ManagingUsers/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в разделе https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Email,EmailConfirmed,PasswordHash,SecurityStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEndDateUtc,LockoutEnabled,AccessFailedCount,UserName")] ApplicationUser applicationUser)
        {
            if (ModelState.IsValid)
            {
                db.Entry(applicationUser).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(applicationUser);
        }

        // GET: ManagingUsers/RolesEdit/5
        public async Task<ActionResult> RolesEdit(string userId, string roleId)
        {
            if (roleId == null || userId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser applicationUser = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            IdentityRole role = await db.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
            var userRole = await db.UserRoles.FirstOrDefaultAsync(u => u.UserId == userId); ;
            if (role == null || applicationUser == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserName = applicationUser.UserName;
            ViewBag.Email = applicationUser.Email;
            ViewBag.PhoneNumber = applicationUser.PhoneNumber;
            ViewBag.UserId = applicationUser.Id;
            return View(userRole);
        }

        // POST: ManagingUsers/RolesEdit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в разделе https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RolesEdit([Bind(Include = "UserId,RoleId")] Microsoft.AspNet.Identity.EntityFramework.IdentityUserRole userRole)
        {
            if (ModelState.IsValid)
            {
                var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(db));
                var oldRoleId = db.UserRoles.FirstOrDefaultAsync(u => u.UserId == userRole.UserId).Result.RoleId;
                var oldRole = db.Roles.FirstOrDefaultAsync(r => r.Id == oldRoleId).Result.Name;
                var newRole = db.Roles.FirstOrDefaultAsync(r => r.Id == userRole.RoleId).Result.Name;
                await userManager.RemoveFromRoleAsync(userRole.UserId, oldRole);
                await userManager.AddToRoleAsync(userRole.UserId, newRole);

                return RedirectToAction("Roles");
            }
            return View(userRole);
        }
        // GET: ManagingUsers/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser applicationUser = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (applicationUser == null)
            {
                return HttpNotFound();
            }
            return View(applicationUser);
        }

        // POST: ManagingUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            ApplicationUser applicationUser = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
            db.Users.Remove(applicationUser);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
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
