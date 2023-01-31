using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class Quest : MonoBehaviour
{
    // BANNER
    //    CANVAS
    //       BACKGROUND
    //       OBJECTIVE
    //       QUESTCOMPLETE
    //       QUESTFAILED   
    public event Action<int,GameObject> OnQuestComplete;
    public int colorID;
    public int objective;
    public bool active;
    private int questStatus;  //  1= COMPLETE   -1= FAIL     0= SHOW
    private bool placed;
    private int currentScore;    
    public GameObject banner;
    public GameObject cameraBase;
    private List<GameObject> questWedges;

    void Start()
    {
        //Crea Banner y Referencia
        banner = Instantiate(Resources.Load("Prefabs/Banner"), CreateBannerVectorPosition(), Quaternion.identity) as GameObject;        
        banner.transform.parent = this.gameObject.transform;
        banner.GetComponentInChildren<Canvas>().transform.GetChild(0).GetComponent<Image>().color = GetColor();
        
        cameraBase = GameObject.Find("Camera Base");
        banner.GetComponentInChildren<Canvas>().worldCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        //Suscripcion
        this.gameObject.transform.parent.GetComponent<HexTile>().onPiecePlaced += OnPlacedQuest;
        //Inicializacion
        currentScore = objective;
        questWedges = new List<GameObject>();
        active = true;
        questStatus = 0;
        placed = false;
        RefreshQuestBanner();        
    }
    void Update()
    {
        banner.transform.LookAt(cameraBase.transform); //Banner face to camera                   
    }
    private Vector3 CreateBannerVectorPosition()
    {
        //this.gameObject.GetComponent<Wedge>().eye.transform.position + new Vector3(0, -10, 0)
        Vector3 eyeOnGround = this.gameObject.GetComponent<Wedge>().eye.transform.position + new Vector3(0, -10, 0);
        Vector3 target = this.gameObject.GetComponent<Wedge>().transform.position;
        Vector3 direction = target - eyeOnGround;
        direction.Normalize();
        return eyeOnGround + direction * 1.5f;
    }
    public void RefreshQuestBanner()
    {
        questWedges.Clear();
        this.gameObject.GetComponent<Wedge>().GetAllNeighbors(questWedges);
        currentScore = objective - questWedges.Count;
        if (active)
        {
            if (questWedges.Count > 0)
            {
                if (currentScore == 0)
                {   //Igual a 0 
                    //QuestComplete();
                    ChangeCanvasStatus(1);
                }
                else
                {
                    if (currentScore < 0)
                    {   //Menor a 0                    
                        if (placed)
                        {
                            //QuestComplete();
                            ChangeCanvasStatus(1);
                        }
                        else
                        {
                            //QuestFailed();
                            ChangeCanvasStatus(-1);
                        }
                    }
                    else
                    {   //Mayor a 0                     
                        if (ClosedGroup())
                        {
                            //QuestFailed();
                            ChangeCanvasStatus(-1);
                        }
                        else
                        {
                            //QuestShow(); //SHOW
                            ChangeCanvasStatus(0);
                        }
                    }
                }
            }
        }
        foreach (GameObject wedge in questWedges)
        {
            wedge.GetComponent<Wedge>().checkedWedge = false;
        }
    }
    private Color GetColor()
    {        
        switch (colorID)
        {
            case 0:                
                return Color.red;                
            case 1:
                return Color.green;                
            case 2:
                return Color.blue;                
            case 3:
                return Color.magenta;               
            default:           
                return Color.yellow;                
        }
    }
    private bool ClosedGroup()
    {        
        List<GameObject> list = GetAdjacentEmptyWedges();        
        if (list.Count > 0)
            return false;
        else
            return true;
    } 
    private List<GameObject> GetAdjacentEmptyWedges()
    {
        List<GameObject> emptyWedges = new List<GameObject>();
        foreach (GameObject wedge in questWedges)
        {
            if (wedge.name != "Wedge0") //todos menos el wedge central
            {
                if (wedge.GetComponent<Wedge>().neighbors[3] == null) //si no tiene un adyacente
                {                    
                    emptyWedges.Add(wedge);
                }                
            }
        }
        return emptyWedges;
    }
    private void ChangeCanvasStatus(int mod)
    {
        switch (mod)
        {
            case 0:
                //apagar complete y failed
                banner.GetComponentInChildren<Canvas>().transform.GetChild(2).GetComponent<Image>().enabled = false;
                banner.GetComponentInChildren<Canvas>().transform.GetChild(3).GetComponent<Image>().enabled = false;
                //activar objective
                banner.GetComponentInChildren<Canvas>().transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().enabled = true;
                banner.GetComponentInChildren<Canvas>().transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = currentScore.ToString();
                questStatus = 0;
                break;
            case 1:
                //apagar objectice y failed
                banner.GetComponentInChildren<Canvas>().transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().enabled = false;
                banner.GetComponentInChildren<Canvas>().transform.GetChild(3).GetComponent<Image>().enabled = false;
                //activar complete
                banner.GetComponentInChildren<Canvas>().transform.GetChild(2).GetComponent<Image>().enabled = true;
                questStatus = 1;                
                break;
            case -1:
                //apagar objectice y complete
                banner.GetComponentInChildren<Canvas>().transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().enabled = false;
                banner.GetComponentInChildren<Canvas>().transform.GetChild(2).GetComponent<Image>().enabled = false;
                //activar failed
                banner.GetComponentInChildren<Canvas>().transform.GetChild(3).GetComponent<Image>().enabled = true;
                questStatus = -1;
                break;
        }
        if (placed && (questStatus == -1 || questStatus == 1) && (AllQuestWedgesIdle() && (AllQuestWedgesIdle() && !HasCheckingNeighbor())))
        {
            ResolveQuest();
        }
    }
    private bool HasCheckingNeighbor()
    {
        foreach (GameObject wedge in questWedges)
        {
            if (wedge.GetComponent<Wedge>().centerWedge == false)
            {
                if (wedge.GetComponent<Wedge>().neighbors[3] != null)
                    if (wedge.GetComponent<Wedge>().neighbors[3].transform.parent.GetComponent<HexTile>().currentState == HexTile.TileState.Checking)
                        return true;
            }            
        }
        return false;
    }
    private bool AllQuestWedgesIdle()
    {
        foreach (GameObject wedge in questWedges)
        {
            if (wedge.transform.parent.GetComponent<HexTile>().currentState == HexTile.TileState.Checking)
                return false;
        }
        return true;
    }
    private void ResolveQuest() //ARREGLAR ESTO URGENTE
    {        
        switch (questStatus)
        {
            case 0:
                //objective = currentScore;
                break;
            case 1:
                //lanzar coroutine de complete
                //destruir quest y dar puntos
                //banner.GetComponentInChildren<Canvas>().transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = "OK"; 
                active = false;
                OnQuestComplete?.Invoke(+5,this.transform.parent.gameObject);
                break;
            case -1:
                //lanzar coroutine de fail
                //destruir y dar puntos
                //banner.GetComponentInChildren<Canvas>().transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = "FAIL";
                active = false;
                OnQuestComplete?.Invoke(-1, this.transform.parent.gameObject);
                break;
        }
    }    
    private void OnPlacedQuest(GameObject currentPiece)
    {        
        if (currentPiece == this.gameObject.transform.parent.gameObject && placed == false)
        {   
            placed = true;            
            this.gameObject.transform.parent.GetComponent<HexTile>().onPiecePlaced -= OnPlacedQuest;
            ResolveQuest();            
        }
    }    
}

   