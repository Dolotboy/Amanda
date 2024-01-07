// Example usage
using Amanda;

trainAI();

var intentRecognizer = new IntentRecognizer();

string userInput = "";

userInput = "Please play my favorite song which is old town road";
//userInput = "Please open streamlabs";
//userInput = "Search whales";

Bot.execute(intentRecognizer.RecognizeIntent(userInput));


void trainAI()
{
    Tokenizer tokenizer = new Tokenizer();
    tokenizer.LearnWordsFromJsonFile(@"E:\Programmation\Apps\Amanda\Trainings\words1.json");
    tokenizer.LearnWordsFromJsonFile(@"E:\Programmation\Apps\Amanda\Trainings\words2.json");
    tokenizer.LearnWordsFromJsonFile(@"E:\Programmation\Apps\Amanda\Trainings\words3.json");
    tokenizer.LearnWordsFromJsonFile(@"E:\Programmation\Apps\Amanda\Trainings\words4.json");
    // https://raw.githubusercontent.com/dwyl/english-words/master/words_dictionary.json
}
