﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Snai.CMS.Manage.Entities.BackManage
{
    [Serializable]
    public class AdminLogin
    {
        public string UserName { get; set; }
        
        public string Password { get; set; }

        public string VerifyCode { get; set; }
    }
}
