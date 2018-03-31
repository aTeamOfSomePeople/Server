﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Server.Models
{
    public class Accounts
    {
        public long Id { get; set; }

        [StringLength(30), Index("login", IsUnique = true), Required]
        public string Login { get; set; }

        [Required]
        public string Password { get; set; }

        public bool IsDeleted { get; set; }
    }
}