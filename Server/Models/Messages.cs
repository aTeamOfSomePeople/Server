using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Server.Models
{
    public class Messages
    {
        public int Id { get; set; }

        //Foreign keys
        [ForeignKey("Id")]
        public int ChatId { get; set; }
        [ForeignKey("Id")]
        public int UserId { get; set; }
    }
}