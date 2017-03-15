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

    public GameObject _LoadSaveMenu;
    public GameObject _EndGameMenu;

    public int[] m_BoardState = new int[9] { 0,0,0,0,0,0,0,0,0};
    public Image[] m_BoardImages = new Image[9];
    public bool m_IsXTurn = true;
    public bool m_IsAI_X = false;

    public int m_Player = 1;
    private bool m_IsGameover = false;

    private bool m_IsSaving = false;

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
            Load(1);
            for (int i = 0; i < 9; i++)
                Debug.Log(m_BoardState[i]);
        }
    }

    public void MakeMove(int move)
    {
        Debug.Log(mainGame.currentTurn);
        if (mainGame.currentTurn != m_Player || m_IsGameover)
        {
            Debug.Log("Cant move now");
            return;
        }
        if (move > 9 || m_BoardState[move] != 0)
        {
            return;
        }
        mainGame.playerMove(move);
        UpdateBoard(move);
        if (mainGame.turnCount == 9)
        {
            return;
        }
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
            m_BoardState[move] = -1;
            m_BoardImages[move].sprite = _O;
        }
        m_IsXTurn = !m_IsXTurn;
        Debug.Log("Board status: " + mainGame.win()+ ". Currently on turn: " + mainGame.turnCount);
        if (mainGame.win() != 0)
        {
            EndGame(mainGame.win());
        }
        else if (mainGame.turnCount == 9)
        {
            EndGame(2);
        }
    }

    IEnumerator WaitForCPU()
    {
        yield return new WaitForSeconds(1f);
       
        UpdateBoard(mainGame.computerMove());
    }

    public void NewGame()
    {
        _LoadSaveMenu.SetActive(false);
        _EndGameMenu.SetActive(false);
        m_IsGameover = false;
        m_BoardState = new int[9] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        mainGame = new TicTac(m_Player);
        m_IsXTurn = true;
        for (int i = 1; i < 10; i++)
        {
            m_BoardImages[i - 1].sprite = _Empty;
        }
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void SelectSlot(int slot)
    {
        Debug.Log("Selecting slot: " + slot + " is saving: " + m_IsSaving);
        if (m_IsSaving)
        {
            Save(slot);
        }
        else
        {
            Load(slot);
        }
    }

    public void Save(int slot)
    {
        
        _LoadSaveMenu.SetActive(false);
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

        _EndGameMenu.SetActive(false);
        _LoadSaveMenu.SetActive(false);
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
        int turncount = 0;
        for (int i = 0; i < 9; i++)
        {
            if (m_BoardState[i] == 0)
            {
                m_BoardImages[i].sprite = _Empty;
            }
            else if (m_BoardState[i] == 1)
            {
                m_BoardImages[i].sprite = _X;
                turncount++;
            }
            else
            {
                m_BoardImages[i].sprite = _O;
                turncount++;
            }
                
        }
        if (turncount % 2 == 0)
        {
            m_IsXTurn = true;
        }
        else
        {
            m_IsXTurn = false;
        }
        mainGame = new TicTac(m_Player  ,m_BoardState);
        m_IsGameover = false;
        return true;
    }

    public void OpenSaveMenu()
    {
        m_IsSaving = true;
        _LoadSaveMenu.transform.FindChild("txt_saveLoad").GetComponent<Text>().text = "Save Game";
        _LoadSaveMenu.SetActive(true);
    }

    public void OpenLoadMenu()
    {
        m_IsSaving = false;
        _LoadSaveMenu.transform.FindChild("txt_saveLoad").GetComponent<Text>().text = "Load Game";
        _LoadSaveMenu.SetActive(true);
    }

    void EndGame(int type)
    {
        m_IsGameover = true;
        _EndGameMenu.SetActive(true);
        if (type == -1)//X wins
        {
            _EndGameMenu.transform.FindChild("Text").GetComponent<Text>().text = "X\n WINS!!!";
        }
        else if (type == 1)//O wins
        {
            _EndGameMenu.transform.FindChild("Text").GetComponent<Text>().text = "O\n WINS!!!";            
        }
        else//draw
        {           
            _EndGameMenu.transform.FindChild("Text").GetComponent<Text>().text = "It's a Draw";
        }
    }
}

class TicTac
{
    public TicTac(int player)
    {
        m_Player = player;
    }
    public TicTac(int player, int[] loaded)
    {
        m_Player = player;
        board = loaded;
    }

    public int[] board = new int[9] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    public int currentTurn = 1;
    private int m_Player;
    public int turnCount = 0;

    public int win() 
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
        turnCount++;
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
        turnCount++;
        currentTurn = 0;
        board[move] = -1;
    }

}