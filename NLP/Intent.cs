using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amanda
{
    public class Intent
    {
        public string userInput;
        public string intent;
        public Application application;
        public string site;
        public string concernedObject;
        public Intent(string userInput, string intent)
        {
            this.userInput = userInput;
            this.intent = intent;
        }

    }
}
