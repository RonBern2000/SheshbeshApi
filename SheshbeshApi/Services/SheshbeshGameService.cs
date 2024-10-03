using SheshbeshApi.Models.GameModel;

namespace SheshbeshApi.Services
{
    public class SheshbeshGameService : ISheshbeshGameService
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
        public bool SkipTurn(string groupName)
        {
            var gameState = activeGames[groupName];
            if (IsDicRollsEmpty(groupName))
            {
                gameState.IsPlayerBlackTurn = !gameState.IsPlayerBlackTurn;
                return true;
            }
            if (!IsTherePotentialMovesAfterEachMove(groupName))
            {
                gameState.IsPlayerBlackTurn = !gameState.IsPlayerBlackTurn;
                return true;
            }
            return false;
        }
        private bool IsDicRollsEmpty(string groupName)
        {
            var gameState = activeGames[groupName];
            return gameState.IsDiceRollsEmpty();
        }
        private bool IsTherePotentialMovesAfterEachMove(string groupName)
        {
            var gameState = activeGames[groupName];

            bool isThereAMove = gameState.GetPotentialMovesAfterEachMove() == 1 ? true : false;

            return isThereAMove;
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
