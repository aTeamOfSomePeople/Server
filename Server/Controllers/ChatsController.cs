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
        
        [HttpPost]
        public async Task<ActionResult> CreateDialog(string accessToken, long? secondUserId)
        {
            if (String.IsNullOrWhiteSpace(accessToken) || !secondUserId.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Arguments is null or empty");
            }
            if (ModelState.IsValid)
            {
                var tokens = await TokensController.ValidToken(accessToken, db);
                if (tokens == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid access token");
                }
                if (tokens.UserId == secondUserId.Value)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "It's impossible to create a dialogue with yourself");
                }
                if (!db.Users.Any(e => e.Id == secondUserId))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Second user is not exists");
                }
                if (db.BannedUsers.Any(e => e.BannerId == secondUserId && e.BannedId == tokens.UserId))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "First user is banned by second user");
                }

                if (db.UsersInChats.Where(e => db.Chats.FirstOrDefault(z => z.Id == e.ChatId).Type == Enums.ChatType.Dialog).Where(e => e.UserId == tokens.UserId || e.UserId == secondUserId.Value).GroupBy(e => e.ChatId).Any(e => e.Count() == 2))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Dialog already exists");
                }

                var chat = new Chats()
                {
                    Name = $"{(await db.Users.FirstOrDefaultAsync(e => e.AccountId == tokens.UserId)).Name} | {(await db.Users.FirstOrDefaultAsync(e => e.AccountId == secondUserId)).Name}",
                    Type = Enums.ChatType.Dialog,
                    Creator = tokens.UserId
                };
                db.Chats.Add(chat);
                await db.SaveChangesAsync();
                db.UsersInChats.Add(new UsersInChats() { ChatId = chat.Id, UserId = tokens.UserId, CanWrite = true, UnreadedMessages = 0});
                db.UsersInChats.Add(new UsersInChats() { ChatId = chat.Id, UserId = secondUserId.Value, CanWrite = true, UnreadedMessages = 0});
                await db.SaveChangesAsync();

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
        }

        [HttpPost]
        public async Task<ActionResult> CreateGroup(string accessToken, string name, long[] userIds)
        {
            if (String.IsNullOrWhiteSpace(accessToken) || String.IsNullOrWhiteSpace(name) || userIds == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Arguments is null or empty");
            }
            if (ModelState.IsValid)
            {
                var tokens = await TokensController.ValidToken(accessToken, db);
                if (tokens == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid access token");
                }

                var userIdsTable = new HashSet<long>(userIds);
                userIdsTable.Add(tokens.UserId);
                if (userIdsTable.Count < 3)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Requires at least 3 users");
                }
                if (!userIdsTable.All(e => db.Users.Find(e) != null))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "One or more users is not exists");
                }
                if (!userIdsTable.All(e => !db.BannedUsers.Any(z => z.BannerId == e && z.BannedId == tokens.UserId)))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "One or more users have banned the creator");
                }

                var chat = new Chats()
                {
                    Name = name,
                    Type = Enums.ChatType.Group,
                    Creator = tokens.UserId
                };
                db.Chats.Add(chat);
                await db.SaveChangesAsync();

                foreach (var e in userIdsTable)
                {
                    db.UsersInChats.Add(new UsersInChats() { ChatId = chat.Id, UserId = e, CanWrite = true, UnreadedMessages = 0 });
                }
                await db.SaveChangesAsync();

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
        }

        [HttpPost]
        public async Task<ActionResult> CreatePublic(string accessToken, string name, long[] userIds)
        {
            if (String.IsNullOrWhiteSpace(accessToken) || String.IsNullOrWhiteSpace(name))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Arguments is null or empty");
            }
            if (ModelState.IsValid)
            {
                var tokens = await TokensController.ValidToken(accessToken, db);
                if (tokens == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid access token");
                }

                HashSet<long> userIdsTable = null;
                if (userIds != null)
                {
                    userIdsTable = new HashSet<long>(userIds);
                    userIdsTable.Add(tokens.UserId);

                    if (!userIdsTable.All(e => db.Users.Find(e) != null))
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "One or more users are not exists");
                    }
                    if (!userIdsTable.All(e => !db.BannedUsers.Any(z => z.BannerId == e && z.BannedId == tokens.UserId)))
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "One or more users have banned the creator");
                    }
                }

                var chat = new Chats()
                {
                    Name = name,
                    Type = Enums.ChatType.Public,
                    Creator = tokens.UserId
                };
                db.Chats.Add(chat);
                await db.SaveChangesAsync();

                db.UsersInChats.Add(new UsersInChats() { ChatId = chat.Id, UserId = tokens.UserId, CanWrite = false, UnreadedMessages = 0 });
                if (userIdsTable != null)
                {
                    foreach (var e in userIdsTable)
                    {
                        db.UsersInChats.Add(new UsersInChats() { ChatId = chat.Id, UserId = e, CanWrite = false, UnreadedMessages = 0 });
                    }
                }
                await db.SaveChangesAsync();

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
        }

        [HttpPost]
        public async Task<ActionResult> ChangeName(string accessToken, long? chatId, string newName)
        {
            if (String.IsNullOrWhiteSpace(accessToken) || String.IsNullOrWhiteSpace(newName) || !chatId.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Arguments is null or empty");
            }

            var tokens = await TokensController.ValidToken(accessToken, db);
            if (tokens == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid access token");
            }

            if (ModelState.IsValid)
            {
                var chat = await db.Chats.FindAsync(chatId);
                if (chat == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Chat is not exists");
                }

                switch (chat.Type)
                {
                    case Enums.ChatType.Dialog:
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "You can't change name of dialog");
                    case Enums.ChatType.Group:
                        if (!await db.UsersInChats.AnyAsync(e => e.UserId == tokens.UserId && e.ChatId == chatId))
                        {
                            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "You don't have access to this chat");
                        }
                        break;
                    case Enums.ChatType.Public:
                        var userInChat = await db.UsersInChats.FirstOrDefaultAsync(e => e.UserId == tokens.UserId && e.ChatId == chatId);
                        if (userInChat == null)
                        {
                            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "You don't have access to this public");
                        }
                        if (!userInChat.CanWrite)
                        {
                            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Only admins can change name");
                        }
                        break;
                }
                chat.Name = newName;
                db.Entry(chat).State = EntityState.Modified;
                await db.SaveChangesAsync();

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
        }

        [HttpPost]
        public async Task<ActionResult> ChangeAvatar(string accessToken, long? chatId, HttpPostedFileBase avatar)
        {
            if (String.IsNullOrWhiteSpace(accessToken) || !chatId.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Arguments is null or empty");
            }
            if (avatar == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "File is null");
            }
            if (!Utils.ValidateString.FileName(avatar.FileName))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, $"File name contains forbidden symbols");
            }
            if (!Utils.FilesExstensions.PosibleImageExtensions.Contains(avatar.FileName.Split('.').LastOrDefault()))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid file type");
            }
            var tokens = await TokensController.ValidToken(accessToken, db);
            if (tokens == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid access token");
            }

            if (ModelState.IsValid)
            {
                var chat = await db.Chats.FindAsync(chatId);
                if (chat == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Chat is not exists");
                }

                var userInChat = await db.UsersInChats.FirstOrDefaultAsync(e => e.UserId == tokens.UserId && e.ChatId == chatId);
                if (userInChat == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "You don't have access to this public");
                }
                if (!userInChat.CanWrite)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "You can't change avatar in this chat");
                }
                var cdnClient = (new ZeroCdnClients.CdnClientsFactory(Properties.Resources.ZeroCDNUsername, Properties.Resources.ZeroCDNKey)).Files;

                var file = new byte[avatar.ContentLength];
                await avatar.InputStream.ReadAsync(file, 0, avatar.ContentLength);
                var avatarFile = await cdnClient.Add(file, $"{DateTime.UtcNow.Ticks}.{avatar.FileName.Split('.').LastOrDefault()}");
                try
                {
                    if (chat.Avatar != null)
                    {
                        await cdnClient.Remove(long.Parse(chat.Avatar.Split('/')[3]));
                    }
                }
                catch { }
                chat.Avatar = $"http://zerocdn.com/{avatarFile.ID}/{avatarFile.Name}";

                db.Entry(chat).State = EntityState.Modified;
                await db.SaveChangesAsync();

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
        }

        //[HttpPost]
        //public async Task<ActionResult> DeletePublic(string accessToken, long? chatId)
        //{
        //    if (String.IsNullOrWhiteSpace(accessToken) || !chatId.HasValue)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Arguments is null or empty");
        //    }
        //    if (ModelState.IsValid)
        //    {
        //        var tokens = await TokensController.ValidToken(accessToken, db);
        //        if (tokens == null)
        //        {
        //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid access token");
        //        }

        //        var chat = await db.Chats.FindAsync(chatId);
        //        if (chat == null)
        //        {
        //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Public is not exists");
        //        }
        //        if (chat.Type != Enums.ChatType.Public)
        //        {
        //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "It's not a public");
        //        }

        //        var userInChat = await db.UsersInChats.FirstOrDefaultAsync(e => e.UserId == tokens.UserId && e.ChatId == chatId);
        //        if (userInChat == null)
        //        {
        //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "You don't have access to this public");
        //        }
        //        if (!userInChat.CanWrite)
        //        {
        //            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Only admins can delete public");
        //        }

        //        Chats chats = await db.Chats.FindAsync(chatId);
        //        db.Chats.Remove(chats);
        //        await db.SaveChangesAsync();

        //        return new HttpStatusCodeResult(HttpStatusCode.OK);
        //    }

        //    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
        //}

        [HttpPost]
        public async Task<ActionResult> JoinThePublic(string accessToken, long? chatId)
        {
            if (String.IsNullOrWhiteSpace(accessToken) || !chatId.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Arguments is null or empty");
            }
            if (ModelState.IsValid)
            {
                var tokens = await TokensController.ValidToken(accessToken, db);
                if (tokens == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid access token");
                }

                var chat = await db.Chats.FindAsync(chatId);
                if (chat == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Chat is not exists");
                }
                if (chat.Type != Enums.ChatType.Public)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "It's not public");
                }
                if (await db.UsersInChats.AnyAsync(e => e.ChatId == chatId && e.UserId == tokens.UserId))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "You already in the public");
                }
                var userInChat = new UsersInChats()
                {
                    ChatId = chat.Id,
                    UserId = tokens.Id,
                    UnreadedMessages = 0,
                    CanWrite = false
                };
                db.UsersInChats.Add(userInChat);
                await db.SaveChangesAsync();

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
        }

        [HttpPost]
        public async Task<ActionResult> Leave(string accessToken, long? chatId)
        {
            if (String.IsNullOrWhiteSpace(accessToken) || !chatId.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Arguments is null or empty");
            }
            if (ModelState.IsValid)
            {
                var tokens = await TokensController.ValidToken(accessToken, db);
                if (tokens == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid access token");
                }

                var chat = await db.Chats.FindAsync(chatId);
                if (chat == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Chat is not exists");
                }

                if (chat.Type == Enums.ChatType.Dialog)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "You can't leave from dialog");
                }
                var userInChat = await db.UsersInChats.FirstOrDefaultAsync(e => e.ChatId == chatId && e.UserId == tokens.UserId);
                if (userInChat == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "You already not in the chat");
                }
                db.UsersInChats.Remove(userInChat);
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
