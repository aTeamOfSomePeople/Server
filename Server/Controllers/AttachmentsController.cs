using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Server.Models;

namespace Server.Controllers
{
    public class AttachmentsController : ApiController
    {
        private ServerContext db = new ServerContext();

        // GET: api/Attachments
        public IQueryable<Attachments> GetAttachments()
        {
            return db.Attachments;
        }

        // GET: api/Attachments/5
        [ResponseType(typeof(Attachments[]))]
        public async Task<IHttpActionResult> GetAttachments(int id)
        {
            var attachments = db.Attachments.SqlQuery("GetAttachmentsToMessage @id", new SqlParameter("id", id));
            if (attachments == null)
            {
                return NotFound();
            }

            return Ok(attachments);
        }

        // PUT: api/Attachments/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutAttachments(int id, Attachments attachments)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != attachments.Id)
            {
                return BadRequest();
            }

            db.Entry(attachments).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AttachmentsExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Attachments
        [ResponseType(typeof(Attachments))]
        public async Task<IHttpActionResult> PostAttachments(Attachments attachments)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Attachments.Add(attachments);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = attachments.Id }, attachments);
        }

        // DELETE: api/Attachments/5
        [ResponseType(typeof(Attachments))]
        public async Task<IHttpActionResult> DeleteAttachments(int id)
        {
            Attachments attachments = await db.Attachments.FindAsync(id);
            if (attachments == null)
            {
                return NotFound();
            }

            db.Attachments.Remove(attachments);
            await db.SaveChangesAsync();

            return Ok(attachments);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool AttachmentsExists(int id)
        {
            return db.Attachments.Count(e => e.Id == id) > 0;
        }
    }
}