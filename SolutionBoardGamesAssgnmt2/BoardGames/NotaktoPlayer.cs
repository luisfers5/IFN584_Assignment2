using System;

namespace BoardGames;

// 14. Human player class
public class NotaktoPlayer : Player
{
    public NotaktoPlayer(string name) : base(name, null) { }  // pass name to base

    public override (int row, int col, int number) MakeMove(Board board)
        => throw new NotImplementedException("Use multi-board version instead");  // single-board not used

    public virtual (int board, int row, int col) MakeMove(List<Board> boards)
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
                return (b, r, c);  // valid move
            }
            Console.WriteLine("Invalid input. Format: board row col (e.g. 0 1 1)");
        }
    }

    public virtual (int board, int row, int col) MakeMove(
        List<Board> boards, HashSet<int> deadBoards)
        => MakeMove(boards);  // ignore dead boards here
} // End of NotaktoPlayer class
