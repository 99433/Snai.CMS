﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Snai.CMS.Manage.Entities.BackManage
{
    [Table("role_right")]
    public class RoleRight
    {
        [Column("role_id")]
        public int RoleID { get; set; }

        [Column("module_id")]
        public int ModuleID { get; set; }
    }
}
