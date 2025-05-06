using System;

namespace BoardGames;

// 15. Computer player class
class NotaktoComputer : NotaktoPlayer
{
    private Random rand = new();               // random move selector

    public NotaktoComputer(string name) : base(name) { }  // pass name up

    public override (int board, int row, int col) MakeMove(List<Board> boards, HashSet<int> deadBoards)
    {
        // Gather all possible moves on active boards
        var options = new List<(int board, int row, int col)>();
        for (int b = 0; b < boards.Count; b++)
        {
            if (deadBoards.Contains(b)) continue;
            for (int i = 0; i < boards[b].Size; i++)
                for (int j = 0; j < boards[b].Size; j++)
                    if (boards[b].Cells[i][j] == null)
                        options.Add((b, i, j));
        }

        // Classify moves into those that kill a board vs those that don't
        var killMoves = new List<(int board, int row, int col)>(); // moves that complete a three-in-a-row
        var safeMoves = new List<(int board, int row, int col)>(); // moves that do not

        int live = boards.Count - deadBoards.Count;

        foreach (var opt in options)
        {
            var (b, r, c) = opt;
            // simulate placing an X
            boards[b].Cells[r][c] = 1;
            bool kills = NotaktoGame.HasThreeInARowStatic(boards[b]);
            // revert simulation
            boards[b].Cells[r][c] = null;

            if (kills)
            {
                // only consider killing if it won't be the last board
                if (deadBoards.Count + 1 < boards.Count)
                    killMoves.Add(opt);
            }
            else
            {
                safeMoves.Add(opt);
            }
        }

        // 1. Prefer kill moves that leave an even number of boards (parity strategy)
        if (killMoves.Count > 0)
        {
            var goodKills = killMoves
                .Where(m => ((live - 1) % 2) == 0)
                .ToList();
            if (goodKills.Count > 0)
                return goodKills[rand.Next(goodKills.Count)];
            // if none maintain parity, pick any kill move
            return killMoves[rand.Next(killMoves.Count)];
        }

        // 2. If no kill move, pick a safe move at random
        if (safeMoves.Count > 0)
            return safeMoves[rand.Next(safeMoves.Count)];

        // 3. Fallback (should never happen): pick any available move
        return options[rand.Next(options.Count)];
    }




} // End of NotaktoComputer class
