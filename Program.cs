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
trainAI();
/*
tokenizer.LearnWordsFromJsonFile(@"E:\Programmation\Apps\Amanda\Trainings\words1.json");
tokenizer.LearnWordsFromJsonFile(@"E:\Programmation\Apps\Amanda\Trainings\words2.json");
tokenizer.LearnWordsFromJsonFile(@"E:\Programmation\Apps\Amanda\Trainings\words3.json");
tokenizer.LearnWordsFromJsonFile(@"E:\Programmation\Apps\Amanda\Trainings\words4.json");
*/

/*
List<string> sentences = new List<string>
        {
            "I love bazooka",
            "I love programming with hoodies",
            "Natural Language Processing is fascinating",
            "Hello, how are you?",
            "The quick brown fox jumps over the lazy dog",
            "Coding is an art"
        };

List<int[]> sequences = tokenizer.CreateTextSequences(sentences, false);

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
*/
void trainAI()
{
    tokenizer.LearnWordsFromJsonFile(@"E:\Programmation\Apps\Amanda\Trainings\words1.json");
    tokenizer.LearnWordsFromJsonFile(@"E:\Programmation\Apps\Amanda\Trainings\words2.json");
    tokenizer.LearnWordsFromJsonFile(@"E:\Programmation\Apps\Amanda\Trainings\words3.json");
    tokenizer.LearnWordsFromJsonFile(@"E:\Programmation\Apps\Amanda\Trainings\words4.json");
    tokenizer.LearnSentencesWithIntentFromJson(@"E:\Programmation\Apps\Amanda\Trainings\PlayMusicIntents.json", IntentType.PlayMusic);
    tokenizer.LearnSentencesWithIntentFromJson(@"E:\Programmation\Apps\Amanda\Trainings\OpenApplicationIntents.json", IntentType.PlayMusic);
}
