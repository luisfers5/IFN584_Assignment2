using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace BoardGames
{
    // 1. Base class for all game types
    abstract class Game
    {
        public Board Board { get; set; }                                 // Shared board for the game
        public Player Player1 { get; set; }                              // First player
        public Player Player2 { get; set; }                              // Second player
        public int CurrentPlayerIndex { get; set; } = 1;                 // Tracks whose turn it is
        public bool IsHumanVsComputer { get; set; }                      // Used to check mode

        // Undo/Redo history (not serialized to file)
        protected Stack<string> UndoStack { get; set; } = new();
        protected Stack<string> RedoStack { get; set; } = new();

        // 2. Game loop
        public virtual void Start()
        {
            SaveSnapshot();                                              // Save initial state

            while (true)
            {
                Console.Clear();
                Board.Display();                                         // Show current board
                Console.WriteLine($"\n{GetCurrentPlayer().Name}'s turn.");
                Console.WriteLine("Commands: move | save | undo | redo | help | quit");
                Console.Write("Enter command: ");

                string input = Console.ReadLine()?.Trim().ToLower();     // Get user input
                switch (input)
                {
                    case "move":
                        if (!TryApplyMove())
                            return;                      // game over
                        SaveSnapshot();
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
                        SaveGame(filename);
                        Console.WriteLine("Press Enter to continue...");
                        Console.ReadLine();
                        break;

                    case "undo":
                        Undo();                            // Revert last move
                        break;

                    case "redo":
                        Redo();                            // Redo a reverted move
                        break;

                    case "help":
                        ShowHelp();                        // Show commands
                        Console.WriteLine("\nPress Enter to continue...");
                        Console.ReadLine();
                        break;

                    case "quit":
                        Console.WriteLine("Returning to main menu...");
                        return;

                    default:
                        Console.WriteLine("Unknown command. Try again.");
                        Console.ReadLine();
                        break;
                }
            }
        } // End of Start() method

        // 3. Handles the actual move logic
        protected virtual bool TryApplyMove()
        {
            var player = GetCurrentPlayer();
            var move = player.MakeMove(Board);                           // Get move from player

            if (!Board.IsValidMove(move.row, move.col))                 // Check if move is allowed
            {
                Console.WriteLine("Invalid move. Press Enter...");
                Console.ReadLine();
                return false;
            }

            if (!Board.PlaceNumber(move.row, move.col, move.number))    // Try to place the number
            {
                Console.WriteLine("Failed to place number. Press Enter...");
                Console.ReadLine();
                return false;
            }

            SwitchPlayer();                                             // Flip turn to other player

            // Auto-move if it's the computer's turn
            if (GetCurrentPlayer() is ComputerPlayer comp)
            {
                Console.WriteLine("\nComputer is thinking...");
                var compMove = comp.MakeMove(Board);
                Board.PlaceNumber(compMove.row, compMove.col, compMove.number);

                SwitchPlayer();
                SaveSnapshot();                                         // Save state after comp move
            }

            return true;
        }

        // 4. Show help for in-game commands
        public virtual void ShowHelp()
        {
            Console.WriteLine("\n Help Menu:");
            Console.WriteLine(" move  - Make a move");
            Console.WriteLine(" save  - Save current game to file");
            Console.WriteLine(" undo  - Undo the previous move");
            Console.WriteLine(" redo  - Redo the last undone move");
            Console.WriteLine(" help  - Show this help menu");
            Console.WriteLine(" quit  - Return to the main menu");
        } // End of SHOWHELP method()

        // 5. Utility — get the player whose turn it is
        public Player GetCurrentPlayer() =>
            CurrentPlayerIndex == 1 ? Player1 : Player2;

        // 6. Switch to the other player
        public void SwitchPlayer() =>
            CurrentPlayerIndex = (CurrentPlayerIndex == 1) ? 2 : 1;

        // 7. Save the game state to a file
        public void SaveGame(string filename)
        {
            string baseName = Path.GetFileNameWithoutExtension(filename);    // Remove .json if present

            string suffix = this switch                                      // Determine game type
            {
                NumericalTTTGame _ => "TicTacToe",
                NotaktoGame _ => "Notakto",
                GomokuGame _ => "Gomoku",
                _ => "UnknownGame"
            };

            string sizePart = Board != null
                ? $"{Board.Size}x{Board.Size}"                              // Include board size
                : "";

            string mode = IsHumanVsComputer
                ? "HumanVsComputer"
                : "HumanVsHuman";

            string finalName = $"{baseName}_{suffix}_{sizePart}_{mode}.json";
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, finalName);

            string json = JsonSerializer.Serialize(this, GetType(), new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(path, json);                                  // Write to file
        } // End of SaveGame() method

        // 8. Load a game from a JSON file
        public static T LoadGame<T>(string filename) where T : Game
        {
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
            string json = File.ReadAllText(fullPath);
            return (T)JsonSerializer.Deserialize(json, typeof(T));          // Deserialize and return
        } //End of LoadGame() medhod

        // 9. Save current state into undo stack
        protected void SaveSnapshot()
        {
            string json = JsonSerializer.Serialize(this, GetType());
            UndoStack.Push(json);                                           // Save to undo history
            RedoStack.Clear();                                              // Clear redo history on new move
        } // End of SaveSnapshot() method

        // 10. Undo the last move
        protected void Undo()
        {
            if (UndoStack.Count <= 1)                                       // Nothing to undo
            {
                Console.WriteLine("Nothing to undo.");
                Console.ReadLine();
                return;
            }

            RedoStack.Push(UndoStack.Pop());                                // Move last state to redo
            var restored = JsonSerializer.Deserialize(
                UndoStack.Peek(), GetType()) as Game;
            CopyFrom(restored);                                             // Restore previous state

            Console.WriteLine("Move undone. Press Enter...");
            Console.ReadLine();
        } // End of Undo() method

        // 11. Redo a move that was undone
        protected void Redo()
        {
            if (RedoStack.Count == 0)
            {
                Console.WriteLine("Nothing to redo.");
                Console.ReadLine();
                return;
            }

            var restored = JsonSerializer.Deserialize(
                RedoStack.Pop(), GetType()) as Game;
            CopyFrom(restored);                                             // Restore from redo stack
            Console.WriteLine("Move redone. Press Enter...");
            Console.ReadLine();
        } // End of Redo() method

        // 12. Copy state from another Game instance
        protected void CopyFrom(Game other)
        {
            Board = other.Board;
            Player1 = other.Player1;
            Player2 = other.Player2;
            CurrentPlayerIndex = other.CurrentPlayerIndex;
            IsHumanVsComputer = other.IsHumanVsComputer;
        } // End of CopyFrom() method
    }
}
