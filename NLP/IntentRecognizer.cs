using System;
using System.Collections.Generic;
using System.Linq;
using FuzzyString;

namespace Amanda
{
    public class IntentRecognizer
    {
        private readonly Dictionary<string, IntentType> intentKeywords;
        private readonly List<Application> installedApplications = new List<Application>();

        private Intent intent;

        public IntentRecognizer()
        {
            // Définissez vos mots clés d'intention et les intentions associées
            intentKeywords = new Dictionary<string, IntentType>
            {
                { "open", IntentType.OpenApplication },
                { "play music", IntentType.PlayMusic },
                { "play song", IntentType.PlayMusic },
                { "play movie", IntentType.PlayMovie },
                { "search", IntentType.Search }, // Assurez-vous que le nom correspond à votre énumération
                // Ajoutez d'autres mots clés au besoin
            };

            // Récupérez la liste des applications installées avec leurs informations
            installedApplications = ApplicationExtractor.GetInstalledApplications();
            installedApplications = installedApplications.OrderBy(app => app.Name).ToList();
        }

        public Intent RecognizeIntent(string userInput)
        {
            // Convertissez l'entrée utilisateur en minuscules pour une correspondance insensible à la casse
            userInput = userInput.ToLower();

            // Vérifiez chaque mot clé d'intention dans l'entrée utilisateur
            foreach (var keyword in intentKeywords.Keys)
            {
                // Vérifiez si chaque mot-clé d'intention est présent dans l'ordre spécifié
                bool allKeywordsPresent = true;
                int currentIndex = 0;

                foreach (var word in keyword.Split(' '))
                {
                    currentIndex = userInput.IndexOf(word, currentIndex);

                    if (currentIndex == -1)
                    {
                        allKeywordsPresent = false;
                        break;
                    }

                    currentIndex += word.Length;
                }

                if (allKeywordsPresent)
                {
                    intent = new Intent(userInput, intentKeywords[keyword]);

                    switch(intentKeywords[keyword])
                    {
                        case IntentType.OpenApplication:
                            intent.application = ExtractApplication(userInput);
                            break;
                        case IntentType.PlayMusic:
                            intent.concernedObject = ExtractConcernedObject(intent);
                            break;
                        case IntentType.PlayMovie:
                            intent.concernedObject = ExtractConcernedObject(intent);
                            break;
                        case IntentType.Search:
                            intent.concernedObject = ExtractConcernedObject(intent);
                            break;
                    }

                    return intent;

                }
            }

            // Retournez une intention par défaut ou gérez l'entrée non reconnue au besoin
            return new Intent(userInput, IntentType.Unknown);
        }

        public string ExtractConcernedObject(Intent intent)
        {
            // Vérifiez si la demande contient un mot clé lié à l'objet
            string[] objectKeywords = { "music", "movie", "search", "song" };

            foreach (var keyword in objectKeywords)
            {
                int keywordIndex = intent.userInput.IndexOf(keyword);
                if (keywordIndex != -1)
                {
                    // L'objet est la partie de la phrase après le mot clé
                    string objectPhrase = intent.userInput.Substring(keywordIndex + keyword.Length).Trim();
                    return objectPhrase;
                }
            }

            // Ajustez le comportement par défaut selon vos besoins
            return null;
        }


        private Application ExtractApplication(string userInput)
        {
            // Convertissez l'entrée utilisateur en minuscules pour une correspondance insensible à la casse
            userInput = userInput.ToLower();

            // Divisez l'entrée utilisateur en mots
            var inputWords = userInput.Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

            // Recherchez chaque application installée dans l'entrée utilisateur
            var matchingApplications = installedApplications
                .Select(app => new
                {
                    Application = app,
                    MatchPercentage = CalculateMatchPercentage(inputWords, app)
                })
                .OrderByDescending(match => match.MatchPercentage)
                .ToList();

            // Obtenez l'application avec le pourcentage de correspondance le plus élevé
            var bestMatch = matchingApplications.FirstOrDefault();

            // Vous pouvez définir un seuil de pourcentage minimum pour considérer une correspondance
            //if (bestMatch != null && bestMatch.MatchPercentage > 50)
            if (bestMatch != null)
            {
                return bestMatch.Application;
            }

            // Si aucun seuil n'est atteint, retournez une valeur par défaut
            return new Application("UnknownApplication");
        }

        private int CalculateMatchPercentage(string[] words, Application app)
        {
            // Convertissez le nom de l'application et ses surnoms en une seule chaîne
            var appFullName = app.Name.ToLower();
            var appNicknames = app.Nicknames.Select(n => n.ToLower());
            var allWords = new HashSet<string>(new[] { appFullName }.Concat(appNicknames));

            // Initialisez le compteur de lettres correspondantes
            var matchingLettersCount = 0;

            // Initialisez le compteur de sous-chaînes consécutives
            var consecutiveSubstringCount = 0;

            foreach (var word in words)
            {
                var appSubstringIndex = appFullName.IndexOf(word);

                while (appSubstringIndex != -1)
                {
                    // Incrémentez le compteur de sous-chaînes consécutives
                    consecutiveSubstringCount++;

                    // Incrémentez le compteur de lettres correspondantes
                    matchingLettersCount += word.Length;

                    // Recherchez la prochaine occurrence de la sous-chaîne
                    appSubstringIndex = appFullName.IndexOf(word, appSubstringIndex + 1);
                }
            }

            // Calculez le pourcentage de correspondance en fonction des lettres correspondantes et des sous-chaînes consécutives
            var totalCharactersCount = words.Sum(word => Math.Max(word.Length, 1)); // Utilisez Math.Max pour éviter une division par zéro
            var matchPercentage = (int)Math.Round((double)(matchingLettersCount + consecutiveSubstringCount) / totalCharactersCount * 100);

            return matchPercentage;
        }

    }
}
