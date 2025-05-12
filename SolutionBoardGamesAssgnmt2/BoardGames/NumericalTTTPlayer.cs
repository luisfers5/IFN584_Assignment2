using System;
using System.Collections.Generic;

namespace BoardGames
{
    // 1. Class Declaration
    class NumericalTTTPlayer : Player   // human-controlled player, inherits shared Player behavior
    {

        public List<int> AvailableNumbers { get; set; }                   // For Numerical TTT (optional elsewhere)

        // 2. Constructors
        public NumericalTTTPlayer() : base() { }                          // default ctor passes to base

        public NumericalTTTPlayer(string name, List<int> numbers)        // custom ctor takes name and available numbers
            : base(name)
        {
            AvailableNumbers = numbers;
        }

        // 3. Move Logic
        public override (int row, int col, int number) MakeMove(List<Board> boards, Board board)
        {
            Console.WriteLine($"{Name}, enter your move as: row col number (e.g., 1 0 5)");  // prompt format

            while (true)                                            // loop until valid input
            {
                string input = Console.ReadLine();                  // read user line
                var tokens = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                // 3a. Validate format
                if (tokens.Length != 3 ||
                    !int.TryParse(tokens[0], out int row) ||
                    !int.TryParse(tokens[1], out int col) ||
                    !int.TryParse(tokens[2], out int number))
                {
                    Console.WriteLine("Invalid format. Use: row col number (e.g., 1 0 5)");
                    continue;                                       // retry
                }

                // 3b. Validate cell availability
                if (!board.IsValidMove(row, col))
                {
                    Console.WriteLine("That cell is already taken or out of bounds");
                    continue;                                       // retry
                }

                // 3c. Validate number choice
                if (!AvailableNumbers.Contains(number))
                {
                    Console.WriteLine($"Invalid number. Available: {string.Join(", ", AvailableNumbers)}");
                    continue;                                       // retry
                }

                AvailableNumbers.Remove(number);                    // consume chosen number
                return (row, col, number);                          // return valid move
            }
        }
    }
}
