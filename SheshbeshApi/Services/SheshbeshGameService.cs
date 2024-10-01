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
        public GameState GetAllPotentialMoves(string groupName, int die1, int die2) 
        {
            var gameState = activeGames[groupName];

            gameState.GetPotentialMovesAfterDiceRoll(die1, die2);

            return gameState;
        }
        public GameState ProcessPosibbleMoves(string groupName, int fromPosition, int die1, int die2)
        {
            var gameState = activeGames[groupName];

            gameState.GetPossibleMoves(fromPosition, die1, die2);

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
