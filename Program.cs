// Example usage
using Amanda;

string userInput = "";

//userInput = "Please play my favorite song which is old town road";
//userInput = "Please open streamlabs";
//userInput = "Search whales";
while(true)
{
    userInput = "Ouvrir Outlook";
    Console.WriteLine("Que puis-je pour vous ?\n");
    userInput = Console.ReadLine();

    Bot bot = new Bot();

    bot.Execute(userInput);
}


