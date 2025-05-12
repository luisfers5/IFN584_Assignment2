using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BoardGames
{
    public abstract class Game
    {

        private const string INVALID_FILENAME_MESSAGE = "Invalid filename. Press Enter...";
        private const string UNKNOWN_COMMAND_MESSAGE = "Unknown command. Try again";

        public Board Board { get; set; }
        public Player Player1 { get; set; }
        public Player Player2 { get; set; }
        public int CurrentPlayerIndex { get; set; } = 1;
        public bool IsHumanVsComputer { get; set; }

        [JsonInclude]
        public  UndoRedoManager UndoRedoManager;


        private  GamePersistence Persistence;

        protected Game()
        {
            UndoRedoManager = new UndoRedoManager();
            Persistence = new GamePersistence();
        }

        public abstract string GetFileSuffix();

        public virtual void Start()
        {
            // Initialize UndoStack with initial state
            UndoRedoManager.SaveSnapshot(this);
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
                        UndoRedoManager.SaveSnapshot(this); // Save with actual move count
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
                        Persistence.SaveGame(this, filename);
                        DisplayMessageAndPause($"Game saved to '{filename}'. Press Enter...");
                        break;

                    case "3":
                    case "undo":
                        UndoRedoManager.Undo(this);
                        break;

                    case "4":
                    case "redo":
                         UndoRedoManager.Redo(this);
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
            Console.WriteLine(" Help Menu:");
            Console.WriteLine(" move  - Make a move");
            Console.WriteLine(" save  - Save current game to file");
            Console.WriteLine(" undo  - Undo the previous move");
            Console.WriteLine(" redo  - Redo the last undone move");
            Console.WriteLine(" help  - Show this help menu");
            Console.WriteLine(" quit  - Return to the main menu");
        }

        public Player GetCurrentPlayer() => CurrentPlayerIndex == 1 ? Player1 : Player2;

        public void SwitchPlayer() => CurrentPlayerIndex = CurrentPlayerIndex == 1 ? 2 : 1;

                

        // 12. Copy state from another Game instance
        public virtual void CopyFrom(Game other)
        {
            Board = other.Board;
            Player1 = other.Player1;
            Player2 = other.Player2;
            CurrentPlayerIndex = other.CurrentPlayerIndex;
            IsHumanVsComputer = other.IsHumanVsComputer;
        } // End of CopyFrom() method

        protected static void DisplayMessageAndPause(string message)
        {
            Console.WriteLine(message);
            Console.ReadLine();
        }




    } 
}