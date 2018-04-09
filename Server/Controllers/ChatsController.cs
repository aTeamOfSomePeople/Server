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

        [HttpPost, RequireHttps]
        public async Task<ActionResult> SetMessagesReaded(string accessToken, long? chatId)
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

                var userInChat = await db.UsersInChats.FirstOrDefaultAsync(e => e.UserId == tokens.UserId && e.ChatId == chatId);
                if (userInChat == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "You don't have access to this chat");
                }

                await db.Messages.Where(e => !e.IsReaded).ForEachAsync(e => e.IsReaded = true);
                userInChat.UnreadedMessages = 0;
                db.Entry(userInChat).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
        }
        
        public async Task<ActionResult> GetChatInfo(string accessToken, long? chatId, string fields)
        {
            if (String.IsNullOrWhiteSpace(accessToken) || !chatId.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Arguments is null or empty");
            }
            var separatedFields = new List<string>();
            if (!String.IsNullOrWhiteSpace(fields))
            {
                separatedFields.AddRange(fields.ToLower().Split(new char[]{ ',', ' '}, StringSplitOptions.RemoveEmptyEntries));
            }

            if (ModelState.IsValid)
            {
                var tokens = await TokensController.ValidToken(accessToken, db);
                if (tokens == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid access token");
                }

                if (!await db.UsersInChats.AnyAsync(e => e.UserId == tokens.UserId && e.ChatId == chatId))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "You don't have access to this chat");
                }

                var chat = await db.Chats.FirstOrDefaultAsync(e => e.Id == chatId);
                if (chat == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound);
                }
                var response = new Dictionary<string, string>();
                response.Add("id", chat.Id.ToString());
                if (separatedFields.Count == 0 || separatedFields.Contains("name"))
                {
                    if (chat.Type == Enums.ChatType.Dialog)
                    {
                        var names = chat.Name.Split('|');
                        response.Add("name", chat.Creator == tokens.UserId ? names[1] : names[0]);
                    }
                    else
                    {
                        response.Add("name", chat.Name.ToString());
                    }
                }
                if (separatedFields.Count == 0 || separatedFields.Contains("avatar"))
                {
                    if (chat.Type == Enums.ChatType.Dialog)
                    {
                        var userInChat = await db.UsersInChats.FirstOrDefaultAsync(e => e.ChatId == chatId && e.UserId != tokens.UserId);
                        var user = await db.Users.FirstOrDefaultAsync(e => e.Id == userInChat.UserId);
                        response.Add("avatar", (await db.UploadedFiles.FirstOrDefaultAsync(e => e.Id == user.Avatar)).Link);
                    }
                    else
                    {
                        response.Add("avatar", (await db.UploadedFiles.FirstOrDefaultAsync(e => e.Id == chat.Avatar)).Link);
                    }
                }
                if ((separatedFields.Count == 0 || separatedFields.Contains("creator")) && chat.Type != Enums.ChatType.Dialog)
                {
                    response.Add("creator", chat.Creator.ToString());
                }
                if (separatedFields.Count == 0 || separatedFields.Contains("type"))
                {
                    response.Add("type", chat.Type.ToString());
                }
                if (separatedFields.Count == 0 || separatedFields.Contains("unreadedmessagescount"))
                {
                    response.Add("unreadedMessagesCount", (await db.UsersInChats.FirstOrDefaultAsync(e => e.ChatId == chatId && e.UserId == tokens.UserId)).UnreadedMessages.ToString());
                }
                if ((separatedFields.Count == 0 || separatedFields.Contains("memberscount")) && chat.Type != Enums.ChatType.Dialog)
                {
                    response.Add("membersCount", chat.MembersCount.ToString());
                }
                return Json(response, JsonRequestBehavior.AllowGet);
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
        }

        [HttpPost, RequireHttps]
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
                    Name = $"{(await db.Users.FirstOrDefaultAsync(e => e.Id == tokens.UserId)).Name}|{(await db.Users.FirstOrDefaultAsync(e => e.Id == secondUserId)).Name}",
                    Type = Enums.ChatType.Dialog,
                    Creator = tokens.UserId
                };
                db.Chats.Add(chat);
                await db.SaveChangesAsync();
                db.UsersInChats.Add(new UsersInChats() { ChatId = chat.Id, UserId = tokens.UserId, CanWrite = true, UnreadedMessages = 0 });
                db.UsersInChats.Add(new UsersInChats() { ChatId = chat.Id, UserId = secondUserId.Value, CanWrite = true, UnreadedMessages = 0 });
                await db.SaveChangesAsync();

                var toAdd = Hubs.ChatHub.users.Where(e => e.Value == tokens.UserId || e.Value == secondUserId);
                NewChat(chat.Id, toAdd);

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
        }

        [HttpPost, RequireHttps]
        public async Task<ActionResult> CreateGroup(string accessToken, string name, string userIds)
        {
            if (String.IsNullOrWhiteSpace(accessToken) || String.IsNullOrWhiteSpace(name) || String.IsNullOrWhiteSpace(userIds))
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
                var userIdsTable = new HashSet<long>();
                try
                {
                    foreach (var e in userIds.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        userIdsTable.Add(long.Parse(e));
                    }
                }
                catch
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Incorrect user ids");
                }
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

                var toAdd = Hubs.ChatHub.users.Where(e => userIdsTable.Contains(e.Value));
                NewChat(chat.Id, toAdd);

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
        }

        [HttpPost, RequireHttps]
        public async Task<ActionResult> CreatePublic(string accessToken, string name, string userIds)
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
                    userIdsTable = new HashSet<long>();
                    try
                    {
                        foreach (var e in userIds.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            userIdsTable.Add(long.Parse(e));
                        }
                    }
                    catch
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Incorrect user ids");
                    }
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

                var toAdd = Hubs.ChatHub.users.Where(e => userIdsTable.Contains(e.Value));
                NewChat(chat.Id, toAdd);

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
        }

        [HttpPost, RequireHttps]
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

        [HttpPost, RequireHttps]
        public async Task<ActionResult> ChangeAvatar(string accessToken, long? chatId, long? fileId)
        {
            if (String.IsNullOrWhiteSpace(accessToken) || !chatId.HasValue || !fileId.HasValue)
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
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "You can't change avatar in dialog");
                }

                var userInChat = await db.UsersInChats.FirstOrDefaultAsync(e => e.UserId == tokens.UserId && e.ChatId == chatId);
                if (userInChat == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "You don't have access to this Chat");
                }
                if (!userInChat.CanWrite)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "You can't change avatar in this chat");
                }
                
                if (!await db.UploadedFiles.AnyAsync(e => e.Id == fileId))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound, "File is not exists");
                }

                if (chat.Avatar != null)
                {
                    await FilesController.RemoveFile(chat.Avatar, db);
                }

                chat.Avatar = fileId;
                db.Entry(chat).State = EntityState.Modified;
                await db.SaveChangesAsync();

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
        }
        
        [HttpPost, RequireHttps]
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
                    UserId = tokens.UserId,
                    UnreadedMessages = 0,
                    CanWrite = false
                };
                db.UsersInChats.Add(userInChat);
                chat.MembersCount++;
                db.Entry(chat).State = EntityState.Modified;
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

                return Json(db.UsersInChats.Where(e => e.ChatId == chatId).OrderBy(e => e.Id).Skip(start).Take(count.Value).Select(e => e.UserId), JsonRequestBehavior.AllowGet);
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
        }

        [HttpPost, RequireHttps]
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
                chat.MembersCount--;
                db.Entry(chat).State = EntityState.Modified;
                await db.SaveChangesAsync();

                return new HttpStatusCodeResult(HttpStatusCode.OK);
                
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
        }

        [HttpPost, RequireHttps]
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
                chat.MembersCount--;
                db.Entry(chat).State = EntityState.Modified;
                await db.SaveChangesAsync();

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
        }

        [HttpPost, RequireHttps]
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

        [HttpPost, RequireHttps]
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

                return Json(await db.BannedByChat.Where(e => e.BannerId == chatId).OrderBy(e => e.Id).Skip(start).Take(count.Value).Select(e => e.BannedId).ToArrayAsync(), JsonRequestBehavior.AllowGet);
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
        }

        [HttpPost, RequireHttps]
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
                chat.MembersCount++;
                db.Entry(chat).State = EntityState.Modified;
                await db.SaveChangesAsync();

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
        }

        public async Task<ActionResult> FindPublicByName(string name, int? count, int start = 0)
        {
            if (String.IsNullOrWhiteSpace(name))
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
                return Json(await db.Chats.Where(e => e.Type == Enums.ChatType.Public && e.Name.StartsWith(name)).OrderBy(f => f.Id).Skip(start).Take(count.Value).Select(e => new { id = e.Id, name = e.Name, membersCount = e.MembersCount, avatar = e.Avatar, type = e.Type, creator = e.Creator}).ToArrayAsync(), JsonRequestBehavior.AllowGet);
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

        private static async void NewChat(long chatId, IEnumerable<KeyValuePair<string, long>> toAdd)
        {
            var context = Microsoft.AspNet.SignalR.GlobalHost.ConnectionManager.GetHubContext<Hubs.ChatHub>();
            foreach (var e in toAdd)
            {
                await context.Groups.Add(e.Key, chatId.ToString());
            }
            context.Clients.Group(chatId.ToString()).newChat(chatId);
        }
    }
}
