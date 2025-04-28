# IFN584 Assignment 2: Object-Oriented Design and Implementation

In this project, we collaborated as a team of four to design and implement an extensible framework for two-player board games, using C# on .NET 8 and a console-based interface. 

The goal was to apply object-oriented design principles and patterns to maximize reuse and flexibility, and then demonstrate our framework by plugging in three different games.

## Submission Deliverables

1. **Design Report (PDF)**
   - **Format & Length:** Up to 12 A4 pages, 2 cm margins, single-spaced, 12 pt Times New Roman or 11 pt Arial.  
   - **Content:**  
     1. **Executive Summary** (≤ 2 pages)  
        - Team member list (full name, student ID, email)  
        - Contribution statement for each member  
        - Declaration of which requirements have/ haven’t been implemented  
     2. **Design Documents** (≤ 5 pages)  
        - Overall class diagram with classes, attributes, methods, and relationships  
        - Two object diagrams showing key runtime snapshots  
        - Two sequence diagrams for representative scenarios (e.g., move, save/load, undo/redo)  
     3. **Design Patterns & Principles** (≤ 3 pages)  
        - Identification and justification of each pattern/principle used, referenced to your diagrams  
     4. **Implementation Details** (≤ 2 pages)  
        - Build and run instructions (`dotnet clean` / `dotnet run`)  
        - External libraries/frameworks used (with class/interface names)  

2. **Peer Review Submission**  
   - Complete anonymous peer reviews for your three teammates in Canvas after group submission.  
   - Your individual contribution score (10 %) is based on these reviews.


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
