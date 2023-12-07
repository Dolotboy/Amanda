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

Tokenizer tokenizer = new Tokenizer();

List<string> sentences = new List<string>
        {
            "I love programming",
            "Natural Language Processing is fascinating",
            "Hello, how are you?",
            "The quick brown fox jumps over the lazy dog",
            "Coding is an art"
        };

List<int[]> sequences = tokenizer.CreateTextSequences(sentences);

Console.WriteLine("Words Index:");
foreach (var entry in tokenizer.GetWordsIndex())
{
    Console.WriteLine($"{entry.Key}: {entry.Value}");
}

Console.WriteLine("\nSequences:");
foreach (var sequence in sequences)
{
    Console.WriteLine(string.Join(", ", sequence));
}
