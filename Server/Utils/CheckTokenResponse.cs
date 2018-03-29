using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Server.Utils
{
    public class CheckTokenResponse
    {
        public long userId { get; set; }
        public DateTime date { get; set; }
        public DateTime expire { get; set; }
    }
}