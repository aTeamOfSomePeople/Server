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
    public class MutedUsersController : Controller
    {
        private ServerContext db = new ServerContext();

        // GET: MutedUsers
        private async Task<ActionResult> Index()
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            var mutedUsers = await db.MutedUsers.Select(e => new { Id = e.Id, UserId = e.UserId, ChatId = e.ChatId, End = e.End}).ToListAsync();
            jsonResult.Data = mutedUsers;

            return jsonResult;
        }

        //// GET: MutedUsers/Details/5
        //public async Task<ActionResult> Details(int? id)
        //{
        //    var jsonResult = new JsonResult();
        //    jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    var mutedUsers = await db.MutedUsers.Select(e => new { Id = e.Id, UserId = e.UserId, ChatId = e.ChatId, End = e.End}).SingleOrDefaultAsync(e => e.Id == id);
        //    if (mutedUsers == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    jsonResult.Data = mutedUsers;

        //    return jsonResult;
        //}

        // POST: MutedUsers/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(MutedUsers mutedUsers)
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            if (ModelState.IsValid)
            {
                db.MutedUsers.Add(mutedUsers);
                await db.SaveChangesAsync();
                jsonResult.Data = mutedUsers;
                return jsonResult;
            }

            jsonResult.Data = false;
            return jsonResult;
        }

        // POST: MutedUsers/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(MutedUsers mutedUsers)
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            if (ModelState.IsValid)
            {
                db.Entry(mutedUsers).State = EntityState.Modified;
                await db.SaveChangesAsync();
                jsonResult.Data = mutedUsers;
                return jsonResult;
            }

            jsonResult.Data = false;
            return jsonResult;
        }

        // POST: MutedUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int? userId, int? chatId)
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            jsonResult.Data = false;

            if (!userId.HasValue || !chatId.HasValue)
            {
                return jsonResult;
            }

            MutedUsers mutedUsers = await db.MutedUsers.Where(e => e.UserId == userId && e.ChatId == chatId).FirstOrDefaultAsync();
            if (mutedUsers != null)
            {
                db.MutedUsers.Remove(mutedUsers);
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
