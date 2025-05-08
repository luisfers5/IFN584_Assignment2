using System;

namespace BoardGames;

public class GomokuPlayer : Player
{
    public int Symbol { get; }

    public GomokuPlayer(string name, int symbol) : base(name)
    {
        Symbol = symbol;
    }

    public override (int row, int col, int number) MakeMove(List<Board> boards, Board board)
    {
        Console.WriteLine($"{Name}, enter move as: row col (for example 2 2, separated by space)");
        while (true)
        {
            var parts = Console.ReadLine()?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts?.Length == 2 &&
                int.TryParse(parts[0], out int row) &&
                int.TryParse(parts[1], out int col) &&
                row >= 0 && row < board.Size &&
                col >= 0 && col < board.Size)
            {
                // We ignore 'number' for Gomoku
                return (row, col, 0);                                
            }
            Console.WriteLine("Invalid input! Format: row col (e.g., 4 7)");
        }
    }
}





