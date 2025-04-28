using System;
using System.Collections.Generic;

namespace BoardGames
{
    // 1. GomokuGame class handles setup and gameplay
    class GomokuGame : Game
    {
        public static new Game SetupNewGame()
        {
            Console.WriteLine("Enter board size (recommended 10–15):");
            int size;
            while (!int.TryParse(Console.ReadLine(), out size) || size < 5 || size > 15)
            {
                Console.WriteLine("Invalid input. Enter a number between 5 and 15.");
            }

            var game = new GomokuGame
            {
                Board = new Board(size),
                CurrentPlayerIndex = 1
            };

            Console.WriteLine("Select game mode:\n[1 vs 1] -> [1]\n[1 vs PC] -> [2]");
            string mode;
            while ((mode = Console.ReadLine()?.Trim()) != "1" && mode != "2")
            {
                Console.WriteLine("Invalid input. Try again.");
            }

            game.IsHumanVsComputer = (mode == "2");

            game.Player1 = new GomokuPlayer("Player 1", 1);               // X
            game.Player2 = game.IsHumanVsComputer
                ? new GomokuComputer("Computer", 2)                       // O
                : new GomokuPlayer("Player 2", 2);

            return game;
        } // End of SetupNewGame() method

        // 2. Handles move logic and checks win/tie
        protected override bool TryApplyMove()
        {
            var human = (GomokuPlayer)GetCurrentPlayer();                // Cast to GomokuPlayer
            var (hRow, hCol, _) = human.MakeMove(Board);                 // Get human move

            if (!Board.IsValidMove(hRow, hCol))
            {
                Console.WriteLine("Cell is taken or out of bounds. Please choose a different cell.");
                Console.WriteLine("Press Enter to try again...");
                Console.ReadLine();
                return true;                                             // Just retry input
            }

            Board.Cells[hRow][hCol] = human.Symbol;                      // Place symbol on board

            if (CheckGomokuWin(hRow, hCol, human.Symbol))               // Check win
            {
                Console.Clear();
                Board.Display();
                Console.WriteLine($"\n {human.Name} wins!");
                Console.WriteLine("Press Enter to return to the main menu...");
                Console.ReadLine();
                return false;                                            // Game over
            }

            if (Board.IsBoardFull())                                     // Tie check
            {
                Console.Clear();
                Board.Display();
                Console.WriteLine("\nIt's a tie!");
                Console.WriteLine("Press Enter to return to the main menu...");
                Console.ReadLine();
                return false;
            }

            SwitchPlayer();                                              // Next player

            if (IsHumanVsComputer && GetCurrentPlayer() is GomokuComputer comp)
            {
                Console.WriteLine("\nComputer is thinking...");
                var (cRow, cCol, _) = comp.MakeMove(Board);
                Board.Cells[cRow][cCol] = comp.Symbol;

                if (CheckGomokuWin(cRow, cCol, comp.Symbol))
                {
                    Console.Clear();
                    Board.Display();
                    Console.WriteLine($"\n{comp.Name} wins!");
                    Console.WriteLine("Press Enter to return to the main menu...");
                    Console.ReadLine();
                    return false;
                }

                if (Board.IsBoardFull())                                 // Tie check again
                {
                    Console.Clear();
                    Board.Display();
                    Console.WriteLine("\nIt's a tie!");
                    Console.WriteLine("Press Enter to return to the main menu...");
                    Console.ReadLine();
                    return false;
                }

                SwitchPlayer();                                          // Back to human
            }

            return true;                                                 // Keep game running
        } // End of TryApplyMove() method

        // 3. Checks all directions from a move for 5-in-a-row
        private bool CheckGomokuWin(int row, int col, int symbol)
        {
            return CheckLine(row, col, symbol, 1, 0) ||                  // Horizontal
                   CheckLine(row, col, symbol, 0, 1) ||                  // Vertical
                   CheckLine(row, col, symbol, 1, 1) ||                  // Diagonal \
                   CheckLine(row, col, symbol, 1, -1);                   // Diagonal /
        }

        // 4. Checks both directions along a line
        private bool CheckLine(int row, int col, int symbol, int dx, int dy)
        {
            int count = 1;
            count += CountInDirection(row, col, symbol, dx, dy);
            count += CountInDirection(row, col, symbol, -dx, -dy);
            return count >= 5;
        }

        // 5. Helper to count matching symbols in a given direction
        private int CountInDirection(int row, int col, int symbol, int dx, int dy)
        {
            int count = 0;
            int x = row + dx, y = col + dy;
            while (x >= 0 && x < Board.Size &&
                   y >= 0 && y < Board.Size &&
                   Board.Cells[x][y] == symbol)
            {
                count++;
                x += dx;
                y += dy;
            }
            return count;
        }
    }

    // 6. Human player class for Gomoku
    class GomokuPlayer : Player
    {
        public int Symbol { get; }

        public GomokuPlayer(string name, int symbol) : base(name, null)
        {
            Symbol = symbol;
        }

        public override (int row, int col, int number) MakeMove(Board board)
        {
            Console.WriteLine($"{Name}, enter move as: row col (for example 2 2, separated by space)");
            while (true)
            {
                var parts = Console.ReadLine()?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts?.Length == 2 &&
                    int.TryParse(parts[0], out int row) &&
                    int.TryParse(parts[1], out int col) &&
                    row >= 0 && row < board.Size &&
                    col >= 0 && col < board.Size)
                {
                    return (row, col, 0);                                // We ignore 'number' for Gomoku
                }
                Console.WriteLine("Invalid input. Format: row col (e.g., 4 7)");
            }
        }
    }

    // 7. Computer player logic for Gomoku
    class GomokuComputer : GomokuPlayer
    {
        private Random rand = new();

        public GomokuComputer(string name, int symbol) : base(name, symbol) { }

        public override (int row, int col, int number) MakeMove(Board board)
        {
            for (int i = 0; i < board.Size; i++)
                for (int j = 0; j < board.Size; j++)
                {
                    if (!board.IsValidMove(i, j)) continue;
                    board.Cells[i][j] = Symbol;                          // Try this move
                    bool wins = CheckWin(board, i, j, Symbol);          // Check win
                    board.Cells[i][j] = null;                            // Undo it
                    if (wins) return (i, j, 0);                          // Pick this one
                }

            var options = new List<(int, int)>();
            for (int i = 0; i < board.Size; i++)
                for (int j = 0; j < board.Size; j++)
                    if (board.IsValidMove(i, j))
                        options.Add((i, j));

            var (r, c) = options[rand.Next(options.Count)];             // Just pick random
            return (r, c, 0);
        } // End of MakeMove() method

        private bool CheckWin(Board board, int row, int col, int symbol)
        {
            int Count(int dx, int dy)
            {
                int cnt = 0;
                int x = row + dx, y = col + dy;
                while (x >= 0 && x < board.Size &&
                       y >= 0 && y < board.Size &&
                       board.Cells[x][y] == symbol)
                {
                    cnt++;
                    x += dx;
                    y += dy;
                }
                return cnt;
            }

            return (Count(1, 0) + Count(-1, 0) + 1 >= 5) ||             // Horizontal
                   (Count(0, 1) + Count(0, -1) + 1 >= 5) ||             // Vertical
                   (Count(1, 1) + Count(-1, -1) + 1 >= 5) ||            // Diagonal \
                   (Count(1, -1) + Count(-1, 1) + 1 >= 5);              // Diagonal /
        }
    }
}
