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
    public class TokensController : Controller
    {
        private ServerContext db = new ServerContext();

        [HttpPost]
        public async Task<ActionResult> RefreshTokens(string refreshToken)
        {
            var jsonResult = new JsonResult();
            jsonResult.Data = null;

            if (String.IsNullOrWhiteSpace(refreshToken))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Arguments is null or empty");
            }

            var tokens = await db.Tokens.FirstOrDefaultAsync(e => e.RefreshToken == refreshToken);
            if (tokens == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Token is invalid");
            }
            try
            {
                tokens.AccessToken = Guid.NewGuid().ToString().Replace("-", "");
                tokens.RefreshToken = Guid.NewGuid().ToString().Replace("-", "");
                tokens.Date = DateTime.UtcNow;
                tokens.Expire = DateTime.UtcNow.AddDays(1);
                db.Entry(tokens).State = EntityState.Modified;
                await db.SaveChangesAsync();

                jsonResult.Data = new Utils.AuthorizeResponse(tokens.AccessToken, tokens.RefreshToken, tokens.UserId);
                return jsonResult;
            }
            catch { }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
        }

        public async Task<ActionResult> CheckToken(string accessToken)
        {
            var jsonResult = new JsonResult();
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            if (String.IsNullOrWhiteSpace(accessToken))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Arguments is null or empty");
            }
            var token = await ValidToken(accessToken, db);
            if (token != null)
            {
                jsonResult.Data = new Utils.CheckTokenResponse()
                {
                    userId = token.UserId,
                    date = token.Date,
                    expire = token.Expire
                };

                return jsonResult;
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Token is invalid");
        }

        internal async static Task<Tokens> ValidToken(string accessToken, ServerContext db)
        {
            return await db.Tokens.FirstOrDefaultAsync(e => e.AccessToken == accessToken && e.Expire > DateTime.UtcNow);
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
