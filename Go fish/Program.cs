using System;
using System.Collections.Generic;
using System.Linq;

namespace Go_fish
{
    class Program
    {
        static void Main(string[] args)
        {
            game g = new game();
            g.prgm(1);
        }
    }

    class game
    {
        // all the possible cards
        enum card
        {
            // Format: [Suit][Rank]
            // ex.: D7 == Diamonds Seven
            Ha, Sa, Da, Ca,
            H2, S2, D2, C2,
            H3, S3, D3, C3,
            H4, S4, D4, C4,
            H5, S5, D5, C5,
            H6, S6, D6, C6,
            H7, S7, D7, C7,
            H8, S8, D8, C8,
            H9, S9, D9, C9,
            H10, S10, D10, C10,
            Hj, Sj, Dj, Cj,
            Hq, Sq, Dq, Cq,
            Hk, Sk, Dk, Ck
        }

        enum state
        {
            unbooked,
            player_1,
            ai
        }

        string name;

        public void prgm(int players)
        {
            // Containers            
            var hand = new List<card>();
            var aiHand = new List<card>();
            var deck = new Stack<card>();
            var books = new Dictionary<int, state>();
            for (int i = 0; i < 13; i++) books.Add(i, state.unbooked);
            var score = new Dictionary<int, int>();
            score.Add(0, 0); score.Add(1, 0);
            var activePlayer = true;

            initiateGame(ref deck, ref hand, ref aiHand);
            while (!gameIsOver(ref deck))
            {
                if (!doTurn(ref deck, ref hand, ref aiHand, ref books, ref score, activePlayer)) activePlayer = !activePlayer;
            }
            Console.WriteLine("Game over!");
            pressKeyToContinue(ConsoleKey.Enter);
        }

        private bool doTurn(ref Stack<card> deck, ref List<card> hand, ref List<card> aiHand, ref Dictionary<int, state> books, ref Dictionary<int, int> score, bool isPlayer)
        {
            var extraTurn = false;
            // Check for an empty start
            if (isPlayer ? hand.Count == 0 : aiHand.Count == 0)
            {
                // Draw a card
                var drawn = deck.Pop();
                if (isPlayer)
                {
                    Console.WriteLine(String.Format("You draw a {0}.", drawn));
                    hand.Add(drawn);
                }
                else aiHand.Add(drawn);
            }

            // Sort hand
            var _hand = new Dictionary<int, List<card>>();
            for (int i = 0; i < 13; i++) _hand.Add(i, new List<card>());
            foreach (var c in isPlayer ? hand : aiHand) _hand[(int)c / 4].Add(c);
            for (int i = 0; i < 13; i++) if (_hand[i].Count == 0) _hand.Remove(i);


            // Book check
            var book = -1;
            foreach (var kvp in _hand)
            {
                if (kvp.Value.Count == 4)
                {
                    // Book found
                    Console.WriteLine("That makes a book of {0}'s! (+1p)", kvp.Key + 1);
                    // Discard book
                    for (int i = 0; i < 4; i++)
                    {
                        if (isPlayer) hand.Remove((card)(kvp.Key * 4 + i));
                        else aiHand.Remove((card)(kvp.Key * 4 + i));
                    }
                    // Add points
                    if (isPlayer)
                    {
                        books[kvp.Key] = state.player_1;
                        score[0]++;
                    }
                    else
                    {
                        books[kvp.Key] = state.ai;
                        score[1]++;
                    }
                    book = kvp.Key;
                    break;
                }
            }
            if (book > -1) _hand.Remove(book);

            // Display hand 
            if (isPlayer) printHand(_hand);

            // Ask a card
            int rank = 0;
            if (isPlayer)
            {
                List<int> askAble = new List<int>();

                bool fault = false;
                string input = "";
                do
                {
                    if (fault) Console.WriteLine("Invalid input...");
                    fault = true;
                    Console.WriteLine("What rank do you want to ask your opponent? (a, 2-10, j, q, k)");
                    input = Console.ReadLine();
                    fault = !(input == "a" | input == "2" | input == "3" | input == "4" | input == "5" | input == "6" | input == "7" | input == "8" | input == "9" | input == "10" | input == "j" | input == "q" | input == "k");
                } while (fault);

                switch (input)
                {
                    case "a":
                        rank = 1;
                        break;

                    case "j":
                        rank = 11;
                        break;

                    case "q":
                        rank = 12;
                        break;

                    case "k":
                        rank = 13;
                        break;

                    default:
                        rank = int.Parse(input);
                        break;
                }
            }
            else
            {
                // Choose random rank to ask
                var rnd = new Random();
                var rndCrd = aiHand[rnd.Next(0, aiHand.Count - 1)];
                rank = (int)rndCrd / 4;
                rank++;
            }

            string verbal;
            switch (rank)
            {
                case 1:
                    verbal = "ace";
                    break;
                case 11:
                    verbal = "jack";
                    break;

                case 12:
                    verbal = "queen";
                    break;
                case 13:
                    verbal = "king";
                    break;

                default:
                    verbal = "" + rank;
                    break;
            }

            // Ask it
            if (!isPlayer) Console.WriteLine("The computer asks you for your {0}s", verbal);
            else Console.WriteLine("You ask the Ai's {0}s", verbal);

            // Check if you have any
            var toGive = new List<card>();
            foreach (var c in isPlayer ? aiHand : hand) if (rank == ((int)c / 4) + 1) toGive.Add(c);
            if (toGive.Count > 0)
            {
                if (isPlayer) Console.WriteLine("You recieve it's {0} card(s)!", toGive.Count);
                else Console.WriteLine("You give it {0} cards!", toGive.Count);

                // Take out of deck A and into deck B
                foreach (var c in toGive)
                {
                    if (isPlayer)
                    {
                        aiHand.Remove(c);
                        hand.Add(c);
                    }
                    else
                    {
                        hand.Remove(c);
                        aiHand.Add(c);
                    }
                }
                extraTurn = true;
                Console.WriteLine("That means an extra turn!");
            }
            else
            {
                // Go fish
                Console.WriteLine("Go fish!");
                if (deck.Count > 0)
                {
                    // Draw a card
                    var drawn = deck.Pop();
                    if (isPlayer)
                    {
                        Console.WriteLine(String.Format("You draw a {0}.", drawn));
                        hand.Add(drawn);
                    }
                    else aiHand.Add(drawn);

                    // Sort hand
                    _hand = new Dictionary<int, List<card>>();
                    for (int i = 0; i < 13; i++) _hand.Add(i, new List<card>());
                    foreach (var c in isPlayer ? hand : aiHand) _hand[(int)c / 4].Add(c);
                    for (int i = 0; i < 13; i++) if (_hand[i].Count == 0) _hand.Remove(i);


                    // Book check
                    book = -1;

                    foreach (var kvp in _hand)
                    {
                        if (kvp.Value.Count == 4)
                        {
                            // Book found
                            Console.WriteLine("That makes a book of {0}'s! (+1p)", kvp.Key + 1);
                            // Discard book
                            for (int i = 0; i < 4; i++)
                            {
                                if (isPlayer) hand.Remove((card)(kvp.Key * 4 + i));
                                else aiHand.Remove((card)(kvp.Key * 4 + i));
                            }
                            // Add points
                            if (isPlayer)
                            {
                                books[kvp.Key] = state.player_1;
                                score[0]++;
                            }
                            else
                            {
                                books[kvp.Key] = state.ai;
                                score[1]++;
                            }
                            book = kvp.Key;
                            break;
                        }
                    }
                    if (book > -1) _hand.Remove(book);

                    // Check for extra turn
                    extraTurn = (((int)drawn / 4) + 1 == rank);
                    if (extraTurn) Console.WriteLine("{0} fished it! That's an extra turn!", isPlayer ? "You" : "It");
                }
            }


            // Display score and deck counter
            Console.WriteLine("{0} cards left!", deck.Count);
            Console.WriteLine("[{2}] {0} - {1} [AI]", score[0], score[1], name);

            // Continue
            pressKeyToContinue(ConsoleKey.Enter);
            return extraTurn;
        }

        private void printHand(Dictionary<int, List<card>> _hand)
        {
            Console.WriteLine("Your hand:");
            foreach (var kvp in _hand)
            {
                string toPrint = "- ";
                foreach (var c in kvp.Value) toPrint += c + ", ";
                Console.WriteLine(toPrint.Substring(0, toPrint.Length - 2));
            }
        }

        private bool gameIsOver(ref Stack<card> deck)
        {
            // Game over condition is an empty deck
            return deck.Count == 0;
        }

        private void initiateGame(ref Stack<card> deck, ref List<card> hand, ref List<card> aiHand)
        {
            // Create a new shuffeled deck
            var rnd = new Random();
            deck = new Stack<card>(Enum.GetValues(typeof(card)).Cast<card>().OrderBy(x => rnd.Next()));

            // Give both players 7 cards
            for (int i = 0; i < 7; i++) hand.Add(deck.Pop());
            for (int i = 0; i < 7; i++) aiHand.Add(deck.Pop());

            // Ask the player's name
            Console.WriteLine("What should we call you? (leaving this empty will result in \"player\"");
            name = Console.ReadLine();
            if (name == "") name = "player";
        }

        private void pressKeyToContinue(ConsoleKey key)
        {
            // Ask the key press
            Console.Write(String.Format("Press {0} to continue: ", key));

            // Until the key is pressed, repeat
            while (Console.ReadKey().Key != key)
            {
                Console.WriteLine("Invalid input...");
                Console.Write(String.Format("Press {0} to continue: ", key));
            }
            Console.WriteLine();
        }
    }
}
