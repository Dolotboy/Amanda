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
                    string executablePath = GetValueFromRegistry(subKey, "DisplayIcon");
                    if (executablePath.EndsWith(".ico") || String.IsNullOrEmpty(executablePath))
                    {
                        executablePath = GetValueFromRegistry(subKey, "UninstallString");
                        if(executablePath.ToLower().Contains("uninstall"))
                        {
                            executablePath = "";
                        }
                    }
                    string uninstallPath = GetValueFromRegistry(subKey, "DisplayIcon");
                    if (uninstallPath.ToString().EndsWith(".ico") || String.IsNullOrEmpty(executablePath))
                    {
                        uninstallPath = GetValueFromRegistry(subKey, "UninstallString");
                    }

                    if (displayName != null && executablePath != null)
                    {
                        List<string> nicknames = GetApplicationNicknames(displayName.ToString());

                        string cleanedExecutePath = CleanExecutablePath(executablePath.ToString());
                        string cleanedUninstallPath = CleanExecutablePath(uninstallPath.ToString());

                        Application app = new Application(displayName.ToString(), cleanedExecutePath, cleanedUninstallPath, nicknames);
                        applications.Add(app);
                    }
                }
            }

            return applications;
        }

        private static string GetValueFromRegistry(RegistryKey subKey, string property)
        {
            object value = subKey.GetValue(property);

            return value != null ? value.ToString() : string.Empty;
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
            var inputWords = targetApp.Split(new[] { ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

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
            if (bestMatch != null && bestMatch.MatchPercentage != 0)
            {
                return bestMatch.Application;
            }

            // Si aucun seuil n'est atteint, retournez une valeur par défaut
            return new Application("UnknownApplication");
        }

        public static int CalculateMatchPercentage(string[] words, Application app)
        {
            var appFullName = app.Name.ToLower();
            var appNicknames = app.Nicknames.Select(n => n.ToLower());
            var allWords = new HashSet<string>(new[] { appFullName }.Concat(appNicknames));

            var matchingLettersCount = 0;
            var extraLettersCount = 0;

            foreach (var word in words)
            {
                // Vérifiez si le mot complet correspond à une partie du nom de l'application
                if (allWords.Any(w => w.Contains(word.ToLower()) || appFullName.Contains(word.ToLower())))
                {
                    // Calculez le nombre de lettres correspondantes
                    matchingLettersCount += word.Length;

                    // Calculez le nombre de lettres en trop
                    extraLettersCount += Math.Max(0, appFullName.Length - word.Length);
                }
            }

            // Ajustez le pourcentage en fonction du nombre de lettres en trop
            var adjustedMatchPercentage = (int)Math.Round((double)matchingLettersCount / (matchingLettersCount + extraLettersCount) * 100);

            return adjustedMatchPercentage;
        }

    }
}
