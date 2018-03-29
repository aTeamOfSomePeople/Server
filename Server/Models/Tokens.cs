using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Server.Models
{
    public class Tokens
    {
        public long Id { get; set; }

        [Required]//, Index("aToken", IsUnique = true, Order = 0)]
        public string AccessToken { get; set; }

        [Required]//, Index("rToken", IsUnique = true, Order = 1)]
        public string RefreshToken { get; set; }

        [Required, ForeignKey("Users")]
        public long UserId { get; set; }
        public virtual Users Users { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public DateTime Expire { get; set; } 
    }
}