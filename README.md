# IFN584 Assignment 2: Object-Oriented Design and Implementation

In this project, we collaborated as a team of four to design and implement an extensible framework for two-player board games, using C# on .NET 8 and a console-based interface. 

The goal was to apply object-oriented design principles and patterns to maximize reuse and flexibility, and then demonstrate our framework by plugging in three different games.

## Supported Games
- **Numerical Tic-Tac-Toe** (from Assignment 1)  
- **Notakto** (Neutral Tic-Tac-Toe on three 3×3 boards; last‐move loser)  
- **Gomoku** (Five-in-a-row on a 15×15 board)  

## Core Features
- **Extensible Framework**  
  ­­Extracted common abstractions (`Game`, `Board`, `Player`, `Move`, etc.) so new games can be added with minimal effort.  
- **Two Modes of Play**  
  - Human vs. Human  
  - Human vs. Computer  
- **AI Move Strategy**  
  - Wins immediately if possible  
  - Otherwise picks a random valid move  
- **Persistence**  
  - Save and load any game state (including mode and full move history)  
- **Undo/Redo**  
  - Infinite undo/redo through the entire move history, even immediately after loading  
- **In-Game Help**  
  - Text-based menu of available commands and usage examples  

## Design Highlights
- **UML Diagrams**  
  - **Class Diagram** showing all framework and game-specific classes  
  - **Object Diagrams** illustrating key game-state snapshots  
  - **Sequence Diagrams** for player turns, save/load, and undo/redo  
- **Design Patterns & Principles**  
  - **Strategy Pattern** for interchangeable player behaviours  
  - **Template Method** for the common game loop  
  - **Command Pattern** to record moves for undo/redo  
  - SOLID principles to ensure single responsibility and open/closed extensibility  

## Getting Started

### Prerequisites
- .NET 8 SDK (run `dotnet --version` to confirm)  
- Console/terminal on Windows, macOS, or Linux  

### Build & Run
```bash
git clone https://github.com/luisfers5/IFN584_Assignment2.git
cd IFN584_Assignment2/SolutionBoardGamesAssgnmt2
dotnet clean
dotnet run
