// Example usage
using Amanda;


var intentRecognizer = new IntentRecognizer();

string userInput = "";

//userInput = "Please play my favorite song which is old town road";
//userInput = "Please open streamlabs";
userInput = "Search whales";

Bot.execute(intentRecognizer.RecognizeIntent(userInput));


void trainAI()
{
    tokenizer.LearnWordsFromJsonFile(@"E:\Programmation\Apps\Amanda\Trainings\words1.json");
    tokenizer.LearnWordsFromJsonFile(@"E:\Programmation\Apps\Amanda\Trainings\words2.json");
    tokenizer.LearnWordsFromJsonFile(@"E:\Programmation\Apps\Amanda\Trainings\words3.json");
    tokenizer.LearnWordsFromJsonFile(@"E:\Programmation\Apps\Amanda\Trainings\words4.json");
    tokenizer.LearnSentencesWithIntentFromJson(@"E:\Programmation\Apps\Amanda\Trainings\OpenApplicationIntents.json", IntentType.OpenApplication);
    tokenizer.LearnSentencesWithIntentFromJson(@"E:\Programmation\Apps\Amanda\Trainings\PlayMusicIntents.json", IntentType.PlayMusic);
    tokenizer.LearnSentencesWithIntentFromJson(@"E:\Programmation\Apps\Amanda\Trainings\PlayMovieIntents.json", IntentType.PlayMovie);
    tokenizer.LearnSentencesWithIntentFromJson(@"E:\Programmation\Apps\Amanda\Trainings\SearchIntents.json", IntentType.Search);
}
