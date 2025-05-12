
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BoardGames;

public class UndoRedoManager
{

    private const string REDO_SUCCESS_MESSAGE = "Redo successful. Press Enter...";
    private const string NO_REDO_MESSAGE = "Nothing to redo. Press Enter...";
    private const string INVALID_FILENAME_MESSAGE = "Invalid filename. Press Enter...";
    private const string UNKNOWN_COMMAND_MESSAGE = "Unknown command. Try again";
    [JsonInclude]
    public Stack<String>  UndoStack;
    [JsonInclude]
    public Stack<String>  RedoStack;

   


    public UndoRedoManager(){ 
        UndoStack = new Stack<String>();
        RedoStack = new Stack<String>();
     }

    public void SaveSnapshot(Game game)
    {
        string json = JsonSerializer.Serialize(game, game.GetType());
        // Save to undo history
        UndoStack.Push(json);       
         // Clear redo history on new move                                    
        RedoStack.Clear();                                             
    } // End of SaveSnapshot() method



    public void Undo(Game game)
    {
        if (UndoStack.Count < 2)
        {
            Console.WriteLine("Nothing to undo. Press Enter...");
            Console.ReadLine();
            return;
        }


        RedoStack.Push(UndoStack.Pop());

        var restored = JsonSerializer.Deserialize(UndoStack.Peek(), game.GetType()) as Game;
        game.CopyFrom(restored);

        Console.WriteLine("Undo successful. Press Enter...");
        Console.ReadLine();
    } // End of Undo() method


    public void Redo(Game game)
    {
        if (RedoStack.Count < 1)
        {
            Console.WriteLine(NO_REDO_MESSAGE);
            Console.ReadLine();
            return;
        }


        UndoStack.Push(RedoStack.Pop());


        var restored = JsonSerializer.Deserialize(UndoStack.Peek(), game.GetType()) as Game;
        // Restore from redo stack
        game.CopyFrom(restored);
        Console.WriteLine(REDO_SUCCESS_MESSAGE);
        Console.ReadLine();
    } // End of Redo() method



}
