using System;
using System.Collections.Generic;

namespace BoardGames
{
    class NumericalTTTGame : Game
    {
        // Constants for messages and settings
        private const string BOARD_SIZE_PROMPT = "Enter board size n (for an NxN board): ";
        private const string INVALID_SIZE_MESSAGE = "Invalid input. Please enter a positive integer greater or equal to 3";
        private const string GAME_MODE_PROMPT = "Select game mode:\n[1 vs 1] -> [1]\n[1 vs PC] -> [2]";
        private const string INVALID_MODE_MESSAGE = "Invalid input. Try again";
        private const string INVALID_MOVE_MESSAGE = "Invalid move. Press Enter...";
        private const string COMPUTER_THINKING_MESSAGE = " Computer is thinking...";
        private const string TIE_MESSAGE = " It's a tie!";
        private const string RETURN_TO_MENU_MESSAGE = "Press Enter to return to the main menu...";
        private const string HUMAN_MODE = "1";
        private const string COMPUTER_MODE = "2";

        // Constants for line types in SumLine
        private const string LINE_TYPE_ROW = "row";
        private const string LINE_TYPE_COLUMN = "column";
        private const string LINE_TYPE_MAIN_DIAGONAL = "main_diagonal";
        private const string LINE_TYPE_ANTI_DIAGONAL = "anti_diagonal";

        public override string GetFileSuffix() => "TicTacToe";

        // Static setup method
        public static new Game SetupNewGame()
        {
            int size = GetValidBoardSize();
            bool isHumanVsComputer = GetValidGameMode();

            var newGame = new NumericalTTTGame
            {
                Board = new Board(size),
                IsHumanVsComputer = isHumanVsComputer,
                CurrentPlayerIndex = 1
            };

            (List<int> odds, List<int> evens) = InitializeNumberPools(size);
            newGame.Player1 = new NumericalTTTPlayer("Player 1", odds);
            newGame.Player2 = isHumanVsComputer
                ? new NumericalTTTComputer("Computer", evens)
                : new NumericalTTTPlayer("Player 2", evens);

            return newGame;
        }

        // Override the move-applying logic
        protected override bool TryApplyMove()
        {
            var player = GetCurrentPlayer();
            if (!ApplyAndValidateMove(player))
                return false;

            if (HandleGameEnd(player))
                return false;

            if (IsHumanVsComputer && GetCurrentPlayer() is NumericalTTTComputer comp)
            {
                Console.WriteLine(COMPUTER_THINKING_MESSAGE);
                if (!ApplyAndValidateMove(comp))
                    return false;

                if (HandleGameEnd(comp))
                    return false;
            }

            return true;
        }

        // Instance helper: delegates to static win checker
        private bool CheckWin() => CheckWinStatic(Board);

        // Static win detection
        public static bool CheckWinStatic(Board board)
        {
            int n = board.Size;
            int winSum = n * (n * n + 1) / 2;

            // Check rows
            for (int i = 0; i < n; i++)
                if (SumLine(board, LINE_TYPE_ROW, i) == winSum)
                    return true;

            // Check columns
            for (int j = 0; j < n; j++)
                if (SumLine(board, LINE_TYPE_COLUMN, j) == winSum)
                    return true;

            // Check diagonals
            if (SumLine(board, LINE_TYPE_MAIN_DIAGONAL, 0) == winSum)
                return true;
            if (SumLine(board, LINE_TYPE_ANTI_DIAGONAL, 0) == winSum)
                return true;

            return false;
        }

        // Helper methods
        private static int GetValidBoardSize()
        {
            while (true)
            {
                Console.WriteLine(BOARD_SIZE_PROMPT);
                if (int.TryParse(Console.ReadLine(), out int size) && size >= 3)
                {
                    Console.Clear();
                    return size;
                }
                Console.WriteLine(INVALID_SIZE_MESSAGE);
            }
        }

        private static bool GetValidGameMode()
        {
            Console.WriteLine(GAME_MODE_PROMPT);
            string mode;
            while ((mode = Console.ReadLine()) != HUMAN_MODE && mode != COMPUTER_MODE)
            {
                Console.Clear();
                Console.WriteLine(INVALID_MODE_MESSAGE);
                Console.WriteLine(GAME_MODE_PROMPT);
            }
            return mode == COMPUTER_MODE;
        }

        private static (List<int> odds, List<int> evens) InitializeNumberPools(int size)
        {
            var allNumbers = new List<int>();
            for (int i = 1; i <= size * size; i++)
                allNumbers.Add(i);

            var odds = new List<int>();
            var evens = new List<int>();
            foreach (int num in allNumbers)
            {
                if (num % 2 == 0)
                    evens.Add(num);
                else
                    odds.Add(num);
            }
            return (odds, evens);
        }

        private bool ApplyAndValidateMove(Player player)
        {
            var (row, col, number) = player.MakeMove(null, Board);
            if (!Board.PlaceNumber(row, col, number))
            {
                DisplayMessageAndPause(INVALID_MOVE_MESSAGE);
                return false;
            }
            SwitchPlayer();
            return true;
        }

        private bool HandleGameEnd(Player player)
        {
            if (CheckWin())
            {
                Console.Clear();
                Board.Display();
                Console.WriteLine($"\n{player.Name} wins!");
                DisplayMessageAndPause(RETURN_TO_MENU_MESSAGE);
                return true;
            }

            if (Board.IsBoardFull())
            {
                Console.Clear();
                Board.Display();
                Console.WriteLine(TIE_MESSAGE);
                DisplayMessageAndPause(RETURN_TO_MENU_MESSAGE);
                return true;
            }

            return false;
        }

        private static void DisplayMessageAndPause(string message)
        {
            Console.WriteLine(message);
            Console.ReadLine();
        }

        private static int SumLine(Board board, string lineType, int index)
        {
            int sum = 0;
            bool full = true;
            for (int i = 0; i < board.Size; i++)
            {
                int row, col;
                switch (lineType)
                {
                    case LINE_TYPE_ROW:
                        row = index;
                        col = i;
                        break;
                    case LINE_TYPE_COLUMN:
                        row = i;
                        col = index;
                        break;
                    case LINE_TYPE_MAIN_DIAGONAL:
                        row = i;
                        col = i;
                        break;
                    case LINE_TYPE_ANTI_DIAGONAL:
                        row = i;
                        col = board.Size - 1 - i;
                        break;
                    default:
                        throw new ArgumentException("Invalid line type");
                }

                if (board.Cells[row][col] == null)
                {
                    full = false;
                    break;
                }
                sum += board.Cells[row][col].Value;
            }
            return full ? sum : 0;
        }
    }
}