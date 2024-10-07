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
        public bool HasRolledDice { get; set; }
        public bool BlackJailFilled { get; set; }
        public bool WhiteJailFilled { get; set; }
        public string? WonPlayer { get; set; }

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
        public GameState HaveAndCanReleasePawns() // after each dice roll we should check it, if the player has pawns in jail and cant free them he should skip turn
        {
            int direction = IsPlayerBlackTurn ? -1 : 1;

            if (IsPlayerBlackTurn && int.Parse(Jail[3].ToString()) > 0) // the black player must free his pawns first, any other moves should not be counted
            {
                InitPossibleMoves();
                BlackJailFilled = true;
                if (IsDouble)
                {
                    int toPosition = 25 + (DiceRolls[3] * direction);
                    var targetPawn = Board[toPosition];
                    if (targetPawn == null || targetPawn[0] == Jail[2] || int.Parse(targetPawn.Substring(1)) == 1)
                    {
                        PossibleMoves[0] = toPosition;
                    }
                }
                else
                {
                    for (int i = 0; i < DiceRolls.Length; i++)
                    {
                        if (i == 2)
                            break;
                        int toPosition = 25 + (DiceRolls[i] * direction);
                        var targetPawn = Board[toPosition];
                        if (targetPawn == null || targetPawn[0] == Jail[2] || int.Parse(targetPawn.Substring(1)) == 1)
                        {
                            PossibleMoves[i] = toPosition;
                        }
                    }
                }
            }

            if (!IsPlayerBlackTurn && int.Parse(Jail[1].ToString()) > 0) // the white player must free his pawns first, any other moves should not be counted
            {
                InitPossibleMoves();
                WhiteJailFilled = true;
                if (IsDouble)
                {
                    int toPosition = 0 + (DiceRolls[3] * direction);
                    var targetPawn = Board[toPosition];
                    if (targetPawn == null || targetPawn[0] == Jail[0] || int.Parse(targetPawn.Substring(1)) == 1)
                    {
                        PossibleMoves[0] = toPosition;
                    }
                }
                else
                {
                    for (int i = 0; i < DiceRolls.Length; i++)
                    {
                        if (i == 2)
                            break;
                        int toPosition = 0 + (DiceRolls[i] * direction);
                        var targetPawn = Board[toPosition];
                        if (targetPawn == null || targetPawn[0] == Jail[0] || int.Parse(targetPawn.Substring(1)) == 1)
                        {
                            PossibleMoves[i] = toPosition;
                        }
                    }
                }
            }
            return this;
        }
        public int GetPotentialMovesAfterEachMove() // if this method returns 0 then we can skip to the next player
        {
            int potentialMoves = 0;
            int direction = IsPlayerBlackTurn ? -1 : 1;

            for (int fromPosition = 1; fromPosition < Board.Length - 1 ; fromPosition++)
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
                            if (targetPawn == null || targetPawn[0] == pawn[0] || int.Parse(targetPawn.Substring(1)) == 1)
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
            bool flag = true;
            foreach (var die in DiceRolls)
            {
                flag = die != 0 ? false : true;
                if (!flag)
                    return flag;
            }
            return flag;
        }
        public GameState GetPossibleMoves(int fromPosition)
        {
            InitPossibleMoves();
            for (int i = 0; i < PossibleMoves.Length; i++)
            {
                if (IsDouble && i == 1)
                {
                    PossibleMoves[i] = -1;
                    break;
                }
                int move = -1;
                if (IsDouble)
                {
                    move = IsPlayerBlackTurn ? fromPosition - DiceRolls[3] : fromPosition + DiceRolls[3];
                }
                else
                {
                    move = IsPlayerBlackTurn ? fromPosition - DiceRolls[i] : fromPosition + DiceRolls[i];
                }
                if (move == fromPosition)
                {
                    PossibleMoves[i] = -1;
                    continue;
                }
                if (move < 0)
                {
                    PossibleMoves[i] = 0;
                    continue;
                }
                if (move > 25)
                {
                    PossibleMoves[i] = 25;
                    continue;
                }

                var toPosition = Board[move];

                if (IsValidMove(toPosition))
                {
                    PossibleMoves[i] = move; // Assign a valid index to PossibleMoves that the player can move the selected pawn to
                }
                else
                {
                    PossibleMoves[i] = -1;
                }
            }
            return this;
        }
        private void InitPossibleMoves()
        {
            PossibleMoves[0] = -1;
            PossibleMoves[1] = -1;
        }
        private bool IsValidMove(string toPosition)
        {
            if (toPosition == null) return true;

            if (IsPlayerBlackTurn)
            {
                // Valid move for black player
                return toPosition[0] == 'b' || int.Parse(toPosition.Substring(1)) == 1;
            }
            else
            {
                // Valid move for white player
                return toPosition[0] == 'w' || int.Parse(toPosition.Substring(1)) == 1;
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

            // After the board has been updated we need to "find" which dice was used for the move and set it to 0
            if ((toPosition == 0 || toPosition == 25) && !IsDouble) // means the black/white player decided to move a pawn to the bearing-off
            {
                int distance = Math.Abs(fromPosition - toPosition); 

                bool canUseFirstDice = DiceRolls[0] >= distance; 
                bool canUseSecondDice = DiceRolls[1] >= distance; 

                if (canUseFirstDice && canUseSecondDice)
                {
                    if (DiceRolls[0] <= DiceRolls[1])
                    {
                        DiceRolls[0] = 0; 
                    }
                    else
                    {
                        DiceRolls[1] = 0;
                    }
                }
                else if (canUseFirstDice)
                {
                    DiceRolls[0] = 0;
                }
                else
                {
                    DiceRolls[1] = 0;
                }
            }
            else if ((toPosition == 0 || toPosition == 25) && IsDouble) // means the black/white player decided to move a pawn to the bearing-off when we have a double
            {
                for (int i = 0; i < DiceRolls.Length; i++)
                {
                    if (DiceRolls[i] != 0)
                    {
                        DiceRolls[i] = 0;
                        break;
                    }
                }
            }
            else
            {
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
            }
            return this;
        }
        private void UpdateSourcePosition(int position, char playerSymbol)
        {
            //Jail = "w0b0"
            if (position == 25 || position == 0)
            {
                if (playerSymbol == 'b')
                {
                    int wCount = int.Parse(Jail[1].ToString());
                    int bCount = int.Parse(Jail[3].ToString());
                    bCount--;
                    if(bCount == 0)
                    {
                        BlackJailFilled = false;
                    }
                    Jail = $"w{wCount}b{bCount}";
                }
                else
                {
                    int wCount = int.Parse(Jail[1].ToString());
                    int bCount = int.Parse(Jail[3].ToString());
                    wCount--;
                    if (wCount == 0)
                    {
                        WhiteJailFilled = false;
                    }
                    Jail = $"w{wCount}b{bCount}";
                }
            }
            else
            {
                var currentValue = Board[position];
                int count = int.Parse(currentValue.Substring(1));
                count--;

                Board[position] = count > 0 ? $"{playerSymbol}{count}" : null!;
            }
        }
        private string IncrementPawnCount(string positionValue)
        {
            int count = int.Parse(positionValue.Substring(1));
            count++;
            return $"{positionValue[0]}{count}";
        }
        private void MoveToJail(char opponentSymbol)
        {
            if(opponentSymbol == 'w')
            {
                WhiteJailFilled = true;
            }
            else
            {
                BlackJailFilled = true;
            }
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
        public string? HasWon() // return black or white or null
        {
            if (int.Parse(Board[0].Substring(1)) == 15)
            {
                WonPlayer = "black";
                return "black";
            }
            if (int.Parse(Board[25].Substring(1)) == 15)
            {
                WonPlayer = "white";
                return "white";
            }
            return null;
        }
    }
}