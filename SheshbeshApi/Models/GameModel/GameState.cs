namespace SheshbeshApi.Models.GameModel
{
    public class GameState
    {
        public string[] Board { get; set; } // 0 => 0, 1 => 1b... index 26 is for jail
        public string Jail { get; set; }
        public bool IsPlayerBlackTurn { get; set; }
        public string? PlayerBlackId { get; set; }
        public string? PlayerWhiteId { get; set; }
        public int[] PossibleMoves { get; set; }
        public int AmountOfMoves { get; set; }

        public GameState()
        {
            Board = new string[26];
            // Empty indexes are null
            Jail = "w0b0";
            Board[0] = "b0";   // Black's bear-off area, initially empty
            Board[25] = "w0";  // White's bear-off area, initially empty

            // Black checkers setup, Move from the top to the bottom right
            Board[24] = "b2";  // 2 black checkers on point 24
            Board[13] = "b5";  // 5 black checkers on point 13
            Board[8] = "b3";  // 3 black checkers on point 8
            Board[6] = "b5";  // 5 black checkers on point 6

            // White checkers setup, Move from the bottom to the top right
            Board[1] = "w2";  // 2 white checkers on point 1
            Board[12] = "w5";  // 5 white checkers on point 12
            Board[17] = "w3";  // 3 white checkers on point 17
            Board[19] = "w5";  // 5 white checkers on point 19

            IsPlayerBlackTurn = true;
            PossibleMoves = new int[2];
        }
        public int GetPotentialMovesAfterDiceRoll(int die1, int die2) // when die1 == die2 => checks if there are indeed 4 avialable moves at least, when die1 != die2 => checks if there are indeed 2 available moves at least
        {
            // 
            int potentialMoves = 0;
            int movesToCheck = (die1 == die2) ? 4 : 2;

            int direction = IsPlayerBlackTurn ? -1 : 1;

            for (int fromPosition = 0; fromPosition < Board.Length; fromPosition++)
            {
                var pawn = Board[fromPosition];

                if (pawn != null && pawn[0] == (IsPlayerBlackTurn ? 'b' : 'w'))
                {
                    int toPositionDie1 = fromPosition + (die1 * direction);
                    if (IsMoveValid(toPositionDie1, pawn))
                    {
                        potentialMoves++;

                        int toPositionDie2 = toPositionDie1 + (die2 * direction);
                        if (IsMoveValid(toPositionDie2, pawn))
                        {
                            potentialMoves++;
                        }
                        if (potentialMoves >= movesToCheck)
                        {
                            return potentialMoves;
                        }
                    }

                    int toPositionDie2First = fromPosition + (die2 * direction);
                    if (IsMoveValid(toPositionDie2First, pawn))
                    {
                        potentialMoves++;

                        int toPositionDie1Second = toPositionDie2First + (die1 * direction);
                        if (IsMoveValid(toPositionDie1Second, pawn))
                        {
                            potentialMoves++;
                        }
                        if (potentialMoves >= movesToCheck)
                        {
                            return potentialMoves;
                        }
                    }
                }
            }
            return potentialMoves;
        }
        private bool IsMoveValid(int toPosition, string pawn)
        {
            if (toPosition < 0 || toPosition > 25)
            {
                return true;
            }

            if (toPosition < Board.Length)
            {
                var targetPawn = Board[toPosition];

                return targetPawn == null || targetPawn[0] == pawn[0] || targetPawn[1] == '1';
            }

            return false; // Invalid move => blocked by opponent
        }
        public GameState GetPossibleMoves(int fromPosition, int die1, int die2)
        {
            int[] dice = { die1, die2 };
            bool isDouble = die1 == die2;

            for (int i = 0; i < dice.Length; i++)
            {
                if (isDouble && i == 1) break; // Skip duplicate die check for double rolls

                int move = IsPlayerBlackTurn ? fromPosition - dice[i] : fromPosition + dice[i];
                var toPosition = Board[move];

                if (IsValidMove(toPosition))
                {
                    PossibleMoves[i] = move; // Assign to PossibleMoves
                }
            }

            return this;
        }
        private bool IsValidMove(string toPosition)
        {
            if (toPosition == null) return true;

            if (IsPlayerBlackTurn)
            {
                // Valid move for black player
                return toPosition[0] == 'b' || toPosition[1] == '1';
            }
            else
            {
                // Valid move for white player
                return toPosition[0] == 'w' || toPosition[1] == '1';
            }
        }
        public GameState MakeMove(int fromPosition, int toPosition)
        {
            // Board[26] = "w0b0" is Jail
            char playerSymbol = IsPlayerBlackTurn ? 'b' : 'w';
            char opponentSymbol = IsPlayerBlackTurn ? 'w' : 'b';

            UpdateSourcePosition(fromPosition, playerSymbol);

            if (Board[toPosition] == null)
            {
                Board[toPosition] = $"{playerSymbol}1";
            }
            else if (Board[toPosition][0] == playerSymbol)
            {
                Board[toPosition] = IncrementPawnCount(Board[toPosition]);
            }
            else
            {
                Board[toPosition] = $"{playerSymbol}1";
                MoveToJail(opponentSymbol);
            }

            AmountOfMoves--;
            if (AmountOfMoves == 0)
            {
                IsPlayerBlackTurn = !IsPlayerBlackTurn;
            }
            return this;
        }
        private void UpdateSourcePosition(int position, char playerSymbol)
        {
            var currentValue = Board[position];
            int count = int.Parse(currentValue[1].ToString());
            count--;

            Board[position] = count > 0 ? $"{playerSymbol}{count}" : null!;
        }
        private string IncrementPawnCount(string positionValue)
        {
            int count = int.Parse(positionValue[1].ToString());
            count++;
            return $"{positionValue[0]}{count}";
        }
        private void MoveToJail(char opponentSymbol)
        {
            var jailValue = Jail;

            int whiteJailCount = int.Parse(jailValue[1].ToString());
            int blackJailCount = int.Parse(jailValue[3].ToString());

            if (opponentSymbol == 'w')
            {
                whiteJailCount++;
            }
            else
            {
                blackJailCount++;
            }

            Jail = $"{jailValue[0]}{whiteJailCount}{jailValue[2]}{blackJailCount}";
        }
    }
}