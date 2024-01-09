using System;
using System.Collections.Generic;
using Microsoft.Win32;

namespace Amanda
{
    public class ApplicationExtractor
    {
        public static List<Application> GetInstalledApplications()
        {
            List<Application> installedApplications = new List<Application>();

            // Clé de registre pour les programmes installés sur un système 64 bits
            RegistryKey key64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            RegistryKey uninstallKey64 = key64.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");

            // Clé de registre pour les programmes installés sur un système 32 bits
            RegistryKey key32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            RegistryKey uninstallKey32 = key32.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");

            // Combinez les noms de tous les programmes des deux clés
            if (uninstallKey64 != null)
            {
                installedApplications.AddRange(GetApplicationsFromRegistry(uninstallKey64));
                uninstallKey64.Close();
            }

            if (uninstallKey32 != null)
            {
                installedApplications.AddRange(GetApplicationsFromRegistry(uninstallKey32));
                uninstallKey32.Close();
            }

            // Ajoutez d'autres clés du registre selon vos besoins
            // Par exemple, vous pouvez explorer HKEY_LOCAL_MACHINE\SOFTWARE\ et d'autres sous-clés

            return installedApplications;
        }

        private static List<Application> GetApplicationsFromRegistry(RegistryKey key)
        {
            List<Application> applications = new List<Application>();

            foreach (string subKeyName in key.GetSubKeyNames())
            {
                using (RegistryKey subKey = key.OpenSubKey(subKeyName))
                {
                    // Obtenez le nom de l'application et le chemin de l'exécutable
                    object displayName = subKey.GetValue("DisplayName");
                    object executablePath = subKey.GetValue("DisplayIcon") ?? subKey.GetValue("UninstallString");

                    if (displayName != null && executablePath != null)
                    {
                        List<string> nicknames = GetApplicationNicknames(displayName.ToString());

                        string cleanedPath = CleanExecutablePath(executablePath.ToString());

                        Application app = new Application(displayName.ToString(), cleanedPath, nicknames);
                        applications.Add(app);
                    }
                }
            }

            return applications;
        }

        private static List<string> GetApplicationNicknames(string displayName)
        {
            // Logique pour générer des surnoms possibles à partir du nom de l'application
            // Cette logique doit être adaptée en fonction de vos besoins spécifiques.
            // Vous pouvez envisager de décomposer le nom en mots et de créer des variantes.

            List<string> nicknames = new List<string>();

            // Ajoutez le nom original comme surnom
            nicknames.Add(displayName);

            // Ajoutez des variantes potentielles basées sur des règles spécifiques
            // ...

            return nicknames;
        }

        private static string CleanExecutablePath(string executablePath)
        {
            // Supprimez les caractères indésirables à la fin du chemin d'accès
            executablePath = executablePath.TrimEnd(',', ' ', '0');

            return executablePath;
        }

        public static Application ExtractApplication(List<Application> installedApplications, string targetApp)
        {
            // Divisez l'entrée utilisateur en mots
            var inputWords = targetApp.Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

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

        public static int CalculateMatchPercentage(string[] words, Application app)
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
