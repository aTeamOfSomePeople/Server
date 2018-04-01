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
                if (db.BannedByUser.Any(e => e.BannerId == secondUserId && e.BannedId == tokens.UserId))
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
                db.UsersInChats.Add(new UsersInChats() { ChatId = chat.Id, UserId = tokens.UserId, CanWrite = true, UnreadedMessages = 0 });
                db.UsersInChats.Add(new UsersInChats() { ChatId = chat.Id, UserId = secondUserId.Value, CanWrite = true, UnreadedMessages = 0 });
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
                if (!userIdsTable.All(e => !db.BannedByUser.Any(z => z.BannerId == e && z.BannedId == tokens.UserId)))
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
                    if (!userIdsTable.All(e => !db.BannedByUser.Any(z => z.BannerId == e && z.BannedId == tokens.UserId)))
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

            if (!Utils.ValidateString.UserName(newName))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Name is incorrect");
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
            var avatarExtention = Utils.ValidateFile.GetImageExtention(avatar.InputStream);
            if (!Utils.FilesExstensions.PosibleImageExtensions.Contains(avatarExtention))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid file type");
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
                var avatarFile = await cdnClient.Add(file, $"{DateTime.UtcNow.Ticks}.{avatarExtention}");
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

                if (await db.BannedByChat.AnyAsync(e => e.BannerId == chat.Id && e.BannedId == tokens.UserId))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "You are banned in this public");
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

        public async Task<ActionResult> GetUsers(string accessToken, long? chatId, int? count, int start = 0)
        {
            if (String.IsNullOrWhiteSpace(accessToken) || !chatId.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Arguments is null or empty");
            }
            if (start < 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Start must be greater than zero");
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

                if (!await db.UsersInChats.AnyAsync(e => e.UserId == tokens.UserId && e.ChatId == chatId))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "You can't get users from this chat");
                }

                return Json(db.UsersInChats.Where(e => e.ChatId == chatId).Skip(start).Take(count.Value).Select(e => e.UserId), JsonRequestBehavior.AllowGet);
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
        }

        [HttpPost]
        public async Task<ActionResult> RemoveUserFromGroup(string accessToken, long? chatId, long? userId)
        {
            if (String.IsNullOrWhiteSpace(accessToken) || !chatId.HasValue || !userId.HasValue)
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

                var chat = await db.Chats.FindAsync(chatId);
                if (chat == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Chat is not exists");
                }
                
                if (chat.Type != Enums.ChatType.Group)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "This chat is not a group");
                }

                if (!await db.UsersInChats.AnyAsync(e => e.UserId == tokens.UserId && e.ChatId == chatId))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "You don't have access to this public");
                }

                var userInChat = await db.UsersInChats.FirstOrDefaultAsync(e => e.UserId == userId && e.ChatId == chatId);
                if (userInChat == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound, "User don't in this chat");
                }

                db.UsersInChats.Remove(userInChat);
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

        [HttpPost]
        public async Task<ActionResult> BanUser(string accessToken, long? chatId, long? userId)
        {
            if (String.IsNullOrWhiteSpace(accessToken) || !chatId.HasValue || !userId.HasValue)
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

                var chat = await db.Chats.FindAsync(chatId);
                if (chat == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Chat is not exists");
                }

                if (chat.Type == Enums.ChatType.Dialog)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "You can't ban user in dialog");
                }

                if (!await db.UsersInChats.AnyAsync(e => e.UserId == tokens.UserId && e.ChatId == chatId))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "You don't have access to this chat");
                }

                if (await db.BannedByChat.AnyAsync(e => e.BannedId == userId && e.BannerId == chatId))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound, "User already banned in this chat");
                }

                var bannedByChat = new BannedByChat()
                {
                    BannedId = userId.Value,
                    BannerId = chatId.Value
                };

                db.BannedByChat.Add(bannedByChat);
                await db.SaveChangesAsync();

                return new HttpStatusCodeResult(HttpStatusCode.OK);

            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
        }

        [HttpPost]
        public async Task<ActionResult> UnBanUser(string accessToken, long? chatId, long? userId)
        {
            if (String.IsNullOrWhiteSpace(accessToken) || !chatId.HasValue || !userId.HasValue)
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

                var chat = await db.Chats.FindAsync(chatId);
                if (chat == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound, "Chat is not exists");
                }

                if (chat.Type == Enums.ChatType.Dialog)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "You can't unban user in dialog");
                }

                if (!await db.UsersInChats.AnyAsync(e => e.UserId == tokens.UserId && e.ChatId == chatId))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "You don't have access to this chat");
                }

                var bannedByChat = await db.BannedByChat.FirstOrDefaultAsync(e => e.BannedId == userId && e.BannerId == chatId);
                if (bannedByChat == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound, "User is not banned in this chat");
                }

                db.BannedByChat.Remove(bannedByChat);
                await db.SaveChangesAsync();

                return new HttpStatusCodeResult(HttpStatusCode.OK);

            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
        }

        public async Task<ActionResult> GetBannedUsers(string accessToken, long? chatId, int? count, int start = 0)
        {
            if (String.IsNullOrWhiteSpace(accessToken) || !chatId.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Arguments is null or empty");
            }

            if (start < 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Start must be greater than zero");
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

                if (!await db.UsersInChats.AnyAsync(e => e.UserId == tokens.UserId && e.ChatId == chatId))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "You can't get banned users from this chat");
                }

                return Json(await db.BannedByChat.Where(e => e.BannerId == chatId).Skip(start).Take(count.Value).Select(e => e.BannedId).ToArrayAsync(), JsonRequestBehavior.AllowGet);
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
        }

        [HttpPost]
        public async Task<ActionResult> InviteToGroup(string accessToken, long? chatId, long? userId)
        {
            if (String.IsNullOrWhiteSpace(accessToken) || !chatId.HasValue || !userId.HasValue)
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

                var chat = await db.Chats.FindAsync(chatId);
                if (chat == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Chat is not exists");
                }

                if (!await db.UsersInChats.AnyAsync(e => e.UserId == tokens.UserId && e.ChatId == chatId && e.CanWrite))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "You can't invite to this chat");
                }

                if (await db.BannedByUser.AnyAsync(e => e.BannerId == userId && e.BannedId == tokens.UserId))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "You are banned by this user");
                }

                if (await db.BannedByChat.AnyAsync(e => e.BannerId == chat.Id && e.BannedId == userId))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "User is banned in this group");
                }

                if (await db.UsersInChats.AnyAsync(e => e.ChatId == chatId && e.UserId == userId))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "User already in the group");
                }

                var userInChat = new UsersInChats()
                {
                    ChatId = chat.Id,
                    UserId = userId.Value,
                    UnreadedMessages = 0,
                    CanWrite = false
                };
                db.UsersInChats.Add(userInChat);
                await db.SaveChangesAsync();

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
        }

        public async Task<ActionResult> FindPublicByName(string accessToken, string name, int? count, int start = 0)
        {
            if (String.IsNullOrWhiteSpace(accessToken) || String.IsNullOrWhiteSpace(name))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Arguments is null or empty");
            }

            if (start < 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Start must be greater than zero");
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

                return Json(await db.Chats.Where(e => e.Type == Enums.ChatType.Public && e.Name.StartsWith(name)).Skip(start).Take(count.Value).Select(e => e.Id).ToArrayAsync(), JsonRequestBehavior.AllowGet);
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
