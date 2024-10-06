using Microsoft.AspNetCore.SignalR;
using SheshbeshApi.Hubs;
using SheshbeshApi.Models.GameModel;
using System.ComponentModel;
using System.Numerics;

namespace SheshbeshApi.Services
{
    public class SheshbeshGameService : ISheshbeshGameService
    {
        private static Dictionary<string, GameState> activeGames = new Dictionary<string, GameState>();
        private readonly IHubContext<SheshbeshHub> _hubContext;

        public SheshbeshGameService(IHubContext<SheshbeshHub> hubContext)
        {
            _hubContext = hubContext;
        }

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
        public GameState? RollDice(string groupName) 
        {
            var gameState = activeGames[groupName];

            if(gameState == null)
                return null;
            var random = new Random();
            int die1 = random.Next(1, 7);
            int die2 = random.Next(1, 7);
            gameState.HasRolledDice = true;
            if (die1 == die2)
            {
                for (int i = 0; i < gameState.DiceRolls.Length; i++)
                    gameState.DiceRolls[i] = die1;
                gameState.IsDouble = true;
            }
            else
            {
                gameState.IsDouble = false;
                gameState.DiceRolls[0] = die1;
                gameState.DiceRolls[1] = die2;
            }

            gameState = gameState.HaveAndCanReleasePawns();
            if (IsThereAndCanThePlayerFreePrinsoners(gameState))
            {
                _hubContext.Clients.Group(groupName).SendAsync("ReceivePossbleMoves", gameState);
            }
            else
            {
                if (SkipTurn(groupName))
                {
                    NotifyClientsTurnSkipped(groupName);
                }
            }

            return gameState;
        }
        private bool IsThereAndCanThePlayerFreePrinsoners(GameState gameState)
        {
            bool flag = false;
            foreach(var possibleMove in gameState.PossibleMoves)
            {
                flag = possibleMove != -1 ? true : false;
                if (flag)
                    return flag;
            }
            return flag;
        }
        private void NotifyClientsTurnSkipped(string groupName)
        {
            var gameState = activeGames[groupName];
            var message = !gameState.IsPlayerBlackTurn ? "Black pawn player's turn has ended. White player turn" : "White pawn player's turn has ended. Black player turn";
            _hubContext.Clients.Group(groupName).SendAsync("TurnSkipped", message);
        }
        public bool SkipTurn(string groupName)
        {
            var gameState = activeGames[groupName];
            if (IsDicRollsEmpty(groupName))
            {
                gameState.IsPlayerBlackTurn = !gameState.IsPlayerBlackTurn;
                gameState.HasRolledDice = false;
                return true;
            }
            if (!IsTherePotentialMovesAfterEachMove(groupName))
            {
                gameState.IsPlayerBlackTurn = !gameState.IsPlayerBlackTurn;
                gameState.HasRolledDice = false;
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

            if (gameState.HasWon() != null)
            {
                _hubContext.Clients.Group(groupName).SendAsync("GameWon", gameState.WonPlayer);
                return gameState;
            }

            gameState = gameState.HaveAndCanReleasePawns();

            if (!IsThereAndCanThePlayerFreePrinsoners(gameState))
            {
                NotifyClientsTurnSkipped(groupName);
            }
            else
            {
                if (SkipTurn(groupName))
                {
                    NotifyClientsTurnSkipped(groupName);
                }
            }

            return gameState;
        }
        public void RemoveGameState(string groupName)
        {
            if (activeGames.ContainsKey(groupName))
            {
                activeGames.Remove(groupName);
            }
        }
    }
}
