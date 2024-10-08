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

            Board[0] = "b0";   // Black's bear-off area, initially empty
            Board[25] = "w0";  // White's bear-off area, initially empty

            // Black checkers setup, Move from the top to the bottom right
            Board[24] = "b2";  
            Board[13] = "b5";
            Board[8] = "b3";  
            Board[6] = "b5";  

            // White checkers setup, Move from the bottom to the top right
            Board[1] = "w2";  
            Board[12] = "w5"; 
            Board[17] = "w3";  
            Board[19] = "w5"; 

            IsPlayerBlackTurn = true;
            PossibleMoves = new int[2]; 
            DiceRolls = new int[4]; 
        }
        public GameState ReleaseFromJailMoves()
        {
            int direction = IsPlayerBlackTurn ? -1 : 1;

            if (IsPlayerBlackTurn && int.Parse(Jail[3].ToString()) > 0)
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

            if (!IsPlayerBlackTurn && int.Parse(Jail[1].ToString()) > 0)
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
        public int GetPotentialMovesAfterEachMove()
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
        public bool IsDiceRollsEmpty()
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
                int moveIndex = -1;
                if (IsDouble)
                {
                    moveIndex = IsPlayerBlackTurn ? fromPosition - DiceRolls[3] : fromPosition + DiceRolls[3];
                }
                else
                {
                    moveIndex = IsPlayerBlackTurn ? fromPosition - DiceRolls[i] : fromPosition + DiceRolls[i];
                }
                if (moveIndex == fromPosition)
                {
                    PossibleMoves[i] = -1;
                    continue;
                }
                if (moveIndex < 0)
                {
                    PossibleMoves[i] = 0;
                    continue;
                }
                if (moveIndex > 25)
                {
                    PossibleMoves[i] = 25;
                    continue;
                }

                var toPosition = Board[moveIndex];

                if (IsValidMove(toPosition))
                {
                    PossibleMoves[i] = moveIndex;
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
                return toPosition[0] == 'b' || int.Parse(toPosition.Substring(1)) == 1;
            }
            else
            {
                return toPosition[0] == 'w' || int.Parse(toPosition.Substring(1)) == 1;
            }
        }
        public GameState MakeMove(int fromPosition, int toPosition)
        {
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

            if ((toPosition == 0 || toPosition == 25) && !IsDouble)
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
            else if ((toPosition == 0 || toPosition == 25) && IsDouble)
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
        private void FindUsedDice(int fromPosition, int toPosition)
        {

        }
        private void UpdateSourcePosition(int position, char playerSymbol)
        {
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
        public string? HasWon()
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