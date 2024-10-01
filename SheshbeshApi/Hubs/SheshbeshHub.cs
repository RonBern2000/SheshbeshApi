using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SheshbeshApi.Services;

namespace SheshbeshApi.Hubs
{
    [Authorize]
    public class SheshbeshHub : Hub
    {
        private static Dictionary<string, string> userGroups = new Dictionary<string, string>();

        private readonly ISheshbeshGameService _gameService;

        public SheshbeshHub(ISheshbeshGameService gameService)
        {
            _gameService = gameService;
        }

        public async Task JoinGame(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            userGroups[Context.ConnectionId] = groupName;

            await Clients.Group(groupName).SendAsync("PlayerJoined", Context.ConnectionId);

            var groupPlayers = userGroups.Values.Count(g => g == groupName);
            if (groupPlayers == 2)
            {
                await Clients.Group(groupName).SendAsync("StartGame");
            }
        }

        public async Task RollDice()
        {
            var random = new Random();
            int die1 = random.Next(1, 7);
            int die2 = random.Next(1, 7);

            string groupName = userGroups[Context.ConnectionId];
            var gameState = _gameService.GetGameState(groupName);
            if (die1 == die2)
            {
                gameState!.AmountOfMoves = 4;
            }
            else
            {
                gameState!.AmountOfMoves = 2;
            }

            // TODO: _gameService.Calc more complex calc of the amount that the user can move

            await Clients.Group(groupName).SendAsync("DiceRolled", die1, die2);
        }

        public async Task GetPossibleMoves(int fromPosition, int die1, int die2)
        {
            string groupName = userGroups[Context.ConnectionId];

            var gameState = _gameService.ProcessPosibbleMoves(groupName, fromPosition, die1, die2);

            await Clients.Group(groupName).SendAsync("ReceivePossbleMoves", fromPosition, gameState.PossibleMoves);
        }
        public async Task MakeMove(int fromPosition, int toPosition)
        {
            string groupName = userGroups[Context.ConnectionId];

            var gameState = _gameService.MakeMove(groupName, fromPosition, toPosition);

            await Clients.Group(groupName).SendAsync("MoveMade", gameState);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (userGroups.TryGetValue(Context.ConnectionId, out var groupName))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

                await Clients.Group(groupName).SendAsync("PlayerLeft", Context.ConnectionId);

                userGroups.Remove(Context.ConnectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }

    }
}
