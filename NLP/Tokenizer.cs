using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

public class Tokenizer
{
    private Dictionary<string, int> wordsIndex;
    private List<int[]> sequencesIndex = new List<int[]>();

    private int sequenceLength = 100;
    string[] amandaPath = { @"c:\Amanda" };
    string wordsIndexFileName = "wordsIndex";
    string sequencesIndexFileName = "sequencesIndex";
    string indexFileExtension = ".json";

    public Tokenizer()
    {
        // Initialisez les dictionnaires à partir des données sauvegardées ou créez de nouveaux dictionnaires vides
        wordsIndex = LoadWordsIndex() ?? new Dictionary<string, int>();
        sequencesIndex = LoadSequencesIndex() ?? new List<int[]>();
    }

    public void LearnWordsFromStringList(List<string> words)
    {
        foreach (string word in words)
        {
            Tokenize(new List<string> { word });
        }
    }

    public void LearnWordsFromJsonFile(string path)
    {
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            List<string> words = JsonConvert.DeserializeObject<List<string>>(json);

            foreach (string word in words)
            {
                Tokenize(new List<string> { word });
            }
        }
        else
        {
            Console.WriteLine("Error, file doesn't exist");
        }
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

        // Sauvegardez le dictionnaire
        SaveWordsIndex(wordsIndex);

        // Retournez la liste de tokens
        return inputs.SelectMany(input => input.Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries).Select(word => word.ToLower())).ToList();
    }

    public List<int[]> CreateTextSequences(List<string> sentences, bool learn)
    {
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
                    if (learn && !wordsIndex.ContainsKey(word))
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

            // If not, add the sequence to the list
            sequences.Add(sequence);
            if (!sequencesIndex.Any(existingSequence => existingSequence.SequenceEqual(sequence)))
            {
                sequencesIndex.Add(sequence);
            }
        }

        SaveSequencesIndex(sequencesIndex);

        return sequences;
    }

    public Dictionary<string, int> GetWordsIndex()
    {
        // Retourne l'index des mots
        return wordsIndex;
    }

    private void SaveWordsIndex(Dictionary<string, int> data)
    {
        string wordsIndexFullPath = Path.Combine(amandaPath);
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

        string wordsIndexFileFullPath = Path.Combine(wordsIndexFullPath, Path.GetFileName(wordsIndexFileName + indexFileExtension));

        File.WriteAllText(wordsIndexFileFullPath, JsonSerializer.Serialize(data));
    }

    private Dictionary<string, int> LoadWordsIndex()
    {
        string wordsIndexFullPath = Path.Combine(amandaPath);
        string wordsIndexFileFullPath = Path.Combine(wordsIndexFullPath, Path.GetFileName(wordsIndexFileName + indexFileExtension));

        if (File.Exists(wordsIndexFileFullPath))
        {
            string json = File.ReadAllText(wordsIndexFileFullPath);
            wordsIndex = JsonConvert.DeserializeObject<Dictionary<string, int>>(json);
        }
        else
        {
            wordsIndex = new Dictionary<string, int>();
        }

        return wordsIndex;
    }

    private void SaveSequencesIndex(List<int[]> data)
    {
        string sequencesIndexFullPath = Path.Combine(amandaPath);
        try
        {
            if (!Directory.Exists(sequencesIndexFullPath))
            {
                // Try to create the directory.
                DirectoryInfo di = Directory.CreateDirectory(sequencesIndexFullPath);
            }
        }
        catch (IOException ioex)
        {
            Console.WriteLine(ioex.Message);
        }

        string sequencesIndexFileFullPath = Path.Combine(sequencesIndexFullPath, Path.GetFileName(sequencesIndexFileName + indexFileExtension));

        File.WriteAllText(sequencesIndexFileFullPath, JsonSerializer.Serialize(data));
    }

    private List<int[]> LoadSequencesIndex()
    {
        string sequencesIndexFullPath = Path.Combine(amandaPath);
        string sequencesIndexFileFullPath = Path.Combine(sequencesIndexFullPath, Path.GetFileName(sequencesIndexFileName + indexFileExtension));

        if (File.Exists(sequencesIndexFileFullPath))
        {
            string json = File.ReadAllText(sequencesIndexFileFullPath);
            sequencesIndex = JsonConvert.DeserializeObject<List<int[]>>(json);
        }
        else
        {
            sequencesIndex = new List<int[]>();
        }

        return sequencesIndex;
    }
}
