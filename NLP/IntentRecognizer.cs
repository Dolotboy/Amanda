using System;
using System.Collections.Generic;
using System.Linq;
using FuzzyString;

namespace Amanda
{
    public class IntentRecognizer
    {
        private readonly Dictionary<string, string> intentKeywords;
        private readonly List<Application> installedApplications = new List<Application>();
        private readonly Tokenizer tokenizer;  // Ajout de la référence à Tokenizer

        private Intent intent;

        public IntentRecognizer()
        {
            installedApplications = ApplicationExtractor.GetInstalledApplications();
            installedApplications = installedApplications.OrderBy(app => app.Name).ToList();

            // Initialisation de l'instance de Tokenizer
            tokenizer = new Tokenizer();
        }

        public Intent RecognizeIntent(string userInput)
        {
            // Utilisation de Tokenizer pour obtenir les séquences de tokens
            var sequences = tokenizer.CreateTextSequences(new List<string> { userInput }, false); // Utilisation de la fonction pour charger les séquences sans apprentissage

            // Calcul du taux de ressemblance pour chaque séquence
            var matchingSequences = sequences
                .Select(seq => new
                {
                    Sequence = seq,
                    MatchPercentage = CalculateSequenceMatchPercentage(seq)
                })
                .OrderByDescending(match => match.MatchPercentage)
                .ToList();

            // Obtention de la séquence avec le taux de ressemblance le plus élevé
            var bestMatchSequence = matchingSequences.FirstOrDefault();

            if (bestMatchSequence != null)
            {
                string bestMatchText = tokenizer.SequenceToText(bestMatchSequence.Sequence);

                // Obtention de l'intent à partir de la séquence
                //intent = new Intent(userInput, tokenizer.sequencesIntentsIndex[bestMatchSequence.Sequence]);
                
                /*
                switch (sequencesIntentsIndex[bestMatchSequence.Sequence])
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
                }*/

                return intent;
            }

            // Retournez une intention par défaut ou gérez l'entrée non reconnue au besoin
            return new Intent(userInput, "Unknown");
        }

        private int CalculateSequenceMatchPercentage(int[] sequence)
        {
            // Convertissez la séquence en un ensemble pour la similarité de Jaccard
            var inputSet = new HashSet<int>(sequence);

            int bestMatchPercentage = 0;

            foreach (var existingSequence in tokenizer.sequencesIndex)
            {
                // Convertissez la séquence existante en un ensemble
                var existingSet = new HashSet<int>(existingSequence);

                // Calculez l'intersection des ensembles
                var intersection = new HashSet<int>(inputSet);
                intersection.IntersectWith(existingSet);

                // Calculez l'union des ensembles
                var union = new HashSet<int>(inputSet);
                union.UnionWith(existingSet);

                // Calculez la similarité de Jaccard
                int matchPercentage = (int)Math.Round((double)intersection.Count / union.Count * 100);

                // Mettez à jour le meilleur pourcentage de correspondance
                if (matchPercentage > bestMatchPercentage)
                {
                    bestMatchPercentage = matchPercentage;
                }
            }

            return bestMatchPercentage;
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
