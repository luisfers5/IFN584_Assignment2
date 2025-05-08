using System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BoardGames;

public class GamePersistence
{
    public void SaveGame(Game game, string filename)
    {
        string suffix = game.GetFileSuffix();
        string sizePart = game.Board != null ? $"{game.Board.Size}x{game.Board.Size}" : "";
        string mode = game.IsHumanVsComputer ? "HumanVsComputer" : "HumanVsHuman";
        string finalName = $"{Path.GetFileNameWithoutExtension(filename)}_{suffix}_{sizePart}_{mode}.json";
        string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, finalName);

        string json = JsonSerializer.Serialize(game, game.GetType(), new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(path, json);
    } // End of SaveGame() method

    public static T LoadGame<T>(string filename) where T : Game
    {
        string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
        string json = File.ReadAllText(fullPath);
        T game = (T)JsonSerializer.Deserialize(json, typeof(T));
       

        var undoList = game.UndoRedoManager.UndoStack.ToList();
        var redoList = game.UndoRedoManager.RedoStack.ToList();

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

        game.UndoRedoManager.UndoStack = correctedUndo;

        var correctedRedo = new Stack<string>();
        for (int i = 0; i < redoList.Count - 1; i++)
        {
            correctedRedo.Push(redoList[i]);
        }
        game.UndoRedoManager.RedoStack = correctedRedo;

        Console.WriteLine("\n=== UndoStack (after manual reverse) ===");
        foreach (var s in game.UndoRedoManager.UndoStack)
        {
            Console.WriteLine(s.Substring(0, Math.Min(100, s.Length)) + "...");
        }

        return game;
    }
    //End of LoadGame() medhod
}
