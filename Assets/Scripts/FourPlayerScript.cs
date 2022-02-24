using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FourPlayerScript : MonoBehaviour
{
    //Holds all the game pieces.
    public GameObject[] pieces;

    //Holds 9 Tiles which each contain 3 slots. This array tells you which slots
    //are filled and with what color.
    public Tile[] board;

    //List of 4 players
    public Player[] players;

    //Holds the current size of the piece that the user has selected
    public string size;

    //Holds the x and y positions of the Tile the user has selected
    public float x, y;

    //Holds the index of the Tile the user has selected
    public GameObject space;

    public GameObject lastSpace;

    //Holds the error message to display when the user tries to put a piece
    //in a slot that has already been used.
    public GameObject errorMessage;

    //Holds the win message
    public GameObject winMessage;
    
    //Holds the win text that changes based on who wins
    public GameObject winText;

    //Holds the current players color
    public string color;

    //Holds the next players index
    public int next;
    
    //Array of buttons to show which players turn it is
    public GameObject[] buttons;

    //Initializes Players and Board
    void Start()
    {
        players = new Player[4];
        players[0] = new Player("Blue");
        players[1] = new Player("Purple");
        players[2] = new Player("Green");
        players[3] = new Player("Red");
        players[0].turn = true;
        board = new Tile[9];
        for(int i=0;i<board.Length;i++)
        {
            board[i] = new Tile(new Slot("Empty"), new Slot("Empty"), new Slot("Empty"));
        }
    }

    //Unused
    void Update()
    {

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

    //This is the main function
    //Checks whos turn it is, then checks which size the user selected
    //Then checks if that player still has any of that size left
    //and if that slot is open. If both are true, then the piece is
    //moved to the proper slot and then sets the next players turn to true. 
    //If that slot is not open, a error message is displayed.
    //Also checks if that player has won or not.
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
                    winCheck();
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
                    winCheck();
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
                    winCheck();
                    break;
                }
                if (pieces[i].name.Contains(color) && pieces[i].name.Contains("Small") && board[int.Parse(space.name)].slots[0].filled == true)
                {
                    errorMessage.SetActive(true);
                }
            }
        }
    }

    //Fills in the Board array with the size and color of the piece
    public void fillSlot(int size, string color)
    {
        board[int.Parse(space.name)].slots[size].filled = true;
        board[int.Parse(space.name)].slots[size].color = color;
    }

    //Combines all functions below to check for all the types of wins
    public void winCheck()
    {
        checkSTB();
        checkBTS();
        checkSameSize();
        checkSameSlot();
    }

    //Checks small to big win
    public void checkSTB()
    {
        //Row Check
        for(int i=0;i<board.Length;i=i+3)
        {
            if(board[i].slots[0].color == board[i+1].slots[1].color &&
                board[i].slots[0].color == board[i+2].slots[2].color && board[i].slots[0].color != "Empty")
            {
                setWinner();
            }
        }
        //Column Check
        for (int i = 0; i < 3; i++)
        {
            if (board[i].slots[0].color == board[i + 3].slots[1].color &&
                board[i].slots[0].color == board[i + 6].slots[2].color && board[i].slots[0].color != "Empty")
            {
                setWinner();
            }
        }
        //Diagonal Check
        if (board[0].slots[0].color == board[4].slots[1].color &&
             board[0].slots[0].color == board[8].slots[2].color && board[0].slots[0].color != "Empty")
        {
            setWinner();
        }
        if (board[2].slots[0].color == board[4].slots[1].color &&
             board[2].slots[0].color == board[6].slots[2].color && board[2].slots[0].color != "Empty")
        {
            setWinner();
        }
    }

    //Checks big to small win
    public void checkBTS()
    {
        //Row Check
        for (int i = 0; i < board.Length; i = i + 3)
        {
            if (board[i].slots[2].color == board[i + 1].slots[1].color &&
                board[i].slots[2].color == board[i + 2].slots[0].color && board[i].slots[2].color != "Empty")
            {
                setWinner();
            }
        }
        //Column Check
        for (int i = 0; i < 3; i++)
        {
            if (board[i].slots[2].color == board[i + 3].slots[1].color &&
                board[i].slots[2].color == board[i + 6].slots[0].color && board[i].slots[2].color != "Empty")
            {
                setWinner();
            }
        }
        //Diagonal Check
        if (board[0].slots[2].color == board[4].slots[1].color &&
             board[0].slots[2].color == board[8].slots[0].color && board[0].slots[2].color != "Empty")
        {
            setWinner();
        }
        if (board[2].slots[2].color == board[4].slots[1].color &&
             board[2].slots[2].color == board[6].slots[0].color && board[2].slots[2].color != "Empty")
        {
            setWinner();
        }
    }

    //Checks same size win
    public void checkSameSize()
    {
        for(int j=0;j<3;j++)
        {
            //Row Check
            for (int i = 0; i < board.Length; i = i + 3)
            {
                if (board[i].slots[j].color == board[i + 1].slots[j].color &&
                    board[i].slots[j].color == board[i + 2].slots[j].color && board[i].slots[j].color != "Empty")
                {
                    setWinner();
                }
            }
            //Column Check
            for (int i = 0; i < 3; i++)
            {
                if (board[i].slots[j].color == board[i + 3].slots[j].color &&
                    board[i].slots[j].color == board[i + 6].slots[j].color && board[i].slots[j].color != "Empty")
                {
                    setWinner();
                }
            }
            //Diagonal Check
            if (board[0].slots[j].color == board[4].slots[j].color &&
                 board[0].slots[j].color == board[8].slots[j].color && board[0].slots[j].color != "Empty")
            {
                setWinner();
            }
            if (board[2].slots[j].color == board[4].slots[j].color &&
                 board[2].slots[j].color == board[6].slots[j].color && board[2].slots[j].color != "Empty")
            {
                setWinner();
            }
        }
    }

    //Checks same slot win
    public void checkSameSlot()
    {
        for(int i=0;i<board.Length;i++)
        {
            bool first = false;
            int check = 0;
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
                    setWinner();
                }
            }
        }
    }

    //Gets current player
    public void getPlayer()
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].turn == true)
            {
                color = players[i].color;

                if (i == 3)
                {
                    next = 0;
                }
                else
                {
                    next = i + 1;
                }

            }
        }
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

    //Gets the winner
    public string getWinner()
    {
        for(int i=0;i<players.Length;i++)
        {
            if(players[i].turn)
            {
                if (i == 0)
                {
                    return players[3].color;
                }
                else
                {
                    return players[i - 1].color;
                }
            }
        }
        return null;
    }

    //Sets win text to the player who won and activates win pop up
    public void setWinner()
    {
        winText.GetComponent<Text>().text = getWinner() + " wins!";
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].turn)
            {

                //Blue
                if (i == 1) { winMessage.GetComponent<Image>().color = new Color(0.0f, 0.0f, 1.0f, 1.0f); }
                //Purple
                if (i == 2) { winMessage.GetComponent<Image>().color = new Color(0.7f, 0.0f, 1.0f, 1.0f); }
                //Green
                if (i == 3) { winMessage.GetComponent<Image>().color = new Color(0.0f, 1.0f, 0.0f, 1.0f); }
                //Red
                if (i == 0) { winMessage.GetComponent<Image>().color = new Color(1.0f, 0.0f, 0.0f, 1.0f); }
            }
        }
        winMessage.SetActive(true);
    }
}

public class Player
{
    public bool turn = false;
    public string color;
    public Player(string color)
    {
        this.color = color;
    }
}
