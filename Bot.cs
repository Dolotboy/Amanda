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

namespace Amanda
{
    public class Bot
    {
        public static async void execute(string userInput)
        {
            var url = "https://api.wit.ai/message?v=20240108&q=" + userInput;

            var httpRequest = (HttpWebRequest)WebRequest.Create(url);

            httpRequest.Accept = "application/json";
            httpRequest.Headers["Authorization"] = "Bearer KRX3H5YNCW26BCBLVEA5TAXUXZA5KKFJ";


            var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = JsonConvert.DeserializeObject<JObject>(streamReader.ReadToEnd());

                foreach (var intentProperty in result["intents"].Children<JProperty>())
                {
                    var intentName = intentProperty.Name; // Nom de l'entité
                    var intentValue = intentProperty.Value; // Valeur de l'entité

                    Console.WriteLine($"Entité : {intentName}, Valeur : {intentValue}");

                    // Si la valeur est un tableau, vous pouvez le parcourir de la même manière
                    if (intentValue is JArray entityArray)
                    {
                        foreach (var item in entityArray)
                        {
                            // Faire quelque chose avec chaque élément du tableau
                        }
                    }
                }

                foreach (var entityProperty in result["entities"].Children<JProperty>())
                {
                    var entityName = entityProperty.Name; // Nom de l'entité
                    var entityValue = entityProperty.Value; // Valeur de l'entité

                    Console.WriteLine($"Entité : {entityName}, Valeur : {entityValue}");

                    // Si la valeur est un tableau, vous pouvez le parcourir de la même manière
                    if (entityValue is JArray entityArray)
                    {
                        foreach (var item in entityArray)
                        {
                            // Faire quelque chose avec chaque élément du tableau
                        }
                    }
                }
            }

            //Console.WriteLine(httpResponse.StatusCode);
        }

    }
}
