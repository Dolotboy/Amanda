using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amanda
{
    public class Bot
    {

        public Bot()
        {

        }

        public static void execute(Intent intent)
        {
            Console.WriteLine($"Recognized Intent: {intent.intent}");
            switch (intent.intent)
            {
                case IntentType.OpenApplication:
                    Console.WriteLine("Openning: " + intent.application.Name);
                    Process.Start(intent.application.ExecutablePath);
                    break;
                case IntentType.PlayMusic:
                    Console.WriteLine("Playing music: " + intent.concernedObject);
                    break;
                case IntentType.PlayMovie:
                    Console.WriteLine("Playing movie:" + intent.concernedObject);
                    break;
                case IntentType.Search:
                    Console.WriteLine("Searching:" + intent.concernedObject);
                    var ps = new ProcessStartInfo("https://www.google.com/search?q=" + intent.concernedObject)
                    {
                        UseShellExecute = true,
                        Verb = "open"
                    };
                    Process.Start(ps);
                    break;
            }
        }
    }
}
