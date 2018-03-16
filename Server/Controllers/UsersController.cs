using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Server.Models;

namespace Server.Controllers
{
    public class UsersController : Controller
    {
        private ServerContext db = new ServerContext();

        // GET: Users
        public async Task<ActionResult> Index()
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            jsonResult.Data = await db.Users.ToListAsync();
            
            return jsonResult;
        }

        // GET: Users/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Users users = await db.Users.FindAsync(id);
            if (users != null)
            {
                users.Password = null;
            }
            jsonResult.Data = users;

            return jsonResult;
        }

        //GET: Users/IsExists
        public async Task<ActionResult> IsExists(string login, string password)
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            var user = await db.Users.SingleOrDefaultAsync(element => element.Login == login && element.Password == password);

            if (user != null)
            {
                user.Password = null;
            }
            jsonResult.Data = user;

            return jsonResult;
        }

        //GET: Users/FindUsers/"test"
        public async Task<ActionResult> FindUsers(string Name)
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            if (Name == null)
            {
                return await Index();
            }

            var users = await db.Users.Where(e => e.Name.Contains(Name) || e.Login.Contains(Name)).ToListAsync();
            users.ForEach(e => e.Password = null);
            jsonResult.Data = users;

            return jsonResult;
        }
        //GET: Users/ChatUsers/5
        public async Task<ActionResult> ChatUsers(int? ChatId)
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            if (ChatId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var users = await db.Users.ToListAsync();//Join(db.UsersInChats, oKey => oKey.Id, iKey => iKey.UserId, (oKey, iKey) => new { oKey, iKey }).Where(element => element.iKey.Id == userId).Select(z => new { Id = z.oKey.Id, Name = z.oKey.Name, Email = z.oKey.Email, Login = z.oKey.Login, Password = z.oKey.Password, Avatar = z.oKey.Avatar }).ToListAsync();
            if (users == null)
            {
                return HttpNotFound();
            }

            jsonResult.Data = users;
            return jsonResult;
        }

        // POST: Users/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Create( Users users)
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            if (ModelState.IsValid)
            {
                db.Users.Add(users);
                await db.SaveChangesAsync();
                jsonResult.Data = true;
                return jsonResult;
            }

            jsonResult.Data = false;
            return jsonResult;
        }

        // POST: Users/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit( Users users)
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            if (ModelState.IsValid)
            {
                db.Entry(users).State = EntityState.Modified;
                await db.SaveChangesAsync();
                jsonResult.Data = true;
                return jsonResult;
            }

            jsonResult.Data = false;
            return jsonResult;
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            Users users = await db.Users.FindAsync(id);
            if (users != null)
            {
                db.Users.Remove(users);
                await db.SaveChangesAsync();
                jsonResult.Data = true;
                return jsonResult;
            }

            jsonResult.Data = false;
            return jsonResult;
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
