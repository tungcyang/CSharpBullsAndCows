using System;
using BullsAndCows;

namespace BullsAndCows
{
    class Program : BullsAndCows
    {
        // This game is based on the description in Wikipedia page "Bulls and Cows" in https://en.wikipedia.org/wiki/Bulls_and_Cows.

        static void GamePreamble()
        {
            System.Console.WriteLine("Beginning of the Bulls and Cows game :)\n");
            System.Console.WriteLine("Please think of a secret " + NUM_OF_GUESSING_DIGITS + "-digit number with different digits.\n");
            System.Console.WriteLine("You can start guessing my secret " + NUM_OF_GUESSING_DIGITS + "-digit number.");
            System.Console.WriteLine("Anything else means you like me to guess first.\n");
        }

        static void Main(string[] args)
        {
            BullsAndCows bac = new BullsAndCows();

            BullsCowsDigits     bcComputerNextGuess, bcUserNextGuess, bcComputerSecretNum;
            BullsCowsResult     bcUserSecretNumResult, bcCompSecretNumResult;

            // Engaging the self-test when needed.  It is not yet implemented in C#.
            // bac.BullsAndCowsTest();

            bac.initBullsCows(out bcUserSecretNumResult, out bcCompSecretNumResult);
            bcComputerSecretNum = bac.RandomlyGeneratedBullsCows();     // * under testing condition with srand(0).
            bcComputerNextGuess = bac.RandomlyGeneratedBullsCows();     // * under testing condition with srand(0).

            GamePreamble();

            // Allowing the user to take a guess first.  It is separated from the main loop because it is okay for
            // the user to enter some bad data (to indicate she likes the computer to guess first).
            if (bac.ParseBullsCowsGuess(out bcUserNextGuess))
            {
                // The user wants to guess first.
                // Print user's A/B result and terminate the game if the user is really lucky.
                bcCompSecretNumResult = bac.guessingResponse(bcComputerSecretNum, bcUserNextGuess);
                if (bcCompSecretNumResult.A < GAMEOVER_NUM_A)
                {
                    System.Console.Write("Okay, your guess leads to ");
                    bac.PrintBullsCowsResult(bcCompSecretNumResult);
                }
            }

            while ((bcCompSecretNumResult.A < GAMEOVER_NUM_A) && (bcUserSecretNumResult.A < GAMEOVER_NUM_A))
            {
                System.Console.Write("\nI think your secret number is: ");
                bac.PrintBullsCows(bcComputerNextGuess);
                System.Console.Write("How many A's and B's did I get? ");

                // Keeping reading Bulls and Cows results until the user's input is valid.
                while (!bac.ParseBullsCowsResult(out bcUserSecretNumResult))
                {
                    System.Console.Write("Sorry I don't understand your answer.\nPlease input your Bulls and Cows answer again: ");
                }

                // Check if the computer wins already.
                if (bcUserSecretNumResult.A == GAMEOVER_NUM_A)
                    continue;

                // Eliminating invalid BullsCows combinations, and then generate bcComputerNextGuess.  Check if bcComputerNextGuess is
                // the only valid candidate left.
                bac.eliminateInvalidBullsCowsNumbers(bcComputerNextGuess, bcUserSecretNumResult);

                System.Console.Write("\nWhat is your guess of my secret number? ");
                // Keeping reading Bulls and Cows guesses until the user's input is valid.
                while (!bac.ParseBullsCowsGuess(out bcUserNextGuess))
                {
                    System.Console.Write("Sorry I don't understand your answer.\nPlease input your Bulls and Cows guess again: ");
                }

                // Print user's A/B result and terminate the game if the user is really lucky.
                bcCompSecretNumResult = bac.guessingResponse(bcComputerSecretNum, bcUserNextGuess);

                // Check if the user wins already.
                if (bcCompSecretNumResult.A == GAMEOVER_NUM_A)
                    continue;

                System.Console.Write("Okay, your guess leads to ");
                bac.PrintBullsCowsResult(bcCompSecretNumResult);

                bac.generateNextGuess(out bcComputerNextGuess);
            }

            if (bcUserSecretNumResult.A == GAMEOVER_NUM_A)
            {
                System.Console.WriteLine("\nI win and I successfully guessed your secret number!");
            }
            else if (bcCompSecretNumResult.A == GAMEOVER_NUM_A)
            {
                System.Console.WriteLine("\nYou win and you successfully guessed my secret number!");
            }
            else
            {
                // This should not happen.
                throw new SystemException("This should not happen -- neither side get 4A0B!");
            }
        }
    }
}
