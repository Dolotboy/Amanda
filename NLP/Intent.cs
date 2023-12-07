using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amanda
{
    public enum IntentType
    {
        OpenApplication,
        PlayMusic,
        PlayMovie,
        Search,
        Unknown
    }
    public class Intent
    {
        public string userInput;
        public IntentType intent;
        public Application application;
        public string site;
        public string concernedObject;
        public Intent(string userInput, IntentType intent)
        {
            this.userInput = userInput;
            this.intent = intent;
        }

    }
}
