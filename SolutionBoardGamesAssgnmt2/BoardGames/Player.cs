using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BoardGames
{
    // Polymorphic setup for JSON serialization — tells the deserializer what type to load
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(HumanPlayer), "human")]
    [JsonDerivedType(typeof(ComputerPlayer), "computer")]
    [JsonDerivedType(typeof(GomokuPlayer), "gomoku")]
    [JsonDerivedType(typeof(GomokuComputer), "gomoku_computer")]
    [JsonDerivedType(typeof(NotaktoPlayer), "notakto")]
    [JsonDerivedType(typeof(NotaktoComputer), "notakto_computer")]

    // 1. Abstract base class for any type of player
    abstract class Player
    {
        public string Name { get; set; }                                  // Player's name

        public List<int> AvailableNumbers { get; set; }                   // For Numerical TTT (optional elsewhere)

        // Default constructor — required for deserialization
        protected Player()
        {
            Name = "Unknown";                                             // Generic default name
            AvailableNumbers = new List<int>();                           // Starts with an empty number list
        }

        // Constructor with parameters — used for setting name and numbers directly
        protected Player(string name, List<int> numbers)
        {
            Name = name;                                                  // Assign given name
            AvailableNumbers = numbers ?? new List<int>();                // Null-safe assignment
        }

        // Abstract move method — each player type (human or AI) implements it differently
        public abstract (int row, int col, int number) MakeMove(Board board);
    }
}
