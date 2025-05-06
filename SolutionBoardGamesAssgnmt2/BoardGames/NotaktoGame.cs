using System;
using System.Collections.Generic;
using System.Text.Json;

namespace BoardGames
{
    class NotaktoGame : Game
    {
        // Constants for messages and settings
        private const string BOARD_SIZE_MESSAGE = "Notakto only uses 3x3 boards. Board size set to 3.\n";
        private const string BOARD_COUNT_PROMPT = "How many boards? (recommended: 3):";
        private const string INVALID_BOARD_COUNT_MESSAGE = "Enter between 1 and 5 boards.";
        private const string GAME_MODE_PROMPT = "Select game mode:\n[1] Human vs Human\n[2] Human vs Computer";
        private const string INVALID_MODE_MESSAGE = "Invalid input. Try again.";
        private const string COMMANDS_MESSAGE = "Commands: move | save | undo | redo | help | quit";
        private const string MOVE_PROMPT = "Enter your move as: board row col (for example 0 1 1)";
        private const string INVALID_MOVE_MESSAGE = "Invalid move. Press Enter...";
        private const string DEAD_BOARD_MESSAGE = "That board is no longer playable. Press Enter...";
        private const string INVALID_FILENAME_MESSAGE = "Invalid filename. Press Enter...";
        private const string SAVE_FILENAME_PROMPT = "Enter filename to save (e.g., mygame): ";
        private const string UNKNOWN_COMMAND_MESSAGE = "Unknown command. Press Enter...";
        private const string NO_UNDO_MESSAGE = "Nothing to undo. Press Enter...";
        private const string NO_REDO_MESSAGE = "Nothing to redo. Press Enter...";
        private const string UNDO_SUCCESS_MESSAGE = "Undo successful. Press Enter...";
        private const string REDO_SUCCESS_MESSAGE = "Redo successful. Press Enter...";
        private const string RETURN_TO_MENU_MESSAGE = "Press Enter to return to the main menu...";
        private const string QUIT_MESSAGE = "Returning to main menu...";
        private const string HELP_CONTINUE_MESSAGE = "\nPress Enter to continue...";
        private const string JSON_EXTENSION = ".json";
        private const int MIN_BOARDS = 1;
        private const int MAX_BOARDS = 5;
        private const int BOARD_SIZE = 3;
        private const string HUMAN_MODE = "1";
        private const string COMPUTER_MODE = "2";


        // Public properties
        public List<Board> Boards { get; set; }
        public HashSet<int> DeadBoards { get; set; }

        // Undo/Redo stacks (not serialized)
        private Stack<string> UndoStack { get; set; } = new();
        private Stack<string> RedoStack { get; set; } = new();

        // Constructor
        public NotaktoGame()
        {
            Boards = new List<Board>();
            DeadBoards = new HashSet<int>();
        }

        // Game Setup
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

        // Main Game Loop
        public override void Start()
        {
            SaveSnapshot();
            while (true)
            {
                Console.Clear();
                DisplayBoards();
                Console.WriteLine($"{GetCurrentPlayer().Name}'s turn.");
                Console.WriteLine(COMMANDS_MESSAGE);
                Console.Write("Enter command: ");
                string input = Console.ReadLine()?.Trim().ToLower();

                switch (input)
                {
                    case "move":
                        if (!TryApplyHumanMove()) return;
                        SaveSnapshot();
                        if (IsHumanVsComputer)
                        {
                            if (!TryApplyComputerMove()) return;
                            SaveSnapshot();
                        }
                        break;

                    case "save":
                        HandleSaveGame();
                        break;

                    case "undo":
                        Undo();
                        break;

                    case "redo":
                        Redo();
                        break;

                    case "help":
                        ShowHelp();
                        DisplayMessageAndPause(HELP_CONTINUE_MESSAGE);
                        break;

                    case "quit":
                        Console.WriteLine(QUIT_MESSAGE);
                        return;

                    default:
                        DisplayMessageAndPause(UNKNOWN_COMMAND_MESSAGE);
                        break;
                }
            }
        }

        // Handle human move
        private bool TryApplyHumanMove()
        {
            Console.WriteLine(MOVE_PROMPT);
            var player = (NotaktoPlayer)GetCurrentPlayer();
            var (boardIndex, row, col) = player.MakeMove(Boards);

            if (!ValidateMove(boardIndex, row, col, player.Name))
                return true;

            return ApplyMoveAndCheckEnd(boardIndex, row, col, player.Name);
        }

        // Handle computer move
        private bool TryApplyComputerMove()
        {
            var computer = (NotaktoComputer)GetCurrentPlayer();
            var (boardIndex, row, col) = computer.MakeMove(Boards, DeadBoards);

            if (!ValidateMove(boardIndex, row, col, computer.Name))
                return true;

            return ApplyMoveAndCheckEnd(boardIndex, row, col, computer.Name);
        }

        // Display all boards
        private void DisplayBoards()
        {
            for (int i = 0; i < Boards.Count; i++)
            {
                bool isDead = DeadBoards.Contains(i);
                Console.WriteLine($"Board {i} {(isDead ? "[DEAD]" : "[ACTIVE]")}");
                DisplayNotaktoBoard(Boards[i], isDead);
                Console.WriteLine();
            }
        }

        // Display single board
        private void DisplayNotaktoBoard(Board board, bool isDead)
        {
            var winningCells = isDead ? GetWinningCells(board) : new HashSet<(int, int)>();
            RenderBoard(board, winningCells, isDead);
        }

        // Helpers for three-in-a-row logic
        private bool HasThreeInARow(Board board) => HasThreeInARowStatic(board);

        public static bool HasThreeInARowStatic(Board board)
        {
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

        private bool CheckLine(int?[] line) => CheckLineStatic(line);

        public static bool CheckLineStatic(int?[] line)
        {
            foreach (var cell in line)
                if (cell != 1) return false;
            return true;
        }

        // Snapshot management
        private void SaveSnapshot()
        {
            string json = SerializeGameState();
            UndoStack.Push(json);
            RedoStack.Clear();
        }

        private void Undo()
        {
            if (UndoStack.Count <= 1)
            {
                DisplayMessageAndPause(NO_UNDO_MESSAGE);
                return;
            }

            if (IsHumanVsComputer)
            {
                if (UndoStack.Count >= 2)
                {
                    RedoStack.Push(UndoStack.Pop()); // Computer move
                    RedoStack.Push(UndoStack.Pop()); // Player move
                    var restored = DeserializeGameState(UndoStack.Peek());
                    CopyFrom(restored);
                    DisplayMessageAndPause(UNDO_SUCCESS_MESSAGE);
                }
                else
                {
                    DisplayMessageAndPause("Not enough moves to undo. Press Enter...");
                    return;
                }
            }
            else
            {
                RedoStack.Push(UndoStack.Pop());
                var restored = DeserializeGameState(UndoStack.Peek());
                CopyFrom(restored);
                DisplayMessageAndPause(UNDO_SUCCESS_MESSAGE);
            }
        }

        private void Redo()
        {
            if (RedoStack.Count == 0)
            {
                DisplayMessageAndPause(NO_REDO_MESSAGE);
                return;
            }

            if (IsHumanVsComputer)
            {
                if (RedoStack.Count >= 2)
                {
                    string currentJson = SerializeGameState();
                    UndoStack.Push(currentJson);
                    string playerMove = RedoStack.Pop(); // Player move
                    string computerMove = RedoStack.Pop(); // Computer move
                    var restored = DeserializeGameState(computerMove);
                    CopyFrom(restored);
                    UndoStack.Push(playerMove);
                    DisplayMessageAndPause(REDO_SUCCESS_MESSAGE);
                }
                else
                {
                    DisplayMessageAndPause("Not enough moves to redo. Press Enter...");
                    return;
                }
            }
            else
            {
                string currentJson = SerializeGameState();
                UndoStack.Push(currentJson);
                string json = RedoStack.Pop();
                var restored = DeserializeGameState(json);
                CopyFrom(restored);
                DisplayMessageAndPause(REDO_SUCCESS_MESSAGE);
            }
        }

        private void CopyFrom(NotaktoGame other)
        {
            Boards = other.Boards;
            DeadBoards = other.DeadBoards;
            Player1 = other.Player1;
            Player2 = other.Player2;
            CurrentPlayerIndex = other.CurrentPlayerIndex;
            IsHumanVsComputer = other.IsHumanVsComputer;
        }


        // Helper Methods
        private static void DisplayMessageAndPause(string message)
        {
            Console.WriteLine(message);
            Console.ReadLine();
        }

        private string SerializeGameState()
        {
            return JsonSerializer.Serialize(this, GetType());
        }

        private NotaktoGame DeserializeGameState(string json)
        {
            return JsonSerializer.Deserialize(json, GetType()) as NotaktoGame;
        }

        private bool ValidateMove(int boardIndex, int row, int col, string playerName)
        {
            if (DeadBoards.Contains(boardIndex))
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

        private bool ApplyMoveAndCheckEnd(int boardIndex, int row, int col, string playerName)
        {
            Boards[boardIndex].Cells[row][col] = 1;
            if (HasThreeInARow(Boards[boardIndex]))
                DeadBoards.Add(boardIndex);

            if (DeadBoards.Count == Boards.Count)
            {
                Console.Clear();
                DisplayBoards();
                Console.WriteLine($"{playerName} loses! You completed the last playable move.");
                DisplayMessageAndPause(RETURN_TO_MENU_MESSAGE);
                return false;
            }

            SwitchPlayer();
            return true;
        }

        private void HandleSaveGame()
        {
            Console.Write(SAVE_FILENAME_PROMPT);
            string filename = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(filename))
            {
                DisplayMessageAndPause(INVALID_FILENAME_MESSAGE);
                return;
            }
            if (!filename.EndsWith(JSON_EXTENSION, StringComparison.OrdinalIgnoreCase))
                filename += JSON_EXTENSION;
            SaveGame(filename);
            DisplayMessageAndPause($"Game saved to '{filename}'. Press Enter...");
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
    }
}