using System;

namespace BoardGames;

// 14. Human player class
public class NotaktoPlayer : Player
{
    public NotaktoPlayer(string name) : base(name) { }  // pass name to base

    public override (int row, int col, int number) MakeMove(List<Board> boards, Board board)
    {
        while (true)
        {
            string input = Console.ReadLine();  // read user input
            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 3 &&
                int.TryParse(parts[0], out int b) &&
                int.TryParse(parts[1], out int r) &&
                int.TryParse(parts[2], out int c) &&
                b >= 0 && b < boards.Count &&
                r >= 0 && r < boards[b].Size &&
                c >= 0 && c < boards[b].Size)
            {
                return (r, c, b);  // valid move
            }
            Console.WriteLine("Invalid input. Format: board row col (e.g. 0 1 1)");
        }
    }

} // End of NotaktoPlayer class
