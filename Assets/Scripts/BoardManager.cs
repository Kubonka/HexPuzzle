using Antlr.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class BoardData
{
    private float questFrequency;    
    private int tileCount;
    private int maxColors;
    int[] colorArr;
    
    public BoardData(int _maxColors)
    {
        maxColors = _maxColors;
        questFrequency = 0.7f;        
        tileCount = 0;
        colorArr = new int[maxColors];
    }
    
    public void AddTile(GameObject tile)
    {
        tileCount++; //agrego tile 
        for (int i = 0; i < 7; i++) //wedge count
        {
            AddColor(tile.GetComponent<HexTile>().wedges[i].GetComponent<Wedge>()); //Agrego color en la lista
        }
    }
    public Vector3 GetQuest(GameObject tile)
    {
        float rnd;
        int wedgeID, amount, colorID;
        rnd = UnityEngine.Random.value;
        if (rnd > questFrequency)
        { return new Vector3(-1, -1, -1); }         // (-1,-1,-1) -> quest denegada
        else
        {
            colorID = CalculateColor();
            wedgeID = SetWedgeID(colorID,tile.GetComponent<HexTile>().wedges);
            amount = SetAmount(colorID); 
        }
        return new Vector3(wedgeID,colorID,amount); // (wedge nro , color , cantidad) -> quest aceptada
    }
    private int CalculateColor() 
    {
        List<float> colorRatio = new List<float>();
        foreach (int colorCount in colorArr)
        {
            colorRatio.Add((float)colorCount / (float)(tileCount * 7));
        }
        float rnd = UnityEngine.Random.value;
        float total = 0;
        int i = 0;
        while (i < colorRatio.Count - 1)
        {
            if (rnd > total && rnd < total + colorRatio[i])
            {
                return i;
            }
            total = total + colorRatio[i];
            i++;
        }
        return i;        
    }
    private int SetWedgeID(int colorID, GameObject[] wedges)
    {
        for (int i = 0; i < 7; i++) //wedge count
        {
            if (wedges[i].GetComponent<Wedge>().colorID == colorID)
            {
                return i;
            }
        }
        return -1;
    }
    private int SetAmount(int colorID) //sujeto a cambios
    {
        int colorCant = colorArr[colorID];
        float rnd = UnityEngine.Random.Range(colorCant * 0.5f, colorCant * 1.5f);
        return Mathf.RoundToInt(rnd);
    }
    private void AddColor(Wedge wedge)
    {
        colorArr[wedge.colorID] += 1;
    }
}
public class Timer
{
    private float currentTime;
    private float timeOut;
    public Timer(float _timeOut)
    {
        timeOut = _timeOut;
        currentTime = Time.time;
    }
    public bool CheckTime()
    {
        if (Time.time - currentTime > timeOut)
        {
            currentTime = Time.time;
            return true;
        }
        else
            return false;
    }
}
public class BoardManager : MonoBehaviour
{
    public enum GameState
    {
        PlayerTurn,
        NewGame,
        GameOver,
        Solving,
        PausedGame,
        Restart
    }
    public GameObject cameraPivot;
    public GameObject hudCanvas;
    public GameObject mainMenuCanvas;
    public GameState currentState;
    private string prefabPath;
    private GameObject currentPiece;
    private int tilesCount;
    private int score;
    public Timer tickTimer;
    //Quest Vars
    private BoardData boardData;
    private int questCount;
    private List<GameObject> questTracker;
    private List<GameObject> questWedgesList;

    //CHECK TILES (highlight)
    private GameObject latestBlankTile;    
    private List<GameObject> highlightList;
    private int tileCounter;
    private int comboCounter;

    private bool flag;
    private int cont;

    //Events    
    public event Action<int> onRemainingTilesChange;
    public event Action<int> onScoreChange;

    void Start()
    {

        score = 0;
        tilesCount = 41;
        cont = 0;
        flag = false;
        questWedgesList = new List<GameObject>();
        questTracker = new List<GameObject>();
        questCount = 10;
        boardData = new BoardData(5); // ----  OJO  !!! ----   MODIFICAR IMPLEMENTACION DE MAXCOLORS EN HEXTILE O BOARDMANAGER
        tileCounter = 1;
        latestBlankTile = null;
        currentState = GameState.NewGame;
        prefabPath = "Prefabs/HexTile";
        tickTimer = new Timer(0.1f);  // <- TICK RATE a modificar        
        highlightList = new List<GameObject>();
        mainMenuCanvas.GetComponent<Canvas>().enabled = false;
        hudCanvas.GetComponent<Canvas>().enabled = true;
        //SUSCRIPCIONES        
        GetComponent<PlayerController>().onMouseLeftPressed += OnMouseClick;
        GetComponent<PlayerController>().onRotatePressed += OnRotateTile;
        GetComponent<PlayerController>().onMenuSelect += OnMenuSelect;
        mainMenuCanvas.GetComponent<MainMenuScript>().onNewGame += OnNewGameSelect;
}
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    StartCoroutine(currentPiece.GetComponent<HexTile>().LaunchFailedTile());
        //}
                
        switch (currentState)
        {
            case GameState.NewGame:
                onRemainingTilesChange?.Invoke(tilesCount);
                GameObject go = Instantiate(Resources.Load(prefabPath), new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                go.name = "HexTile " + tileCounter;
                for (int i = 0; i < 7; i++)
                {
                    go.GetComponent<HexTile>().wedges[i].name = "( T " + tileCounter + " - " + "W " + i + " )";                    
                }
                tileCounter++;
                go.GetComponent<HexTile>().MainGenerate();
                go.GetComponent<HexTile>().AdaptHexMesh(); 
                //go.GetComponent<HexTile>().Dock();
                go.GetComponent<HexTile>().currentState = HexTile.TileState.Placed;
                go.transform.parent = this.gameObject.transform;
                boardData.AddTile(go);
                SpawnPiece();
                boardData.AddTile(currentPiece);
                currentState = GameState.PlayerTurn;
                break;
            case GameState.PlayerTurn:
                if (tickTimer.CheckTime()) //si se cumple el intervalo lanzar ray a donde esta el mouse
                {
                    GameObject rayTick = CheckOnMousePosition();
                    if (rayTick == null)
                    {
                        ResetWedges();
                        currentPiece.GetComponent<HexTile>().currentState = HexTile.TileState.Hidden;
                        if (latestBlankTile != null)
                        {
                            latestBlankTile.SetActive(true);
                        }
                        CheckQuest();
                    }
                    else if (rayTick.tag == "BlankTile") //SI LE PEGO A UN BLANK  //CHEQUEAR QUEST !!
                    {
                        if (flag)
                        {
                            Debug.Log("ENTRO");
                        }
                        ResetWedges();
                        CheckMain(rayTick);
                    }
                    else if (rayTick.transform.parent.GetComponent<HexTile>().currentState == HexTile.TileState.Idle) //SI LE PEGO A UN TILE IDLE
                    {
                        Debug.Log("asidASD");
                        ResetWedges();
                        currentPiece.GetComponent<HexTile>().currentState = HexTile.TileState.Hidden;
                        if (latestBlankTile != null)
                        {
                            latestBlankTile.SetActive(true);
                        }
                        CheckQuest();
                    }
                    else if (rayTick.transform.parent.gameObject == currentPiece)
                    {
                        //foreach (GameObject wedge in highlightList)
                        //{
                        //    wedge.GetComponent<Wedge>().checkedWedge = false;
                        //}
                        //CheckQuest();
                        Debug.Log("aca");
                    }
                }
                break;
            case GameState.Solving:
                //CALCULAR SCORES , RESOLVER PERFECTS , TILES GANADAS ETC.
                ResolveScore();
                CalculatePerfects();
                // SI NO ES GAME OVER
                // PREPARAR EL PROXIMO TURNO
                SpawnPiece();
                boardData.AddTile(currentPiece);
                ResetWedges();
                GenerateQuest();
                currentState = GameState.PlayerTurn;
                break;
            case GameState.PausedGame:
                mainMenuCanvas.GetComponent<Canvas>().enabled = true;
                //hudCanvas.GetComponent<Canvas>().enabled = false;
                break;
            case GameState.Restart:
                foreach (Transform child in this.transform)
                {
                    Destroy(child.gameObject);
                }
                GetComponent<PlayerController>().onMouseLeftPressed -= OnMouseClick;
                GetComponent<PlayerController>().onRotatePressed -= OnRotateTile;
                GetComponent<PlayerController>().onMenuSelect -= OnMenuSelect;
                mainMenuCanvas.GetComponent<MainMenuScript>().onNewGame -= OnNewGameSelect;
                Start();
                break;
        }
    }
    //------------------------------------------------------
    //SPAWNEO DE HEXTILE Y GENERACION DE QUEST
    //------------------------------------------------------
    private void SpawnPiece()
    {
        onRemainingTilesChange?.Invoke(-1);
        tilesCount--;
        currentPiece = Instantiate(Resources.Load(prefabPath), new Vector3(0, 100, 0), Quaternion.identity) as GameObject;        
        currentPiece.name = "HexTile " + tileCounter;
        for (int i = 0; i < 7; i++)
        {
            currentPiece.GetComponent<HexTile>().wedges[i].name = "( T " + tileCounter + " - " + "W " + i + " )";
        }
        currentPiece.GetComponent<HexTile>().DistributeColors();
        currentPiece.GetComponent<HexTile>().AdaptHexMesh();
        currentPiece.GetComponent<HexTile>().currentState = HexTile.TileState.Hidden;
        currentPiece.transform.parent = this.gameObject.transform;
        tileCounter++;        
    }
    private void GenerateQuest() 
    {
        if (questTracker.Count < questCount)
        {
            Vector3 questData = boardData.GetQuest(currentPiece);
            if (questData.x != -1)// (wedge nro , color , cantidad) -> quest aceptada -> Agregar Valores a CURRENT PIECE
            {
                currentPiece.GetComponent<HexTile>().wedges[(int)questData.x].AddComponent<Quest>();
                currentPiece.GetComponent<HexTile>().wedges[(int)questData.x].GetComponent<Quest>().colorID = (int)questData.y;
                //Setear el numero correcto de objetivo en base a los neighbors que tiene la tile
                List<GameObject> auxList = new List<GameObject>();
                currentPiece.GetComponent<HexTile>().wedges[(int)questData.x].GetComponent<Wedge>().GetAllNeighbors(auxList);
                currentPiece.GetComponent<HexTile>().wedges[(int)questData.x].GetComponent<Quest>().objective = (int)questData.z + auxList.Count;
                currentPiece.GetComponent<HexTile>().wedges[(int)questData.x].GetComponent<Quest>().OnQuestComplete += OnQuestCompleteEvent;                
                questTracker.Add(currentPiece.GetComponent<HexTile>().wedges[(int)questData.x]);                
            }
        }
    }
    //------------------------------------------------------
    //SET UP DE LA HEXTILE, CHEQUEO DE VECINOS Y HIGHLIGHTEO
    //------------------------------------------------------
    private void CheckMain(GameObject rayTick)
    {        
        if (latestBlankTile != null)
        {
            SetUpBlankTiles(rayTick, false);
        }
        else
        {
            SetUpBlankTiles(rayTick, true);
        }
        CheckOnBlankTile();
    }
    private void SetUpBlankTiles(GameObject rayTick, bool blankNull)
    {
        if (!blankNull)
            latestBlankTile.SetActive(true);
        //MUESTRO LA CURRENT Y SACO LA BLANK
        latestBlankTile = rayTick.transform.parent.gameObject;
        currentPiece.GetComponent<HexTile>().currentState = HexTile.TileState.Checking;
        currentPiece.transform.position = rayTick.transform.parent.transform.position;
        latestBlankTile.SetActive(false);
    }
    private void CheckOnBlankTile()
    {
        //DOCKEO LA TILE AL BOARD
        currentPiece.GetComponent<HexTile>().Dock();
        //POR CADA WEDGE EXTERIOR BUSCO NEIGHBORS 
        for (int i = 1; i <= 6; i++)
        {
            GetSameNeighbors(i);
        }
        //HIGHLIGHTEAR WEDGES
        ToggleHighlightTiles(); 
    }    
    private void GetSameNeighbors(int pos) 
    {
        List<GameObject> temp;     
        temp = new List<GameObject>();
        if (currentPiece.GetComponent<HexTile>().wedges[pos].GetComponent<Wedge>().neighbors[3] != null)
        {
            if (currentPiece.GetComponent<HexTile>().wedges[pos].GetComponent<Wedge>().neighbors[3].GetComponent<Wedge>().colorID == currentPiece.GetComponent<HexTile>().wedges[pos].GetComponent<Wedge>().colorID)
            {
                currentPiece.GetComponent<HexTile>().wedges[pos].GetComponent<Wedge>().GetAllNeighbors(temp);
                highlightList.AddRange(temp);
            }
        }
    }
    private void ToggleHighlightTiles()
    {
        foreach (GameObject wedge in highlightList)
        {
            wedge.GetComponent<Wedge>().Highlight(1);
        }        
    }
    //------------------------------------------------------
    //RESETEO DE HEXTILES
    //------------------------------------------------------
    private void ResetWedges()
    {
        currentPiece.GetComponent<HexTile>().Undock();
        currentPiece.GetComponent<HexTile>().RestoreBordersAndHardCorners();
        if (highlightList.Count > 0)
        {
            Debug.Log("Highlight tiles enter");
            //UNDOCK
            currentPiece.GetComponent<HexTile>().Undock();
            //HIGHLIGHTS OFF
            foreach (GameObject wedge in highlightList)
            {
                wedge.GetComponent<Wedge>().Highlight(-1);
                wedge.GetComponent<Wedge>().checkedWedge = false;
            }
            //HIGHLIGHT LIST CLEAR
            highlightList.Clear();
        }
        //currentPiece.GetComponent<HexTile>().currentState = HexTile.TileState.Hidden;
    }
    //------------------------------------------------------
    //RESOLUCION, CALCULO DE SCORE Y PERFECT TILES
    //------------------------------------------------------
    private void CheckQuest()
    {        
        foreach (GameObject wedge in questTracker)
        {
            if (wedge.GetComponent<Quest>().active)
                wedge.GetComponent<Quest>().RefreshQuestBanner();            
        }
        for (int i = 0; i < questTracker.Count; i++)
        {
            if (questTracker[i].GetComponent<Quest>().active == false)
            {
                questTracker[i].GetComponent<Quest>().OnQuestComplete -= OnQuestCompleteEvent;
                Destroy(questTracker[i].GetComponent<Quest>().banner);
                Destroy(questTracker[i].GetComponent<Quest>());
                questTracker.RemoveAt(i);
            }
        }
    }
    private void ResolveScore()
    {
        int total = 0;
        HexTile hexTile = currentPiece.GetComponent<HexTile>();
        for (int i = 1; i < hexTile.wedges.Length; i++)
        {
            Wedge wedge = hexTile.wedges[i].GetComponent<Wedge>();
            if (wedge.neighbors[3] != null)
                if (wedge.colorID == wedge.neighbors[3].GetComponent<Wedge>().colorID)
                {
                    total += 10;
                }
        }
        if (total != 60)
            StartCoroutine(hexTile.LaunchPopUp(total.ToString()));
        onScoreChange?.Invoke(total);
    }
    private void CalculatePerfects() //1=perfect -1=fail 0=incomplete
    {
        HexTile hexTile = currentPiece.GetComponent<HexTile>();
        hexTile.onPerfectTile += OnPerfectTileEvent;
        hexTile.ResolvePerfect();
        
        for (int i = 1; i < hexTile.wedges.Length; i++)
        {
            if (hexTile.wedges[i].GetComponent<Wedge>().neighbors[3] != null)
            {
                hexTile.wedges[i].GetComponent<Wedge>().neighbors[3].transform.parent.GetComponent<HexTile>().onPerfectTile += OnPerfectTileEvent;
                hexTile.wedges[i].GetComponent<Wedge>().neighbors[3].transform.parent.GetComponent<HexTile>().ResolvePerfect();
            }
        }        
    }    
    //------------------------------------------------------
    //REACCION A EVENTOS
    //------------------------------------------------------
    private void OnQuestCompleteEvent(int value,GameObject hexTileAsociated) //ARREGLAR ESTA MIERDA
    {
        onRemainingTilesChange?.Invoke(value);
        tilesCount = tilesCount + value;
        StartCoroutine(GenerateMiniHexes(value, hexTileAsociated));
        
    }
    private void OnPerfectTileEvent(int status,GameObject hexTileGo)  //SUMAR SCORE Y CALCULAR COMBOS EN CASO DE SER 1  
    {
        //MANEJAR UN COMBO COUNTER
        if (status > 0)
        {
            comboCounter++;
            StartCoroutine(hexTileGo.GetComponent<HexTile>().LaunchPopUp("PERFECT  " + "X" + comboCounter.ToString()));
            onScoreChange(100);
            for (int i = 0; i < comboCounter; i++)
            {
                StartCoroutine(GenerateMiniHexes(comboCounter, hexTileGo));
            }            
            onRemainingTilesChange?.Invoke(comboCounter);
            hexTileGo.GetComponent<HexTile>().onPerfectTile -= OnPerfectTileEvent;
        }
        else
        {
            hexTileGo.GetComponent<HexTile>().onPerfectTile -= OnPerfectTileEvent;
            comboCounter = 0;
        }
    }
    private void OnMenuSelect()
    {
        if (currentState == GameState.PlayerTurn)
            currentState = GameState.PausedGame;
        else if (currentState == GameState.PausedGame)
        {
            currentState = GameState.PlayerTurn;
            //hudCanvas.GetComponent<Canvas>().enabled = true;
            mainMenuCanvas.GetComponent<Canvas>().enabled = false;
        }
    }
    private void OnNewGameSelect()
    {
        currentState = GameState.Restart;
    }
    private void OnMouseClick()
    {        
        if (currentState == GameState.PlayerTurn && currentPiece.GetComponent<HexTile>().currentState == HexTile.TileState.Checking)
        {
            Destroy(latestBlankTile);
            currentPiece.GetComponent<HexTile>().currentState = HexTile.TileState.Placed;
            currentState = GameState.Solving;
        }
    }
    private void OnRotateTile(int direction)
    {
        if (currentState == GameState.PlayerTurn && currentPiece.GetComponent<HexTile>().currentState == HexTile.TileState.Checking)
        {
            //ResetWedges();
            //foreach (GameObject wedge in currentPiece.GetComponent<HexTile>().wedges)
            //{
            //    wedge.GetComponent<Wedge>().SetAdjacentNeighbor();
            //}
            //onBoardChange?.Invoke();
            currentPiece.GetComponent<HexTile>().RotateHexTile(direction);
            ResetWedges();
            currentPiece.GetComponent<HexTile>().Dock();
            CheckOnBlankTile();
            CheckQuest();
        }
        //CUIDADO FALTA CHECKEAR 
    }
    //------------------------------------------------------
    //COROUTINES
    //------------------------------------------------------
    private IEnumerator GenerateMiniHexes(int value, GameObject hexTile)
    {
        Timer timer = new Timer(0.3f);
        int val = value;
        while (val > 0)
        {
            if (timer.CheckTime())
            {
                StartCoroutine(hexTile.GetComponent<HexTile>().LaunchMiniHex());
                val--;
            }
            yield return null;
        }        
    }
    //------------------------------------------------------
    //
    //------------------------------------------------------
    private GameObject CheckOnMousePosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100))
        {
            //Debug.Log("HITEA");
            return hit.collider.gameObject;
        }
        else
        {
            //Debug.Log("MISSEA");
            return null;
        }
    }
}
