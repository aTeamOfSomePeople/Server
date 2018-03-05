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
    public class UsersInChatsController : ApiController
    {
        private ServerContext db = new ServerContext();

        // GET: api/UsersInChats
        public IQueryable<UsersInChats> GetUsersInChats()
        {
            return db.UsersInChats;
        }

        // GET: api/UsersInChats/5
        [ResponseType(typeof(UsersInChats))]
        public async Task<IHttpActionResult> GetUsersInChats(int id)
        {
            UsersInChats usersInChats = await db.UsersInChats.FindAsync(id);
            if (usersInChats == null)
            {
                return NotFound();
            }

            return Ok(usersInChats);
        }

        // PUT: api/UsersInChats/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutUsersInChats(int id, UsersInChats usersInChats)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != usersInChats.ChatId)
            {
                return BadRequest();
            }

            db.Entry(usersInChats).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsersInChatsExists(id))
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

        // POST: api/UsersInChats
        [ResponseType(typeof(UsersInChats))]
        public async Task<IHttpActionResult> PostUsersInChats(UsersInChats usersInChats)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.UsersInChats.Add(usersInChats);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (UsersInChatsExists(usersInChats.ChatId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = usersInChats.ChatId }, usersInChats);
        }

        // DELETE: api/UsersInChats/5
        [ResponseType(typeof(UsersInChats))]
        public async Task<IHttpActionResult> DeleteUsersInChats(int id)
        {
            UsersInChats usersInChats = await db.UsersInChats.FindAsync(id);
            if (usersInChats == null)
            {
                return NotFound();
            }

            db.UsersInChats.Remove(usersInChats);
            await db.SaveChangesAsync();

            return Ok(usersInChats);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool UsersInChatsExists(int id)
        {
            return db.UsersInChats.Count(e => e.ChatId == id) > 0;
        }
    }
}