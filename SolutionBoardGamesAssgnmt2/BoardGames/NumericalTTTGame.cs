using System;
using System.Collections.Generic;

namespace BoardGames
{
    // 1. Numerical Tic-Tac-Toe game class
    class NumericalTTTGame : Game
    {
        // Static setup method called from Program
        public static new Game SetupNewGame()
        {
            int size;
            while (true)
            {
                Console.WriteLine("Enter board size n (for an NxN board): ");
                string input = Console.ReadLine();
                if (int.TryParse(input, out size) && size > 0)
                {
                    Console.Clear();
                    break;
                }
                Console.WriteLine("Invalid input. Please enter a positive integer.");
            }

            var newGame = new NumericalTTTGame
            {
                Board = new Board(size)
            };

            Console.WriteLine("Select game mode:\n[1 vs 1] -> [1]\n[1 vs PC] -> [2]");
            string mode;
            while ((mode = Console.ReadLine()) != "1" && mode != "2")
            {
                Console.Clear();
                Console.WriteLine("Invalid input. Try again.");
                Console.WriteLine("Select game mode:\n[1 vs 1] -> [1]\n[1 vs PC] -> [2]");
            }

            newGame.IsHumanVsComputer = (mode == "2");

            // Prepare number pools
            var allNumbers = new List<int>();
            for (int i = 1; i <= size * size; i++)
                allNumbers.Add(i);

            var odds = new List<int>();
            var evens = new List<int>();
            foreach (int num in allNumbers)
            {
                if (num % 2 == 0) evens.Add(num);
                else odds.Add(num);
            }

            newGame.Player1 = new HumanPlayer("Player 1", odds);
            newGame.Player2 = newGame.IsHumanVsComputer
                ? new ComputerPlayer("Computer", evens)
                : new HumanPlayer("Player 2", evens);

            newGame.CurrentPlayerIndex = 1;
            return newGame;
        } // End of SetupNewGame() method

        // 2. Override the move-applying logic so it lives here instead of in Board
        protected override bool TryApplyMove()
        {
            var player = GetCurrentPlayer();
            var (row, col, number) = player.MakeMove(Board);

            // Place the number
            if (!Board.PlaceNumber(row, col, number))
            {
                Console.WriteLine("Invalid move. Press Enter...");
                Console.ReadLine();
                return false;
            }

            // Check if human just won
            if (CheckWin())
            {
                Console.Clear();
                Board.Display();
                Console.WriteLine($"\n{player.Name} wins!");
                Console.WriteLine("Press Enter to return to the main menu...");
                Console.ReadLine();
                return false;
            }

            // Check for tie
            if (Board.IsBoardFull())
            {
                Console.Clear();
                Board.Display();
                Console.WriteLine("\nIt's a tie!");
                Console.WriteLine("Press Enter to return to the main menu...");
                Console.ReadLine();
                return false;
            }

            // Switch to next player
            SwitchPlayer();

            // Computer move if needed
            if (IsHumanVsComputer && GetCurrentPlayer() is ComputerPlayer comp)
            {
                Console.WriteLine("\nComputer is thinking...");
                var (cr, cc, cnum) = comp.MakeMove(Board);
                Board.PlaceNumber(cr, cc, cnum);

                // Computer win?
                if (CheckWin())
                {
                    Console.Clear();
                    Board.Display();
                    Console.WriteLine($"\n{comp.Name} wins!");
                    Console.WriteLine("Press Enter to return to the main menu...");
                    Console.ReadLine();
                    return false;
                }

                // Computer tie?
                if (Board.IsBoardFull())
                {
                    Console.Clear();
                    Board.Display();
                    Console.WriteLine("\nIt's a tie!");
                    Console.WriteLine("Press Enter to return to the main menu...");
                    Console.ReadLine();
                    return false;
                }

                // Back to human
                SwitchPlayer();
            }

            return true;
        } // End of TryApplyMove() method

        // 3. Instance helper: delegates to static win checker
        private bool CheckWin()
            => CheckWinStatic(Board);

        // 4. Static win detection (magic sum across rows, columns, diagonals)
        public static bool CheckWinStatic(Board board)
        {
            int n = board.Size;
            int winSum = n * (n * n + 1) / 2;                   // Magic square sum

            // Check rows
            for (int i = 0; i < n; i++)
            {
                int sum = 0;
                bool full = true;
                for (int j = 0; j < n; j++)
                {
                    if (board.Cells[i][j] == null) { full = false; break; }
                    sum += board.Cells[i][j].Value;
                }
                if (full && sum == winSum) return true;
            }

            // Check columns
            for (int j = 0; j < n; j++)
            {
                int sum = 0;
                bool full = true;
                for (int i = 0; i < n; i++)
                {
                    if (board.Cells[i][j] == null) { full = false; break; }
                    sum += board.Cells[i][j].Value;
                }
                if (full && sum == winSum) return true;
            }

            // Check diagonal (top-left to bottom-right)
            int d1 = 0; bool ok1 = true;
            for (int i = 0; i < n; i++)
            {
                if (board.Cells[i][i] == null) { ok1 = false; break; }
                d1 += board.Cells[i][i].Value;
            }
            if (ok1 && d1 == winSum) return true;

            // Check diagonal (top-right to bottom-left)
            int d2 = 0; bool ok2 = true;
            for (int i = 0; i < n; i++)
            {
                if (board.Cells[i][n - 1 - i] == null) { ok2 = false; break; }
                d2 += board.Cells[i][n - 1 - i].Value;
            }

            return ok2 && d2 == winSum;
        } // End of CheckWinStatic() method
    }
}
