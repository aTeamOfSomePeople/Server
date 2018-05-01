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
using System.Text;

namespace Server.Controllers
{
    public class AccountsController : Controller
    {
        private ServerContext db = new ServerContext();

        [HttpPost, RequireHttps]
        public async Task<ActionResult> Auth(string login, string password)
        {
            if (String.IsNullOrWhiteSpace(login) || String.IsNullOrWhiteSpace(password))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Arguments is null or empty");
            }

            var account = await db.Accounts.FirstOrDefaultAsync(e => e.Login == login);
            if (account == null)
            {
                return HttpNotFound();
            }
            if (account.IsDeleted)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Account is deleted");
            }

            var passwordHash = StringHash(System.Text.Encoding.UTF8.GetBytes($"{login}{Properties.Resources.HMACKey}"), System.Text.Encoding.UTF8.GetBytes(password));

            if (account.Password == passwordHash)
            {
                var tokens = await NewTokens(account.Id);
                return Json(new Utils.AuthorizeResponse(tokens.AccessToken, tokens.RefreshToken, tokens.UserId));
            }

            return HttpNotFound();
        }

        [HttpPost, RequireHttps]
        public async Task<ActionResult> OAuth(string accessToken, string service)
        {
            if (String.IsNullOrWhiteSpace(accessToken) || String.IsNullOrWhiteSpace(service))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Arguments is null or empty");
            }

            var serv = await db.Services.FirstOrDefaultAsync(e => e.Name == service.ToLower());
            if (serv == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, $"Service '{service}' is not supported");
            }
            try
            {
                switch (serv.Id)
                {
                    case 1:
                        long VKUserId;
                        try
                        {
                            var httpResponse = await (new System.Net.Http.HttpClient()).GetAsync(String.Format("https://api.vk.com/method/secure.checkToken?token={0}&access_token={1}&client_secret={2}&v=5.73", accessToken, Properties.Resources.VKAccessToken, Properties.Resources.VKSecretKey));
                            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
                            VKUserId = long.Parse(Newtonsoft.Json.JsonConvert.DeserializeObject<VKTokenCheckResponse>(stringResponse).response["user_id"]);
                        }
                        catch
                        {
                            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid access token");
                        }
                        var externalAccount = await db.ExternalAccounts.FirstOrDefaultAsync(e => e.UserId == VKUserId && e.Service == serv.Id);
                        if (externalAccount == null)
                        {
                            var account = new ExternalAccounts();
                            account.UserId = VKUserId;
                            account.Service = serv.Id;
                            account.IsDeleted = false;
                            db.ExternalAccounts.Add(account);
                            
                            var httpResponse = await (new System.Net.Http.HttpClient()).GetAsync(String.Format("https://api.vk.com/method/users.get?user_ids={0}&fields=photo_max_orig&access_token={1}&client_secret={2}&v=5.73", VKUserId, Properties.Resources.VKAccessToken, Properties.Resources.VKSecretKey));
                            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
                            var response = Newtonsoft.Json.JsonConvert.DeserializeObject<VKUserInfoResponse>(stringResponse).response;
                            var fileId = await FilesController.UploadFile(response[0]["photo_max_orig"], db);
                            var user = new Users
                            {
                                AccountId = VKUserId,
                                Name = $"{response[0]["first_name"]} {response[0]["last_name"]}",
                                Avatar = fileId.Value,
                                ServiceId = serv.Id
                            };
                            db.Users.Add(user);
                            await db.SaveChangesAsync();
                        }
                        else if (externalAccount.IsDeleted)
                        {
                            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Account is deleted");
                        }

                        var tokens = await NewTokens(VKUserId);

                        return Json(new Utils.AuthorizeResponse(tokens.AccessToken, tokens.RefreshToken, tokens.UserId));
                         
                    case 2:
                        return Json(null);
                    case 3:
                        return Json(null);
                }
            }
            catch(Exception e){ System.Console.WriteLine(e.Message);
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, e.Message); }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
        }
        
        [HttpPost, RequireHttps]
        public async Task<ActionResult> Register(string login, string password, string name)
        {
            if (String.IsNullOrWhiteSpace(login) || String.IsNullOrWhiteSpace(password) || String.IsNullOrWhiteSpace(name))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Arguments is null or empty");
            }

            if (db.Accounts.Any(e => e.Login == login))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Account already exists");
            }

            if (ModelState.IsValid)
            {
                var account = new Accounts()
                {
                    Login = login,
                    Password = StringHash(Encoding.UTF8.GetBytes($"{login}{Properties.Resources.HMACKey}"), Encoding.UTF8.GetBytes(password)),
                    IsDeleted = false
                };
                db.Accounts.Add(account);
                await db.SaveChangesAsync();

                var user = new Users()
                {
                    Name = name,
                    ServiceId = (await db.Services.FirstOrDefaultAsync(e => e.Name == "zeromessenger")).Id,
                    AccountId = account.Id
                };
                db.Users.Add(user);

                await db.SaveChangesAsync();
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
        }
        
        [HttpPost, RequireHttps]
        public async Task<ActionResult> ChangePassword(string accessToken, string oldPassword, string newPassword)
        {
            if (String.IsNullOrWhiteSpace(accessToken) || String.IsNullOrWhiteSpace(oldPassword) || String.IsNullOrWhiteSpace(newPassword))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Arguments is null or empty");
            }

            if (ModelState.IsValid)
            {
                var tokens = await (new TokensController().ValidToken(accessToken));
                if (tokens == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid access token");
                }

                var user = await db.Users.FindAsync(tokens.UserId);
                if (user.ServiceId != (await db.Services.FirstOrDefaultAsync(e => e.Name == "zeromessenger")).Id)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Allowed only for internal users");
                }

                var account = await db.Accounts.FindAsync(user.AccountId);
                if (account.Password != StringHash(Encoding.UTF8.GetBytes($"{account.Login}{Properties.Resources.HMACKey}"), Encoding.UTF8.GetBytes(oldPassword)))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Passwords don't match");
                }

                account.Password = StringHash(Encoding.UTF8.GetBytes($"{account.Login}{Properties.Resources.HMACKey}"), Encoding.UTF8.GetBytes(newPassword));
                db.Entry(account).State = EntityState.Modified;
                try
                {
                    db.Tokens.RemoveRange(db.Tokens.Where(e => e.UserId == db.Users.FirstOrDefault(z => z.AccountId == account.Id).Id));
                }
                catch { }
                await db.SaveChangesAsync();

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
        }

        [HttpPost, RequireHttps]
        public async Task<ActionResult> Delete(string accessToken)
        {
            if (ModelState.IsValid)
            {
                var tokens = await (new TokensController().ValidToken(accessToken));
                if (tokens == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid access token");
                }

                var user = await db.Users.FindAsync(tokens.UserId);

                var account = await db.Accounts.FindAsync(user.AccountId);
                account.IsDeleted = true;
                db.Entry(account).State = EntityState.Modified;
                db.Tokens.RemoveRange(db.Tokens.Where(e => e.UserId == user.Id));
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

        private async Task<Tokens> NewTokens(long userId)
        {
            var tokens = new Tokens
            {
                AccessToken = Guid.NewGuid().ToString().Replace("-", ""),
                RefreshToken = Guid.NewGuid().ToString().Replace("-", ""),
                UserId = (await db.Users.FirstOrDefaultAsync(e => e.AccountId == userId)).Id,
                Date = DateTime.UtcNow,
                Expire = DateTime.UtcNow.AddDays(1)
            };

            db.Tokens.Add(tokens);
            await db.SaveChangesAsync();

            return tokens;
        }

        private string StringHash(byte[] key, byte[] message)
        {
            return Convert.ToBase64String(new System.Security.Cryptography.HMACSHA1(key).ComputeHash(message));
        }

        private class VKTokenCheckResponse
        {
            public Dictionary<string, string> response { get; set; }
        }
        private class VKUserInfoResponse
        {
            public Dictionary<string, string>[] response { get; set; }
        }
    }
}
