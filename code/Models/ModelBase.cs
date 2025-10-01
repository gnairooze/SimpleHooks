using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleTools.SimpleHooks.Models
{
    public abstract class ModelBase
    {
        public long Id { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string ModifyBy { get; set; }
        public DateTime ModifyDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        public bool Active { get; set; }
    }
}
