using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Server.Models
{
    public class Users
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [StringLength(30)]
        public string Email { get; set; }

        [StringLength(20), Index("login", IsUnique = true), Required]
        public string Login { get; set; }

        [Required]
        public string Password { get; set; }

        public string Avatar { get; set; }
    }   
}