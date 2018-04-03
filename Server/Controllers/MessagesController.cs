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
        public async Task<ActionResult> SendMessage(string accessToken, long? chatId, string text, HttpFileCollectionBase files)
        {
            if (String.IsNullOrWhiteSpace(accessToken) || !chatId.HasValue || !(!String.IsNullOrWhiteSpace(text) || files != null))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Arguments is null or empty");
            }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
            if (files != null)
            {
                foreach (HttpPostedFileBase file in files)
                {
                    var fileExtension = Utils.ValidateFile.GetImageExtention(file.InputStream);
                    if (!Utils.FilesExstensions.PosibleImageExtensions.Contains(fileExtension))
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Only images can be uploaded");
                    }

                    if (file.ContentLength > 8388608)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Too big file");
                    }
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

                if (files != null)
                {
                    var cdnClient = (new ZeroCdnClients.CdnClientsFactory(Properties.Resources.ZeroCDNUsername, Properties.Resources.ZeroCDNKey)).Files;
                    foreach (HttpPostedFileBase file in files)
                    {
                        var fileBytes = new byte[file.ContentLength];
                        await file.InputStream.ReadAsync(fileBytes, 0, file.ContentLength);
                        var uploadedFile = await cdnClient.Add(fileBytes, $"{DateTime.UtcNow.Ticks}.{Utils.ValidateFile.GetImageExtention(file.InputStream)}");
                        var attachment = new Attachments()
                        {
                            MessageId = message.Id,
                            //Link = $"http://zerocdn.com/{uploadedFile.ID}/{uploadedFile.Name}",
                            //FileSize = uploadedFile.Size
                        };
                        db.Attachments.Add(attachment);
                    }
                    await db.SaveChangesAsync();
                }

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
        }

        [HttpPost]
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
                        return Json(await db.Messages.Where(e => e.ChatId == chatId && e.Date > date).Take(count.Value).ToArrayAsync());
                    case Enums.MessagesDirection.Before:
                        return Json(await db.Messages.Where(e => e.ChatId == chatId && e.Date < date).Reverse().Take(count.Value).ToArrayAsync());
                }
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
        }

        [HttpPost]
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

        [HttpPost]
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
                            UserId = tokens.Id,
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
