﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GroupBasedAuthorise.Models
{
    public class PermissionViewModel
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public bool Checked { get; set; }
    }
}
