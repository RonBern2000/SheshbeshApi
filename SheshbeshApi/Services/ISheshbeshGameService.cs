using SheshbeshApi.Models.GameModel;

namespace SheshbeshApi.Services
{
    public interface ISheshbeshGameService
    {
        public GameState CreateNewGame(string groupName);
        public GameState? GetGameState(string groupName);
        public GameState? RollDice(string groupName);
        public bool SkipTurn(string groupName);
        public GameState ProcessPosibbleMoves(string groupName, int fromPosition);
        public GameState MakeMove(string groupName, int fromPosition, int toPosition);
        public void RemoveGameState(string groupName);
    }
}
