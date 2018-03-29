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

        [HttpPost]
        public async Task<ActionResult> SendMessage(string accessToken, long? chatId, string text)
        {
            if (String.IsNullOrWhiteSpace(accessToken) || !chatId.HasValue || String.IsNullOrWhiteSpace(text))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Arguments is null or empty");
            }

            var tokens = await db.Tokens.FirstOrDefaultAsync(e => e.AccessToken == accessToken);
            if (tokens == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Token is invalid");
            }

            var chat = await db.Chats.FindAsync(chatId);
            if (chat == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Chat is not exists");
            }

            if (!await db.UsersInChats.AnyAsync(e => e.UserId == tokens.UserId && e.ChatId == chatId && e.CanWrite))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "You can't write to this chat");
            }

            if (ModelState.IsValid)
            {
                var message = new Messages()
                {
                    ChatId = chatId.Value,
                    UserId = tokens.UserId,
                    Date = DateTime.UtcNow,
                    Text = text,
                    IsReaded = false
                };
                db.Messages.Add(message);
                await db.SaveChangesAsync();

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
        }

        [HttpPost]
        public async Task<ActionResult> GetMessages(string accessToken, long? chatId, Enums.MessagesDate? messagesDate, DateTime? date, int? count)
        {
            if (String.IsNullOrWhiteSpace(accessToken) || !chatId.HasValue || !messagesDate.HasValue || !date.HasValue || !count.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Arguments is null or empty");
            }

            if (count <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Count must be greater than zero");
            }

            if (count > 50)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Count must be lower or equal to 50");
            }

            var tokens = await db.Tokens.FirstOrDefaultAsync(e => e.AccessToken == accessToken);
            if (tokens == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Token is invalid");
            }

            var chat = await db.Chats.FindAsync(chatId);
            if (chat == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Chat is not exists");
            }
            
            if (!await db.UsersInChats.AnyAsync(e => e.UserId == tokens.UserId && e.ChatId == chatId && e.CanWrite))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "You can't get messages from this chat");
            }

            if (ModelState.IsValid)
            {
                switch (messagesDate)
                {
                    case Enums.MessagesDate.After:
                        return Json(db.Messages.Where(e => e.ChatId == chatId && e.Date > date).Take(count.Value));
                    case Enums.MessagesDate.Before:
                        return Json(db.Messages.Where(e => e.ChatId == chatId && e.Date < date).Reverse().Take(count.Value));
                }
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
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
