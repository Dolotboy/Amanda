using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class Tokenizer
{
    private Dictionary<int, List<string>> tokenDictionary;
    private Dictionary<string, int> wordsIndex;

    public Tokenizer()
    {
        // Initialisez les dictionnaires à partir des données sauvegardées ou créez de nouveaux dictionnaires vides
        tokenDictionary = LoadTokenDictionary() ?? new Dictionary<int, List<string>>();
        wordsIndex = LoadWordsIndex() ?? new Dictionary<string, int>();
    }

    public List<string> Tokenize(List<string> inputs)
    {
        foreach (var input in inputs)
        {
            // Divisez la phrase en mots (tokens) en prenant en compte les ponctuations
            string[] words = input.Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

            // Ajoutez les tokens au dictionnaire avec leur position dans la phrase
            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i].ToLower(); // Convertir en minuscules pour éviter la sensibilité à la casse

                // Ajouter le mot à l'index s'il n'est pas déjà présent
                if (!wordsIndex.ContainsKey(word))
                {
                    wordsIndex[word] = wordsIndex.Count + 1; // Index commence à 1, 0 peut être utilisé pour les mots non présents dans l'index
                }

                // Ajouter l'index du mot au dictionnaire des tokens
                if (!tokenDictionary.ContainsKey(i))
                {
                    tokenDictionary[i] = new List<string>();
                }

                tokenDictionary[i].Add(word);
            }
        }

        // Sauvegardez les dictionnaires
        SaveTokenDictionary(tokenDictionary);
        SaveWordsIndex(wordsIndex);

        // Retournez la liste de tokens
        return inputs.SelectMany(input => input.Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries).Select(word => word.ToLower())).ToList();
    }

    public List<int[]> CreateTextSequences(List<string> sentences, int sequenceLength)
    {
        // Crée des séquences de texte à partir des tokens avec un rembourrage jusqu'à la longueur spécifiée
        List<int[]> sequences = new List<int[]>();

        foreach (var sentence in sentences)
        {
            string[] words = sentence.Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

            int[] sequence = new int[sequenceLength];

            for (int i = 0; i < sequenceLength; i++)
            {
                if (i < words.Length)
                {
                    string word = words[i].ToLower();

                    // Ajouter le mot à l'index s'il n'est pas déjà présent
                    if (!wordsIndex.ContainsKey(word))
                    {
                        wordsIndex[word] = wordsIndex.Count + 1; // Index commence à 1, 0 peut être utilisé pour les mots non présents dans l'index
                    }

                    int index = wordsIndex[word];
                    sequence[i] = index;
                }
                else
                {
                    // Remplir avec des zéros si la séquence est plus courte que la longueur spécifiée
                    sequence[i] = 0;
                }
            }

            sequences.Add(sequence);
        }

        return sequences;
    }

    public Dictionary<string, int> GetWordsIndex()
    {
        // Retourne l'index des mots
        return wordsIndex;
    }

    private Dictionary<int, List<string>> LoadTokenDictionary()
    {
        // Implémentez la logique pour charger le dictionnaire de tokens depuis le stockage persistant (fichier, base de données, etc.)
        // Retournez null s'il n'y a pas de données sauvegardées
        // Exemple : return LoadDataFromFile<Dictionary<int, List<string>>>("tokenDictionary.json");
        return null;
    }

    private void SaveTokenDictionary(Dictionary<int, List<string>> data)
    {
        // Implémentez la logique pour sauvegarder le dictionnaire de tokens dans le stockage persistant
        // Exemple : SaveDataToFile("tokenDictionary.json", data);
    }

    private Dictionary<string, int> LoadWordsIndex()
    {
        // Implémentez la logique pour charger l'index des mots depuis le stockage persistant
        // Retournez null s'il n'y a pas de données sauvegardées
        // Exemple : return LoadDataFromFile<Dictionary<string, int>>("wordsIndex.json");
        return null;
    }

    private void SaveWordsIndex(Dictionary<string, int> data)
    {
        // Implémentez la logique pour sauvegarder l'index des mots dans le stockage persistant
        // Exemple : SaveDataToFile("wordsIndex.json", data);
    }

    // Ajoutez d'autres méthodes de chargement et de sauvegarde si nécessaire

    // Exemple générique de méthode de chargement depuis un fichier
    // private T LoadDataFromFile<T>(string filePath)
    // {
    //     if (File.Exists(filePath))
    //     {
    //         string json = File.ReadAllText(filePath);
    //         return JsonConvert.DeserializeObject<T>(json);
    //     }
    //     return default(T);
    // }

    // Exemple générique de méthode de sauvegarde dans un fichier
    // private void SaveDataToFile<T>(string filePath, T data)
    // {
    //     string json = JsonConvert.SerializeObject(data);
    //     File.WriteAllText(filePath, json);
    // }
}
