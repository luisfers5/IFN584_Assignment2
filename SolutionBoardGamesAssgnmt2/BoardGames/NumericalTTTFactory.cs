using System;

namespace BoardGames;

public class NumericalTTTFactory : GameFactory
{
    public override Game SetupNewGame(){
        return NumericalTTTGame.SetupNewGame();
    }
}
