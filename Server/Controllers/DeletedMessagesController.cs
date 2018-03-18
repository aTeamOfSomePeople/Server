﻿using System;
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
    public class DeletedMessagesController : Controller
    {
        private ServerContext db = new ServerContext();

        // GET: DeletedMessages
        private async Task<ActionResult> Index()
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            var deletedMessages = await db.DeletedMessages.Select(e => new { Id = e.Id, MessageId = e.MessageId, UserId = e.UserId}).ToListAsync();
            jsonResult.Data = deletedMessages;

            return jsonResult;
        }

        //// GET: DeletedMessages/Details/5
        //public async Task<ActionResult> Details(int? id)
        //{
        //    var jsonResult = new JsonResult();
        //    jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    var deletedMessages = await db.DeletedMessages.Select(e => new { Id = e.Id, MessageId = e.MessageId, UserId = e.UserId }).SingleOrDefaultAsync(e => e.Id == id);
        //    if (deletedMessages == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    jsonResult.Data = deletedMessages;

        //    return jsonResult;
        //}

        // POST: DeletedMessages/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(DeletedMessages deletedMessages)
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            if (ModelState.IsValid && db.UsersInChats.Any(e => e.UserId == deletedMessages.UserId && db.Messages.SingleOrDefault(z => z.Id == deletedMessages.MessageId).ChatId == e.ChatId))
            {
                db.DeletedMessages.Add(deletedMessages);
                await db.SaveChangesAsync();
                jsonResult.Data = deletedMessages;
                return jsonResult;
            }

            jsonResult.Data = false;
            return jsonResult;
        }

        // POST: DeletedMessages/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(DeletedMessages deletedMessages)
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            if (ModelState.IsValid)
            {
                db.Entry(deletedMessages).State = EntityState.Modified;
                await db.SaveChangesAsync();
                jsonResult.Data = deletedMessages;
                return jsonResult;
            }

            jsonResult.Data = false;
            return jsonResult;
        }

        // POST: DeletedMessages/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            DeletedMessages deletedMessages = await db.DeletedMessages.FindAsync(id);
            if (deletedMessages != null)
            {
                db.DeletedMessages.Remove(deletedMessages);
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
