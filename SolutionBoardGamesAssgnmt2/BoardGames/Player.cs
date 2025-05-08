using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BoardGames
{
    // Polymorphic setup for JSON serialization — tells the deserializer what type to load
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(NumericalTTTPlayer), "human")]
    [JsonDerivedType(typeof(NumericalTTTComputer), "computer")]
    [JsonDerivedType(typeof(GomokuPlayer), "gomoku")]
    [JsonDerivedType(typeof(GomokuComputer), "gomoku_computer")]
    [JsonDerivedType(typeof(NotaktoPlayer), "notakto")]
    [JsonDerivedType(typeof(NotaktoComputer), "notakto_computer")]

    // 1. Abstract base class for any type of player
    public abstract class Player
    {
        public string Name { get; set; }                                  // Player's name

 

        // Default constructor — required for deserialization
        protected Player()
        {
            Name = "Unknown";                                             // Generic default name
        }

        // Constructor with parameters — used for setting name and numbers directly
        protected Player(string name)
        {
            Name = name;                                                  // Assign given name
        }

        // Abstract move method — each player type (human or Computer) implements it differently
        public abstract (int row, int col, int number) MakeMove(List<Board> boards, Board board);
    }
}
