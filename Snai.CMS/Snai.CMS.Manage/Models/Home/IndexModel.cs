﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Snai.CMS.Manage.Models.Home
{
    public class IndexModel : LayoutModel
    {
        public string LastLogonIP { get; set; }
        public DateTime LastLogonTime { get; set; }
    }
}
