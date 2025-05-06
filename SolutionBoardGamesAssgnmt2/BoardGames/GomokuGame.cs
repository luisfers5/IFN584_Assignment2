using System;
using System.Collections.Generic;

namespace BoardGames
{
    // 1. GomokuGame class handles setup and gameplay
    class GomokuGame : Game
    {

        // Constants
        private const int MIN_BOARD_SIZE = 5;
        private const int MAX_BOARD_SIZE = 15;
        private const int WINNING_LINE_LENGTH = 5;
        private const int PLAYER1_SYMBOL = 1; // X
        private const int PLAYER2_SYMBOL = 2; // O

        private const int CURRENT_PLAYER_INDEX = 1;
        private const string INVALID_SIZE_MESSAGE = "Invalid input! Enter a number between 5 and 15";
        private const string INVALID_MODE_MESSAGE = "Invalid input! Try again";
        private const string INVALID_MOVE_MESSAGE = "Cell is taken or out of bounds. Please choose a different cell";
        private const string PRESS_ENTER_MESSAGE = "Press Enter to return to the main menu...";
        private const string COMPUTER_THINKING_MESSAGE = "\nComputer is thinking...";


        public static new Game SetupNewGame()
        {
            int boardSize = GetValidBoardSize();
            var game = new GomokuGame
            {
                Board = new Board(boardSize),
                CurrentPlayerIndex = CURRENT_PLAYER_INDEX
            };

            game.IsHumanVsComputer = GetGameMode();
            game.Player1 = new GomokuPlayer("Player 1", PLAYER1_SYMBOL);

            game.Player2 = game.IsHumanVsComputer
                ? new GomokuComputer("Computer", PLAYER2_SYMBOL)
                : new GomokuPlayer("Player 2", PLAYER2_SYMBOL);

            return game;
        }

        private static int GetValidBoardSize()
        {
            Console.WriteLine($"Enter board size (recommended {MIN_BOARD_SIZE}–{MAX_BOARD_SIZE}):");
            int size;
            while (!int.TryParse(Console.ReadLine(), out size) || size < MIN_BOARD_SIZE || size > MAX_BOARD_SIZE)
            {
                Console.WriteLine(INVALID_SIZE_MESSAGE);
            }
            return size;
        }

        private static bool GetGameMode()
        {
            Console.WriteLine("Select game mode:\n[1 vs 1] -> [1]\n[1 vs PC] -> [2]");
            string mode;
            while ((mode = Console.ReadLine()?.Trim()) != "1" && mode != "2")
            {
                Console.WriteLine(INVALID_MODE_MESSAGE);
            }
            return mode == "2";
        }

        // 2. Handles move logic and checks win/tie
        protected override bool TryApplyMove()
        {
            // Cast to GomokuPlayer
            var currentPlayer = (GomokuPlayer)GetCurrentPlayer();
            // Get human move
            var (row, col, _) = currentPlayer.MakeMove(Board);

            if (TryPlaceMove(row, col, currentPlayer.Symbol))
                return true;

            return HandleGameState(currentPlayer, row, col);
        }

        private bool TryPlaceMove(int row, int col, int symbol)
        {
            if (!Board.IsValidMove(row, col))
            {
                Console.WriteLine(INVALID_MOVE_MESSAGE);
                Console.WriteLine("Press Enter to try again...");
                Console.ReadLine();
                return true;
            }

            Board.Cells[row][col] = symbol;
            return false;
        }

        private bool HandleGameState(GomokuPlayer player, int row, int col)
        {
            if (CheckGomokuWin(row, col, player.Symbol, player))
            {
                return false;
            }

            if (IsTie())
            {
                return false;
            }

            SwitchPlayer();
            return HandleComputerMove();
        }

        private bool HandleComputerMove()
        {
            if (IsHumanVsComputer && GetCurrentPlayer() is GomokuComputer comp)
            {
                Console.WriteLine(COMPUTER_THINKING_MESSAGE);
                var (row, col, _) = comp.MakeMove(Board);
                Board.Cells[row][col] = comp.Symbol;

                if (CheckGomokuWin(row, col, comp.Symbol, comp))
                {
                    return false;
                }

                if (IsTie())
                {
                    return false;
                }

                SwitchPlayer();
            }
            return true;
        }

        private bool IsTie()
        {
            if (Board.IsBoardFull())
            {
                DisplayGameResult("It's a tie!");
                return true;
            }
            return false;
        }

        private void DisplayGameResult(string message)
        {
            Console.Clear();
            Board.Display();
            Console.WriteLine($"\n{message}");
            Console.WriteLine(PRESS_ENTER_MESSAGE);
            Console.ReadLine();
        }


        // 3. Checks all directions from a move for 5-in-a-row
        private bool CheckGomokuWin(int row, int col, int symbol, GomokuPlayer player)
        {
            if (CheckLine(row, col, symbol, 1, 0) ||                    // Horizontal
                   CheckLine(row, col, symbol, 0, 1) ||                  // Vertical
                   CheckLine(row, col, symbol, 1, 1) ||                  // Diagonal \
                   CheckLine(row, col, symbol, 1, -1))                  // Diagonal /

            {
                DisplayGameResult($"{player.Name} wins!");
                return true;

            }
            return false;
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


}
