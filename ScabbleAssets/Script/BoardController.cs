using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum TileType
{
    Simple,
    DL,
    TL,
    DW,
    TW,
    START,
}
[System.Serializable]
public class TileReferences
{
    public TileType type;
    public GameObject position;
}

public class BoardController : MonoBehaviour
{
    public GameObject tempDragTile;
    public GameObject lastSelectedTile;
    public GameObject baseTile;
    public GameObject dragTile;
	
    public Vector2 gridSize;
    public float tileFactor;
    public List<TileReferences> tileReferences;
    public Color tileColor;
    public List<GameObject> optionTiles;
    public List<(GameObject, string)> currentMove;
    public List<GameObject> currentHoldingMove;
    public Button onWordSubmit;
    public Button onWordShuffle;
    public Button onTurnSkip;
    public List<string> wordsConstructed;
    public TextAsset dic;
    private List<string> dicWords;
    public bool emptyBoard;
    public List<GameObject> baseTiles;
    private string str = "EEEEEEEEEEEEAAAAAAAAAIIIIIIIIIOOOOOOOONNNNNNTTTTTTLLLLSSSSUUUUDDDDGGGBBCCMMPPFFHHVVYYKJXQZ";
    private List<string> lettersStr;
    private Dictionary<string, int> scoring;
    public Button btnVsPlayer;
    public Button btnVsCPU;
    public Button btnExit;
    public Text cpuOrOpponent;
    public Text cpuOrOpponentScore;
    public Text playerscore;
    public Text txtTurn;

    public GameObject panelMainMenu;
    public GameObject panelGameWon;
    public GameObject panelGameUI;
    public GameObject gameplay;

    public Button btnBack;
    public Button btnToHome;
    public Button btnContinue;
    public GameObject panelPause;

    public Text wonplayer1txt;
    public Text wonplayer2txt;
    public Button wonhome;

    public GameObject panelSettings;
    public Button btnSettings;
    public Button btnSettingsClose;
    public Button board1;
    public Button board2;
    public Toggle music;
    public SpriteRenderer board;
    public AudioSource audioSource;
    public Sprite[] boards;
    
    public bool vsPlayerMode;
    public bool vsCPUMode;
    public int playerTurn;

    public int player1Score;
    public int player2Score;
    private void Start()
    {
        player1Score = player2Score = 0;
        btnSettings.onClick.AddListener(() => { panelSettings.SetActive(true); });
        btnSettingsClose.onClick.AddListener(() => { panelSettings.SetActive(false); });

        music.isOn = PlayerPrefs.GetInt("music", 1) == 1 ? true : false;

        board1.onClick.AddListener(() => { PlayerPrefs.SetInt("board", 0); });
        board2.onClick.AddListener(() => { PlayerPrefs.SetInt("board", 1); });
        music.onValueChanged.AddListener((value) => { PlayerPrefs.SetInt("music", value == true ? 1 : 0); audioSource.enabled = value; });

        onWordSubmit.onClick.AddListener(ClickSubmit);
        onWordShuffle.onClick.AddListener(ClickShuffle);
        onTurnSkip.onClick.AddListener(ClickSkip);
        dicWords = new List<string>(dic.text.Split(new char[] { '\r', '\n' }));
        btnExit.onClick.AddListener(Application.Quit);
        btnVsCPU.onClick.AddListener(SetGameWithAI);
        btnVsPlayer.onClick.AddListener(SetGameWithPlayer);

        btnBack.onClick.AddListener(() => { gameplay.SetActive(false); panelPause.SetActive(true); });
        btnContinue.onClick.AddListener(() => { gameplay.SetActive(true); panelPause.SetActive(false); });
        btnToHome.onClick.AddListener(() => { SceneManager.LoadScene(0); });

        wonhome.onClick.AddListener(() => { SceneManager.LoadScene(0); });

        panelMainMenu.SetActive(true);
        panelGameUI.SetActive(false);
        gameplay.SetActive(false);
    }
    private void showWon()
    {
        gameplay.SetActive(false);
        panelGameWon.SetActive(true);
        wonplayer1txt.text = $"Player 1 Score : {playerscore}";
        wonplayer2txt.text = $"Player 2 Score : {cpuOrOpponent}";
    }
    private void init()
    {
        board.sprite = boards[PlayerPrefs.GetInt("board", 0)];
        //Vector2 newPosition = new Vector2(-Mathf.Ceil(gridSize.x / 2) * tileFactor,
        //                                  Mathf.Floor(gridSize.y / 2) * tileFactor);
        //baseTiles = new List<GameObject>();
        //for (int i = 0; i < gridSize.y; i++)
        //{
        //    for (int j = 0; j < gridSize.x; j++)
        //    {
        //        newPosition.x = newPosition.x + tileFactor;
        //        GameObject obj = Instantiate(baseTile, newPosition, Quaternion.identity, gameObject.transform);
        //        obj.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, 0);
        //        obj.name = $"Tile_x{j}_y{i}";
        //        obj.GetComponentInChildren<TextMesh>().text = string.Empty;
        //        baseTiles.Add(obj);
        //    }
        //    newPosition = new Vector2(-Mathf.Ceil(gridSize.x / 2) * tileFactor, newPosition.y - tileFactor);
        //}


        if (vsPlayerMode)
            playerTurn = 2;
        if (vsCPUMode)
            playerTurn = 0;

        scoring = new Dictionary<string, int>();
        scoring.Add("A", 1);
        scoring.Add("B", 3);
        scoring.Add("C", 3);
        scoring.Add("D", 2);
        scoring.Add("E", 1);
        scoring.Add("F", 4);
        scoring.Add("G", 2);
        scoring.Add("H", 4);
        scoring.Add("I", 1);
        scoring.Add("J", 8);
        scoring.Add("K", 5);
        scoring.Add("L", 1);
        scoring.Add("M", 3);
        scoring.Add("N", 1);
        scoring.Add("O", 1);
        scoring.Add("P", 3);
        scoring.Add("Q", 10);
        scoring.Add("R", 1);
        scoring.Add("S", 1);
        scoring.Add("T", 1);
        scoring.Add("U", 1);
        scoring.Add("V", 4);
        scoring.Add("W", 4);
        scoring.Add("X", 8);
        scoring.Add("Y", 4);
        scoring.Add("Z", 10);
        lettersStr = new List<string>();
        foreach (var item in str)
        {
            lettersStr.Add(Convert.ToString(item));
        }
        emptyBoard = true;
        NextMove();
    }
    private void SetGameWithPlayer()
    {
        vsPlayerMode = true;
        vsCPUMode = false;
        init();
        ChangeTurn();
        panelMainMenu.SetActive(false);
        panelGameUI.SetActive(true);
        gameplay.SetActive(true);
    }
    private void SetGameWithAI()
    {
        vsPlayerMode = false;
        vsCPUMode = true;
        init();
    }
    private void ClickSkip()
    {
        ChangeTurn();
    }
    private void AddScore(List<string> words)
    {
        int sum = 0;
        for (int i = 0; i < words.Count; i++)
        {
            for (int j = 0; j < words[i].Length; j++)
            {
                sum += scoring[Convert.ToString(words[i][j])];
            }
        }
        switch (playerTurn)
        {
            case 1:
                sum += player1Score;
                playerscore.text = sum.ToString();
                player1Score = sum;
                break;
            case 2:
            case 0:
                sum += player2Score;
                cpuOrOpponentScore.text = sum.ToString();
                player2Score = sum;
                break;
            default:
                break;
        }
    }
    private void ChangeTurn()
    {
        if (vsPlayerMode)
        {
            if (playerTurn == 1)
            {
                playerTurn = 2;
                txtTurn.text = "Player 2 Turn";
            }
            else if (playerTurn == 2)
            {
                playerTurn = 1;
                txtTurn.text = "Player 1 Turn";
            }
        }
        else if (vsCPUMode)
        {
            if (playerTurn == 1)
            {
                playerTurn = 0;
                txtTurn.text = "CPU Turn";
            }
            else if (playerTurn == 0)
            {
                playerTurn = 1;
                txtTurn.text = "Player Turn";
            }
        }
    }
    private void ClickShuffle()
    {
        for (int i = 0; i < 10; i++)
        {
            int x1 = Random.Range(0, optionTiles.Count);
            int x2 = Random.Range(0, optionTiles.Count);
            string s1 = optionTiles[x1].GetComponentInChildren<TextMesh>().text;
            optionTiles[x1].GetComponentInChildren<TextMesh>().text = optionTiles[x2].GetComponentInChildren<TextMesh>().text;
            optionTiles[x2].GetComponentInChildren<TextMesh>().text = s1;
        }
        currentMove = new List<(GameObject, string)>();
        foreach (var item in optionTiles)
        {
            var temp = (item, item.GetComponentInChildren<TextMesh>().text);
            currentMove.Add(temp);
        }
    }
    private void Update()
    {
        TrueInput();
    }
    private string RandomCharacter()
    {
        if(lettersStr.Count == 0)
        {
            showWon();
            return string.Empty;
        }
        int index = Random.Range(0, lettersStr.Count);
        string temp = lettersStr[index];
        lettersStr.RemoveAt(index);
        return Convert.ToString(temp);
    }
    private void SetToConnected()
    {
        foreach (var item in currentHoldingMove)
        {
            item.GetComponent<tile>().connected = true;
        }
    }
    private void ClickSubmit()
    {
        bool areConnected = false;
        wordsConstructed = new List<string>();
        for (int i = 0; i < currentHoldingMove.Count; i++)
        {

            int indexBase = baseTiles.IndexOf(currentHoldingMove[i]);
            while (indexBase % 15 >= 0)
            {
                if (baseTiles[indexBase].GetComponentInChildren<TextMesh>().text != string.Empty)
                {
                    if (baseTiles[indexBase].GetComponent<tile>().connected == true)
                        areConnected = true;
                    indexBase -= 1;
                    if (indexBase < 0)
                        break;
                }
                else
                {
                    indexBase += 1;
                    string thisWord = "";
                    while (baseTiles[indexBase].GetComponentInChildren<TextMesh>().text != string.Empty)
                    {
                        if (baseTiles[indexBase].GetComponent<tile>().connected == true)
                            areConnected = true;
                        thisWord += baseTiles[indexBase].GetComponentInChildren<TextMesh>().text;
                        indexBase += 1;
                        if (indexBase >= gridSize.x * gridSize.y)
                            break;
                    }
                    thisWord = thisWord.Trim();
                    if (thisWord != string.Empty && thisWord.Length > 1)
                    {
                        if (!wordsConstructed.Contains(thisWord))
                            wordsConstructed.Add(thisWord);
                    }
                    break;
                }
            }
            indexBase = baseTiles.IndexOf(currentHoldingMove[i]);
            while (indexBase % 15 >= 0)
            {
                if (baseTiles[indexBase].GetComponentInChildren<TextMesh>().text != string.Empty)
                {
                    if (baseTiles[indexBase].GetComponent<tile>().connected == true)
                        areConnected = true;
                    indexBase -= 15;
                    if (indexBase < 0)
                        break;
                }
                else
                {
                    indexBase += 15;
                    string thisWord = "";

                    while (baseTiles[indexBase].GetComponentInChildren<TextMesh>().text != string.Empty)
                    {
                        if (baseTiles[indexBase].GetComponent<tile>().connected == true)
                            areConnected = true;
                        thisWord += baseTiles[indexBase].GetComponentInChildren<TextMesh>().text;
                        indexBase += 15;
                        if (indexBase >= gridSize.x * gridSize.y)
                            break;
                    }
                    thisWord = thisWord.Trim();
                    if (thisWord.Trim() != string.Empty && thisWord.Length > 1)
                    {
                        if (!wordsConstructed.Contains(thisWord))
                            wordsConstructed.Add(thisWord);
                    }
                    break;
                }
            }

        }
        foreach (var item in wordsConstructed)
        {
            print(item);
        }
        //return;
        if (ValidWords(wordsConstructed))
        {
            if (emptyBoard)
            {
                if (!currentHoldingMove.Contains(tileReferences[0].position))//ref to middle tile
                {
                    invalidmove();
                }
                else
                {
                    AddScore(wordsConstructed);
                    SetToConnected();
                    validmove();
                    emptyBoard = false;
                }
            }
            else
            {
                if (areConnected)
                {
                    AddScore(wordsConstructed);
                    SetToConnected();
                    validmove();
                }
                else
                {
                    invalidmove();
                }
            }
        }
        else
        {
            invalidmove();
        }
        currentHoldingMove = new List<GameObject>();
    }
    public void invalidmove()
    {
        foreach (var item in currentMove)
        {
            item.Item1.GetComponentInChildren<TextMesh>().text = item.Item2;
        }
        foreach (var item in currentHoldingMove)
        {
            item.GetComponentInChildren<TextMesh>().text = string.Empty;
            item.gameObject.tag = "Untagged";
        }
    }
    public void validmove()
    {
        foreach (var item in currentHoldingMove)
        {
            item.gameObject.tag = "Fixed";
            item.gameObject.GetComponent<tile>().connected = true;
        }
        foreach (var item in currentMove)
        {
            if (item.Item1.GetComponentInChildren<TextMesh>().text == string.Empty)
            {
                item.Item1.GetComponentInChildren<TextMesh>().text = RandomCharacter();
            }
        }
        ChangeTurn();
    }
    private void TrueInput()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 1f);
        if (hit.collider != null)
        {
            if (hit.collider.gameObject.tag == "Respawn")
            {
                lastSelectedTile = hit.collider.gameObject;
            }
            else
            {
                //hit.collider.gameObject.GetComponent<SpriteRenderer>().color = tileColor;
                //hit.collider.gameObject.GetComponentInChildren<TextMesh>().text = 
            }
        }
        //if (Input.touchCount > 0)
        //if (Input.GetKey(KeyCode.Mouse0))
        {
            //Touch touch = Input.touches[0];
            //print($"{Input.touchCount} : {touch.phase} : {touch.position}");

            //if (touch.phase == TouchPhase.Began)
            if(Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (lastSelectedTile != null)
                    if (tempDragTile == null && lastSelectedTile.gameObject.tag == "Respawn")
                    {
                        //tempDragTile = Instantiate(dragTile, Camera.main.ScreenToWorldPoint(touch.position), Quaternion.identity);
                        tempDragTile = Instantiate(dragTile, Camera.main.ScreenToWorldPoint(Input.mousePosition), Quaternion.identity);
                        tempDragTile.GetComponentInChildren<TextMesh>().text = lastSelectedTile.gameObject.GetComponentInChildren<TextMesh>().text;
                    }
            }
            //if (touch.phase == TouchPhase.Moved)
            if (Input.GetKey(KeyCode.Mouse0))
            {
                if (tempDragTile != null)
                {
                    //tempDragTile.transform.position = new Vector3(Camera.main.ScreenToWorldPoint(touch.position).x, Camera.main.ScreenToWorldPoint(touch.position).y, -1);
                    tempDragTile.transform.position = new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y, -1);
                }
            }
            //if (touch.phase == TouchPhase.Stationary)
            //{ }
            //if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            if(Input.GetKeyUp(KeyCode.Mouse0))
            //else
            {
                print("hit1");
                if (tempDragTile == null) return;
                List<Collider2D> coll = new List<Collider2D>();
                if (tempDragTile.GetComponent<Rigidbody2D>().GetAttachedColliders(coll) > 0)
                {
                    ContactFilter2D filter2D = new ContactFilter2D();
                    List<Collider2D> results = new List<Collider2D>();
                    if (coll[0].OverlapCollider(filter2D, results) > 0)
                    {
                        GameObject minDistance = results[0].gameObject;
                        for (int i = 0; i < results.Count; i++)
                        {
                            if (Mathf.Abs(Vector2.Distance(tempDragTile.transform.position, results[i].transform.position)) < Mathf.Abs(Vector2.Distance(tempDragTile.transform.position, minDistance.transform.position)))
                            {
                                minDistance = results[i].gameObject;
                            }
                        }
                        if (minDistance.gameObject.tag == "Untagged")
                        {
                            minDistance.gameObject.GetComponentInChildren<TextMesh>().text = tempDragTile.GetComponentInChildren<TextMesh>().text;
                            minDistance.gameObject.tag = "Placed";
                            lastSelectedTile.GetComponentInChildren<TextMesh>().text = string.Empty;
                            currentHoldingMove.Add(minDistance);
                        }

                    }
                }
                Destroy(tempDragTile);
                tempDragTile = null;
                lastSelectedTile = null;
            }
        }
    }
    private void NextMove()
    {
        currentHoldingMove = new List<GameObject>();
        currentMove = new List<(GameObject, string)>();
        foreach (var item in optionTiles)
        {
            item.GetComponentInChildren<TextMesh>().text = RandomCharacter();
            var temp = (item, item.GetComponentInChildren<TextMesh>().text);
            currentMove.Add(temp);
        }
    }
    private bool ValidWords(List<string> words)
    {
        if (words.Count > 0)
        {
            for (int i = 0; i < words.Count; i++)
            {
                if (!dicWords.Contains(words[i].ToUpper()))
                {
                    return false;
                }
            }
            return true;
        }
        else
            return false;
    }
  
}
