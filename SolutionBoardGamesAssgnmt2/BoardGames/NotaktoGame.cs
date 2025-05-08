using System;
using System.Collections.Generic;

namespace BoardGames
{
    class NotaktoGame : Game
    {
        private const string BOARD_SIZE_MESSAGE = "Notakto only uses 3x3 boards. Board size set to 3\n";
        private const string BOARD_COUNT_PROMPT = "How many boards? (recommended: 3):";
        private const string INVALID_BOARD_COUNT_MESSAGE = "Enter between 1 and 5 boards.";
        private const string GAME_MODE_PROMPT = "Select game mode:\n[1] Human vs Human\n[2] Human vs Computer";
        private const string INVALID_MODE_MESSAGE = "Invalid input. Try moments.";
        private const string MOVE_PROMPT = "Enter your move as: board row col (for example 0 1 1)";
        private const string INVALID_MOVE_MESSAGE = "Invalid move. Press Enter...";
        private const string DEAD_BOARD_MESSAGE = "That board is no longer playable. Press Enter...";
        private const string RETURN_TO_MENU_MESSAGE = "Press Enter to return to the main menu...";
        private const string COMPUTER_THINKING_MESSAGE = "\nComputer is thinking...";
        private const int MIN_BOARDS = 1;
        private const int MAX_BOARDS = 5;
        private const int BOARD_SIZE = 3;
        private const string HUMAN_MODE = "1";
        private const string COMPUTER_MODE = "2";

        public override string GetFileSuffix() => "Notakto";

        public List<Board> Boards { get; set; }
        public HashSet<int> DeadBoards { get; set; }

        public NotaktoGame()
        {
            Boards = new List<Board>();
            DeadBoards = new HashSet<int>();
        }

        public static new Game SetupNewGame()
        {
            Console.Clear();
            Console.WriteLine(BOARD_SIZE_MESSAGE);
            int boardCount = GetValidBoardCount();
            bool isHumanVsComputer = GetValidGameMode();

            var game = new NotaktoGame
            {
                Boards = new List<Board>(),
                DeadBoards = new HashSet<int>(),
                IsHumanVsComputer = isHumanVsComputer,
                CurrentPlayerIndex = 1
            };

            for (int i = 0; i < boardCount; i++)
                game.Boards.Add(new Board(BOARD_SIZE));

            game.Player1 = new NotaktoPlayer("Player 1");
            game.Player2 = isHumanVsComputer
                ? new NotaktoComputer("Computer")
                : new NotaktoPlayer("Player 2");

            return game;
        }

        protected override void DisplayBoard()
        {
            if (Boards == null || Boards.Count == 0)
            {
                Console.WriteLine("No boards available.");
                return;
            }

            for (int i = 0; i < Boards.Count; i++)
            {
                bool isDead = DeadBoards?.Contains(i) ?? false;
                Console.WriteLine($"Board {i} {(isDead ? "[DEAD]" : "[ACTIVE]")}");
                DisplayNotaktoBoard(Boards[i], isDead);
                Console.WriteLine();
            }
        }

        protected override bool TryApplyMove()
        {
            if (Boards == null || DeadBoards == null)
            {
                DisplayMessageAndPause("Game state is invalid. Press Enter...");
                return false;
            }

            var player = GetCurrentPlayer();
            if (!ApplyAndValidateMove(player))
                return true; // Invalid move, continue prompting

            if (HandleGameEnd(player))
                return false; // Game ended

            if (IsHumanVsComputer && GetCurrentPlayer() is NotaktoComputer comp)
            {
                Console.WriteLine(COMPUTER_THINKING_MESSAGE);
                if (!ApplyAndValidateMove(comp))
                    return true; // Invalid computer move, continue prompting

                if (HandleGameEnd(comp))
                    return false; // Game ended
            }

            return true;
        }

        public override void CopyFrom(Game other)
        {
            if (other is NotaktoGame notakto)
            {
                Boards = notakto.Boards ?? new List<Board>();
                DeadBoards = notakto.DeadBoards ?? new HashSet<int>();
                Player1 = notakto.Player1;
                Player2 = notakto.Player2;
                CurrentPlayerIndex = notakto.CurrentPlayerIndex;
                IsHumanVsComputer = notakto.IsHumanVsComputer;
            }
            else
            {
                Boards = new List<Board>();
                DeadBoards = new HashSet<int>();
            }
        }

        private bool ApplyAndValidateMove(Player player)
        {
            bool isComputer = player is NotaktoComputer;
            if (!isComputer)
                Console.WriteLine(MOVE_PROMPT);

            var (boardIndex, row, col) = isComputer
                ? ((NotaktoComputer)player).MakeMove(Boards, DeadBoards)
                : ((NotaktoPlayer)player).MakeMove(Boards, null);

            if (!ValidateMove(boardIndex, row, col, player.Name))
                return false;

            Boards[boardIndex].Cells[row][col] = 1;
            SwitchPlayer();
            return true;
        }

        private bool HandleGameEnd(Player player)
        {
            foreach (var board in Boards)
            {
                if (HasThreeInARowStatic(board))
                {
                    DeadBoards.Add(Boards.IndexOf(board));
                }
            }

            if (DeadBoards.Count == Boards.Count)
            {
                Console.Clear();
                DisplayBoard();
                Console.WriteLine($"{player.Name} loses! You completed the last playable move.");
                DisplayMessageAndPause(RETURN_TO_MENU_MESSAGE);
                return true;
            }

            return false;
        }

        private bool ValidateMove(int boardIndex, int row, int col, string playerName)
        {
            if (boardIndex < 0 || boardIndex >= Boards.Count || DeadBoards.Contains(boardIndex))
            {
                DisplayMessageAndPause(DEAD_BOARD_MESSAGE);
                return false;
            }

            if (!Boards[boardIndex].IsValidMove(row, col))
            {
                DisplayMessageAndPause(INVALID_MOVE_MESSAGE);
                return false;
            }

            return true;
        }

        private void DisplayNotaktoBoard(Board board, bool isDead)
        {
            if (board == null)
            {
                Console.WriteLine("Board is null");
                return;
            }

            var winningCells = isDead ? GetWinningCells(board) : new HashSet<(int, int)>();
            RenderBoard(board, winningCells, isDead);
        }

        private HashSet<(int, int)> GetWinningCells(Board board)
        {
            var winningCells = new HashSet<(int, int)>();
            int size = board.Size;

            for (int i = 0; i < size; i++)
            {
                if (CheckLineStatic(board.Cells[i]))
                    for (int j = 0; j < size; j++)
                        winningCells.Add((i, j));
                bool colWin = true;
                for (int j = 0; j < size; j++)
                    if (board.Cells[j][i] != 1) colWin = false;
                if (colWin)
                    for (int j = 0; j < size; j++)
                        winningCells.Add((j, i));
            }

            bool diag1 = true, diag2 = true;
            for (int i = 0; i < size; i++)
            {
                if (board.Cells[i][i] != 1) diag1 = false;
                if (board.Cells[i][size - 1 - i] != 1) diag2 = false;
            }
            if (diag1)
                for (int i = 0; i < size; i++)
                    winningCells.Add((i, i));
            if (diag2)
                for (int i = 0; i < size; i++)
                    winningCells.Add((i, size - 1 - i));

            return winningCells;
        }

        private void RenderBoard(Board board, HashSet<(int, int)> winningCells, bool isDead)
        {
            int size = board.Size;
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    string val = board.Cells[r][c] == null ? " " : "X";
                    if (isDead && winningCells.Contains((r, c)))
                        val = "*";
                    Console.Write($" {val} ");
                    if (c < size - 1) Console.Write("|");
                }
                Console.WriteLine();
                if (r < size - 1)
                    Console.WriteLine(new string('-', size * 4 - 1));
            }
        }

        private static int GetValidBoardCount()
        {
            Console.WriteLine(BOARD_COUNT_PROMPT);
            int boardCount;
            while (!int.TryParse(Console.ReadLine(), out boardCount) || boardCount < MIN_BOARDS || boardCount > MAX_BOARDS)
            {
                Console.Clear();
                Console.WriteLine(INVALID_BOARD_COUNT_MESSAGE);
            }
            return boardCount;
        }

        private static bool GetValidGameMode()
        {
            Console.Clear();
            Console.WriteLine(GAME_MODE_PROMPT);
            string mode;
            while ((mode = Console.ReadLine()?.Trim()) != HUMAN_MODE && mode != COMPUTER_MODE)
            {
                Console.Clear();
                Console.WriteLine(INVALID_MODE_MESSAGE);
                Console.WriteLine(GAME_MODE_PROMPT);
            }
            return mode == COMPUTER_MODE;
        }

        public static bool HasThreeInARowStatic(Board board)
        {
            if (board == null) return false;

            int size = board.Size;
            for (int i = 0; i < size; i++)
            {
                if (CheckLineStatic(board.Cells[i])) return true;
                var col = new int?[size];
                for (int j = 0; j < size; j++)
                    col[j] = board.Cells[j][i];
                if (CheckLineStatic(col)) return true;
            }
            var d1 = new int?[size];
            var d2 = new int?[size];
            for (int i = 0; i < size; i++)
            {
                d1[i] = board.Cells[i][i];
                d2[i] = board.Cells[i][size - 1 - i];
            }
            return CheckLineStatic(d1) || CheckLineStatic(d2);
        }

        public static bool CheckLineStatic(int?[] line)
        {
            if (line == null) return false;

            foreach (var cell in line)
                if (cell != 1) return false;
            return true;
        }
    }
}