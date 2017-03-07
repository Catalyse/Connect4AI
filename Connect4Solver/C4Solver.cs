using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Connect4Solver
{
    public struct Connect4Grid
    {
        public List<List<int>> grid;
        public int height;
        public int player;
        public int width;
    }

    public struct Connect4Response
    {
        public Connect4Response(int _move)
        {
            move = _move;
        }
        public int move;
    }

    public class C4Solver
    {
        public Connect4Grid input;
        static void Main(string[] args)
        {
            Console.Error.WriteLine("Start");
            while(true)
                Run();
        }

        public static void Run()
        {
            //read stdin
            string input;
            input = Console.ReadLine();
            Connect4Grid state = JsonConvert.DeserializeObject<Connect4Grid>(input);

            int enemy;
            if (state.player == 1)
                enemy = 2;
            else
                enemy = 1;

            if(CheckZero(state))//If there are no moves we will go to the center
            {
                if(state.width % 2 == 0)//even
                {
                    Connect4Response response = new Connect4Response(state.width / 2);
                    string strResponse = JsonConvert.SerializeObject(response);
                    Console.WriteLine(strResponse);
                    Console.Out.Flush();
                }
                else//odd
                {
                    Connect4Response response = new Connect4Response((state.width / 2) + 1);
                    string strResponse = JsonConvert.SerializeObject(response);
                    Console.WriteLine(strResponse);
                    Console.Out.Flush();
                }
            }
            else
            {//first we will check if there is a counter move needed.
                int xCounter;
                List<int> disallowedMoves = new List<int>();
                if (CounterMoveCheck(state, enemy, out xCounter, out disallowedMoves))
                {//run counter move
                    Connect4Response response = new Connect4Response(xCounter);
                    string strResponse = JsonConvert.SerializeObject(response);
                    Console.WriteLine(strResponse);
                    Console.Out.Flush();
                }
                else
                {
                    List<int> idealMoves = new List<int>();
                    if(CounterMoveCheck(state, state.player, out xCounter, out idealMoves))
                    {//Here we check if we can counter ourself(find a winning move), if so we make the move regardless of the banned moves because we win
                        Connect4Response response = new Connect4Response(xCounter);
                        string strResponse = JsonConvert.SerializeObject(response);
                        Console.WriteLine(strResponse);
                        Console.Out.Flush();
                    }
                    else//check the two lists of ideal and banned moves
                    {
                        Console.Error.WriteLine("Start Ideal Move Check");
                        if (idealMoves.Count > 0)
                        {
                            for (int i = 0; i < idealMoves.Count; i++)
                            {
                                if (!disallowedMoves.Contains(idealMoves[i]))//here we check if there is overlap between banned moves and ideal moves(both players win with the same tile)
                                {
                                    Connect4Response response = new Connect4Response(idealMoves[i]);
                                    string strResponse = JsonConvert.SerializeObject(response);
                                    Console.WriteLine(strResponse);
                                    Console.Out.Flush();
                                    break;
                                }
                            }
                        }
                        Console.Error.WriteLine("No Ideal Moves found");
                        List<bool> allowedMoves = ValidColumns(state);
                        List<int> pickList = new List<int>();
                        for(int i = 0; i < allowedMoves.Count; i++)//we will try to make a move here that isnt banned
                        {
                            int move;
                            while (true)
                            {
                                Random r = new Random(DateTime.Now.Millisecond);
                                move = r.Next(0, 6);
                                if (!pickList.Contains(move))
                                {
                                    pickList.Add(move);
                                    break;
                                }
                            }
                            if (allowedMoves[i] && !disallowedMoves.Contains(move))
                            {
                                Connect4Response response = new Connect4Response(move);
                                string strResponse = JsonConvert.SerializeObject(response);
                                Console.WriteLine(strResponse);
                                Console.Out.Flush();
                                break;
                            }
                        }//If we make it past here then we just need to start guessing based on what moves are available.
                        pickList = new List<int>();
                        for (int i = 0; i < allowedMoves.Count; i++)
                        {
                            int move;
                            while (true)
                            {
                                Random r = new Random(DateTime.Now.Millisecond);
                                move = r.Next(0, 6);
                                if (!pickList.Contains(move))
                                {
                                    pickList.Add(move);
                                    break;
                                }
                            }
                            if (allowedMoves[move])
                            {
                                Connect4Response response = new Connect4Response(move);
                                string strResponse = JsonConvert.SerializeObject(response);
                                Console.WriteLine(strResponse);
                                Console.Out.Flush();
                                break;
                            }
                        }//if we make it past this, then something is broke OR the mediator passed us a finished board
                    }
                }
            }
        }

        public static bool CounterMoveCheck(Connect4Grid state, int player, out int x, out List<int> disallowedMoves)
        {
            disallowedMoves = new List<int>();
            int height = state.height;
            int width = state.width;
            int counter;
            for (int i = width - 1; i >= 0; i--)
            {
                for (int k = height - 1; k >= 0; k--)
                {
                    if (state.grid[i][k] == player)//dont waste time if this isnt their piece
                    {
                        if (k > 2)//they can win up
                        {
                            if (CheckUp(state, i, k, out counter, player))
                            {
                                x = counter;
                                return true;
                            }
                        }
                        if (i > 2)//means they can win left
                        {
                            if (CheckLeftSplit(state, i, k, out counter, player))
                            {//They can win left, we need to move there
                                x = counter;
                                return true;
                            }
                            else
                            {//They can win left if we move there, disallow this column
                                if (counter != -1)
                                {
                                    disallowedMoves.Add(counter);
                                }
                            }
                            if (k > height - 4)//and up
                            {
                                if (CheckUpLeft(state, i, k, out counter, player))
                                {//They can win left, we need to move there
                                    x = counter;
                                    return true;
                                }
                                else
                                {//They can win left if we move there, disallow this column
                                    if (counter != -1)
                                    {
                                        disallowedMoves.Add(counter);
                                    }
                                }
                            }
                        }
                        if (i < width - 4)//means they can win right
                        {
                            if (CheckRightSplit(state, i, k, out counter, player))
                            {//They can win Right, we need to move there
                                x = counter;
                                return true;
                            }
                            else
                            {//They can win right if we move there, disallow this column
                                if (counter != -1)
                                {
                                    disallowedMoves.Add(counter);
                                }
                            }
                            if (k > height - 4)//and up
                            {
                                if (CheckUpRight(state, i, k, out counter, player))
                                {//They can win Right, we need to move there
                                    x = counter;
                                    return true;
                                }
                                else
                                {//They can win right if we move there, disallow this column
                                    if (counter != -1)
                                    {
                                        disallowedMoves.Add(counter);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            x = -1;
            return false;
        }

        public static bool CheckMoveCanBeMade(Connect4Grid state, int x, int y)
        {//im assuming that the spot in question is already known to be zero
            if(y == state.height -1)//piece is on the ground we can make the move
            {
                return true;
            }
            if (state.grid[x][y + 1] != 0)// check below the move if something is already there
            {
                return true;
            }
            else
                return false;
        }

        public static bool CheckUp(Connect4Grid state, int x, int y, out int counterMove/*This is x*/, int player)
        {
            counterMove = -1;
            if ((y - 1) >= 0 && state.grid[x][y - 1] == player)
            {
                if ((y - 2) >= 0 && state.grid[x][y - 2] == player)
                {
                    counterMove = x;
                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }

        public static bool CheckRightSplit(Connect4Grid state, int x, int y, out int counterMove/*This is x*/, int player)
        {
            counterMove = -1;
            if ((x + 1) < state.width && state.grid[x + 1][y] == player)
            {
                if ((x + 2) < state.width && state.grid[x + 2][y] == player)
                {//E E E ?
                    if ((x + 3) < state.width && state.grid[x + 3][y] == 0)
                    {//E E E 0
                        if (CheckMoveCanBeMade(state, x + 3, y))
                        {
                            counterMove = x + 3;
                            return true;
                        }
                        else if (CheckMoveCanBeMade(state, x + 3, y + 1))
                        {
                            counterMove = x + 3;
                            return false;
                        }
                        else
                            return false;
                    }
                    else//E E E P
                        return false;
                }
                else if ((x + 2) < state.width && state.grid[x + 2][y] != 0)//E E P/0 ?
                    return false;//E E P ?
                else
                {//E E 0 ?
                    if ((x + 3) < state.width && state.grid[x + 3][y] == player)
                    {//E E 0 E
                        if (CheckMoveCanBeMade(state, x + 2, y))
                        {
                            counterMove = x + 2;//counter prevent split win
                            return true;
                        }
                        else if (CheckMoveCanBeMade(state, x - 2, y + 1))
                        {
                            counterMove = x + 2;//disallow move
                            return false;
                        }
                        else
                            return false;
                    }
                    else//E E 0 P/0
                        return false;
                }
            }
            else if ((x + 1) < state.width && state.grid[x + 1][y] != 0)//opposite player
                return false;//E P ? ?
            else
            {
                if ((x + 2) < state.width && state.grid[x + 2][y] == player)
                {//E 0 E ?
                    if ((x + 3) < state.width && state.grid[x + 3][y] == player)
                    {
                        if (CheckMoveCanBeMade(state, x + 1, y))
                        {
                            counterMove = x + 1;//Counter move to prevent split win
                            return true;
                        }
                        else if (CheckMoveCanBeMade(state, x - 1, y + 1))
                        {
                            counterMove = x + 1;//disallow move
                            return false;
                        }
                        else
                            return false;
                    }
                    else//E 0 E P/0
                        return false;
                }
                else//E 0 P/0 ?
                    return false;
            }
        }

        public static bool CheckLeftSplit(Connect4Grid state, int x, int y, out int counterMove/*This is x*/, int player)
        {
            counterMove = -1;
            if ((x - 1) >= 0 && state.grid[x - 1][y] == player)
            {
                if ((x - 2) >= 0 && state.grid[x - 2][y] == player)
                {//E E E ?
                    if ((x - 3) >= 0 && state.grid[x - 3][y] == 0)
                    {//E E E 0
                        if (CheckMoveCanBeMade(state, x - 3, y))
                        {
                            counterMove = x - 3;
                            return true;
                        }
                        else if (CheckMoveCanBeMade(state, x - 3, y + 1))
                        {
                            counterMove = x - 3;
                            return false;
                        }
                        else
                            return false;
                    }
                    else//E E E P
                        return false;
                }
                else if ((x - 2) >= 0 && state.grid[x - 2][y] != 0)//E E P/0 ?
                    return false;//E E P ?
                else
                {//E E 0 ?
                    if ((x - 3) >= 0 && state.grid[x - 3][y] == player)
                    {//E E 0 E
                        if (CheckMoveCanBeMade(state, x - 2, y))
                        {
                            counterMove = x - 2;//counter prevent split win
                            return true;
                        }
                        else if (CheckMoveCanBeMade(state, x - 2, y + 1))
                        {
                            counterMove = x - 2;//disallow move
                            return false;
                        }
                        else
                            return false;
                    }
                    else//E E 0 P/0
                        return false;
                }
            }
            else if ((x - 1) >= 0 && state.grid[x - 1][y] != 0)//opposite player
                return false;//E P ? ?
            else
            {
                if ((x - 2) >= 0 && state.grid[x - 2][y] == player)
                {//E 0 E ?
                    if ((x - 3) >= 0 && state.grid[x - 3][y] == player)
                    {
                        if (CheckMoveCanBeMade(state, x - 1, y))
                        {
                            counterMove = x - 1;//Counter move to prevent split win
                            return true;
                        }
                        else if (CheckMoveCanBeMade(state, x - 1, y + 1))
                        {
                            counterMove = x - 1;//disallow move
                            return false;
                        }
                        else
                            return false;
                    }
                    else//E 0 E P/0
                        return false;
                }
                else//E 0 P/0 ?
                    return false;
            }
        }

        public static bool CheckUpRight(Connect4Grid state, int x, int y, out int counterMove/*This is x*/, int player)
        {
            counterMove = -1;
            if ((x + 1) < state.width && (y - 1) > 0 && state.grid[x + 1][y - 1] == player)
            {
                if ((x + 2) < state.width && (y - 2) > 0 && state.grid[x + 2][y - 2] == player)
                {
                    if (CheckMoveCanBeMade(state, x + 3, y - 3))
                    {
                        counterMove = x + 3;
                        return true;
                    }
                    else if (CheckMoveCanBeMade(state, x + 3, y - 2))
                    {
                        counterMove = x + 3;
                        return false;
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
            else
                return false;
        }

        public static bool CheckUpLeft(Connect4Grid state, int x, int y, out int counterMove/*This is x*/, int player)
        {
            counterMove = -1;
            if ((x - 1) >= 0 && (y - 1) > 0 && state.grid[x - 1][y - 1] == player)
            {
                if ((x - 2) >= 0 && (y - 2) > 0 && state.grid[x - 2][y - 2] == player)
                {
                    if (CheckMoveCanBeMade(state, x - 3, y - 3))
                    {
                        counterMove = x - 3;
                        return true;
                    }
                    else if (CheckMoveCanBeMade(state, x - 3, y - 2))
                    {
                        counterMove = x - 3;
                        return false;
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
            else
                return false;
        }

        public static List<bool> ValidColumns(Connect4Grid state)
        {
            List<bool> validMoves = new List<bool>();
            for(int i = 0; i < state.width; i++)
            {//init list
                validMoves.Add(false);
            }
            for(int i = 0; i < state.width; i ++)
            {
                for(int k = 0; k < state.height; k++)
                {
                    if (state.grid[i][k] == 0)
                    {
                        validMoves[i] = true;
                        break;
                    }
                }
            }
            return validMoves;
        }

        public static bool CheckZero(Connect4Grid check)
        {
            for (int k = 0; k < check.grid[check.grid.Count - 1].Count; k++)
            {
                if (check.grid[check.grid.Count - 1][k] != 0)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
