namespace BoardGames
{
    public class Menu
    {


        private GameLauncher launcher = new();
        
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
                        launcher.LoadGame();
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
                }else{
                    launcher.StartNewGame((GameType)selectedType);
                }

                
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

        

      
    }
}
