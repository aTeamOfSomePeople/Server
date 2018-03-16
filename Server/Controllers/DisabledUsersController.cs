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
    public class DisabledUsersController : Controller
    {
        private ServerContext db = new ServerContext();

        // GET: DisabledUsers
        public async Task<ActionResult> Index()
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            var disabledUsers = await db.DisabledUsers.ToListAsync();
            disabledUsers.ForEach(e => { e.User1 = null; e.User2 = null; });
            jsonResult.Data = disabledUsers;

            return jsonResult;
        }

        // GET: DisabledUsers/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DisabledUsers disabledUsers = await db.DisabledUsers.FindAsync(id);
            if (disabledUsers == null)
            {
                return HttpNotFound();
            }
            disabledUsers.User1 = null;
            disabledUsers.User2 = null;
            jsonResult.Data = disabledUsers;

            return jsonResult;
        }

        // POST: DisabledUsers/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(DisabledUsers disabledUsers)
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            if (ModelState.IsValid)
            {
                db.DisabledUsers.Add(disabledUsers);
                await db.SaveChangesAsync();
                jsonResult.Data = disabledUsers;
                return jsonResult;
            }

            jsonResult.Data = false;
            return jsonResult;
        }

        // POST: DisabledUsers/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(DisabledUsers disabledUsers)
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            if (ModelState.IsValid)
            {
                db.Entry(disabledUsers).State = EntityState.Modified;
                await db.SaveChangesAsync();
                jsonResult.Data = true;
                return jsonResult;
            }

            jsonResult.Data = false;
            return jsonResult;
        }

        // POST: DisabledUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            DisabledUsers disabledUsers = await db.DisabledUsers.FindAsync(id);
            if (disabledUsers != null)
            {
                db.DisabledUsers.Remove(disabledUsers);
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
