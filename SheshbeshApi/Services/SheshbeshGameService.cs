using SheshbeshApi.Models.GameModel;

namespace SheshbeshApi.Services
{
    public class SheshbeshGameService: ISheshbeshGameService
    {
        private Dictionary<string, GameState> activeGames = new Dictionary<string, GameState>();

        public GameState CreateNewGame(string groupName)
        {
            var newGame = new GameState();
            activeGames[groupName] = newGame;
            return newGame;
        }

        public GameState? GetGameState(string groupName)
        {
            return activeGames.ContainsKey(groupName) ? activeGames[groupName] : null;
        }
        public GameState ProcessPosibbleMoves(string groupName, int fromPosition)
        {
            var gameState = activeGames[groupName];

            gameState.GetPossibleMoves(fromPosition);

            return gameState;
        }
        public GameState MakeMove(string groupName, int fromPosition, int toPosition)
        {
            var gameState = activeGames[groupName];

            gameState.MakeMove(fromPosition, toPosition);

            return gameState;
        }
    }
}
