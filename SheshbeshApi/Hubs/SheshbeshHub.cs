using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SheshbeshApi.Services;
using System.Collections.Concurrent;

namespace SheshbeshApi.Hubs
{
    public class SheshbeshHub : Hub
    {
        private static Dictionary<string, string> userGroups = new Dictionary<string, string>();
        private static readonly ConcurrentDictionary<string, int> roomPlayerCount = new();

        private readonly ISheshbeshGameService _gameService;

        public SheshbeshHub(ISheshbeshGameService gameService)
        {
            _gameService = gameService;
        }
        public override async Task OnConnectedAsync()
        {
            var user = Context.User;
            if (user == null || !user.Identity!.IsAuthenticated)
            {
                await Clients.Caller.SendAsync("Unauthorized", "You are not authorized. Redirecting...");
                Context.Abort();
            }
            else
            {
                await base.OnConnectedAsync();
            }
        }
        [Authorize]
        public async Task GetCurrentPlayerCounts()
        {
            await Clients.Caller.SendAsync("ReceiveCurrentPlayerCount", roomPlayerCount);
        }
        [Authorize]
        public async Task JoinGame(string groupName)
        {
            var groupPlayers = userGroups.Values.Count(g => g == groupName);

            if (groupPlayers >= 2)
            {
                await Clients.Caller.SendAsync("GameIsFull", true);
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            userGroups[Context.ConnectionId] = groupName;

            CountKeysWithSameValue(userGroups, roomPlayerCount); // Update the roomPlayerCount Dic

            await Clients.All.SendAsync("UpdatePlayerCount", roomPlayerCount);

            groupPlayers = userGroups.Values.Count(g => g == groupName);

            await Clients.Caller.SendAsync("PlayerJoined", Context.ConnectionId);

            if (groupPlayers == 1)
            {
                await Clients.Caller.SendAsync("FirstPlayerJoined", new { hasJoined = true, roomName = $"{groupName}" });
                return;
            }
            if (groupPlayers == 2)
            {
                _gameService.CreateNewGame(groupName);
                var gameState = _gameService.GetGameState(groupName);

                var playersInGroup = userGroups.Where(u => u.Value == groupName).Select(u => u.Key).ToList();
                if (playersInGroup.Count == 2)
                {
                    gameState!.PlayerBlackId = playersInGroup[0]; // First player to join
                    gameState.PlayerWhiteId = playersInGroup[1]; // Second player to join
                }
                await Clients.Group(groupName).SendAsync("GameHasStartedMsg", true);
                await Clients.Group(groupName).SendAsync("StartGame", gameState);
            }
        }
        [Authorize]
        public async Task LeaveRoom()
        {
            string groupName = userGroups[Context.ConnectionId];
            var players = userGroups.Where(u => u.Value == groupName).Select(u => u.Key).ToList();
            _gameService.RemoveGameState(groupName);

            foreach (var playerId in players)
            {
                if (playerId == Context.ConnectionId)
                {
                    await Clients.Client(playerId).SendAsync("RedirectTheLeaver", Context.ConnectionId);
                }
                else
                {
                    await Clients.Client(playerId).SendAsync("RedirectTheOther", playerId);
                }
            }
        }
        [Authorize]
        public async Task RollDice()
        {
            string groupName = userGroups[Context.ConnectionId];
            var gameState = _gameService.RollDice(groupName);

            await Clients.Group(groupName).SendAsync("DiceRolled", gameState);
        }
        [Authorize]
        public async Task GetPossibleMoves(int fromPosition)
        {
            string groupName = userGroups[Context.ConnectionId];

            var gameState = _gameService.ProcessPosibbleMoves(groupName, fromPosition);

            await Clients.Group(groupName).SendAsync("ReceivePossbleMoves", gameState);
        }
        [Authorize]
        public async Task MakeMove(int fromPosition, int toPosition)
        {
            string groupName = userGroups[Context.ConnectionId];

            var gameState = _gameService.MakeMove(groupName, fromPosition, toPosition);

            await Clients.Group(groupName).SendAsync("MoveMade", gameState);
        }
        [Authorize]
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (userGroups.TryGetValue(Context.ConnectionId, out var groupName))
            {
                _gameService.RemoveGameState(groupName);

                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

                userGroups.Remove(Context.ConnectionId);

                CountKeysWithSameValue(userGroups, roomPlayerCount); // Update the roomPlayerCount Dic

                await Clients.All.SendAsync("UpdatePlayerCount", roomPlayerCount);
            }
            await base.OnDisconnectedAsync(exception);
        }
        [Authorize]
        private void CountKeysWithSameValue(Dictionary<string, string> userGroups, ConcurrentDictionary<string, int> roomPlayerCount)
        {
            roomPlayerCount.Clear();
            foreach (var kvp in userGroups)
            {
                var groupName = kvp.Value;

                if (roomPlayerCount.ContainsKey(groupName))
                {
                    roomPlayerCount[groupName]++;
                }
                else
                {
                    roomPlayerCount[groupName] = 1;
                }
            }
        }

    }
}
