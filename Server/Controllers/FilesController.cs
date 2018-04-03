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
    public class FilesController : Controller
    {
        private ServerContext db = new ServerContext();

        [HttpPost]
        public async Task<ActionResult> UploadFile(HttpPostedFile file)
        {
            if (file == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "File is null");
            }
            if (file.ContentLength > 2097152)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Too big file");
            }

            var fileExtention = Utils.ValidateFile.GetImageExtention(file.InputStream);
            if (!Utils.FilesExstensions.PosibleImageExtensions.Contains(fileExtention))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid file type");
            }

            //if (ModelState.IsValid)
            //{
            //    var cdnClient = (new ZeroCdnClients.CdnClientsFactory(Properties.Resources.ZeroCDNUsername, Properties.Resources.ZeroCDNKey)).Files;

            //    var fileBytes = new byte[file.ContentLength];
            //    await file.InputStream.ReadAsync(fileBytes, 0, file.ContentLength);
            //    var uploadedFile = await cdnClient.Add(fileBytes, $"{DateTime.UtcNow.Ticks}.{fileExtention}");
            //    try
            //    {
            //        if (chat.Avatar != null)
            //        {
            //            await cdnClient.Remove(long.Parse(chat.Avatar.Split('/')[3]));
            //        }
            //    }
            //    catch { }
            //    chat.Avatar = $"http://zerocdn.com/{uploadedFile.ID}/{uploadedFile.Name}";
            //    await db.SaveChangesAsync();
            //}

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
