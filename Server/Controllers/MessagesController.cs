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
    public class MessagesController : Controller
    {
        private ServerContext db = new ServerContext();

        // GET: Messages
        private async Task<ActionResult> Index()
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            jsonResult.Data = await db.Messages.Select(e => new { Id = e.Id, Text = e.Text, Date = e.Date, ChatId = e.ChatId, UserId = e.UserId, IsReader = e.IsReaded}).ToListAsync();

            return jsonResult;
        }

        //// GET: Messages/Details/5
        //public async Task<ActionResult> Details(int? id)
        //{
        //    var jsonResult = new JsonResult();
        //    jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    var messages = await db.Messages.Select(e => new { Id = e.Id, Text = e.Text, Date = e.Date, ChatId = e.ChatId, UserId = e.UserId, IsReader = e.IsReaded }).FirstOrDefaultAsync(e => e.Id == id);
        //    if (messages == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    jsonResult.Data = messages;

        //    return jsonResult;
        //}

        public async Task<ActionResult> ChatMessages(int? ChatId, int? UserId, int? Start, int? Count)
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            jsonResult.Data = null;

            if (!ChatId.HasValue || !UserId.HasValue)
            {
                return jsonResult;
            }
            var messages = await db.Messages.Select(e => new { Id = e.Id, Text = e.Text, Date = e.Date, ChatId = e.ChatId, UserId = e.UserId, IsReaded = e.IsReaded }).Where(z => z.ChatId == ChatId && !db.DeletedMessages.Any(e => e.UserId == UserId && e.MessageId == z.Id)).ToListAsync();
            if (messages != null)
            {
                
                if (!Start.HasValue || Start.Value < 0)
                {
                    Start = 0;
                }
                if (Start.Value >= messages.Count)
                {
                    return jsonResult;
                }
                if (!Count.HasValue || Count.Value <= 0)
                {
                    Count = messages.Count;
                }
                Count = Math.Min(Math.Min(Count.Value, messages.Count - Start.Value), 100);
                Start = messages.Count - Count - Start;
                messages = messages.GetRange(Start.Value, Count.Value);
                messages.Reverse();
                jsonResult.Data = messages;
            }

            return jsonResult;
        }

        // POST: Messages/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Messages messages)
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            if (ModelState.IsValid && !db.Users.Any(e => e.IsDeleted && e.Id == messages.UserId) && db.UsersInChats.Any(e => e.ChatId == messages.ChatId && e.UserId == messages.UserId && e.CanWrite))
            {
                messages.IsReaded = false;
                messages.Date = DateTime.Now;
                db.Messages.Add(messages);
                await db.SaveChangesAsync();
                jsonResult.Data = messages;
                return jsonResult;
            }

            jsonResult.Data = false;
            return jsonResult;
        }

        // POST: Messages/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit( Messages messages)
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            if (ModelState.IsValid)
            {
                db.Entry(messages).State = EntityState.Modified;
                await db.SaveChangesAsync();
                jsonResult.Data = messages;
                return jsonResult;
            }

            jsonResult.Data = false;
            return jsonResult;
        }


        // POST: Messages/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, int chatId, string userLogin, string userPassword)
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            Messages messages = await db.Messages.FirstOrDefaultAsync(e => e.Id == id && db.UsersInChats.Any(z => z.ChatId == chatId));
            if (messages != null)
            {
                db.Messages.Remove(messages);
                await db.SaveChangesAsync();
                var cdnClient = (new ZeroCdnClients.CdnClientsFactory(Resource.ZeroCDNUsername, Resource.ZeroCDNKey)).Files;
                var attachments = await db.Attachments.Where(e => e.MessageId == id).ToListAsync();
                try
                {
                    foreach (var attachment in attachments)
                    {
                        await cdnClient.Remove(attachment.CDNId);
                    }
                }
                catch { }
                db.Attachments.RemoveRange(attachments);
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
