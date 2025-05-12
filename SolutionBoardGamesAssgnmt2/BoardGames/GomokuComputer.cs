using System;

namespace BoardGames;

// 7. Computer player logic for Gomoku
class GomokuComputer : Player
{

    private readonly Random rand = new();
    private const int WINNING_LINE_LENGTH = 5;

    private const int PLAYER1_SYMBOL = 1; // X
    private const int PLAYER2_SYMBOL = 2; // O

    public int Symbol { get; }
    public GomokuComputer(string name, int symbol)
    {
        Symbol = symbol;
    }


    public override (int row, int col, int number) MakeMove(List<Board> boards, Board board)
    {
        // Try winning move
        var move = FindWinningMove(board, Symbol);
        if (move.HasValue) return (move.Value.row, move.Value.col, 0);

        // Try opponent's winning move
        int opponentSymbol = Symbol == PLAYER1_SYMBOL ? PLAYER2_SYMBOL : PLAYER1_SYMBOL;
        move = FindWinningMove(board, opponentSymbol);
        if (move.HasValue) return (move.Value.row, move.Value.col, 0);

        // Try to extend own line
        move = FindBestLineExtension(board);
        if (move.HasValue) return (move.Value.row, move.Value.col, 0);

        // Fallback to random move
        return GetRandomMove(board);
    }

    private (int row, int col)? FindWinningMove(Board board, int symbol)
    {
        for (int i = 0; i < board.Size; i++)
        {
            for (int j = 0; j < board.Size; j++)
            {
                if (!board.IsValidMove(i, j)) continue;
                board.Cells[i][j] = symbol;
                bool wins = CheckWin(board, i, j, symbol);
                board.Cells[i][j] = null;
                if (wins) return (i, j);
            }
        }
        return null;
    }

    private (int row, int col)? FindBestLineExtension(Board board)
    {
        (int row, int col)? bestMove = null;
        int maxLineLength = 0;

        for (int i = 0; i < board.Size; i++)
        {
            for (int j = 0; j < board.Size; j++)
            {
                if (!board.IsValidMove(i, j)) continue;
                board.Cells[i][j] = Symbol;
                int lineLength = EvaluateLineLength(board, i, j, Symbol);
                board.Cells[i][j] = null;
                if (lineLength > maxLineLength)
                {
                    maxLineLength = lineLength;
                    bestMove = (i, j);
                }
            }
        }
        return bestMove;
    }

    private (int row, int col, int number) GetRandomMove(Board board)
    {
        var options = new List<(int, int)>();
        for (int i = 0; i < board.Size; i++)
        {
            for (int j = 0; j < board.Size; j++)
            {
                if (board.IsValidMove(i, j))
                    options.Add((i, j));
            }
        }
        var (r, c) = options[rand.Next(options.Count)];
        return (r, c, 0);
    }



    // Helper method to evaluate the length of the line created by a move
    private int EvaluateLineLength(Board board, int row, int col, int symbol)
    {
        return Math.Max(
            CountInDirection(board, row, col, 1, 0, symbol) + CountInDirection(board, row, col, -1, 0, symbol) + 1, // Horizontal
            Math.Max(
                CountInDirection(board, row, col, 0, 1, symbol) + CountInDirection(board, row, col, 0, -1, symbol) + 1, // Vertical
                Math.Max(
                    CountInDirection(board, row, col, 1, 1, symbol) + CountInDirection(board, row, col, -1, -1, symbol) + 1, // Diagonal \
                    CountInDirection(board, row, col, 1, -1, symbol) + CountInDirection(board, row, col, -1, 1, symbol) + 1  // Diagonal /
                )
            )
        );
    }

    private bool CheckWin(Board board, int row, int col, int symbol)
    {
        return (CountInDirection(board, row, col, 1, 0, symbol) + CountInDirection(board, row, col, -1, 0, symbol) + 1 >= WINNING_LINE_LENGTH) || // Horizontal
               (CountInDirection(board, row, col, 0, 1, symbol) + CountInDirection(board, row, col, 0, -1, symbol) + 1 >= WINNING_LINE_LENGTH) || // Vertical
               (CountInDirection(board, row, col, 1, 1, symbol) + CountInDirection(board, row, col, -1, -1, symbol) + 1 >= WINNING_LINE_LENGTH) || // Diagonal \
               (CountInDirection(board, row, col, 1, -1, symbol) + CountInDirection(board, row, col, -1, 1, symbol) + 1 >= WINNING_LINE_LENGTH);   // Diagonal /
    }


    private int CountInDirection(Board board, int startRow, int startCol, int dx, int dy, int symbol)
    {
        int count = 0;
        int x = startRow + dx, y = startCol + dy;

        while (x >= 0 && x < board.Size &&
               y >= 0 && y < board.Size &&
               board.Cells[x][y] == symbol)
        {
            count++;
            x += dx;
            y += dy;
        }

        return count;
    }

}
