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
        private static ZeroCdnClients.CdnFilesClient cdnClient = (new ZeroCdnClients.CdnClientsFactory(Properties.Resources.ZeroCDNUsername, Properties.Resources.ZeroCDNKey)).Files;

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

            if (ModelState.IsValid)
            {
                var fileBytes = new byte[file.ContentLength];
                await file.InputStream.ReadAsync(fileBytes, 0, file.ContentLength);
                var cdnFile = await cdnClient.Add(fileBytes, $"{DateTime.UtcNow.Ticks}.{fileExtention}");
                var uploadedFile = new UploadedFiles()
                {
                    Link = $"http://zerocdn.com/{cdnFile.ID}/{cdnFile.Name}",
                    FileSize = cdnFile.Size
                };
                db.UploadedFiles.Add(uploadedFile);
                await db.SaveChangesAsync();

                return Json(uploadedFile.Id);
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Fail");
        }


        internal static async Task<ActionResult> RemoveFile(long? id, ServerContext db)
        {
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Arguments is null or empty");
            }
            var uploadedFile = await db.UploadedFiles.FindAsync(id);
            if (uploadedFile == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound, "File is not exists");
            }

            db.UploadedFiles.Remove(uploadedFile);
            await db.SaveChangesAsync();

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        internal static async Task<long?> UploadFile(string fileLink, ServerContext db)
        {
            var fileBytes = await (new System.Net.Http.HttpClient()).GetByteArrayAsync(fileLink);

            if (fileBytes == null)
            {
                return null;
            }
            if (fileBytes.Length > 2097152)
            {
                return null;
            }

            var fileExtention = Utils.ValidateFile.GetImageExtention(new System.IO.MemoryStream(fileBytes));
            if (!Utils.FilesExstensions.PosibleImageExtensions.Contains(fileExtention))
            {
                return null;
            }

            var file = await cdnClient.Add(fileBytes, $"{DateTime.UtcNow.Ticks}.jpg");
            var uploadedFile = new UploadedFiles()
            {
                Link = $"http://zerocdn.com/{file.ID}/{file.Name}",
                FileSize = file.Size
            };
            db.UploadedFiles.Add(uploadedFile);
            await db.SaveChangesAsync();

            return uploadedFile.Id;
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
