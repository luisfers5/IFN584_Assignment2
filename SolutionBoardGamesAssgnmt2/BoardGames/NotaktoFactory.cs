using System;

namespace BoardGames;

public class NotaktoFactory : GameFactory
{
    public override Game SetupNewGame()
    {
        return NotaktoGame.SetupNewGame();
    }

}
