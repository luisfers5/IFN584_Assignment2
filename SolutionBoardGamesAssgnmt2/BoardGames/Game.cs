using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BoardGames
{
    public abstract class Game
    {
        private const string REDO_SUCCESS_MESSAGE = "Redo successful. Press Enter...";
        private const string UNDO_SUCCESS_MESSAGE = "Undo successful. Press Enter...";
        private const string NO_UNDO_MESSAGE = "Nothing to undo. Press Enter...";
        private const string NO_REDO_MESSAGE = "Nothing to redo. Press Enter...";
        private const string INVALID_FILENAME_MESSAGE = "Invalid filename. Press Enter...";
        private const string UNKNOWN_COMMAND_MESSAGE = "Unknown command. Try again";

        public Board Board { get; set; }
        public Player Player1 { get; set; }
        public Player Player2 { get; set; }
        public int CurrentPlayerIndex { get; set; } = 1;
        public bool IsHumanVsComputer { get; set; }

        [JsonInclude]
        public Stack<String> UndoStack { get; set; } = new();
        [JsonInclude]
        public Stack<String> RedoStack { get; set; } = new();

        public virtual void Start()
        {
            // Initialize UndoStack with initial state
            SaveSnapshot();
            while (true)
            {
                Console.Clear(); // debug !!
                DisplayBoard();
                Console.WriteLine($"\n{GetCurrentPlayer().Name}'s turn");
                Console.WriteLine("Commands:");
                Console.WriteLine("  1. Move");
                Console.WriteLine("  2. Save");
                Console.WriteLine("  3. Undo");
                Console.WriteLine("  4. Redo");
                Console.WriteLine("  5. Help");
                Console.WriteLine("  6. Quit");
                Console.Write("Enter command (1-6): ");

                string input = Console.ReadLine()?.Trim();
                switch (input)
                {
                    case "1":
                    case "move":
                        bool continueGame = TryApplyMove();
                        SaveSnapshot(); // Save with actual move count
                        if (!continueGame)
                            return;
                        break;

                    case "2":
                    case "save":
                        Console.Write("Enter filename to save (e.g., mygame): ");
                        string filename = Console.ReadLine()?.Trim();
                        if (string.IsNullOrWhiteSpace(filename))
                        {
                            DisplayMessageAndPause(INVALID_FILENAME_MESSAGE);
                            break;
                        }
                        SaveGame(filename);
                        DisplayMessageAndPause($"Game saved to '{filename}'. Press Enter...");
                        break;

                    case "3":
                    case "undo":
                        Undo();
                        break;

                    case "4":
                    case "redo":
                        Redo();
                        break;

                    case "5":
                    case "help":
                        ShowHelp();
                        DisplayMessageAndPause("\nPress Enter to continue...");
                        break;

                    case "6":
                    case "quit":
                        Console.WriteLine("Returning to main menu...");
                        return;

                    default:
                        DisplayMessageAndPause(UNKNOWN_COMMAND_MESSAGE);
                        break;
                }
            }
        }

        protected virtual void DisplayBoard()
        {
            Board?.Display();
        }

        protected abstract bool TryApplyMove();

        public virtual void ShowHelp()
        {
            Console.WriteLine("\nHelp Menu:");
            Console.WriteLine(" move  - Make a move");
            Console.WriteLine(" save  - Save current game to file");
            Console.WriteLine(" undo  - Undo the previous move");
            Console.WriteLine(" redo  - Redo the last undone move");
            Console.WriteLine(" help  - Show this help menu");
            Console.WriteLine(" quit  - Return to the main menu");
        }

        public Player GetCurrentPlayer() => CurrentPlayerIndex == 1 ? Player1 : Player2;

        public void SwitchPlayer() => CurrentPlayerIndex = CurrentPlayerIndex == 1 ? 2 : 1;

        public void SaveGame(string filename)
        {
            string baseName = Path.GetFileNameWithoutExtension(filename);
            string suffix = this switch
            {
                NumericalTTTGame _ => "TicTacToe",
                NotaktoGame _ => "Notakto",
                GomokuGame _ => "Gomoku",
                _ => "UnknownGame"
            };

            string sizePart = Board != null ? $"{Board.Size}x{Board.Size}" : "";
            string mode = IsHumanVsComputer ? "HumanVsComputer" : "HumanVsHuman";
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
            T game = (T)JsonSerializer.Deserialize(json, typeof(T));

            
            var undoList = game.UndoStack.ToList();
            var redoList = game.RedoStack.ToList();

            Console.WriteLine("=== UndoStack (original) ===");
            foreach (var s in undoList)
            {
                Console.WriteLine(s.Substring(0, Math.Min(100, s.Length)) + "...");
            }

            var correctedUndo = new Stack<string>();
            for (int i = 0; i < undoList.Count - 1; i++)
            {
                correctedUndo.Push(undoList[i]);
            }
            
            game.UndoStack = correctedUndo;

            var correctedRedo = new Stack<string>();
            for (int i = 0; i < redoList.Count - 1; i++)
            {
                correctedRedo.Push(redoList[i]);
            }
            game.RedoStack = correctedRedo;

            Console.WriteLine("\n=== UndoStack (after manual reverse) ===");
            foreach (var s in game.UndoStack)
            {
                Console.WriteLine(s.Substring(0, Math.Min(100, s.Length)) + "...");
            }

            return game;
        }
        //End of LoadGame() medhod

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
            if (UndoStack.Count < 2)
            {
                Console.WriteLine("Nothing to undo. Press Enter...");
                Console.ReadLine();
                return;
            }


            RedoStack.Push(UndoStack.Pop());

            var restored = JsonSerializer.Deserialize(UndoStack.Peek(), GetType()) as Game;
            CopyFrom(restored);

            Console.WriteLine("Undo successful. Press Enter...");
            Console.ReadLine();
        } // End of Undo() method

        // 11. Redo a move that was undone
        protected void Redo()
        {
            if (RedoStack.Count < 1)
            {
                Console.WriteLine(NO_REDO_MESSAGE);
                Console.ReadLine();
                return;
            }


            UndoStack.Push(RedoStack.Pop());


            var restored = JsonSerializer.Deserialize(UndoStack.Peek(), GetType()) as Game;
            // Restore from redo stack
            CopyFrom(restored);
            Console.WriteLine(REDO_SUCCESS_MESSAGE);
            Console.ReadLine();
        } // End of Redo() method

        // 12. Copy state from another Game instance
        protected virtual void CopyFrom(Game other)
        {
            Board = other.Board;
            Player1 = other.Player1;
            Player2 = other.Player2;
            CurrentPlayerIndex = other.CurrentPlayerIndex;
            IsHumanVsComputer = other.IsHumanVsComputer;
            Console.WriteLine("UndoStack count: " + UndoStack.Count);
            Console.WriteLine("RedoStack count: " + RedoStack.Count);
        } // End of CopyFrom() method

        protected static void DisplayMessageAndPause(string message)
        {
            Console.WriteLine(message);
            Console.ReadLine();
        }




    }
}