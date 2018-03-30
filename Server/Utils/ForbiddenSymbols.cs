using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Server.Utils
{
    public class ForbiddenSymbols
    {
        public static readonly char[] inFileName = { '/', '\\', '?', '%', '*', ':', '|', '\"', '<', '>'};
    }
}