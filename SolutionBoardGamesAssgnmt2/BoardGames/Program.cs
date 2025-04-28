using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;

namespace BoardGames
{
    // 1. Class Declaration
    class Program
    {
        // 2. Main Loop
        static void Main(string[] args)
        {
            while (true)                    // Infinite loop to keep showing the main menu
            {
                MainMenu();                // Just keeps returning here after any action
            }
        }// End of Main Loop

        // 3. UI Header
        static void DisplayHeader()
        {
            Console.Clear();                                              // Clears the console for a clean display
            Console.WriteLine("Welcome to Board Games Suite");           // Game title
            Console.WriteLine("Developed by: Team XXXX\n");              // Placeholder for dev credit
        }// End of UI Header

        // 4. Main Menu System
        static void MainMenu()
        {
            string error = null;                 // Used to show error messages after invalid input

            while (true)
            {
                DisplayHeader();                // Clear and show the game title/header again

                // Show menu options
                Console.WriteLine("Please select an option:");
                Console.WriteLine("[1] Numerical Tic-Tac-Toe");
                Console.WriteLine("[2] Notakto");
                Console.WriteLine("[3] Gomoku");
                Console.WriteLine("[4] Load Previous Game");
                Console.WriteLine("[5] Help");
                Console.WriteLine("[6] Quit");

                if (!string.IsNullOrEmpty(error))        // If there was a previous error, display it
                {
                    Console.WriteLine($"\n{error}");
                    error = null;                        // Clear it so it doesn't repeat
                }

                Console.Write("\nYour choice: ");
                string choice = Console.ReadLine()?.Trim().ToUpper();     // Grab input from user

                // Handle each option
                switch (choice)
                {
                    case "1":
                        StartGame(NumericalTTTGame.SetupNewGame());     // Start Numerical TTT
                        return;

                    case "2":
                        StartGame(NotaktoGame.SetupNewGame());          // Start Notakto
                        return;

                    case "3":
                        StartGame(GomokuGame.SetupNewGame());           // Start Gomoku
                        return;

                    case "4":
                        LoadGame();                                     // Load a saved game
                        return;

                    case "5":
                        ShowHelp();                                     // Show help menu
                        return;

                    case "6":
                        Console.WriteLine("Goodbye!");                  // Exit message
                        Environment.Exit(0);                            // Kill the program
                        return;

                    default:
                        error = "Invalid option. Please try again.";    // If input doesn't match
                        break;
                }
            }
        } // End of MainMenu() method

        // 5. Help Menu
        static void ShowHelp()
        {
            DisplayHeader();
            Console.WriteLine("Game Instructions:\n");

            // Quick help for each game
            Console.WriteLine("Numerical Tic-Tac-Toe:");
            Console.WriteLine(" - NxN grid.");
            Console.WriteLine(" - Players use odd/even numbers.");
            Console.WriteLine(" - First to create a line with the correct sum wins.\n");

            Console.WriteLine("Notakto:");
            Console.WriteLine(" - Multiple 3×3 boards.");
            Console.WriteLine(" - Players both place Xs.");
            Console.WriteLine(" - If you complete a three-in-a-row, you lose.\n");

            Console.WriteLine("Gomoku:");
            Console.WriteLine(" - Large board (e.g. 15×15).");
            Console.WriteLine(" - Players alternate placing X and O.");
            Console.WriteLine(" - First to form a line of five wins.\n");

            // Explain shared commands
            Console.WriteLine("Common In-Game Commands:");
            Console.WriteLine(" move  - Make a move");
            Console.WriteLine(" save  - Save current game to a file");
            Console.WriteLine(" undo  - Undo the previous move");
            Console.WriteLine(" redo  - Redo a previously undone move");
            Console.WriteLine(" help  - Show this help menu");
            Console.WriteLine(" quit  - Return to the main menu");

            Console.WriteLine("\nPress Enter to return to the main menu...");
            Console.ReadLine();
        } // End of ShowHelp() method

        // 6. Load Game From File
        static void LoadGame()
        {
            DisplayHeader();
            Console.WriteLine("Load Saved Game\n");

            string savePath = AppDomain.CurrentDomain.BaseDirectory;       // Get path to where app is running
            var allJsonFiles = Directory.GetFiles(savePath, "*.json");     // Get all .json files there

            var saveFiles = new List<string>();
            foreach (var file in allJsonFiles)
            {
                string fn = Path.GetFileName(file).ToLower();
                if (!fn.EndsWith(".runtimeconfig.json") &&                 // Filter out system json files
                    !fn.EndsWith(".deps.json") &&
                    !fn.StartsWith("boardgames"))
                {
                    saveFiles.Add(file);                                   // Add valid game files
                }
            }

            if (saveFiles.Count == 0)                                      // No save files? Notify and return
            {
                Console.WriteLine("No valid saved games found.");
                Console.WriteLine("Press Enter to return to the main menu...");
                Console.ReadLine();
                return;
            }

            // Show save files
            Console.WriteLine("Available saved games:");
            for (int i = 0; i < saveFiles.Count; i++)
            {
                Console.WriteLine($"[{i + 1}] {Path.GetFileName(saveFiles[i])}");
            }

            Console.Write("\nSelect a file by number or enter a filename manually: ");
            string input = Console.ReadLine()?.Trim();

            string selectedFile = null;

            // If number entered, pick that save
            if (int.TryParse(input, out int num) && num >= 1 && num <= saveFiles.Count)
            {
                selectedFile = Path.GetFileName(saveFiles[num - 1]);
            }
            else if (!string.IsNullOrEmpty(input)) // If they typed a filename
            {
                string full = Path.Combine(savePath, input);
                if (File.Exists(full) &&
                    !input.EndsWith(".runtimeconfig.json") &&
                    !input.EndsWith(".deps.json") &&
                    !input.StartsWith("boardgames"))
                {
                    selectedFile = input;       // Accept valid custom filename
                }
            }

            if (selectedFile == null)           // Nothing valid selected
            {
                Console.WriteLine("Invalid selection. Press Enter to return to the main menu...");
                Console.ReadLine();
                return;
            }

            // Try to figure out which game type it is
            var nameWithoutExt = Path.GetFileNameWithoutExtension(selectedFile);
            var parts = nameWithoutExt.Split('_', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
            {
                Console.WriteLine("Filename does not include game type info. Press Enter...");
                Console.ReadLine();
                return;
            }

            string gameType = parts[1];
            try
            {
                Game game = gameType switch
                {
                    "TicTacToe" => Game.LoadGame<NumericalTTTGame>(selectedFile),
                    "Notakto" => Game.LoadGame<NotaktoGame>(selectedFile),
                    "Gomoku" => Game.LoadGame<GomokuGame>(selectedFile),
                    _ => throw new Exception($"Unrecognized game type '{gameType}'")
                };

                StartGame(game);    // Launch the loaded game
            }
            catch (Exception ex)   // Handle any load issues
            {
                DisplayHeader();
                Console.WriteLine($"Error loading game: {ex.Message}");
                Console.WriteLine("Press Enter to try again...");
                Console.ReadLine();
            }
        } // End of LoadGame() method

        // 7. Start Game Launcher
        static void StartGame(Game game)
        {
            try
            {
                game.Start();     // Calls the Start method on whatever game is passed in
            }
            catch (Exception ex)
            {
                DisplayHeader();
                Console.WriteLine($"Unexpected error: {ex.Message}");
                Console.WriteLine("Press Enter to return to the main menu...");
                Console.ReadLine();
            }
        }// End of StartGame() method

    }
}
