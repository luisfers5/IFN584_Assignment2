using System;
using System.Collections.Generic;
using static BoardGames.NumericalTTTGame;

namespace BoardGames
{
    // 1. Class Declaration
    class NumericalTTTComputer : Player             // Computer-controlled player, inherits from Player
    {
        // 2. Random generator for fallback moves
        private Random rand = new Random();    // used to pick random cell or number

        // 3. Constructors
        public NumericalTTTComputer() : base() { }   // default ctor, passes to base

        public NumericalTTTComputer(string name, List<int> numbers)
            : base(name, numbers) { }         // ctor with name and available numbers

        // 4. Move Logic
        public override (int row, int col, int number) MakeMove(Board board)
        {
            // 4a. Try to find an immediate winning move
            for (int i = 0; i < board.Size; i++)
            {
                for (int j = 0; j < board.Size; j++)
                {
                    if (!board.IsValidMove(i, j))      // skip if cell not free
                        continue;

                    // simulate each available number here
                    foreach (int num in new List<int>(AvailableNumbers))
                    {
                        board.Cells[i][j] = num;        // place number temporarily
                        if (CheckWinStatic(board))      // check for win
                        {
                            board.Cells[i][j] = null;   // undo simulation
                            AvailableNumbers.Remove(num);
                            return (i, j, num);        // take winning move
                        }
                        board.Cells[i][j] = null;       // undo simulation
                    }
                }
            }

            // 4b. No winning move found -> pick random valid cell and number
            var freeCells = new List<(int row, int col)>();
            for (int i = 0; i < board.Size; i++)
                for (int j = 0; j < board.Size; j++)
                    if (board.IsValidMove(i, j))
                        freeCells.Add((i, j));                 // collect all free cells

            // ensure there's at least one move left
            if (freeCells.Count == 0 || AvailableNumbers.Count == 0)
                throw new Exception("No valid moves available for the computer.");

            // pick a random cell
            var (r, c) = freeCells[rand.Next(freeCells.Count)];
            // pick a random available number
            int chosenNum = AvailableNumbers[rand.Next(AvailableNumbers.Count)];
            AvailableNumbers.Remove(chosenNum);  // consume that number
            return (r, c, chosenNum);           // return random move
        } // End of MakeMove() method

    } // End of NumericalTTTComputer class

} // End of BoardGames namespace
