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
            }
        }

        private async void TreatPlayIntent()
        {
            foreach(WitEntity entity in currentQuery.entities)
            {
                switch (entity.type)
                {
                    case "Applications:Applications":
                        OpenApp(entity.value);
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

        private async void OpenApp(string appName)
        {
            List<Application> installedApplications = new List<Application>();
            installedApplications = ApplicationExtractor.GetInstalledApplications();
            installedApplications = installedApplications.OrderBy(app => app.Name).ToList();

            Application app = ApplicationExtractor.ExtractApplication(installedApplications, appName);

            Console.WriteLine("App trouvé: " + app.Name + " Chemin d'exec: " + app.ExecutablePath + " Chemin uninstall: " + app.UninstallPath);
            if (app.ExecutablePath != null)
            {
                Console.WriteLine("Ouverture de l'application: " + app.Name);
                try
                {
                    Process.Start(app.ExecutablePath);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private async void Update()
        {

        }
    }
}
