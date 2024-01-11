// Example usage
using Amanda;

string userInput = "";

while(true)
{
    Console.WriteLine("Que puis-je pour vous ?\n");
    userInput = Console.ReadLine();

    Bot bot = new Bot();

    bot.Execute(userInput);
}


