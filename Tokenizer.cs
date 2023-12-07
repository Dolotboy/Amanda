using System;
using System.Collections.Generic;
using System.Linq;

public class Tokenizer
{
    private Dictionary<int, List<string>> tokenDictionary;
    private Dictionary<string, int> wordsIndex;

    public Tokenizer()
    {
        tokenDictionary = new Dictionary<int, List<string>>();
        wordsIndex = new Dictionary<string, int>();
    }

    public List<string> Tokenize(string input)
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

        // Retournez la liste de tokens
        return words.ToList();
    }

    public Dictionary<int, List<string>> GetTokenDictionary()
    {
        // Retourne le dictionnaire de tokens
        return tokenDictionary;
    }

    public List<int[]> CreateTextSequences(string sentence, int sequenceLength)
    {
        // Crée des séquences de texte à partir des tokens avec un rembourrage jusqu'à la longueur spécifiée
        List<int[]> sequences = new List<int[]>();

        string[] words = sentence.Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var word in words)
        {
            string lowerWord = word.ToLower();

            // Ajouter le mot à l'index s'il n'est pas déjà présent
            if (!wordsIndex.ContainsKey(lowerWord))
            {
                wordsIndex[lowerWord] = wordsIndex.Count + 1; // Index commence à 1, 0 peut être utilisé pour les mots non présents dans l'index
            }
        }

        for (int i = 0; i < sequenceLength; i++)
        {
            int[] sequence = new int[sequenceLength];

            if (i < words.Length)
            {
                string word = words[i].ToLower();
                int index = wordsIndex[word];
                sequence[i] = index;
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
}
