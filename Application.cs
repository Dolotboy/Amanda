using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amanda
{
    public class Application
    {
        public string Name { get; set; }
        public string ExecutablePath { get; set; }
        public List<string> Nicknames { get; set; }

        public Application(string name)
        {
            this.Name = name;
        }

        public Application(string name, string executablePath, List<string> nicknames)
        {
            Name = name;
            ExecutablePath = executablePath;
            Nicknames = nicknames;
        }
    }
}
