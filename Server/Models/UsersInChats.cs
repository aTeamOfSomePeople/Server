using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Server.Models
{
    public class UsersInChats
    {
        public int Id { get; set; }
        //Foreign keys
        public int ChatId { get; set; }
        public int UserId { get; set; }
    }
}