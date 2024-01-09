using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.CognitiveServices.Speech.Transcription;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using System.Management.Automation;

namespace Amanda
{
    public class Bot
    {
        public List<WitQuery> queries;
        public WitQuery currentQuery;

        public Bot()
        {
            queries = new List<WitQuery>();
        }

        public async void Execute(string userInput)
        {
            var url = "https://api.wit.ai/message?v=20240108&q=" + userInput;

            var httpRequest = (HttpWebRequest)WebRequest.Create(url);

            httpRequest.Accept = "application/json";
            httpRequest.Headers["Authorization"] = "Bearer KRX3H5YNCW26BCBLVEA5TAXUXZA5KKFJ";


            var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                GetQuery(streamReader);
            }

            Console.WriteLine(currentQuery.ToString());
            Console.Write(httpResponse.StatusCode + "\n");

            TreatCurrentQuery();
        }

        public async void GetQuery(StreamReader streamReader)
        {
            var result = JsonConvert.DeserializeObject<JObject>(streamReader.ReadToEnd());

            currentQuery = new WitQuery(result);

            var intentsArray = result["intents"] as JArray;
            if (intentsArray != null)
            {
                foreach (var item in intentsArray)
                {
                    var intentName = item["name"];

                    // Vérifiez si la propriété "name" existe et est une chaîne
                    if (intentName != null && intentName.Type == JTokenType.String)
                    {
                        // Attribuez la valeur à currentQuery.intent
                        currentQuery.intent = intentName.ToString();
                    }
                }
            }

            var traitsObject = result["traits"] as JObject;
            if (traitsObject != null)
            {
                foreach (var traitProperty in traitsObject.Properties())
                {
                    var traitArray = traitProperty.Value as JArray;

                    if (traitArray != null && traitArray.Count > 0)
                    {
                        var traitValue = traitArray[0]["value"];

                        // Vérifiez si la propriété "value" existe et est une chaîne
                        if (traitValue != null && traitValue.Type == JTokenType.String)
                        {
                            // Attribuez la valeur à currentQuery.trait
                            currentQuery.trait = traitValue.ToString();
                        }
                    }
                }
            }

            foreach (var entityProperty in result["entities"].Children<JProperty>())
            {
                var entityName = entityProperty.Name; // Nom de l'entité
                var entityValue = entityProperty.Value; // Valeur de l'entité

                // Si la valeur est un tableau, vous pouvez le parcourir de la même manière
                if (entityValue is JArray entityArray)
                {
                    foreach (var item in entityArray)
                    {
                        currentQuery.entities.Add(new WitEntity(Convert.ToString(item["value"]), entityName));
                    }
                }
            }

            queries.Add(currentQuery);
        }
    
        private async void TreatCurrentQuery()
        {
            switch(currentQuery.intent)
            {
                case "play":
                    TreatPlayIntent();
                    break;
                case "play_music":
                    TreatPlayIntent();
                    break;
                case "play_movie":
                    TreatPlayIntent();
                    break;
                case "play_video":
                    TreatPlayIntent();
                    break;
                case "play_last_video":
                    TreatPlayIntent();
                    break;
                case "play_site":
                    TreatPlayIntent();
                    break;
                case "update":
                    Update();
                    break;
                case "update_computer":
                    Update();
                    break;
                case "current_time":
                    TreatTime();
                    break;
            }
        }

        private async void TreatPlayIntent()
        {
            foreach(WitEntity entity in currentQuery.entities)
            {
                switch (entity.type)
                {
                    case "Applications:Applications":
                        if(currentQuery.trait == "open")
                        {
                            ManageApp(entity.value, true);
                        }
                        else
                        {
                            ManageApp(entity.value, false);
                        }
                        break;
                    case "Musics:Musics":
                        Console.WriteLine("Lancement de la musique");
                        break;
                    case "Movies:Movies":
                        Console.WriteLine("Lancement du film");
                        break;
                    case "Sites:Sites":
                        Console.WriteLine("Ouverture du site");
                        break;
                }
            }
        }

        private async void ManageApp(string appName, bool open)
        {
            List<Application> installedApplications = new List<Application>();
            installedApplications = ApplicationExtractor.GetInstalledApplications();
            installedApplications = installedApplications.OrderBy(app => app.Name).ToList();

            Application app = ApplicationExtractor.ExtractApplication(installedApplications, appName);

            Console.WriteLine("App trouvé: " + app.Name + " Chemin d'exec: " + app.ExecutablePath + " Chemin uninstall: " + app.UninstallPath);
            if (app.ExecutablePath != null)
            {
                try
                {
                    if(open)
                    {
                        Console.WriteLine("Ouverture de l'application: " + app.Name);
                        Process.Start(app.ExecutablePath);
                    }
                    else
                    {
                        Console.WriteLine("Fermeture de l'application: " + app.Name);
                        foreach (Process process in Process.GetProcessesByName(appName))
                        {
                            process.Kill();
                        }
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private async void Update()
        {
            // Installe le module PSWindowsUpdate
            ExecutePowerShellCommand("Install-Module -Name PSWindowsUpdate -Force -AllowClobber");

            // Obtient la liste des stratégies d'exécution
            ExecutePowerShellCommand("Get-ExecutionPolicy -List");

            // Définit la stratégie d'exécution à RemoteSigned
            ExecutePowerShellCommand("Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned");

            // Recherche et installe les mises à jour Windows avec redémarrage automatique
            ExecutePowerShellCommand("Get-WindowsUpdate -AcceptAll -Install -AutoReboot");
        }

        private async void ExecutePowerShellCommand(string command)
        {
            using (PowerShell PowerShellInstance = PowerShell.Create())
            {
                // Utilise l'option AddScript pour ajouter la commande PowerShell
                PowerShellInstance.AddScript(command);

                // Exécute la commande
                Collection<PSObject> PSOutput = PowerShellInstance.Invoke();

                // Affiche la sortie de la commande
                foreach (PSObject outputItem in PSOutput)
                {
                    if (outputItem != null)
                    {
                        // Affiche la sortie sous forme de chaîne
                        Console.WriteLine(outputItem.BaseObject.ToString());
                    }
                }

                // Affiche les erreurs, le cas échéant
                if (PowerShellInstance.Streams.Error.Count > 0)
                {
                    foreach (ErrorRecord error in PowerShellInstance.Streams.Error)
                    {
                        Console.WriteLine("Erreur : " + error.Exception.Message);
                    }
                }
            }
        }

        private async void TreatTime()
        {
            switch(currentQuery.trait)
            {
                case "datetime":
                    Console.WriteLine(DateTime.Now.ToString());
                    break;
                case "time":
                    Console.WriteLine(DateTime.Now.ToString("h:mm:ss tt"));
                    break;
                case "date":
                    Console.WriteLine(DateOnly.FromDateTime(DateTime.Now).ToString());
                    break;
            }
        }
    }
}
