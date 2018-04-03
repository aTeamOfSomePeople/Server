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
    public class UsersController : Controller
    {
        private ServerContext db = new ServerContext();

        [HttpPost]
        public async Task<ActionResult> ChangeName(string accessToken, string newName)
        {
            if (String.IsNullOrWhiteSpace(accessToken) || String.IsNullOrWhiteSpace(newName))
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

                var user = await db.Users.FindAsync(tokens.UserId);
                user.Name = newName;
                db.Entry(user).State = EntityState.Modified;
                await db.SaveChangesAsync();

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
        }

        [HttpPost]
        public async Task<ActionResult> ChangeDescription(string accessToken, string newDescription = "")
        {
            if (String.IsNullOrWhiteSpace(accessToken))
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

                var user = await db.Users.FindAsync(tokens.UserId);
                user.Description = newDescription;
                db.Entry(user).State = EntityState.Modified;
                await db.SaveChangesAsync();

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
        }

        [HttpPost]
        public async Task<ActionResult> ChangeAvatar(string accessToken, HttpPostedFileBase avatar)
        {
            if (String.IsNullOrWhiteSpace(accessToken))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Arguments is null or empty");
            }

            if (avatar == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "File is null");
            }
            if (avatar.ContentLength > 2097152)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Too big file");
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
                var user = await db.Users.FindAsync(tokens.UserId);
                var cdnClient = (new ZeroCdnClients.CdnClientsFactory(Properties.Resources.ZeroCDNUsername, Properties.Resources.ZeroCDNKey)).Files;

                var file = new byte[avatar.ContentLength];
                await avatar.InputStream.ReadAsync(file, 0, avatar.ContentLength);
                var avatarFile = await cdnClient.Add(file, $"{DateTime.UtcNow.Ticks}.{avatarExtention}");
                try
                {
                    if (user.Avatar != null)
                    {
                        await cdnClient.Remove(long.Parse(user.Avatar.Split('/')[3]));
                    }
                }
                catch { }
                user.Avatar = $"http://zerocdn.com/{avatarFile.ID}/{avatarFile.Name}";

                db.Entry(user).State = EntityState.Modified;
                await db.SaveChangesAsync();

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
        }

        [HttpPost]
        public async Task<ActionResult> BanUser(string accessToken, long? userId)
        {
            if (String.IsNullOrWhiteSpace(accessToken) || !userId.HasValue)
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

                if (await db.BannedByUser.AnyAsync(e => e.BannedId == userId && e.BannerId == tokens.UserId))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound, "User already banned");
                }

                var bannedByUser = new BannedByUser()
                {
                    BannedId = userId.Value,
                    BannerId = tokens.Id
                };

                db.BannedByUser.Add(bannedByUser);
                await db.SaveChangesAsync();

                return new HttpStatusCodeResult(HttpStatusCode.OK);

            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
        }

        [HttpPost]
        public async Task<ActionResult> UnBanUser(string accessToken, long? userId)
        {
            if (String.IsNullOrWhiteSpace(accessToken) || !userId.HasValue)
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

                var bannedByUser = await db.BannedByUser.FirstOrDefaultAsync(e => e.BannedId == userId && e.BannerId == tokens.UserId);
                if (bannedByUser == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound, "User is not banned");
                }

                db.BannedByUser.Remove(bannedByUser);
                await db.SaveChangesAsync();

                return new HttpStatusCodeResult(HttpStatusCode.OK);

            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
        }

        public async Task<ActionResult> GetBannedUsers(string accessToken, int? count, int start = 0)
        {
            if (String.IsNullOrWhiteSpace(accessToken))
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

                return Json(await db.BannedByUser.Where(e => e.BannerId == tokens.Id).Skip(start).Take(count.Value).Select(e => e.BannedId).ToArrayAsync(), JsonRequestBehavior.AllowGet);
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
        }

        //public async Task<ActionResult> GetUsers()
        //{
        //    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
        //}

        public async Task<ActionResult> FindUsersByName(string name, int? count, int start = 0)
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
                return Json(await db.Users.Where(e => e.Name.StartsWith(name)).Skip(start).Take(count.Value).Select(e => e.Id).ToArrayAsync(), JsonRequestBehavior.AllowGet);
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
