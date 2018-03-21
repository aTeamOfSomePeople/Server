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
    public class UsersInChatsController : Controller
    {
        private ServerContext db = new ServerContext();

        // GET: UsersInChats
        private async Task<ActionResult> Index()
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            var usersInChats = await db.UsersInChats.Select(e => new { Id = e.Id, UserId = e.UserId, ChatId = e.ChatId, CanWrite = e.CanWrite, UnreadedMessages = e.UnreadedMessages}).ToListAsync();
            jsonResult.Data = usersInChats;

            return jsonResult;
        }

        //// GET: UsersInChats/Details/5
        //public async Task<ActionResult> Details(int? id)
        //{
        //    var jsonResult = new JsonResult();
        //    jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;


        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    var usersInChats = await db.UsersInChats.Select(e => new { Id = e.Id, UserId = e.UserId, ChatId = e.ChatId, CanWrite = e.CanWrite, UnreadedMessages = e.UnreadedMessages }).SingleOrDefaultAsync(e => e.Id == id);
        //    if (usersInChats == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    jsonResult.Data = usersInChats;

        //    return jsonResult;
        //}

        // POST: UsersInChats/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Create( UsersInChats usersInChats)
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            if (ModelState.IsValid)
            {
                db.UsersInChats.Add(usersInChats);
                await db.SaveChangesAsync();
                jsonResult.Data = usersInChats;
                return jsonResult;
            }

            jsonResult.Data = false;
            return jsonResult;
        }

        // POST: UsersInChats/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(UsersInChats usersInChats)
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            if (ModelState.IsValid)
            {
                db.Entry(usersInChats).State = EntityState.Modified;
                await db.SaveChangesAsync();
                jsonResult.Data = usersInChats;
                return jsonResult;
            }

            jsonResult.Data = false;
            return jsonResult;
        }

        // POST: UsersInChats/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string login, string password, int? chatId, int? userId)
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            jsonResult.Data = false;

            if (!chatId.HasValue || !userId.HasValue || login == null || password == null)
            {
                return jsonResult;
            }

            UsersInChats usersInChats = await db.UsersInChats.FirstOrDefaultAsync(e => e.ChatId == chatId && (db.Users.FirstOrDefault(z => z.Login == login && z.Password == password).Id == db.Chats.FirstOrDefault(z => z.Id == chatId).Creator) || (db.Users.FirstOrDefault(z => z.Login == login && z.Password == password).Id == userId));

            if (usersInChats != null)
            {
                db.UsersInChats.Remove(usersInChats);
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
