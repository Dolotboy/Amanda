using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amanda
{
    public class WitQuery
    {
        public JObject result;
        public string intent;
        public List<WitEntity> entities;

        public WitQuery(JObject result)
        {
            this.result = result;
            entities = new List<WitEntity>();
        }

        public WitQuery(JObject result, string intent, List<WitEntity> entities) : this(result)
        {
            this.intent = intent;
            this.entities = entities;
        }

        public override string ToString()
        {
            string entitiesString = "";
            foreach(WitEntity entity in entities)
            {
                entitiesString += entity.ToString() + " ";
            }
            return "Intent: " + this.intent + " Entities: " + entitiesString;
        }
    }
}
