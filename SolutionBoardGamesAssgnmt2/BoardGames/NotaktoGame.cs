using System;
using System.Collections.Generic;
using System.Text.Json;

namespace BoardGames
{
    // 1. Class Declaration
    class NotaktoGame : Game   // define NotaktoGame as subclass of Game
    {
        // 2. Public properties
        public List<Board> Boards { get; set; }           // list of all boards
        public HashSet<int> DeadBoards { get; set; }      // indexes of boards that are dead

        // 3. Undo/Redo stacks (not serialized)
        private Stack<string> UndoStack { get; set; } = new();  // JSON snapshots for undo
        private Stack<string> RedoStack { get; set; } = new();  // JSON snapshots for redo

        // 4. Constructor
        public NotaktoGame()                               // initialize new game instance
        {
            Boards = new List<Board>();                // start with empty board list
            DeadBoards = new HashSet<int>();               // no dead boards yet
        }

        // 5. Game Setup
        public static new Game SetupNewGame()              // ask user for settings, return configured game
        {
            const int size = 3;                           // fixed board size for Notakto
            Console.Clear();                              // clear console
            Console.WriteLine("Notakto only uses 3x3 boards. Board size set to 3.\n");
            Console.WriteLine("How many boards? (recommended: 3):");

            int boardCount;                               // to store user input
            while (!int.TryParse(Console.ReadLine(), out boardCount)
                   || boardCount < 1 || boardCount > 5)   // ensure between 1 and 5
            {
                Console.Clear();                          // re-prompt
                Console.WriteLine("Enter between 1 and 5 boards.");
            }

            Console.Clear();                              // move on to mode selection
            Console.WriteLine("Select game mode:\n[1] Human vs Human\n[2] Human vs Computer");

            string mode;                                  // user choice
            while ((mode = Console.ReadLine()?.Trim()) != "1"
                   && mode != "2")                        // accept only "1" or "2"
            {
                Console.Clear();                          // re-prompt
                Console.WriteLine("Invalid input. Try again.");
                Console.WriteLine("Select game mode:\n[1] Human vs Human\n[2] Human vs Computer");
            }

            // instantiate and initialize game object
            var game = new NotaktoGame
            {
                Boards = new List<Board>(),   // reset boards list
                DeadBoards = new HashSet<int>(),  // reset dead set
                IsHumanVsComputer = mode == "2",         // set AI flag
                CurrentPlayerIndex = 1                    // start with player 1
            };

            for (int i = 0; i < boardCount; i++)         // add each board
                game.Boards.Add(new Board(size));        // each is 3×3

            game.Player1 = new NotaktoPlayer("Player 1");    // first player
            game.Player2 = game.IsHumanVsComputer
                        ? new NotaktoComputer("Computer")    // AI opponent
                        : new NotaktoPlayer("Player 2");     // human opponent

            return game;                                     // return configured game
        } // End of SetupNewGame() method

        // 6. Main Game Loop
        public override void Start()                     // run the game until quit
        {
            SaveSnapshot();                              // save initial state

            while (true)                                 // loop for each turn
            {
                Console.Clear();                         // clear screen
                DisplayBoards();                         // show current boards
                Console.WriteLine($"{GetCurrentPlayer().Name}'s turn.");
                Console.WriteLine("Commands: move | save | undo | redo | help | quit");
                Console.Write("Enter command: ");
                string input = Console.ReadLine()?.Trim().ToLower();

                switch (input)                           // handle user command
                {
                    case "move":
                        if (!TryApplyHumanMove()) return;  // human move, exit if game over
                        SaveSnapshot();

                        if (IsHumanVsComputer)            // if vs. computer
                        {
                            if (!TryApplyComputerMove()) return;
                            SaveSnapshot();
                        }
                        break;

                    case "save":
                        Console.Write("Enter filename to save (e.g., mygame): ");
                        string filename = Console.ReadLine()?.Trim();
                        if (string.IsNullOrWhiteSpace(filename))
                        {
                            Console.WriteLine("Invalid filename. Press Enter...");
                            Console.ReadLine();
                            break;
                        }
                        if (!filename.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                            filename += ".json";
                        SaveGame(filename);
                        Console.WriteLine($"Game saved to '{filename}'. Press Enter...");
                        Console.ReadLine();
                        break;

                    case "undo":
                        Undo();
                        break;

                    case "redo":
                        Redo();
                        break;

                    case "help":
                        ShowHelp();
                        Console.WriteLine("\nPress Enter to continue...");
                        Console.ReadLine();
                        break;

                    case "quit":
                        Console.WriteLine("Returning to main menu...");
                        return;

                    default:
                        Console.WriteLine("Unknown command. Press Enter...");
                        Console.ReadLine();
                        break;
                }
            }
        } // End of Start() method

        // 7. Handle human move
        private bool TryApplyHumanMove()
        {
            Console.WriteLine("Enter your move as: board row col (for example 0 1 1)");
            var player = (NotaktoPlayer)GetCurrentPlayer();
            var (boardIndex, row, col) = player.MakeMove(Boards);

            if (DeadBoards.Contains(boardIndex))
            {
                Console.WriteLine("That board is no longer playable. Press Enter...");
                Console.ReadLine();
                return true;
            }

            if (!Boards[boardIndex].IsValidMove(row, col))
            {
                Console.WriteLine("Invalid move. Press Enter...");
                Console.ReadLine();
                return true;
            }

            Boards[boardIndex].Cells[row][col] = 1;       // place X
            if (HasThreeInARow(Boards[boardIndex]))
                DeadBoards.Add(boardIndex);               // mark board dead

            if (DeadBoards.Count == Boards.Count)         // all boards dead?
            {
                Console.Clear();
                DisplayBoards();
                Console.WriteLine($"{player.Name} loses! You completed the last playable move.");
                Console.WriteLine("Press Enter to return to the main menu...");
                Console.ReadLine();
                return false;
            }

            SwitchPlayer();                              // switch turn
            return true;
        }

        // 8. Handle computer move
        private bool TryApplyComputerMove()
        {
            var computer = (NotaktoComputer)GetCurrentPlayer();
            var (boardIndex, row, col) = computer.MakeMove(Boards, DeadBoards);

            Boards[boardIndex].Cells[row][col] = 1;       // place X
            if (HasThreeInARow(Boards[boardIndex]))
                DeadBoards.Add(boardIndex);               // mark dead

            if (DeadBoards.Count == Boards.Count)         // all boards dead?
            {
                Console.Clear();
                DisplayBoards();
                Console.WriteLine("Computer loses! It completed the last playable move.");
                Console.WriteLine("Press Enter to return to the main menu...");
                Console.ReadLine();
                return false;
            }

            SwitchPlayer();                              // switch turn
            return true;
        }

        // 9. Display all boards
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

        // 10. Display single board
        private void DisplayNotaktoBoard(Board board, bool isDead)
        {
            int size = board.Size;
            var winningCells = new HashSet<(int, int)>();

            if (isDead)
            {
                // find winning rows & columns
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
                // check diagonals
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
            }

            // render grid
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
        } // End of DisplayNotaktoBoard() method

        // 11. Helpers for three‑in‑a‑row logic
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

        // 12. Snapshot management
        private void SaveSnapshot()
        {
            string json = JsonSerializer.Serialize(this, GetType());
            UndoStack.Push(json);
            RedoStack.Clear();
        }

        private void Undo()
        {
            if (UndoStack.Count <= 1)
            {
                Console.WriteLine("Nothing to undo. Press Enter...");
                Console.ReadLine();
                return;
            }
            RedoStack.Push(UndoStack.Pop());
            var restored = JsonSerializer.Deserialize(
                UndoStack.Peek(), GetType()) as NotaktoGame;
            CopyFrom(restored);
            Console.WriteLine("Undo successful. Press Enter...");
            Console.ReadLine();
        }

        private void Redo()
        {
            if (RedoStack.Count == 0)
            {
                Console.WriteLine("Nothing to redo. Press Enter...");
                Console.ReadLine();
                return;
            }
            var restored = JsonSerializer.Deserialize(
                RedoStack.Pop(), GetType()) as NotaktoGame;
            CopyFrom(restored);
            Console.WriteLine("Redo successful. Press Enter...");
            Console.ReadLine();
        }

        private void CopyFrom(NotaktoGame other)  // copy state from snapshot
        {
            Boards = other.Boards;
            DeadBoards = other.DeadBoards;
            Player1 = other.Player1;
            Player2 = other.Player2;
            CurrentPlayerIndex = other.CurrentPlayerIndex;
            IsHumanVsComputer = other.IsHumanVsComputer;
        }

        // 13. Help Display
        public override void ShowHelp()
        {
            Console.WriteLine("Notakto Help:");
            Console.WriteLine(" - Players take turns placing Xs on multiple boards.");
            Console.WriteLine(" - If you complete a three-in-a-row, that board is dead.");
            Console.WriteLine(" - If you complete the last live board, you lose.");
            Console.WriteLine(" - Enter moves as: board row col");
            Console.WriteLine(" - Boards and positions are zero-indexed.");
            base.ShowHelp();
        }

    } // End of NotaktoGame class

    // 14. Human player class
    class NotaktoPlayer : Player
    {
        public NotaktoPlayer(string name) : base(name, null) { }  // pass name to base

        public override (int row, int col, int number) MakeMove(Board board)
            => throw new NotImplementedException("Use multi-board version instead.");  // single-board not used

        public virtual (int board, int row, int col) MakeMove(List<Board> boards)
        {
            while (true)
            {
                string input = Console.ReadLine();  // read user input
                var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 3 &&
                    int.TryParse(parts[0], out int b) &&
                    int.TryParse(parts[1], out int r) &&
                    int.TryParse(parts[2], out int c) &&
                    b >= 0 && b < boards.Count &&
                    r >= 0 && r < boards[b].Size &&
                    c >= 0 && c < boards[b].Size)
                {
                    return (b, r, c);  // valid move
                }
                Console.WriteLine("Invalid input. Format: board row col (e.g. 0 1 1)");
            }
        }

        public virtual (int board, int row, int col) MakeMove(
            List<Board> boards, HashSet<int> deadBoards)
            => MakeMove(boards);  // ignore dead boards here
    } // End of NotaktoPlayer class

    // 15. Computer player class
    class NotaktoComputer : NotaktoPlayer
    {
        private Random rand = new();               // random move selector

        public NotaktoComputer(string name) : base(name) { }  // pass name up

        public override (int board, int row, int col) MakeMove(
            List<Board> boards, HashSet<int> deadBoards)
        {
            var options = new List<(int board, int row, int col)>();  // possible moves
            for (int b = 0; b < boards.Count; b++)
            {
                if (deadBoards.Contains(b)) continue;  // skip dead boards
                for (int i = 0; i < boards[b].Size; i++)
                    for (int j = 0; j < boards[b].Size; j++)
                        if (boards[b].Cells[i][j] == null)
                            options.Add((b, i, j));  // add empty cell
            }

            if (options.Count == 0)
                throw new Exception("No valid moves left.");  // should never happen

            // prefer moves that don't kill the last board
            foreach (var opt in options)
            {
                var (b, r, c) = opt;
                boards[b].Cells[r][c] = 1;
                bool killsLast = (deadBoards.Count
                                  + (NotaktoGame.HasThreeInARowStatic(boards[b]) ? 1 : 0))
                                 == boards.Count;
                boards[b].Cells[r][c] = null;
                if (!killsLast) return opt;  // choose safe move
            }

            return options[rand.Next(options.Count)];  // random fallback
        }
    } // End of NotaktoComputer class

} // End of BoardGames namespace
