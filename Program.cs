// Example usage
using Amanda;

/*
var intentRecognizer = new IntentRecognizer();

string userInput = "";

//userInput = "Please play my favorite song which is old town road";
//userInput = "Please open streamlabs";
userInput = "Search whales";

Bot.execute(intentRecognizer.RecognizeIntent(userInput));
*/

var tokenizer = new Tokenizer();

// Exemple d'utilisation
string userInput = "Please play my favorite song!";
List<string> tokens = tokenizer.Tokenize(userInput);

Console.WriteLine("Tokens:");
foreach (var token in tokens)
{
    Console.WriteLine(token);
}

Console.WriteLine("\nToken Dictionary:");
var tokenDictionary = tokenizer.GetTokenDictionary();
foreach (var entry in tokenDictionary)
{
    Console.WriteLine($"Position {entry.Key}: {string.Join(", ", entry.Value)}");
}

// Exemple pour CreateTextSequences avec rembourrage jusqu'à la longueur spécifiée
string inputSentence = "Please play my favorite song!";
var sequences = tokenizer.CreateTextSequences(inputSentence, 10);

Console.WriteLine("\nText Sequences:");
foreach (var sequence in sequences)
{
    Console.WriteLine($"[{string.Join(", ", sequence)}]");
}

Console.WriteLine("\nWords Index:");
var wordsIndex = tokenizer.GetWordsIndex();
foreach (var entry in wordsIndex)
{
    Console.WriteLine($"{entry.Key}: {entry.Value}");
}
