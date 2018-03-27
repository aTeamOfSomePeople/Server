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
    public class AccountsController : Controller
    {
        private ServerContext db = new ServerContext();

        // GET: Accounts/Details/5
        public async Task<ActionResult> Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Accounts accounts = await db.Accounts.FindAsync(id);
            if (accounts == null)
            {
                return HttpNotFound();
            }
            return View(accounts);
        }

        [HttpPost]
        public async Task<ActionResult> OAuth(string accessToken, string service)
        {
            var jsonResult = new JsonResult();
            jsonResult.Data = null;

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

                        if (await db.ExternalAccounts.FirstOrDefaultAsync(e => e.UserId == VKUserId && e.Service == serv.Id) == null)
                        {
                            var account = new ExternalAccounts();
                            account.UserId = VKUserId;
                            account.Service = serv.Id;
                            account.IsDeleted = false;
                            db.ExternalAccounts.Add(account);

                            var cdnClient = (new ZeroCdnClients.CdnClientsFactory(Properties.Resources.ZeroCDNUsername, Properties.Resources.ZeroCDNKey)).Files;

                            var httpResponse = await (new System.Net.Http.HttpClient()).GetAsync(String.Format("https://api.vk.com/method/users.get?user_ids={0}&fields=photo_200&access_token={1}&client_secret={2}&v=5.73", VKUserId, Properties.Resources.VKAccessToken, Properties.Resources.VKSecretKey));
                            var stringResponse = await httpResponse.Content.ReadAsStringAsync();
                            var response = Newtonsoft.Json.JsonConvert.DeserializeObject<VKUserInfoResponse>(stringResponse).response;
                            var avatar = await cdnClient.Add(await (new System.Net.Http.HttpClient()).GetByteArrayAsync(response[0]["photo_200"]), response[0]["photo_200"].Split('/').LastOrDefault());
                            var user = new Users
                            {
                                IsExternal = true,
                                AccountId = VKUserId,
                                Name = $"{response[0]["first_name"]} {response[0]["last_name"]}",
                                Avatar = $"http://zerocdn.com/{avatar.ID}/{avatar.Name}"
                            };
                            db.Users.Add(user);
                            await db.SaveChangesAsync();
                        }

                        var tokens = new Tokens();
                        tokens.AccessToken = Guid.NewGuid().ToString().Replace("-", "");
                        tokens.RefreshToken = Guid.NewGuid().ToString().Replace("-", "");
                        tokens.UserId = (await db.Users.FirstOrDefaultAsync(e => e.AccountId == VKUserId)).Id;
                        tokens.EndDate = DateTime.UtcNow.AddDays(1);

                        db.Tokens.Add(tokens);
                        await db.SaveChangesAsync();

                        jsonResult.Data = new Utils.AuthorizeResponse(tokens.AccessToken, tokens.RefreshToken, tokens.UserId);
                        return jsonResult;
                    case 2:
                        return jsonResult;
                    case 3:
                        return jsonResult;
                }
            }
            catch(Exception e) { jsonResult.Data = e.Message; return jsonResult; }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
        }

        [HttpPost]
        public async Task<ActionResult> RefreshTokens(string refreshToken)
        {
            var jsonResult = new JsonResult();
            jsonResult.Data = null;

            if (String.IsNullOrWhiteSpace(refreshToken))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var tokens = await db.Tokens.FirstOrDefaultAsync(e => e.RefreshToken == refreshToken);
            if (tokens != null)
            {
                tokens.AccessToken = Guid.NewGuid().ToString().Replace("-", "");
                tokens.RefreshToken = Guid.NewGuid().ToString().Replace("-", "");

                db.Entry(tokens).State = EntityState.Modified;
                await db.SaveChangesAsync();

                jsonResult.Data = new Utils.AuthorizeResponse(tokens.AccessToken, tokens.RefreshToken, tokens.UserId);
                return jsonResult;
            }
                

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        // POST: Accounts/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public async Task<ActionResult> Create([Bind(Include = "Email,Login,Password")] Accounts accounts)
        {
            if (ModelState.IsValid)
            {
                db.Accounts.Add(accounts);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(accounts);
        }

        // POST: Accounts/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Email,Login,Password")] Accounts accounts)
        {
            if (ModelState.IsValid)
            {
                db.Entry(accounts).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(accounts);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
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
