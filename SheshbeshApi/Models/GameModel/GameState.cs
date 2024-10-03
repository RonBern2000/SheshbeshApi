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
        public int[] DiceRolls { get; set; }
        public bool IsDouble { get; set; }

        public GameState()
        {
            Jail = "w0b0";
            Board = new string[26];
            // Empty indexes are null
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
            PossibleMoves = new int[2]; // the index might be filld or not
            DiceRolls = new int[4]; // 0,1 for not equal and 0,1,2,3 for qual dices
        }
        public int GetPotentialMovesAfterEachMove() // if this method returns 0 then we can skip to the next player
        {
            int potentialMoves = 0;
            int direction = IsPlayerBlackTurn ? -1 : 1;

            for (int fromPosition = 0; fromPosition < Board.Length; fromPosition++)
            {
                var pawn = Board[fromPosition];
                if (pawn != null && pawn[0] == (IsPlayerBlackTurn ? 'b' : 'w'))
                {
                    foreach (int die in DiceRolls.Where(d => d != 0))
                    {
                        int toPosition = fromPosition + (die * direction);

                        if (toPosition >= 0 && toPosition <= 25)
                        {
                            var targetPawn = Board[toPosition];

                            if (targetPawn == null || targetPawn[0] == pawn[0] || targetPawn[1] == '1')
                            {
                                potentialMoves++;
                            }
                        }
                        else
                        {
                            potentialMoves++;
                        }

                        if (potentialMoves == 1)
                        {
                            return potentialMoves;
                        }
                    }
                }
            }
            return potentialMoves;
        }
        public bool IsDiceRollsEmpty() // check if the player utilized all of his moves, so we dont need to check "GetPotentialMovesAfterEachMove" And we could skip the turn to the other player
        {
            bool flag = false;
            foreach (var die in DiceRolls)
            {
                flag = die != 0 ? false : true;
            }
            return flag;
        }
        public GameState GetPossibleMoves(int fromPosition)
        {
            for (int i = 0; i < PossibleMoves.Length; i++)
            {
                if (IsDouble && i == 1)
                    break;
                int move = IsPlayerBlackTurn ? fromPosition - DiceRolls[i] : fromPosition + DiceRolls[i];
                var toPosition = Board[move];

                if (IsValidMove(toPosition))
                {
                    PossibleMoves[i] = move; // Assign a valid index to PossibleMoves that the player can move the selected pawn to
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
        public GameState MakeMove(int fromPosition, int toPosition) // represting the indexes from and to
        {
            // Jail = "w0b0" structure
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

            for (int i = 0; i < DiceRolls.Length; i++)
            {
                if (Math.Abs(fromPosition - toPosition) == DiceRolls[i] && DiceRolls[i] != 0)
                {
                    DiceRolls[i] = 0;
                    break;
                }
                if (!IsDouble && i == 2)
                {
                    break;
                }
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