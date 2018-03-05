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
        //Foreign keys
        [Key, Column(Order = 0)]
        public int ChatId { get; set; }
        [Key, Column(Order = 1)]
        public int UserId { get; set; }
    }
}