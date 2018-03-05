using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Server.Models;

namespace Server.Controllers
{
    public class MessagesController : ApiController
    {
        private ServerContext db = new ServerContext();

        // GET: api/Messages
        public IQueryable<Messages> GetMessages()
        {
            return db.Messages;
        }

        // GET: api/Messages/5
        [ResponseType(typeof(Messages))]
        public async Task<IHttpActionResult> GetMessages(int id)
        {
            Messages messages = await db.Messages.FindAsync(id);
            if (messages == null)
            {
                return NotFound();
            }

            return Ok(messages);
        }

        // PUT: api/Messages/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutMessages(int id, Messages messages)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != messages.Id)
            {
                return BadRequest();
            }

            db.Entry(messages).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MessagesExists(id))
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

        // POST: api/Messages
        [ResponseType(typeof(Messages))]
        public async Task<IHttpActionResult> PostMessages(Messages messages)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Messages.Add(messages);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = messages.Id }, messages);
        }

        // DELETE: api/Messages/5
        [ResponseType(typeof(Messages))]
        public async Task<IHttpActionResult> DeleteMessages(int id)
        {
            Messages messages = await db.Messages.FindAsync(id);
            if (messages == null)
            {
                return NotFound();
            }

            db.Messages.Remove(messages);
            await db.SaveChangesAsync();

            return Ok(messages);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool MessagesExists(int id)
        {
            return db.Messages.Count(e => e.Id == id) > 0;
        }
    }
}