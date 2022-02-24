using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AIScript : MonoBehaviour
{

    //Game Variables
    public GameObject[] buttons;    // Array of the buttons that show which players turn it is
    public GameObject[] pieces;     // Holds all the game pieces.
    public GameObject[] spaces;     // Contains all the spaces(GameObjects) on the board
    public Tile[] board;            // Holds 9 Tiles which each contain 3 slots. This array tells you which slots are filled and with what color.
    public Player[] players;        // Max 4 players
    public string size;             // Current size the user picked
    public float x, y;              // Positions of selected slot
    public GameObject space;        // Holds the index of the Tile the user has selected
    public GameObject lastSpace;    // Space before the space currently chosen. Used to remove the color indicator from space picked.
    public string color;            // Holds the current players color
    public int next;                // Holds the next players index
    public bool gameOver = false;   // Game ends if this equals true
    public int turn;                // Contains how many turns have passed so far


    //In-Game Text Objects
    public GameObject errorMessage; // Message that gets displayed if the user picks a slot that is already filled
    public GameObject winMessage;   // Background behind win text, changes color based on winner.
    public GameObject winText;      // Text that changes based on winner
    
    //Initializes Players and Board
    void Start()
    {
        players = new Player[4];
        players[0] = new Player("Blue");
        players[1] = new Player("Purple");
        players[2] = new Player("Green");
        players[3] = new Player("Red");

        //The index here chooses who goes first
        players[1].turn = true;
        board = new Tile[9];
        for(int i=0;i<board.Length;i++)
        {
            board[i] = new Tile(new Slot("Empty"), new Slot("Empty"), new Slot("Empty"));
        }
    }

    void Update()
    {
        bestAIMove();
    }

    /**************************************************************
     * PROBLEMS:
     * Algorithm doesn't make final win move. Tends to make two in a row but not three.
     * Algorithm doesn't block players.
     * Sometimes Win Banner doesn't shows wrong winner.
     * 
     * Only purple wins and it seems to be only about 40% of the time.
     * Red occasionally makes horrible moves while purple seems to make okay moves most of the time.
     * Sometimes AI will make two in row even if the third is filled.
     * 
     **************************************************************/

    void bestAIMove()
    {
        int player = getPlayer();
        float bestScore = -100;
        int bestI = -10;
        int bestJ = -10;
        //If its the AI's turn, find best move using MiniMax Algorithm.
        if(player == 1 || player == 3)
        {
            //Run through the 9 spaces on the board
            for (int i = 0; i < board.Length; i++)
            {
                //Run through the 3 slots in each space
                for (int j = 0; j < 3; j++)
                {
                    //Checks if the slot is filled and if the current AI even has a piece to put there.
                    if (board[i].slots[j].filled == false && checkIfPieceIsAvailable(j, players[player].color) != -1)
                    {
                        float random = Random.Range(-0.10f, 0.10f);
                        fillSlot(i, j, players[player].color);
                        float score = minimax(board, 0, true, player, player, -100.0f, 100.0f);
                        score += random;    //Adds Randomness to score to make tied scores not always go to first spot on the board
                        removeSlot(i, j);
                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestI = i;
                            bestJ = j;
                        }
                    }
                }
            }

            if(bestI != -10 || bestJ != -10)
            {
                Debug.Log(players[player].color + ": " + bestScore);
                int pieceIndex = checkIfPieceIsAvailable(bestJ, players[player].color);
                if (pieceIndex != -1 && board[bestI].slots[bestJ].filled == false)
                {
                    space = spaces[bestI];
                    x = space.transform.position.x;
                    y = space.transform.position.y;
                    pieces[pieceIndex].transform.position = new Vector2(x, y);
                    pieces[pieceIndex].name = players[player].color + " Used";
                    fillSlot(bestI, bestJ, players[player].color);
                    setTurn();
                    setButtons();
                    winCheck(false);
                }
            }
        }
        
    }
    
    //Evaluates current state of the board
    public float evaluateBoard(Tile[] board)
    {
        Dictionary<string, float> result = new Dictionary<string, float>();
        float score = 0;
        float negScore = 0;
        result = checkSTB(result);
        result = checkBTS(result);
        result = checkSameSize(result);
        result = checkSameSlot(result);

        foreach(KeyValuePair<string, float> entry in result)
        {
            //AI Team
            if (entry.Key == "Purple" || entry.Key == "Red")
            {
                if(entry.Value > score)
                {
                    score = entry.Value;
                }
            }
            //Human Team
            if (entry.Key == "Blue" || entry.Key == "Green")
            {
                if (entry.Value > negScore)
                {
                    negScore = entry.Value;
                }
            }
        }
        //If the Human team score is higher than the AI team. Return the negative score
        if(negScore > score)
        {
            return negScore * -1;
        }
        //Else return AI score
        else
        {
            return score;
        }

    }

    //Implement max depth with 2 teams
    public float minimax(Tile[] board, int depth, bool isMax, int player, int original, float alpha, float beta)
    {

        if(depth == 3 && turn < 10)
        {
            return evaluateBoard(board);
        }
        if(depth == 4)
        {
            return evaluateBoard(board);
        }
        int nextPlayer;
        if(player == 3)
        {
            nextPlayer = 0;
        }
        else
        {
            nextPlayer = player++;
        }
        string winner = winCheck(true);
        if(winner != null)
        {
            int team = 0;
            if(original == 1)
            {
                team = 3;
            }
            else
            {
                team = 1;
            }
            if(winner == players[original].color || winner == players[team].color)
            {
                return 100;
            }
            else if(winner == "tie")
            {
                return 0;
            }
            else
            {
                return -100;
            }
        }

        if(isMax)
        {
            float bestScore = -1000000;
            for (int i = 0; i < board.Length; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (board[i].slots[j].filled == false && checkIfPieceIsAvailable(j, players[player].color) != -1)
                    {
                        fillSlot(i, j, players[player].color);
                        float score = minimax(board, depth+1, false, nextPlayer, original, alpha, beta);
                        removeSlot(i, j);
                        bestScore = Mathf.Max(score, bestScore);
                        alpha = Mathf.Max(alpha, score);
                        if (beta <= alpha) { break; }
                    }
                }
            }
            return bestScore;
        }
        else
        {
            float bestScore = 1000000;
            for (int i = 0; i < board.Length; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (board[i].slots[j].filled == false && checkIfPieceIsAvailable(j, players[nextPlayer].color) != -1)
                    {
                        fillSlot(i, j, players[player].color);
                        float score = minimax(board, depth + 1, true, nextPlayer, original, alpha, beta);
                        removeSlot(i, j);
                        bestScore = Mathf.Min(score, bestScore);
                        beta = Mathf.Min(beta, score);
                        if(beta <= alpha) { break; }
                    }
                }
            }
            return bestScore;
        }
        
    }
    
    //Random legal move AI
    void AIRandomLegal()
    {
        getPlayer();
        string size;
        int randomInt = Random.Range(0, 3);
        if (randomInt == 0) { size = "Small"; }
        else if (randomInt == 1) { size = "Middle"; }
        else { size = "Large"; }
        int randomSpace = Random.Range(0, 9);
        if (players[1].turn)
        {
            checkAvailableAndFill("Purple", size, randomSpace, randomInt);
        }
        else if (players[2].turn)
        {
            checkAvailableAndFill("Green", size, randomSpace, randomInt);
        }
        else if (players[3].turn)
        {
            checkAvailableAndFill("Red", size, randomSpace, randomInt);
        }
    }
    

    //Assigns the x and y positions based on which Tile the user chose
    public void SpaceSelector()
    {
        if(lastSpace != null)
        {
            lastSpace.GetComponent<Image>().color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        }
        x = EventSystem.current.currentSelectedGameObject.transform.position.x;
        y = EventSystem.current.currentSelectedGameObject.transform.position.y;
        
        space = EventSystem.current.currentSelectedGameObject;
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].turn)
            {
                if (i == 0) { space.GetComponent<Image>().color = new Color(0.0f, 0.0f, 1.0f, 0.3f); }
                if (i == 1) { space.GetComponent<Image>().color = new Color(0.5f, 0.0f, 1.0f, 0.3f); }
                if (i == 2) { space.GetComponent<Image>().color = new Color(0.0f, 1.0f, 0.0f, 0.3f); }
                if (i == 3) { space.GetComponent<Image>().color = new Color(1.0f, 0.0f, 0.0f, 0.3f); }
            }
        }
        lastSpace = space;
    }

    /* This is the main function
     * Checks whos turn it is, then checks which size the user selected
     * Then checks if that player still has any of that size left
     * and if that slot is open. If both are true, then the piece is
     * moved to the proper slot and then sets the next players turn to true. 
     * If that slot is not open, a error message is displayed.
     * Also checks if that player has won or not.
     */
    public void ButtonSize()
    {
        errorMessage.SetActive(false);
        space.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.0f);
        getPlayer();

        size = EventSystem.current.currentSelectedGameObject.name;

        //Large piece
        if (size.Contains("large"))
        {
            for (int i = 0; i < pieces.Length; i++)
            {
                if (pieces[i].name.Contains(color) && pieces[i].name.Contains("Large") && board[int.Parse(space.name)].slots[2].filled == false)
                {
                    pieces[i].transform.position = new Vector2(x, y);
                    pieces[i].name = color + " Used";
                    fillSlot(2, color);
                    setTurn();
                    setButtons();
                    winCheck(false);
                    break;
                }
                if (pieces[i].name.Contains(color) && pieces[i].name.Contains("Large") && board[int.Parse(space.name)].slots[2].filled == true)
                {
                    errorMessage.SetActive(true);
                }
            }
        }
        //Medium piece
        else if (size.Contains("medium"))
        {
            for (int i = 0; i < pieces.Length; i++)
            {
                if (pieces[i].name.Contains(color) && pieces[i].name.Contains("Middle") && board[int.Parse(space.name)].slots[1].filled == false)
                {
                    pieces[i].transform.position = new Vector2(x, y);
                    pieces[i].name = color + " Used";
                    fillSlot(1, color);
                    setTurn();
                    setButtons();
                    winCheck(false);
                    break;
                }
                if (pieces[i].name.Contains(color) && pieces[i].name.Contains("Middle") && board[int.Parse(space.name)].slots[1].filled == true)
                {
                    errorMessage.SetActive(true);
                }
            }
        }
        //Small piece
        else if (size.Contains("small"))
        {
            for (int i = 0; i < pieces.Length; i++)
            {
                if (pieces[i].name.Contains(color) && pieces[i].name.Contains("Small") && board[int.Parse(space.name)].slots[0].filled == false)
                {
                    pieces[i].transform.position = new Vector2(x, y);
                    pieces[i].name = color + " Used";
                    fillSlot(0, color);
                    setTurn();
                    setButtons();
                    winCheck(false);
                    break;
                }
                if (pieces[i].name.Contains(color) && pieces[i].name.Contains("Small") && board[int.Parse(space.name)].slots[0].filled == true)
                {
                    errorMessage.SetActive(true);
                }
            }
        }
    }

    public void checkAvailableAndFill(string color, string size, int chosenSpace, int slotsize)
    {
        for (int i = 0; i < pieces.Length; i++)
        {
            if (pieces[i].name.Contains(color) && pieces[i].name.Contains(size) && board[chosenSpace].slots[slotsize].filled == false)
            {
                space = spaces[chosenSpace];
                x = space.transform.position.x;
                y = space.transform.position.y;
                pieces[i].transform.position = new Vector2(x, y);
                pieces[i].name = color + " Used";
                fillSlot(slotsize, color);
                setTurn();
                setButtons();
                winCheck(false);
            }
        }
    }

    public int checkIfPieceIsAvailable(int size, string color)
    {
        string sizeString = "";
        if(size == 0)
        {
            sizeString = "Small";
        }
        else if(size == 1)
        {
            sizeString = "Middle";
        }
        else
        {
            sizeString = "Large";
        }
        for(int i=0;i<pieces.Length;i++)
        {
            if (pieces[i].name.Contains(color) && pieces[i].name.Contains(sizeString))
            {
                return i;
            }
        }
        return -1;
    }

    //Fills in the Board array with the size and color of the piece
    public void fillSlot(int size, string color)
    {
        board[int.Parse(space.name)].slots[size].filled = true;
        board[int.Parse(space.name)].slots[size].color = color;
    }

    public void fillSlot(int spaceint, int size, string color)
    {
        board[spaceint].slots[size].filled = true;
        board[spaceint].slots[size].color = color;
    }

    public void removeSlot(int spaceint, int size)
    {
        board[spaceint].slots[size].filled = false;
        board[spaceint].slots[size].color = "Empty";
    }

    //Combines all functions below to check for all the types of wins
    public string winCheck(bool inMinimax)
    {
        Dictionary<string, float> result = new Dictionary<string, float>();
        string winner = null;

        result = checkSTB(result);
        result = checkBTS(result);
        result = checkSameSize(result);
        result = checkSameSlot(result);

        foreach(KeyValuePair<string, float> entry in result)
        {
            //Debug dictionary here
            if(entry.Value == 100)
            {
                winner = entry.Key;
                break;
            }
        }
        int openSlots = 0;
        for(int i=0;i<board.Length;i++)
        {
            for(int j=0;j<3;j++)
            {
                if(board[i].slots[j].filled == false)
                {
                    openSlots++;
                }
            }
        }
        if (gameOver == false && openSlots == 0)
        {
            return "tie";
        }
        if(winner != null && inMinimax == false)
        {
            setWinner(winner);
            return winner;
        }
        return winner;
    }

    //Checks small to big win
    public Dictionary<string, float> checkSTB(Dictionary<string, float> result)
    {
        
        //Row Check
        for(int i=0;i<board.Length;i=i+3)
        {
            if(board[i].slots[0].color == board[i+1].slots[1].color &&
                board[i].slots[0].color == board[i+2].slots[2].color && board[i].slots[0].color != "Empty")
            {
                result = addScore(result, i, 0, true);
                return result;
            }
            else if (board[i].slots[0].color == board[i + 1].slots[1].color && board[i].slots[0].color != "Empty")
            {
                result = addScore(result, i, 0, false);
                return result;
            }
            else if (board[i].slots[0].color == board[i + 2].slots[2].color && board[i].slots[0].color != "Empty")
            {
                result = addScore(result, i, 0, false);
                return result;
            }
        }
        //Column Check
        for (int i = 0; i < 3; i++)
        {
            if (board[i].slots[0].color == board[i + 3].slots[1].color &&
                board[i].slots[0].color == board[i + 6].slots[2].color && board[i].slots[0].color != "Empty")
            {
                result = addScore(result, i, 0, true);
                return result;
            }
            else if (board[i].slots[0].color == board[i + 3].slots[1].color && board[i].slots[0].color != "Empty")
            {
                result = addScore(result, i, 0, false);
                return result;
            }
            else if (board[i].slots[0].color == board[i + 6].slots[2].color && board[i].slots[0].color != "Empty")
            {
                result = addScore(result, i, 0, false);
                return result;
            }
        }
        //Diagonal Check #1
        if (board[0].slots[0].color == board[4].slots[1].color &&
             board[0].slots[0].color == board[8].slots[2].color && board[0].slots[0].color != "Empty")
        {
            result = addScore(result, 0, 0, true);
            return result;
        }
        else if (board[0].slots[0].color == board[4].slots[1].color &&board[0].slots[0].color != "Empty")
        {
            result = addScore(result, 0, 0, false);
            return result;
        }
        else if (board[0].slots[0].color == board[8].slots[2].color && board[0].slots[0].color != "Empty")
        {
            result = addScore(result, 0, 0, false);
            return result;
        }
        //Diagonal Check #2
        if (board[2].slots[0].color == board[4].slots[1].color &&
             board[2].slots[0].color == board[6].slots[2].color && board[2].slots[0].color != "Empty")
        {
            result = addScore(result, 2, 0, true);
            return result;
        }
        else if (board[2].slots[0].color == board[4].slots[1].color && board[2].slots[0].color != "Empty")
        {
            result = addScore(result, 2, 0, false);
            return result;
        }
        else if (board[2].slots[0].color == board[6].slots[2].color && board[2].slots[0].color != "Empty")
        {
            result = addScore(result, 2, 0, false);
            return result;
        }
        return result;
    }

    //Checks big to small win
    public Dictionary<string, float> checkBTS(Dictionary<string, float> result)
    {
        //Row Check
        for (int i = 0; i < board.Length; i = i + 3)
        {
            if (board[i].slots[2].color == board[i + 1].slots[1].color &&
                board[i].slots[2].color == board[i + 2].slots[0].color && board[i].slots[2].color != "Empty")
            {
                result = addScore(result, i, 2, true);
                return result;
            }
            else if (board[i].slots[2].color == board[i + 1].slots[1].color && board[i].slots[2].color != "Empty")
            {
                result = addScore(result, i, 2, false);
                return result;
            }
            else if (board[i].slots[2].color == board[i + 2].slots[0].color && board[i].slots[2].color != "Empty")
            {
                result = addScore(result, i, 2, false);
                return result;
            }
        }
        //Column Check
        for (int i = 0; i < 3; i++)
        {
            if (board[i].slots[2].color == board[i + 3].slots[1].color &&
                board[i].slots[2].color == board[i + 6].slots[0].color && board[i].slots[2].color != "Empty")
            {
                result = addScore(result, i, 2, true);
                return result;
            }
            else if (board[i].slots[2].color == board[i + 3].slots[1].color && board[i].slots[2].color != "Empty")
            {
                result = addScore(result, i, 2, false);
                return result;
            }
            else if (board[i].slots[2].color == board[i + 6].slots[0].color && board[i].slots[2].color != "Empty")
            {
                result = addScore(result, i, 2, false);
                return result;
            }
        }
        //Diagonal Check #1
        if (board[0].slots[2].color == board[4].slots[1].color &&
             board[0].slots[2].color == board[8].slots[0].color && board[0].slots[2].color != "Empty")
        {
            result = addScore(result, 0, 2, true);
            return result;
        }
        else if (board[0].slots[2].color == board[4].slots[1].color && board[0].slots[2].color != "Empty")
        {
            result = addScore(result, 0, 2, false);
            return result;
        }
        else if (board[0].slots[2].color == board[8].slots[0].color && board[0].slots[2].color != "Empty")
        {
            result = addScore(result, 0, 2, false);
            return result;
        }
        //Diagonal Check #2
        if (board[2].slots[2].color == board[4].slots[1].color &&
             board[2].slots[2].color == board[6].slots[0].color && board[2].slots[2].color != "Empty")
        {
            result = addScore(result, 2, 2, true);
            return result;
        }
        else if (board[2].slots[2].color == board[4].slots[1].color && board[2].slots[2].color != "Empty")
        {
            result = addScore(result, 2, 2, false);
            return result;
        }
        else if (board[2].slots[2].color == board[6].slots[0].color && board[2].slots[2].color != "Empty")
        {
            result = addScore(result, 2, 2, false);
            return result;
        }
        return result;
    }

    //Checks same size win
    public Dictionary<string, float> checkSameSize(Dictionary<string, float> result)
    {
        for(int j=0;j<3;j++)
        {
            //Row Check
            for (int i = 0; i < board.Length; i = i + 3)
            {
                if (board[i].slots[j].color == board[i + 1].slots[j].color &&
                    board[i].slots[j].color == board[i + 2].slots[j].color && board[i].slots[j].color != "Empty")
                {
                    result = addScore(result, i, j, true);
                    return result;
                }
                else if (board[i].slots[j].color == board[i + 1].slots[j].color && board[i].slots[j].color != "Empty")
                {
                    result = addScore(result, i, j, false);
                    return result;
                }
                else if (board[i].slots[j].color == board[i + 2].slots[j].color && board[i].slots[j].color != "Empty")
                {
                    result = addScore(result, i, j, false);
                    return result;
                }
            }
            //Column Check
            for (int i = 0; i < 3; i++)
            {
                if (board[i].slots[j].color == board[i + 3].slots[j].color &&
                    board[i].slots[j].color == board[i + 6].slots[j].color && board[i].slots[j].color != "Empty")
                {
                    result = addScore(result, i, j, true);
                    return result;
                }
                else if (board[i].slots[j].color == board[i + 3].slots[j].color && board[i].slots[j].color != "Empty")
                {
                    result = addScore(result, i, j, false);
                    return result;
                }
                else if (board[i].slots[j].color == board[i + 6].slots[j].color && board[i].slots[j].color != "Empty")
                {
                    result = addScore(result, i, j, false);
                    return result;
                }
            }
            //Diagonal Check #1
            if (board[0].slots[j].color == board[4].slots[j].color &&
                 board[0].slots[j].color == board[8].slots[j].color && board[0].slots[j].color != "Empty")
            {
                result = addScore(result, 0, j, true);
                return result;
            }
            else if (board[0].slots[j].color == board[4].slots[j].color && board[0].slots[j].color != "Empty")
            {
                result = addScore(result, 0, j, false);
                return result;
            }
            else if (board[0].slots[j].color == board[8].slots[j].color && board[0].slots[j].color != "Empty")
            {
                result = addScore(result, 0, j, false);
                return result;
            }
            //Diagonal Check #1
            if (board[2].slots[j].color == board[4].slots[j].color &&
                 board[2].slots[j].color == board[6].slots[j].color && board[2].slots[j].color != "Empty")
            {
                result = addScore(result, 2, j, true);
                return result;
            }
            else if (board[2].slots[j].color == board[4].slots[j].color && board[2].slots[j].color != "Empty")
            {
                result = addScore(result, 2, j, false);
                return result;
            }
            else if (board[2].slots[j].color == board[6].slots[j].color && board[2].slots[j].color != "Empty")
            {
                result = addScore(result, 2, j, false);
                return result;
            }
        }
        return result;
    }

    //Checks same slot win
    public Dictionary<string, float> checkSameSlot(Dictionary<string, float> result)
    {
        for(int i=0;i<board.Length;i++)
        {
            bool first = false;
            float check = 0;
            string c = "";
            for(int j=0;j<3;j++)
            {
                if (board[i].slots[j].filled && first == false)
                {
                    first = true;
                    c = board[i].slots[j].color;
                    check++;
                }
                else if (board[i].slots[j].color == c)
                {
                    check++;
                }
                if(check == 3)
                {
                    result = addScore(result, c, true);
                    return result;
                }
            }
            if(check == 2)
            {
                result = addScore(result, c, false);
                return result;
            }
        }
        return result;
    }

    //Adds score to results dictionary using the pieces location (i and j)
    //Results dictionary contains the pieces color (string) and the score.
    public Dictionary<string, float> addScore(Dictionary<string, float> dict, int i, int j, bool win)
    {
        if(win)
        {
            if (!dict.ContainsKey(board[i].slots[j].color))
                {
                dict.Add(board[i].slots[j].color, 100);
                return dict;
            }
            else
            {
                dict.Remove(board[j].slots[j].color);
                dict.Add(board[j].slots[j].color, 100);
                return dict;
            }
        }
        else
        {
            if (!dict.ContainsKey(board[i].slots[j].color))
                {
                dict.Add(board[i].slots[j].color, 1);
                return dict;
            }
            else
            {
                float score = dict[board[i].slots[j].color];
                dict.Remove(board[j].slots[j].color);
                dict.Add(board[j].slots[j].color, score + 1);
                return dict;
            }
        }
    }

    //Adds score to results dictionary using color
    //Results dictionary contains the pieces color (string) and the score.
    public Dictionary<string, float> addScore(Dictionary<string, float> dict, string color, bool win)
    {
        if (win)
        {
            if (!dict.ContainsKey(color))
                {
                dict.Add(color, 100);
                return dict;
            }
            else
            {
                dict.Remove(color);
                dict.Add(color, 100);
                return dict;
            }
        }
        else
        {
            if (!dict.ContainsKey(color))
                {
                dict.Add(color, 1);
                return dict;
            }
            else
            {
                float score = dict[color];
                dict.Remove(color);
                dict.Add(color, score + 1);
                return dict;
            }
        }
    }

    //Gets current player
    public int getPlayer()
    {
        int current = -1;
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].turn == true)
            {
                color = players[i].color;
                current = i;
                if (i == 3)
                {
                    next = 0;
                }
                else
                {
                    next = i + 1;
                }
                return current;
            }
        }
        return current;
    }

    //Sets next players turn to true and the rest to false
    public void setTurn()
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (i == next)
            {
                players[i].turn = true;
            }
            else
            {
                players[i].turn = false;
            }
        }
        turn += 1;
    }

    //Sets button to the current players color
    public void setButtons()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i].name.Contains(players[next].color))
            {
                buttons[i].SetActive(true);
            }
            else
            {
                buttons[i].SetActive(false);
            }
        }
    }

    //Sets win text to the player who won and activates win pop up
    public string setWinner(string color)
    {
        Debug.Log(color + " wins!");
        winText.GetComponent<Text>().text = color + " wins!";
        //Blue
        if (color == "Blue") { winMessage.GetComponent<Image>().color = new Color(0.0f, 0.0f, 1.0f, 1.0f); }
        //Purple
        if (color == "Purple") { winMessage.GetComponent<Image>().color = new Color(0.7f, 0.0f, 1.0f, 1.0f); }
        //Green
        if (color == "Green") { winMessage.GetComponent<Image>().color = new Color(0.0f, 1.0f, 0.0f, 1.0f); }
        //Red
        if (color == "Red") { winMessage.GetComponent<Image>().color = new Color(1.0f, 0.0f, 0.0f, 1.0f); }
        winMessage.SetActive(true);
        gameOver = true;
        
        return color;
    }
}
