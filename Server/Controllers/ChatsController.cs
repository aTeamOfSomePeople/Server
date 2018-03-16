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
    public class ChatsController : Controller
    {
        private ServerContext db = new ServerContext();

        // GET: Chats
        public async Task<ActionResult> Index()
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            jsonResult.Data = await db.Chats.Select(e => new { Id = e.Id, Creator = e.Creator, Name = e.Name, Type = e.Type }).ToListAsync();

            return jsonResult;
        }

        // GET: Chats/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var chats = db.Chats.Select(e => new { Id = e.Id, Creator = e.Creator, Name = e.Name, Type = e.Type }).Where(e => e.Id == id).First();
            if (chats == null)
            {
                return HttpNotFound();
            }
            jsonResult.Data = chats;

            return jsonResult;
        }

        public async Task<ActionResult> UserChats(int? UserId)
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;


            if (UserId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var chats = await db.Chats.Select(e => new { Id = e.Id, Creator = e.Creator, Name = e.Name, Type = e.Type }).Where(e =>  db.UsersInChats.Any(el => el.UserId == UserId && e.Id == el.ChatId)).ToListAsync();
            if (chats == null)
            {
                return HttpNotFound();
            }
            jsonResult.Data = chats;

            return jsonResult;
        }

        // POST: Chats/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Create( Chats chats)
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            if (ModelState.IsValid)
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
        public async Task<ActionResult> Edit( Chats chats)
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            if (ModelState.IsValid)
            {
                db.Entry(chats).State = EntityState.Modified;
                await db.SaveChangesAsync();
                jsonResult.Data = true;
                return jsonResult;
            }

            jsonResult.Data = false;
            return jsonResult;
        }

        // POST: Chats/Delete/5
        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            Chats chats = await db.Chats.FindAsync(id);
            if (chats != null)
            {
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
