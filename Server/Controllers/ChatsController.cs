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
    public class ChatsController : ApiController
    {
        private ServerContext db = new ServerContext();

        // GET: api/Chats
        public IQueryable<Chats> GetChats()
        {
            return db.Chats;
        }

        // GET: api/Chats/5
        [ResponseType(typeof(Chats))]
        public async Task<IHttpActionResult> GetChats(int id)
        {
            Chats chats = await db.Chats.FindAsync(id);
            if (chats == null)
            {
                return NotFound();
            }

            return Ok(chats);
        }

        // PUT: api/Chats/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutChats(int id, Chats chats)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != chats.Id)
            {
                return BadRequest();
            }

            db.Entry(chats).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ChatsExists(id))
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

        // POST: api/Chats
        [ResponseType(typeof(Chats))]
        public async Task<IHttpActionResult> PostChats(Chats chats)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Chats.Add(chats);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = chats.Id }, chats);
        }

        // DELETE: api/Chats/5
        [ResponseType(typeof(Chats))]
        public async Task<IHttpActionResult> DeleteChats(int id)
        {
            Chats chats = await db.Chats.FindAsync(id);
            if (chats == null)
            {
                return NotFound();
            }

            db.Chats.Remove(chats);
            await db.SaveChangesAsync();

            return Ok(chats);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ChatsExists(int id)
        {
            return db.Chats.Count(e => e.Id == id) > 0;
        }
    }
}