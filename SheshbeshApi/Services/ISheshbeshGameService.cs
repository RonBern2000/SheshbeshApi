﻿using SheshbeshApi.Models.GameModel;

namespace SheshbeshApi.Services
{
    public interface ISheshbeshGameService
    {
        public GameState CreateNewGame(string groupName);
        public GameState? GetGameState(string groupName);
        public GameState ProcessPosibbleMoves(string groupName, int fromPosition);
        public GameState MakeMove(string groupName, int fromPosition, int toPosition);
    }
}
