using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amanda
{
    public class WitEntity
    {
        public string value;
        public string type;

        public WitEntity(string value, string type)
        {
            this.value = value;
            this.type = type;
        }

        public override string ToString()
        {
            return "Value: " + this.value + " / Type: " + this.type;
        }
    }
}
