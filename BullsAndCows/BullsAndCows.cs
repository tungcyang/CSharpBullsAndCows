using System;

namespace BullsAndCows
{
    class BullsAndCows
    {
        // Class structs
        // We will use the lower bytes of BullsCowsDigits (unsigned int) to keep the (4) digits.  Note that in C# there
        // is nothing similar to typedef in C/C++.
        public struct BullsCowsDigits
        {
            public uint d;
        }

        public struct BullsCowsResult
        {
            // These are the responses for a guessed 4-digit, like "1A2B" or "2B".
            public UInt32 A;
            public UInt32 B;
        }

        // Class constants
        protected const int NUM_OF_DIGITS = 10;
        protected const int NUM_OF_GUESSING_DIGITS = 4;
        protected const int NUM_OF_GUESSING_TARGETS = (10 * 9 * 8 * 7); // 10!/(10 - NUM_OF_GUESSING_DIGETS)! = 5040; if we allow the digits
                                                                        // to duplicate, then this becomes 10^NUM_OF_GUESSING_DIGITS.
        protected const int INVALID_DIGIT = 10;                         // Valid digits range from 0 to 9.
        protected const int BITS_PER_BAC_DIGIT = 8;                     // Number of bits each Bulls and Cows digit takes.
        protected const int GAMEOVER_NUM_A = NUM_OF_GUESSING_DIGITS;    // When we reach 4A(0B) the game is over.
        protected const int MAX_GUESSING_ALLOWED = 20;                  // Declaring a runaway guess (failure of the given algorithm)

        // Class variables.
        // Setting random number seed.  This should be really random (unpredictable).
        // The random seed is initialized where it is used.
        // https://msdn.microsoft.com/en-us/library/ctssatww(v=vs.110).aspx
        // static Random rand = new Random(0);                 // Using 0 as the random seed during tests.
        static Random rand = new Random((int) DateTime.Now.Ticks & 0x0000ffff);

        BullsCowsDigits[]   bcBuffer0 = new BullsCowsDigits[NUM_OF_GUESSING_TARGETS];
        BullsCowsDigits[]   bcBuffer1 = new BullsCowsDigits[NUM_OF_GUESSING_TARGETS];
        // In the original C project, we used pointers *bcPossibleNumbers and *bcPossibleCandidates to implement double buffering,
        // that we process data from one buffer to the other; for the next round we just swap the pointers instead of making a copy.
        // but this is not okay with C# as pointer operations are deemed unsafe.  Therefore we introduce a bool bBuffer0toBuffer1.
        // When true, we process data from bcBuffer0 to bcBuffer1, and vice versa.
        bool    bBuffer0toBuffer1 = true;
        UInt32  uiNumPossibleAnswers = 0, uiNumPossibleCandidates = 0;

        // Class methods
        /// isThisNumberValid() checks whether the given input uiNumber corresponds to a valid Bulls and Cows digit pattern.  it returns true
        /// if the number corresponds to a valid Bulls and Cows digit pattern (while at the same time puts the Bulls and Cows digits in
        /// *bcResult, and it returns false otherwise.  *bcResult is undefined if isThisNumberValid() returns false.
        bool    isThisNumberValid(ref BullsCowsDigits bcResult, UInt32 uiNumber)
        {
            bool bResult = true;
            uint uiDigit;
            bool[] bDigitUsed = new bool[NUM_OF_DIGITS];

            // Initialization.
            bcResult.d = 0;
            for (int i = 0; i < NUM_OF_DIGITS; i++)
                bDigitUsed[i] = false;

            for (int i = 0; i < NUM_OF_GUESSING_DIGITS; i++)
            {
                // Deriving individual digits.
                uiDigit = uiNumber % 10;
                uiNumber = uiNumber / 10;
                bcResult.d = (bcResult.d << BITS_PER_BAC_DIGIT) | uiDigit;
                if (bDigitUsed[uiDigit])
                {
                    // The digit we derived is already in use, and there is no need to continue;
                    bResult = false;
                    break;
                }
                else
                {
                    bDigitUsed[uiDigit] = true;
                }
            }

            return bResult;
        }


        /// initBullsCows() carries out the required initialization of the Bulls and Cows game, including populating
        /// the array pbcPossibleNumbers[] holding all the possible combinations.
        public void     initBullsCows(out BullsCowsResult bcUserSecretNumResult, out BullsCowsResult bcCompSecretNumResult)
        {
            //unsigned int i, j;
            UInt32  uiNumCombinations;
            BullsCowsDigits bcCurrentNumber;

            bcCurrentNumber.d = 0;

            // Initializing bcUserSecretNumResult and bcCompSecretNumResult.  Both the computer and the user start with 0A0B.
            bcUserSecretNumResult.A = 0;
            bcUserSecretNumResult.B = 0;
            bcCompSecretNumResult.A = 0;
            bcCompSecretNumResult.B = 0;

            if (NUM_OF_GUESSING_DIGITS >= NUM_OF_DIGITS)
                throw new SystemException("We want to avoid the modulo-by-0 case below.");

            // It does not seem straightforward to derive NUM_OF_GUESSING_DIGITS non-repeating digits to fill bcPossibleNumbers.
            // Here we simply iterate over all possible 10^NUM_OF_GUESSING_DIGITS combinations, and then skip over the invalid ones
            // (with duplicated digits, for example).
            uiNumPossibleAnswers = 0;
            uiNumCombinations = (UInt32) Math.Pow(10.0, NUM_OF_GUESSING_DIGITS);

            // Moving bBuffer0toBuffer1 test outside the loop.
            if (bBuffer0toBuffer1)
            {
                for (uint i = 0; i < uiNumCombinations; i++)
                {
                    if (isThisNumberValid(ref bcCurrentNumber, i))
                    {
                        // i leads to a valid Bulls and Cows combination.  Save it.
                        bcBuffer0[uiNumPossibleAnswers] = bcCurrentNumber;
                        uiNumPossibleAnswers++;
                    }
                }
            }
            else
            {
                for (uint i = 0; i < uiNumCombinations; i++)
                {
                    if (isThisNumberValid(ref bcCurrentNumber, i))
                    {
                        // i leads to a valid Bulls and Cows combination.  Save it.
                        bcBuffer1[uiNumPossibleAnswers] = bcCurrentNumber;
                        uiNumPossibleAnswers++;
                    }
                }
            }

            // This should be true.
            if (uiNumPossibleAnswers != NUM_OF_GUESSING_TARGETS)
                throw new SystemException("uiNumPossibleAnswers is not equal to NUM_OF_GUESSING_TARGETS!");
        }


        /// RandomlyGeneratedBullsCows() returns a randomly generated Bulls and Cows digit pattern.
        public BullsCowsDigits RandomlyGeneratedBullsCows()
        {
            int     uRandRange = (int) Math.Pow(10.0, NUM_OF_GUESSING_DIGITS);
            int     iRandNum;
            BullsCowsDigits     bc4Digits;

            bc4Digits.d = 0;

            do
            {
                // Generating a random integer between 0 and 9999 (inclusive on both ends).
                iRandNum = rand.Next(0, uRandRange);
            } while (!isThisNumberValid(ref bc4Digits, (uint) iRandNum));

            return bc4Digits;
        }


        /// PrintBullsCows() prints a Bulls and Cows digit pattern.
        public void PrintBullsCows(BullsCowsDigits bcData)
        {
            for (int i = 0; i < NUM_OF_GUESSING_DIGITS; i++)
                System.Console.Write((bcData.d >> (BITS_PER_BAC_DIGIT * (NUM_OF_GUESSING_DIGITS - 1 - i))) & 0x000000ff);
            System.Console.WriteLine();
        }


        /// PrintBullsCowsResult() prints a Bulls and Cows result.
        public void PrintBullsCowsResult(BullsCowsResult bcResult)
        {
            System.Console.WriteLine(bcResult.A + "A" + bcResult.B + "B");
        }


        // ParseBullsCowsResult() parses (reads from standard input) the user's answer on matching results, for instance,
        // "1A2B" or "0A1B".  If it returns false, it means the parsing failed (with invalid inputs).
        public bool ParseBullsCowsResult(out BullsCowsResult bcResult)
        {
            bool bResult = true;
            int     iOut;
            String  str;

            bcResult.A = bcResult.B = 0;

            // We only read the characters filling str[] (extra characters are ignored for security consideration).
            str = Console.ReadLine();

            if (str.Length == 4)
            {
                // Verifying str[1] is 'a' or 'A'.
                bResult &= str.Substring(1, 1).ToUpper().Equals("A");
                // Verifying str[3] is 'b' or 'B'.
                bResult &= str.Substring(3, 1).ToUpper().Equals("B");
                // We are processing a fixed-sized string.  Using atoi() seems awkward.
                bResult &= Int32.TryParse(str.Substring(0, 1), out iOut);
                bcResult.A = (uint) iOut;
                bResult &= Int32.TryParse(str.Substring(2, 1), out iOut);
                bcResult.B = (uint) iOut;
            }
            else if (str.Length == 2)
            {
                // The user is lazy to reply only "2A" or "3B".
                if (str.Substring(1, 1).ToUpper().Equals("A"))
                {
                    bResult &= Int32.TryParse(str.Substring(0, 1), out iOut);
                    bcResult.A = (uint) iOut;
                }
		        else if (str.Substring(1, 1).ToUpper().Equals("B"))
                {
                    bResult &= Int32.TryParse(str.Substring(0, 1), out iOut);
                    bcResult.B = (uint) iOut;
                }
                else
			        // str[1] is not one of 'a', 'A', 'b' or 'B'.
			        bResult = false;
            }
            else
            {
                // This should not happen.
                bResult = false;
            }

            // Verifying the A and B answers are valid.
            bResult &= (bcResult.A <= NUM_OF_GUESSING_DIGITS);
            bResult &= (bcResult.B <= NUM_OF_GUESSING_DIGITS);
            bResult &= ((bcResult.A + bcResult.B) <= NUM_OF_GUESSING_DIGITS);

            // There is no need to slurp all the remaining characters from stdin.  We already used ReadLine() to read into str.

            return bResult;
        }


        /// ParseBullsCowsGuess() parses (reads from standard input) the user's guess on the computer's secret number, for instance,
        /// "1234" or "6537".  If it returns false, it means the parsing failed (with invalid inputs, like containing non-digit
        /// characters, or duplicated digits).
        public bool ParseBullsCowsGuess(out BullsCowsDigits bcResult)
        {
            bool    bResult = true;
            String  str;
            int     iGuess = 0;
            uint    uiGuess = 0;

            bcResult.d = 0;
            str = Console.ReadLine();

            if (str.Length == NUM_OF_GUESSING_DIGITS)
            {
                if (!Int32.TryParse(str, out iGuess))
                {
                    // The input data leads to a digit outside the valid range.
                    return false;
                }

                // We successfully converted str into iGuess as an integer, but iGuess could be negative and that would be invalid.
                if (iGuess < 0)
                    return false;

                uiGuess = (uint) iGuess;
            }
            else
            {
                // This should not happen.
                bResult = false;
            }

            // Verifying that the digits do not repeat.
            bResult &= isThisNumberValid(ref bcResult, uiGuess);

            // There is no need to slurp all the remaining characters from stdin.  We already used ReadLine() to read into str.

            return bResult;
        }


        /// guessingResponse() returns the Bulls and Cows response on the inputs bcFirst and bcSecond.  For instance, when bcFirst corresponds
        /// to "0123" and bcSecond corresponds to "0326", guessingResponse() returns 2A1B.
        public BullsCowsResult guessingResponse(BullsCowsDigits bcFirst, BullsCowsDigits bcSecond)
        {
            // unsigned int i;
            uint    uiNumAs, uiNumBs;
            uint    uiTemp;
            BullsCowsResult     bcResult;
            uint[]  bDigitsUsedFirst = new uint[NUM_OF_DIGITS];
            uint[]  bDigitsUsedSecond = new uint[NUM_OF_DIGITS];

            // Checking A's.  We take an EXOR of bcFirst and bcSecond, then count the number of 0x0000's.
            uiNumAs = 0;
            uiTemp = bcFirst.d ^ bcSecond.d;
            for (int i = 0; i < NUM_OF_GUESSING_DIGITS; i++)
            {
                if ((uiTemp & 0x000000ff) == 0)
                    uiNumAs++;
                uiTemp >>= BITS_PER_BAC_DIGIT;
            }
            bcResult.A = uiNumAs;

            // Checking B's.  Note that bcFirst and bcSecond will be altered here.
            for (int i = 0; i < NUM_OF_DIGITS; i++)
                bDigitsUsedFirst[i] = bDigitsUsedSecond[i] = 0;
            for (int i = 0; i < NUM_OF_GUESSING_DIGITS; i++)
            {
                bDigitsUsedFirst[bcFirst.d & 0x000000ff]++;
                bDigitsUsedSecond[bcSecond.d & 0x000000ff]++;
                bcFirst.d >>= BITS_PER_BAC_DIGIT;
                bcSecond.d >>= BITS_PER_BAC_DIGIT;
            }
            // We accumulate the smaller of bDigitsUsedFirst[] and bDigitsUsedSecond[] -- it should be the sume of number of A's
            // and the number of B's, no matter if we allow duplicated digits or not.
            uiNumBs = 0;
            for (int i = 0; i < NUM_OF_DIGITS; i++)
                if (bDigitsUsedFirst[i] < bDigitsUsedSecond[i])
                    uiNumBs += bDigitsUsedFirst[i];
                else
                    uiNumBs += bDigitsUsedSecond[i];
            uiNumBs -= uiNumAs;
            bcResult.B = uiNumBs;

            return bcResult;
        }


        /// eliminateInvalidBullsCowsNumbers() goes through the array bcPossibleNumbers[] of size uiNumPossibleAnswers
        /// and remove the entries inconsistent of bcComputerNextGuess and bcRUserGuess.  bcPossibleNumbers[] and
        /// uiNumPossibleAnswers will be updated upon return.
        public void eliminateInvalidBullsCowsNumbers(BullsCowsDigits bcComputerNextGuess, BullsCowsResult bcRUserGuess)
        {
            // BullsCowsDigits* pbcTemp = NULL;
            BullsCowsResult     bcResult;

            bcResult.A = bcResult.B = 0;

            // We use pbcPossibleCandidates[] and uiNumPossibleCandidates as scratch variables.
            uiNumPossibleCandidates = 0;

            // Moving bBuffer0toBuffer1 test outside the loop.
            if (bBuffer0toBuffer1)
            {
                for (int i = 0; i < uiNumPossibleAnswers; i++)
                {
                    bcResult = guessingResponse(bcComputerNextGuess, bcBuffer0[i]);
                    if ((bcRUserGuess.A == bcResult.A) && (bcRUserGuess.B == bcResult.B))
                    {
                        bcBuffer1[uiNumPossibleCandidates] = bcBuffer0[i];
                        uiNumPossibleCandidates++;
                    }
                }
            }
            else
            {
                for (int i = 0; i < uiNumPossibleAnswers; i++)
                {
                    bcResult = guessingResponse(bcComputerNextGuess, bcBuffer1[i]);
                    if ((bcRUserGuess.A == bcResult.A) && (bcRUserGuess.B == bcResult.B))
                    {
                        bcBuffer0[uiNumPossibleCandidates] = bcBuffer1[i];
                        uiNumPossibleCandidates++;
                    }
                }
            }

            // Swapping the pointers to the BullsCowsDigits arrays.
            bBuffer0toBuffer1 = !bBuffer0toBuffer1;
            uiNumPossibleAnswers = uiNumPossibleCandidates;

            // uiNumPossibleAnswers must be positive.  If it is 0, there is something wrong -- there are no valid candidates left.
            // Either there is a bug, or the user provided wrong clues.
            if (uiNumPossibleAnswers == 0)
                throw new SystemException("This should not happen -- no valid secret number candidates exist!");
        }


        /// generateNextGuess() scans over the remaining valid bcPossibleNumbers[] and returns a guess for the computer in *pbcNextGuess.
        /// It returns true if there is only one candidate (the generated guess must be the answer).
        public bool generateNextGuess(out BullsCowsDigits bcNextGuess)
        {
            uint[] uiDigitsStatistics = new uint[NUM_OF_DIGITS];
            uint    uiMostLikelyDigit, uiMostLikelyDigitFreq;
            BullsCowsDigits[]   bcCandidates = new BullsCowsDigits[NUM_OF_GUESSING_TARGETS];
            uint    uiNumCandidates;

            // Initialization.
            uiNumCandidates = uiNumPossibleAnswers;
            bcNextGuess.d = 0;

            // Making a copy of data into the local buffer bcCandidates[].
            if (bBuffer0toBuffer1)
            {
                for (int j = 0; j < uiNumCandidates; j++)
                {
                    bcCandidates[j] = bcBuffer0[j];
                }
            }
            else
            {
                for (int j = 0; j < uiNumCandidates; j++)
                {
                    bcCandidates[j] = bcBuffer1[j];
                }
            }

            // Iterating to find digits for bcResult.
            for (int i = 0; i < NUM_OF_GUESSING_DIGITS; i++)
            {
                // Find out the most frequently occurring digit.
                for (int j = 0; j < NUM_OF_DIGITS; j++)
                    uiDigitsStatistics[j] = 0;
                for (int j = 0; j < uiNumCandidates; j++)
                {
                    uiDigitsStatistics[(bcCandidates[j].d >> (BITS_PER_BAC_DIGIT * (NUM_OF_GUESSING_DIGITS - 1 - i))) & 0x000000ff]++;
                }
                uiMostLikelyDigit = INVALID_DIGIT;
                uiMostLikelyDigitFreq = 0;
                for (uint j = 0; j < NUM_OF_DIGITS; j++)
                    if (uiDigitsStatistics[j] > uiMostLikelyDigitFreq)
                    {
                        uiMostLikelyDigit = j;
                        uiMostLikelyDigitFreq = uiDigitsStatistics[j];
                    }
                // Now uiMostLikelyDigit points to the most likely digit for bcResult.
                bcNextGuess.d = (bcNextGuess.d << BITS_PER_BAC_DIGIT) | uiMostLikelyDigit;

                // Having determined uiMostLikelyDigit and combining it into *pbcNextGuess, we want to copy those pbcCandidates[]
                // with chosen digit (uiMostLikelyDigit) into the scratch buffer and continuing finding the next digit.
                uiNumPossibleCandidates = 0;
                if (bBuffer0toBuffer1)
                {
                    for (int j = 0; j < uiNumCandidates; j++)
                    {
                        if (((bcCandidates[j].d >> (BITS_PER_BAC_DIGIT * (NUM_OF_GUESSING_DIGITS - 1 - i))) & 0x000000ff) == uiMostLikelyDigit)
                        {
                            bcBuffer1[uiNumPossibleCandidates] = bcCandidates[j];
                            uiNumPossibleCandidates++;
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < uiNumCandidates; j++)
                    {
                        if (((bcCandidates[j].d >> (BITS_PER_BAC_DIGIT * (NUM_OF_GUESSING_DIGITS - 1 - i))) & 0x000000ff) == uiMostLikelyDigit)
                        {
                            bcBuffer0[uiNumPossibleCandidates] = bcCandidates[j];
                            uiNumPossibleCandidates++;
                        }
                    }
                }

                // Pointing *pbcCandidates to local buffer and then copy the data back to *pbcCandidates to get ready for the
                // next iteration.
                if (bBuffer0toBuffer1)
                {
                    for (int j = 0; j < uiNumPossibleCandidates; j++)
                        bcCandidates[j] = bcBuffer1[j];
                }
                else
                {
                    for (int j = 0; j < uiNumPossibleCandidates; j++)
                        bcCandidates[j] = bcBuffer0[j];
                }

                uiNumCandidates = uiNumPossibleCandidates;
            }

            return (uiNumPossibleAnswers == 1);
        }

        /// BullsAndCowsTest() essentially tests the guessing algorithm adopted in this game to make sure it does not lead to an
        /// infinite loop, and likely collect some statistics for the algorithm.  It essentially follows the main program except:
        /// (1) there are no user inputs/outputs involved;
        /// (2) only the computer attempts to guess the user's secret number with the implemented algorithm.
        public void BullsAndCowsTest()
        {
            uint    uiUserSecretNum, uiCompGuessNum;
            BullsCowsDigits bcUserSecretNum, bcCompGuessNum;
            BullsCowsResult bcUserSecretNumResult, bcCompSecretNumResult;
            uint    uiNumValidTests, uiCurrNumGuesses, uiMaxNumGuesses, uiTotalNumGuesses;

            // Initialization
            uiNumValidTests = 0;
            uiMaxNumGuesses = 0;
            uiTotalNumGuesses = 0;
            bcUserSecretNum.d = 0;
            bcCompGuessNum.d = 0;

            for (uiUserSecretNum = 0; uiUserSecretNum < 10000; uiUserSecretNum++)
            {
                // Moving to the next number if this one is not valid.
                if (!isThisNumberValid(ref bcUserSecretNum, uiUserSecretNum))
                    continue;

                // Printing current status (uiUserSecretNum) while trying all combinations of uiCompGuessNum so the tester knows
                // where we are during the test.
                System.Console.WriteLine("Currently testing uiUserSecretNum " + uiUserSecretNum + " and all possible uiCompGuessNum.");

                for (uiCompGuessNum = 0; uiCompGuessNum < 10000; uiCompGuessNum++)
                {
                    // Moving to the next number if this one is not valid.
                    if (!isThisNumberValid(ref bcCompGuessNum, uiCompGuessNum))
                        continue;

                    // Updating status
                    uiNumValidTests++;
                    uiCurrNumGuesses = 1;

                    // Now we have both bcUserSecretNum and bcCompGuessNum being valid and exhaustive.  Continue the guessing.
                    initBullsCows(out bcUserSecretNumResult, out bcCompSecretNumResult);

                    while ((bcUserSecretNumResult.A < GAMEOVER_NUM_A) && (uiCurrNumGuesses < MAX_GUESSING_ALLOWED))
                    {
                        // Use bcUserSecretNum and bcCompGuessNum to derive bcUserSecretNumResult
                        bcUserSecretNumResult = guessingResponse(bcUserSecretNum, bcCompGuessNum);

                        // Check if the computer wins already.
                        if (bcUserSecretNumResult.A == GAMEOVER_NUM_A)
                            continue;

                        eliminateInvalidBullsCowsNumbers(bcCompGuessNum, bcUserSecretNumResult);
                        generateNextGuess(out bcCompGuessNum);
                        uiCurrNumGuesses++;
                    }

                    if (uiCurrNumGuesses == MAX_GUESSING_ALLOWED)
                    {
                        // Runaway guess.  Report it and exit.
                        System.Console.WriteLine("We have a runaway guess under current implementation!\n");
                        return;
                    }

                    // We are done and reach a success.  Record the statistics
                    if (uiMaxNumGuesses < uiCurrNumGuesses)
                        uiMaxNumGuesses = uiCurrNumGuesses;
                    uiTotalNumGuesses += uiCurrNumGuesses;
                }
            }

            // We have iterated over all possible combinations.  Reporting the statistics we have collected.
            System.Console.WriteLine("We have attempted " + uiNumValidTests + " different valid combinations.");
            System.Console.WriteLine("The longest one takes " + uiMaxNumGuesses + " guesses.");
            System.Console.WriteLine("On average each attempt takes " + (float)uiTotalNumGuesses / uiNumValidTests + " guesses.");

            // We have attempted 25401600 different valid combinations (25401600 = (10*9*8*7)^2), 5040 combinations for
            // uiUserSecretNum and 5040 for uiCompGuessNum.
            // The longest game takes 9 guesses.
            // On average each attempt takes 5.455535 guesses for the computer to successfully guess the user's secret number.
            //
            // The longest session will look like the followings.  Note that the digits in uiUserSecretNum and those
            // in bcUserSecretNum are in reversed order as implemented.
            //
            // We hit 9 guesses when uiUserSecretNum is 0138 (bcUserSecretNum: 0x08030100) and uiCompGuessNum starts with 3209:
            // uiCompGuessNum --> 3209 : 0A2B
            //                    4120 : 1A1B
            //                    5310 : 0A3B
            //                    4053 : 0A2B
            //                    2135 : 2A0B
            //                    0195 : 2A0B
            //                    0136 : 3A0B
            //                    0137 : 3A0B
            //                    0138 : 4A0B
        }

    }
}
