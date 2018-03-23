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
    public class ChatsController : Controller
    {
        private ServerContext db = new ServerContext();

        // GET: Chats
        private async Task<ActionResult> Index()
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            jsonResult.Data = await db.Chats.Select(e => new { Id = e.Id, Creator = e.Creator, Name = e.Name, Type = e.Type, Avatar = e.Avatar }).ToListAsync();

            return jsonResult;
        }

        //// GET: Chats/Details/5
        //public async Task<ActionResult> Details(int? id)
        //{
        //    var jsonResult = new JsonResult();
        //    jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    var chats = await db.Chats.Select(e => new { Id = e.Id, Creator = e.Creator, Name = e.Name, Type = e.Type }).FirstOrDefaultAsync(e => e.Id == id);
        //    if (chats == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    jsonResult.Data = chats;

        //    return jsonResult;
        //}

        public async Task<ActionResult> FindPublics(string name, int? start, int? count)
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            jsonResult.Data = null;

            if (name == null)
            {
                return jsonResult;
            }

            var publics = await db.Chats.Where(e => e.Type.ToLower() == "public" && e.Name.Contains(name)).Select(e => new { Id = e.Id, Creator = e.Creator, Name = e.Name, Type = e.Type, Avatar = e.Avatar }).ToListAsync();

            if (publics != null)
            {
                if (!start.HasValue || start.Value < 0)
                {
                    start = 0;
                }
                if (start.Value >= publics.Count)
                {
                    return jsonResult;
                }
                if (!count.HasValue || count.Value <= 0)
                {
                    count = publics.Count;
                }
                count = Math.Min(Math.Min(count.Value, publics.Count - start.Value), 100);
                jsonResult.Data = publics.GetRange(start.Value, count.Value);
            }

            return jsonResult;
        }

        public async Task<ActionResult> UserChats(int? UserId, int? Start, int? Count)
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            jsonResult.Data = null;

            if (!UserId.HasValue)
            {
                return jsonResult;
            }
            var chats = await db.Chats.Select(e => new { Id = e.Id, Creator = e.Creator, Name = e.Name, Type = e.Type, Avatar = e.Avatar }).Where(e =>  db.UsersInChats.Any(el => el.UserId == UserId && e.Id == el.ChatId)).ToListAsync();
            if (chats != null)
            {
                chats.Sort((first, second) =>
                {
                    var a = db.Messages.Where(e => e.ChatId == first.Id).ToList().LastOrDefault();
                    var b = db.Messages.Where(e => e.ChatId == second.Id).ToList().LastOrDefault();
                    if (a == b)
                    {
                        return 0;
                    }
                    if (a == null)
                    {
                        return 1;
                    }
                    else if (b == null)
                    {
                        return -1;
                    }
                    if (a.Date > b.Date)
                    {
                        return -1;
                    }

                    return 1;
                });
                //chats.Sort((a, b) => db.Messages.Any(e => e.ChatId == a.Id) != db.Messages.Any(e => e.ChatId == b.Id) && db.Messages.Any(e => e.ChatId == a.Id) ? -1 : db.Messages.Any(e => e.ChatId == a.Id) != db.Messages.Any(e => e.ChatId == b.Id) && db.Messages.Any(e => e.ChatId == b.Id) ? 1 : db.Messages.Any(e => e.ChatId == a.Id) && db.Messages.Where(e => e.ChatId == a.Id).Last().Date > db.Messages.Where(e => e.ChatId == b.Id).Last().Date ? 1 : db.Messages.Any(e => e.ChatId == a.Id) && db.Messages.Where(e => e.ChatId == a.Id).Last().Date < db.Messages.Where(e => e.ChatId == b.Id).Last().Date ? -1 : 0);
                if (!Start.HasValue || Start.Value < 0)
                {
                    Start = 0;
                }
                if (Start.Value >= chats.Count)
                {
                    return jsonResult;
                }
                if (!Count.HasValue || Count.Value <= 0)
                {
                    Count = chats.Count;
                }
                Count = Math.Min(Math.Min(Count.Value, chats.Count - Start.Value), 100);
                jsonResult.Data = chats.GetRange(Start.Value, Count.Value);
            }
            return jsonResult;
        }

        // POST: Chats/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Chats chats)
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            if (ModelState.IsValid && !db.Users.Any(e => e.IsDeleted && e.Id == chats.Creator))
            {
                db.Chats.Add(chats);
                await db.SaveChangesAsync();
                jsonResult.Data = chats;
                return jsonResult;
            }

            jsonResult.Data = null;
            return jsonResult;
        }

        // POST: Chats/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Chats chats)
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            if (ModelState.IsValid)
            {
                db.Entry(chats).State = EntityState.Modified;
                await db.SaveChangesAsync();
                jsonResult.Data = chats;
                return jsonResult;
            }

            jsonResult.Data = false;
            return jsonResult;
        }

        // POST: Chats/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int? chatId, string login, string password)
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            jsonResult.Data = false;

            var chats = await db.Chats.FirstOrDefaultAsync(e => e.Id == chatId && db.Users.FirstOrDefault(z => z.Login == login && z.Password == password).Id == e.Creator);
            if (chats != null)
            {
                db.UsersInChats.RemoveRange(await db.UsersInChats.Where(e => e.ChatId == chats.Id).ToListAsync());
                db.Chats.Remove(chats);
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
