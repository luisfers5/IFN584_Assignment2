using System;

namespace BoardGames
{
    // 1. Board class used by all games
    public class Board
    {
        public int Size { get; set; }                          // Board size (e.g., 3 means 3x3)

        public int?[][] Cells { get; set; }                    // 2D grid of nullable integers

        // 2. Constructor — sets up an empty board
        public Board(int size)
        {
            Size = size;
            Cells = new int?[size][];                          // Create rows
            for (int i = 0; i < size; i++)
            {
                Cells[i] = new int?[size];                     // Create columns per row
            }
        }

        // 3. Display board in console using ASCII layout
        public void Display()
        {
            for (int i = 0; i < Size; i++)                     // Loop through rows
            {
                for (int j = 0; j < Size; j++)                 // Loop through columns
                {
                    string cellValue = Cells[i][j]?.ToString() ?? " ";  // Show number or blank
                    Console.Write($" {cellValue} ");
                    if (j < Size - 1)
                        Console.Write("|");                   // Draw vertical separator
                }
                Console.WriteLine();

                if (i < Size - 1)
                    Console.WriteLine(new string('-', Size * 4 - 1));  // Draw horizontal line
            }
        }

        // 4. Check if a move is inside bounds and the cell is empty
        public bool IsValidMove(int row, int col)
        {
            bool inBounds = row >= 0 && row < Size && col >= 0 && col < Size;
            return inBounds && Cells[row][col] == null;
        }

        // 5. Attempt to place a number on the board
        public bool PlaceNumber(int row, int col, int number)
        {
            if (!IsValidMove(row, col))                       // Only place if move is valid
                return false;

            Cells[row][col] = number;                          // Store the number
            return true;
        }

        // 6. Check if the board is full
        public bool IsBoardFull()
        {
            for (int i = 0; i < Size; i++)                     // Check all cells
            {
                for (int j = 0; j < Size; j++)
                {
                    if (Cells[i][j] == null)                   // Still has empty cell
                        return false;
                }
            }
            return true;
        }
    }
}
