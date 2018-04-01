using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;

namespace Server.Models
{
    public class ServerContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx
    
        public ServerContext() : base("name=LocalDB")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            base.OnModelCreating(modelBuilder);
        }

        public System.Data.Entity.DbSet<Server.Models.Attachments> Attachments { get; set; }

        public System.Data.Entity.DbSet<Server.Models.Chats> Chats { get; set; }

        public System.Data.Entity.DbSet<Server.Models.Messages> Messages { get; set; }

        public System.Data.Entity.DbSet<Server.Models.Users> Users { get; set; }

        public System.Data.Entity.DbSet<Server.Models.UsersInChats> UsersInChats { get; set; }
        
        public System.Data.Entity.DbSet<Server.Models.MutedUsers> MutedUsers { get; set; }

        public System.Data.Entity.DbSet<Server.Models.DeletedMessages> DeletedMessages { get; set; }

        public System.Data.Entity.DbSet<Server.Models.BannedByUser> BannedByUser { get; set; }

        public System.Data.Entity.DbSet<Server.Models.BannedByChat> BannedByChat { get; set; }

        public System.Data.Entity.DbSet<Server.Models.ExternalServices> Services { get; set; }

        public System.Data.Entity.DbSet<Server.Models.Accounts> Accounts { get; set; }

        public System.Data.Entity.DbSet<Server.Models.ExternalAccounts> ExternalAccounts { get; set; }

        public System.Data.Entity.DbSet<Server.Models.Tokens> Tokens { get; set; }
    }
}
