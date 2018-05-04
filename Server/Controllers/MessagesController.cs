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

        [HttpPost, RequireHttps]
        public async Task<ActionResult> SendMessage(string accessToken, long? chatId, string text = null, string fileIds = null)
        {
            if (String.IsNullOrWhiteSpace(accessToken) || !chatId.HasValue || (String.IsNullOrWhiteSpace(text) && String.IsNullOrWhiteSpace(fileIds)))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Arguments is null or empty");
            }

            var ids = new List<long>();
            if (!String.IsNullOrWhiteSpace(fileIds))
            {
                try
                {
                    foreach (var e in fileIds.Split(',', ' '))
                    {
                        ids.Add(long.Parse(e));
                    }
                }
                catch
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Incorrect ids");
                }
            }

            if (ModelState.IsValid)
            {
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

                if (ids != null || ids.Count != 0)
                {
                    foreach (var e in ids)
                    {
                        if (!await db.UploadedFiles.AnyAsync(z => z.Id == e))
                        {
                            return new HttpStatusCodeResult(HttpStatusCode.NotFound, "One or more files is not exists");
                        }
                    }

                    ids.ForEach(e => db.Attachments.Add(new Attachments() {MessageId = message.Id, FileId = e }));
                    await db.SaveChangesAsync();
                }

                var context = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<Hubs.ChatHub>();
                context.Clients.Group(chatId.ToString()).newMessage(message.Id);

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }

            
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
        }
        
        public async Task<ActionResult> GetMessages(string accessToken, long? chatId, Enums.MessagesDirection? direction, DateTime? date, int? count)
        {
            if (String.IsNullOrWhiteSpace(accessToken) || !chatId.HasValue || !direction.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Arguments is null or empty");
            }

            if (!date.HasValue)
            {
                date = (direction == Enums.MessagesDirection.After) ? DateTime.MinValue : DateTime.MaxValue;
            }

            if (!count.HasValue)
            {
                count = 50;
            }
            if (count <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Count must be greater than zero");
            }

            if (count > 50)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Count must be lower or equal to 50");
            }

            if (ModelState.IsValid)
            {
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

                switch (direction)
                {
                    case Enums.MessagesDirection.After:
                        return Json(await db.Messages.Where(e => e.ChatId == chatId && e.Date > date && db.DeletedMessages.FirstOrDefault(el => el.MessageId == e.Id && el.UserId == tokens.UserId) == null).Take(count.Value).Select(e => new { id = e.Id, text = e.Text, chatId = e.ChatId, userId = e.UserId, isReaded = e.IsReaded, date = e.Date, attachments = db.Attachments.Where(el => el.MessageId == e.Id).Select(el => db.UploadedFiles.FirstOrDefault(ele => ele.Id == el.UploadedFiles.Id).Link) }).ToArrayAsync(), JsonRequestBehavior.AllowGet);
                    case Enums.MessagesDirection.Before:
                        return Json(await db.Messages.Where(e => e.ChatId == chatId && e.Date < date && db.DeletedMessages.FirstOrDefault(el => el.MessageId == e.Id && el.UserId == tokens.UserId) == null).Reverse().Take(count.Value).Select(e => new { id = e.Id, text = e.Text, chatId = e.ChatId, userId = e.UserId, isReaded = e.IsReaded, date = e.Date, attachments = db.Attachments.Where(el => el.MessageId == e.Id).Select(el => db.UploadedFiles.FirstOrDefault(ele => ele.Id == el.UploadedFiles.Id).Link) }).ToArrayAsync(), JsonRequestBehavior.AllowGet);
                }
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
        }

        public async Task<ActionResult> GetMessage(string accessToken, long? messageId)
        {
            if (String.IsNullOrWhiteSpace(accessToken) || !messageId.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Arguments is null or empty");
            }

            if (ModelState.IsValid)
            {
                var tokens = await db.Tokens.FirstOrDefaultAsync(e => e.AccessToken == accessToken);
                if (tokens == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Token is invalid");
                }

                var message = await db.Messages.FindAsync(messageId);
                if (message == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Message is not exists");
                }

                var chat = await db.Chats.FindAsync(message.ChatId);
                if (chat == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Chat is not exists");
                }

                if (!await db.UsersInChats.AnyAsync(e => e.UserId == tokens.UserId && e.ChatId == chat.Id && e.CanWrite))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "You can't get messages from this chat");
                }

                return Json(new { message.Id, message.ChatId, message.UserId, message.Text, message.Date, message.IsReaded }, JsonRequestBehavior.AllowGet);
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
        }

        [HttpPost, RequireHttps]
        public async Task<ActionResult> EditMessage(string accessToken, long? messageId, string newText)
        {
            if (String.IsNullOrWhiteSpace(accessToken) || !messageId.HasValue || String.IsNullOrWhiteSpace(newText))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Arguments is null or empty");
            }

            if (ModelState.IsValid)
            {
                var tokens = await db.Tokens.FirstOrDefaultAsync(e => e.AccessToken == accessToken);
                if (tokens == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Token is invalid");
                }

                var message = await db.Messages.FindAsync(messageId);
                if (message == null)
                {
                    return HttpNotFound();
                }
                if (message.UserId != tokens.UserId)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "You can't edit this message");
                }
                if (message.Date <= DateTime.UtcNow.AddMinutes(-5))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Too late to edit this message");
                }
                message.Text = newText;
                db.Entry(message).State = EntityState.Modified;
                await db.SaveChangesAsync();

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
        }

        [HttpPost, RequireHttps]
        public async Task<ActionResult> DeleteMessage(string accessToken, long? messageId, bool fromAll = true)
        {
            if (String.IsNullOrWhiteSpace(accessToken) || !messageId.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Arguments is null or empty");
            }

            if (ModelState.IsValid)
            {
                var tokens = await db.Tokens.FirstOrDefaultAsync(e => e.AccessToken == accessToken);
                if (tokens == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Token is invalid");
                }

                var message = await db.Messages.FindAsync(messageId);
                if (message == null)
                {
                    return HttpNotFound();
                }
                if (message.UserId != tokens.UserId)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "You can't delete this message");
                }

                switch (fromAll)
                {
                    case true:
                        try
                        {
                            db.DeletedMessages.Remove(await db.DeletedMessages.FirstOrDefaultAsync(e => e.MessageId == message.Id && e.UserId == tokens.UserId));
                        }
                        catch { }
                        try
                        {
                            db.Attachments.RemoveRange(db.Attachments.Where(e => e.MessageId == message.Id));
                        }
                        catch { }
                        db.Messages.Remove(message);
                        break;
                    case false:
                        if (await db.DeletedMessages.AnyAsync(e => e.MessageId == message.Id && e.UserId == tokens.UserId))
                        {
                            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Message already deleted from you");
                        }

                        db.DeletedMessages.Add(new DeletedMessages()
                        {
                            MessageId = message.Id,
                            UserId = tokens.UserId,
                        });
                        break;
                }
                await db.SaveChangesAsync();

                return new HttpStatusCodeResult(HttpStatusCode.OK);
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
