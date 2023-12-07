using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

public class Tokenizer
{
    private Dictionary<int, List<string>> tokenDictionary;
    private Dictionary<string, int> wordsIndex;

    private int sequenceLength = 100;
    string[] wordsIndexPath = { @"c:\Amanda" };
    string wordIndexFileName = "wordsIndex";
    string wordIndexFileExtension = ".json";

    public Tokenizer()
    {
        // Initialisez les dictionnaires à partir des données sauvegardées ou créez de nouveaux dictionnaires vides
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
            }
        }

        // Sauvegardez les dictionnaires
        SaveWordsIndex(wordsIndex);

        // Retournez la liste de tokens
        return inputs.SelectMany(input => input.Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries).Select(word => word.ToLower())).ToList();
    }

    public List<int[]> CreateTextSequences(List<string> sentences)
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
                        Tokenize(new List<string> { word });
                        //wordsIndex[word] = wordsIndex.Count + 1; // Index commence à 1, 0 peut être utilisé pour les mots non présents dans l'index
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

    private void SaveWordsIndex(Dictionary<string, int> data)
    {
        string wordsIndexFullPath = Path.Combine(wordsIndexPath);
        try
        {
            if (!Directory.Exists(wordsIndexFullPath))
            {
                // Try to create the directory.
                DirectoryInfo di = Directory.CreateDirectory(wordsIndexFullPath);
            }
        }
        catch (IOException ioex)
        {
            Console.WriteLine(ioex.Message);
        }

        string wordIndexFileFullPath = Path.Combine(wordsIndexFullPath, Path.GetFileName(wordIndexFileName + wordIndexFileExtension));

        File.WriteAllText(wordIndexFileFullPath, JsonSerializer.Serialize(data));
    }

    private Dictionary<string, int> LoadWordsIndex()
    {
        string wordsIndexFullPath = Path.Combine(wordsIndexPath);
        string wordIndexFileFullPath = Path.Combine(wordsIndexFullPath, Path.GetFileName(wordIndexFileName + wordIndexFileExtension));

        if (File.Exists(wordIndexFileFullPath))
        {
            string json = File.ReadAllText(wordIndexFileFullPath);
            wordsIndex = JsonConvert.DeserializeObject<Dictionary<string, int>>(json);
        }
        else
        {
            wordsIndex = new Dictionary<string, int>();
        }

        return wordsIndex;
    }
}
