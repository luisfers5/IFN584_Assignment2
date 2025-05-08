using System;

namespace BoardGames;

public class GameLauncher
{
    public void LoadGame()
    {
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
        Console.WriteLine("Debug  gameType: " + gameType);

        try
        {
            Game game = gameType switch
            {
                "TicTacToe" => GamePersistence.LoadGame<NumericalTTTGame>(selectedFile),
                "Notakto" => GamePersistence.LoadGame<NotaktoGame>(selectedFile),
                "Gomoku" => GamePersistence.LoadGame<GomokuGame>(selectedFile),
                _ => throw new Exception($"Unrecognized game type '{gameType}'")
            };
            Console.WriteLine("Debug : " + selectedFile);

            StartGame(game);
        }
        catch (Exception ex)
        {
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
            Console.WriteLine($"Unexpected error: {ex.Message}");
            Console.WriteLine($"Unexpected error: {ex}");
            Console.WriteLine("Press Enter to return to the main menu...");
            Console.ReadLine();
        }
    }
    public void StartNewGame(GameType type)
    {

        var factory = GameFactory.GetFactory(type);
        var game = factory.SetupNewGame();

        StartGame(game);
    }
}
