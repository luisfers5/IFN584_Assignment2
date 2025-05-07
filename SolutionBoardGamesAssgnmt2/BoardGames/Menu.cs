namespace BoardGames
{
    public class Menu
    {
        public void Run()
        {
            while (true)
            {
                ShowMainMenu();
            }
        }

        private void ShowHeader()
        {
            Console.Clear();
            Console.WriteLine("Welcome to Board Games Suite");
            Console.WriteLine("Developed by: Team 20\n");
        }

        private void ShowMainMenu()
        {
            string error = null;

            while (true)
            {
                ShowHeader();

                Console.WriteLine("Please select an option:");
                Console.WriteLine("[1] Start New Game");
                Console.WriteLine("[2] Load Previous Game");
                Console.WriteLine("[3] Help");
                Console.WriteLine("[4] Quit");

                if (!string.IsNullOrEmpty(error))
                {
                    Console.WriteLine($"\n{error}");
                    error = null;
                }

                Console.Write("\nYour choice: ");
                string choice = Console.ReadLine()?.Trim();

                switch (choice)
                {
                    case "1":
                        ShowGameSelectionMenu();
                        break;

                    case "2":
                        LoadGame();
                        return;

                    case "3":
                        ShowHelp();
                        return;

                    case "4":
                        Console.WriteLine("Goodbye!");
                        Environment.Exit(0);
                        return;

                    default:
                        error = "Invalid option. Please try again.";
                        break;
                }
            }
        }

        private void ShowGameSelectionMenu()
        {
            string error = null;

            while (true)
            {
                ShowHeader();

                Console.WriteLine("Select a game to start:");
                Console.WriteLine("[1] Numerical Tic-Tac-Toe");
                Console.WriteLine("[2] Notakto");
                Console.WriteLine("[3] Gomoku");
                Console.WriteLine("[4] Go Back");

                if (!string.IsNullOrEmpty(error))
                {
                    Console.WriteLine($"\n{error}");
                    error = null;
                }

                Console.Write("\nYour choice: ");
                string input = Console.ReadLine()?.Trim();

                GameType? selectedType = input switch
                {
                    "1" => GameType.NumericalTTT,
                    "2" => GameType.Notakto,
                    "3" => GameType.Gomoku,
                    "4" => null, // Go back
                    _ => null
                };

                if (selectedType == null)
                {
                    if (input == "4") return;
                    error = "Invalid option. Please try again ";
                    continue;
                }

                var factory = GameFactory.GetFactory(selectedType.Value);
                var game = factory.SetupNewGame();
                StartGame(game);
                return;
            }
        }

        private void ShowHelp()
        {
            ShowHeader();
            Console.WriteLine("Game Instructions:\n");

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

            Console.WriteLine("Common In-Game Commands:");
            Console.WriteLine(" move  - Make a move");
            Console.WriteLine(" save  - Save current game to a file");
            Console.WriteLine(" undo  - Undo the previous move");
            Console.WriteLine(" redo  - Redo a previously undone move");
            Console.WriteLine(" help  - Show this help menu");
            Console.WriteLine(" quit  - Return to the main menu");

            Console.WriteLine("\nPress Enter to return to the main menu...");
            Console.ReadLine();
        }

        private void LoadGame()
        {
            ShowHeader();
            Console.WriteLine("Load Saved Game\n");

            string savePath = AppDomain.CurrentDomain.BaseDirectory;
            var allJsonFiles = Directory.GetFiles(savePath, "*.json");

            var saveFiles = new List<string>();
            foreach (var file in allJsonFiles)
            {
                string fn = Path.GetFileName(file).ToLower();
                if (!fn.EndsWith(".runtimeconfig.json") &&
                    !fn.EndsWith(".deps.json") &&
                    !fn.StartsWith("boardgames"))
                {
                    saveFiles.Add(file);
                }
            }

            if (saveFiles.Count == 0)
            {
                Console.WriteLine("No valid saved games found.");
                Console.WriteLine("Press Enter to return to the main menu...");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("Available saved games:");
            for (int i = 0; i < saveFiles.Count; i++)
            {
                Console.WriteLine($"[{i + 1}] {Path.GetFileName(saveFiles[i])}");
            }

            Console.Write("\nSelect a file by number : ");
            string input = Console.ReadLine()?.Trim();

            string selectedFile = null;

            if (int.TryParse(input, out int num) && num >= 1 && num <= saveFiles.Count)
            {
                selectedFile = Path.GetFileName(saveFiles[num - 1]);
            }
            

            if (selectedFile == null)
            {
                Console.WriteLine("Invalid selection. Press Enter to return to the main menu...");
                Console.ReadLine();
                return;
            }

            var nameWithoutExt = Path.GetFileNameWithoutExtension(selectedFile);
            var parts = nameWithoutExt.Split('_', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
            {
                Console.WriteLine("Filename does not include game type info. Press Enter...");
                Console.ReadLine();
                return;
            }

            string gameType = parts[1];
            Console.WriteLine("Debug  gameType: "+ gameType);
            try
            {
                Game game = gameType switch
                {
                    "TicTacToe" => Game.LoadGame<NumericalTTTGame>(selectedFile),
                    "Notakto" => Game.LoadGame<NotaktoGame>(selectedFile),
                    "Gomoku" => Game.LoadGame<GomokuGame>(selectedFile),
                    _ => throw new Exception($"Unrecognized game type '{gameType}'")
                };
                Console.WriteLine("Debug : "+ selectedFile);
                
                StartGame(game);
            }
            catch (Exception ex)
            {
                ShowHeader();
                Console.WriteLine($"Error loading game: {ex.Message}");
                Console.WriteLine("Press Enter to try again...");
                Console.ReadLine();
            }
        }

        private void StartGame(Game game)
        {
            try
            {
                game.Start();
            }
            catch (Exception ex)
            {
                ShowHeader();
                Console.WriteLine($"Unexpected error: {ex.Message}");
                Console.WriteLine($"Unexpected error: {ex}");
                Console.WriteLine("Press Enter to return to the main menu...");
                Console.ReadLine();
            }
        }
    }
}
