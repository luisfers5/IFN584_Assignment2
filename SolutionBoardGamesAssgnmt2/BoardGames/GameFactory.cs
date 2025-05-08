namespace BoardGames
{
    public abstract class GameFactory
    {
        private static readonly Dictionary<GameType, GameFactory> mapping = new(); // singleton

        public abstract Game SetupNewGame();

        public static GameFactory GetFactory(GameType type)
        {
            // singleton
            if (mapping.TryGetValue(type, out var factory)){
                return factory;
            }
            
            //not exist -> init
            factory = type switch
            {
                GameType.NumericalTTT => new NumericalTTTFactory(),
                GameType.Notakto => new NotaktoFactory(),
                GameType.Gomoku => new GomokuFactory(),
                _ => throw new ArgumentException("Unsupported GameType")
            };

            mapping[type] = factory;
            return factory;
        }

        internal static object GetFactory(GameType? type)
        {
            throw new NotImplementedException();
        }
    }


}
