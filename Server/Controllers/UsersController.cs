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
        private async Task<ActionResult> Index()
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            jsonResult.Data = await db.Users.Select(z => new { Id = z.Id, Name = z.Name, Avatar = z.Avatar, IsDeleted = z.IsDeleted }).ToListAsync();
            
            return jsonResult;
        }

        // GET: Users/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            jsonResult.Data = null;

            if (id == null)
            {
                return jsonResult;
            }
            var users = await db.Users.Select(z => new { Id = z.Id, Name = z.Name, Avatar = z.Avatar, IsDeleted = z.IsDeleted }).FirstOrDefaultAsync(e => e.Id == id);
            jsonResult.Data = users;

            return jsonResult;
        }

        //GET: Users/IsExists
        public async Task<ActionResult> IsExists(string login, string password)
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            var user = await db.Users.FirstOrDefaultAsync(element => !element.IsDeleted && (element.Login == login || element.Email == login) && element.Password == password);

            jsonResult.Data = user;

            return jsonResult;
        }

        //GET: Users/FindUsers/"test"
        public async Task<ActionResult> FindUsers(string Name, int? Start, int? Count)
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            jsonResult.Data = null;
            if (Name == null)
            {
                Name = "";
            }
            var users = await db.Users.Where(e => e.Name.Contains(Name)).ToListAsync();
            if (users != null)
            {
                if (!Start.HasValue || Start.Value < 0)
                {
                    Start = 0;
                }
                if (Start.Value >= users.Count)
                {
                    return jsonResult;
                }
                if (!Count.HasValue || Count.Value <= 0)
                {
                    Count = users.Count;
                }
                Count = Math.Min(Math.Min(Count.Value, users.Count - Start.Value), 100);
                jsonResult.Data = users.GetRange(Start.Value, Count.Value);
            }

            return jsonResult;
        }
        //GET: Users/ChatUsers/5
        public async Task<ActionResult> ChatUsers(int? ChatId, int? Start, int? Count)
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            jsonResult.Data = null;

            if (!ChatId.HasValue)
            {
                return jsonResult;
            }
            var users = await db.Users.Where(element => db.UsersInChats.Any(e => e.ChatId == ChatId && e.UserId == element.Id)).Select(z => new { Id = z.Id, Name = z.Name, Avatar = z.Avatar, IsDeleted = z.IsDeleted}).ToListAsync();
            if (users != null)
            {
                if (!Start.HasValue || Start.Value < 0)
                {
                    Start = 0;
                }
                if (Start.Value >= users.Count)
                {
                    return jsonResult;
                }
                if (!Count.HasValue || Count.Value <= 0)
                {
                    Count = users.Count;
                }
                Count = Math.Min(Math.Min(Count.Value, users.Count - Start.Value), 100);
                jsonResult.Data = users.GetRange(Start.Value, Count.Value);
            }
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
        public async Task<ActionResult> Edit(Users users, string oldPassword)
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            var pass = oldPassword == null ? users.Password : oldPassword;
            if (ModelState.IsValid && db.Users.Any(e => e.Login == users.Login && e.Password == pass))
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
