using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Server.Models
{
    public class Chats
    {
        public long Id { get; set; }

        [Required, ForeignKey("User")]
        public long Creator { get; set; }
        public virtual Users User { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Type { get; set; }

        public string Avatar { get; set; }
    }
}