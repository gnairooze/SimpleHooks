using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Definition
{
    public class AppOption: ModelBase
    {
        public string Category { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
