using System;

namespace BoardGames;

public class GomokuFactory : GameFactory
{
    public override Game SetupNewGame()
    {
       return GomokuGame.SetupNewGame();
    }
}
