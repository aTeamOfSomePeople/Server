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
    public class BannedUsersController : Controller
    {
        private ServerContext db = new ServerContext();

        // GET: BannedUsers
        private async Task<ActionResult> Index()
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            jsonResult.Data = null;

            jsonResult.Data = await db.BannedUsers.Select(e => new { Id = e.Id, BannerId = e.BannerId, BannedId = e.BannedId }).ToListAsync();
            return jsonResult;
        }

        //// GET: BannedUsers/Details/5
        //public async Task<ActionResult> Details(int? id)
        //{
        //    var jsonResult = new JsonResult();
        //    jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
        //    jsonResult.Data = null;

        //    if (id == null)
        //    {
        //        return jsonResult;
        //    }
        //    var bannedUsers = await db.BannedUsers.Select(e => new { Id = e.Id, BannerId = e.BannerId, BannedId = e.BannedId }).FirstOrDefaultAsync(e => e.Id == id);
        //    if (bannedUsers == null)
        //    {
        //        return jsonResult;
        //    }
        //    jsonResult.Data = bannedUsers;

        //    return jsonResult;
        //}

        // POST: BannedUsers/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        ////[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Create(BannedUsers bannedUsers, string login, string password)
        //{
        //    var jsonResult = new JsonResult();
        //    jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
        //    jsonResult.Data = null;

        //    if (ModelState.IsValid && db.Users.Any(e => e.Login == login && e.Password == password && e.Id == bannedUsers.BannerId))
        //    {
        //        db.BannedUsers.Add(bannedUsers);
        //        await db.SaveChangesAsync();
        //        jsonResult.Data = bannedUsers;
        //        return jsonResult;
        //    }
            
        //    return jsonResult;
        //}

        // POST: BannedUsers/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(BannedUsers bannedUsers)
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            jsonResult.Data = false;

            if (ModelState.IsValid)
            {
                db.Entry(bannedUsers).State = EntityState.Modified;
                await db.SaveChangesAsync();
                jsonResult.Data = true;
                return jsonResult;
            }
            return jsonResult;
        }

        // POST: BannedUsers/Delete/5
        //[HttpPost, ActionName("Delete")]
        ////[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Delete(int? userId, string login, string password)
        //{
        //    var jsonResult = new JsonResult();
        //    jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
        //    jsonResult.Data = false;

        //    if (!userId.HasValue || login == null || password == null)
        //    {
        //        return jsonResult;
        //    }
        //    var bannerId = (await db.Users.Where(e => e.Login == login && e.Password == password).FirstOrDefaultAsync()).Id;
        //    var bannedUsers = await db.BannedUsers.Where(e => e.BannedId == userId && e.BannerId == bannerId).FirstOrDefaultAsync();
        //    if (bannedUsers != null)
        //    {
        //        db.BannedUsers.Remove(bannedUsers);
        //        await db.SaveChangesAsync();
        //        jsonResult.Data = true;
        //        return jsonResult;
        //    }

        //    return jsonResult;
        //}

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
