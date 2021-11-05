using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public abstract class ModelBase
    {
        public ModelBase()
        {
            this.Notes = string.Empty;
        }
        public long Id { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string ModifyBy { get; set; }
        public DateTime ModifyDate { get; set; }
        public string Notes { get; set; }
        public bool Active { get; set; }
    }
}
