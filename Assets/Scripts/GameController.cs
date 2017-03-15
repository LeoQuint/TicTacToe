using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

    [SerializeField]
    Sprite _X;
    [SerializeField]
    Sprite _O;
    [SerializeField]
    Sprite _Empty;

    [SerializeField]
    Transform _Board;

    public int[] m_BoardState = new int[9] { 0,0,0,0,0,0,0,0,0};
    public Image[] m_BoardImages = new Image[9];
    public bool m_IsXTurn = true;
    public bool m_IsAI_X = false;

    public int m_Player = 1;


    TicTac mainGame;

	// Use this for initialization
	void Start () {
        mainGame = new TicTac(m_Player);
        for (int i = 1; i < 10; i++)
        {
            m_BoardImages[i-1] = _Board.FindChild(i.ToString()).GetComponent<Image>();
            m_BoardImages[i - 1].sprite = _Empty;
        }
	}


    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown(KeyCode.S))
        {
            Save(1);
            for(int i = 0; i < 9; i++)
                Debug.Log(m_BoardState[i]);
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            Load(2);
            for (int i = 0; i < 9; i++)
                Debug.Log(m_BoardState[i]);
        }
    }

    public void MakeMove(int move)
    {
        if (mainGame.currentTurn != m_Player)
        {
            return;
        }
        if (move > 9 || m_BoardState[move] != 0)
        {
            return;
        }
        mainGame.playerMove(move);
        UpdateBoard(move);
        StartCoroutine(WaitForCPU());
    }


    void UpdateBoard(int move)
    {             
        if (m_IsXTurn)//X is playing
        {
            m_BoardState[move] = 1;
            m_BoardImages[move].sprite = _X;
        }
        else//O is playing
        {
            m_BoardState[move] = 2;
            m_BoardImages[move].sprite = _O;
        }
        m_IsXTurn = !m_IsXTurn;
    }

    IEnumerator WaitForCPU()
    {
        yield return new WaitForSeconds(1f);
        UpdateBoard(mainGame.computerMove());
    }

    public void Save(int slot)
    {
        string saved = string.Join(";", new List<int>(m_BoardState).ConvertAll(i => i.ToString()).ToArray());
             Debug.Log(saved);
        switch (slot)
        {
            case 1:
                PlayerPrefs.SetString("TicTac1", saved);
                break;
            case 2:
                PlayerPrefs.SetString("TicTac2", saved);
                break;
            case 3:
                PlayerPrefs.SetString("TicTac3", saved);
                break;
        }
    }

    public bool Load(int slot)
    {
        string loaded = "";
  
        switch (slot)
        {
            case 1:
                if (PlayerPrefs.HasKey("TicTac1"))
                    loaded = PlayerPrefs.GetString("TicTac1");
                else
                    return false;
                break;
            case 2:
                if (PlayerPrefs.HasKey("TicTac2"))
                    loaded = PlayerPrefs.GetString("TicTac2");
                else
                    return false;
                break;
            case 3:
                if (PlayerPrefs.HasKey("TicTac3"))
                    loaded = PlayerPrefs.GetString("TicTac3");
                else
                    return false;
                break;
        }
        m_BoardState = loaded.Split(';').Select(n => System.Convert.ToInt32(n)).ToArray();
        return true;
    }
}

class TicTac
{
    public TicTac(int player)
    {
        m_Player = player;
    }

    public int[] board = new int[9] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    public int currentTurn = 1;
    private int m_Player;

    int win() 
    {
        //determines if a player has won, returns 0 otherwise.
        int[,] wins = { { 0, 1, 2 }, { 3, 4, 5 }, { 6, 7, 8 }, { 0, 3, 6 }, { 1, 4, 7 }, { 2, 5, 8 }, { 0, 4, 8 }, { 2, 4, 6 } };
	    int i;
	    for (i = 0; i< 8; ++i) 
        {
		    if (board[wins[i,0]] != 0 &&
			    board[wins[i,0]] == board[wins[i,1]] &&
			    board[wins[i,0]] == board[wins[i,2]])
			    return board[wins[i,2]];
	    }
	    return 0;
    }

    int minimax(int player)
    {
        //How is the position like for player (their turn) on board?
        int winner = win();
        if (winner != 0) return winner * player;

        int move = -1;
        int score = -2;//Losing moves are preferred to no move
        int i;
        for (i = 0; i < 9; ++i)
        {//For all moves,
            if (board[i] == 0)
            {//If legal,
                board[i] = player;//Try the move
                int thisScore = -minimax(player * -1);
                if (thisScore > score)
                {
                    score = thisScore;
                    move = i;
                }//Pick the one that's worst for the opponent
                board[i] = 0;//Reset board after try
            }
        }
        if (move == -1) return 0;
        return score;
    }

    public int computerMove()
    {
        int move = -1;
        int score = -2;
        int i;
        for (i = 0; i < 9; ++i)
        {
            if (board[i] == 0)
            {
                board[i] = 1;
                int tempScore = -minimax(-1);
                board[i] = 0;
                if (tempScore > score)
                {
                    score = tempScore;
                    move = i;
                }
            }
        }
        //returns a score based on minimax tree at a given node.
        board[move] = 1;
        currentTurn = 1;
        return move;
    }

    public void playerMove(int move)
    {
        currentTurn = 0;
        board[move] = -1;
    }

}