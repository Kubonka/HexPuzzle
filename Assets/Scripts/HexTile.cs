using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Tilemaps;
using Unity.VisualScripting;
using System.Diagnostics.Tracing;
using TMPro;
using System.IO;

public class HexTile : MonoBehaviour
{
    private List<int[]>[] distributions;
    //
    private List<int> randomColorsList;
    private int tileSize = 3;
    private int outerWedges = 6;
    private int allWedges = 7;
    private float neighborDistance;
    //
    public enum TileState
    {
        Waiting,
        Idle,
        Placed,
        Hidden,
        Checking,
        Blank
    }
    public TileState currentState;
    private bool visible;
    private Vector3 hiddenPosition;
    private int maxColors = 5;
    public GameObject[] wedges;
    private List<GameObject> bufferedMeshes;
    //EVENTS
    public event Action<GameObject> onPiecePlaced;
    public event Action<int,GameObject> onPerfectTile;
    

    private void Awake()
    {
        randomColorsList = new List<int>();
        CreateDistributionArray();
        bufferedMeshes = new List<GameObject>();
    }
    private void Start()
    {        
        
        hiddenPosition = new Vector3(0, 100, 0);
        neighborDistance = GetNeighborDistance();
        visible = false;
        
    }
    private void Update()
    {
        switch (currentState)
        {
            case TileState.Placed:
                FillEmptyNeighbors();
                //LANZAR EVENT DE PLACED
                onPiecePlaced?.Invoke(this.gameObject);
                currentState = TileState.Idle;
                break;
            case TileState.Checking:
                break;
            case TileState.Hidden:
                //Debug.Log("ASDASD");
                transform.position = hiddenPosition;
                break;
            case TileState.Idle:
                break;
            case TileState.Blank:
                break;
        }
    }
    //public void SetEdgesAndCornersOnMesh()
    //{
    //    for (int i = 0; i < wedges.Length; i++)
    //    {
    //        for (int j = i; j < wedges.Length; j++)
    //        {
    //            if (i != j)
    //            {
    //                LevelMesh(wedges[i], wedges[j]);
    //            }
    //        }
    //    }
    //}
    private void UnBufferMeshes()
    {
        if (bufferedMeshes.Count > 0)
        {
            foreach (GameObject wedgeGo in bufferedMeshes)
            {
                Mesh mesh = wedgeGo.GetComponent<MeshFilter>().mesh;
                mesh.SetVertices(wedgeGo.GetComponent<Wedge>().verticesBuffer);
                mesh.SetColors(wedgeGo.GetComponent<Wedge>().colorsBuffer);
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
            }
        }
    }
    private void BufferMeshes()
    {
        bufferedMeshes.Clear();
        for (int i = 1; i < wedges.Length; i++)
        {            
            //wedges[i].GetComponent<Wedge>().SetMeshBuffer();
            //bufferedMeshes.Add(wedges[i]);
            GameObject wedgeGoNeighbor = wedges[i].GetComponent<Wedge>().neighbors[3];
            if (wedgeGoNeighbor != null)
            {
                bufferedMeshes.Add(wedges[i]);
                bufferedMeshes.Add(wedges[i].GetComponent<Wedge>().neighbors[3]);
                //HexTile hexTileNeighbor = wedgeGoNeighbor.transform.parent.GetComponent<HexTile>();
                //for (int j = 1; j < hexTileNeighbor.wedges.Length; j++)
                //{
                //    //wedges[j].GetComponent<Wedge>().SetMeshBuffer();                    
                //    bufferedMeshes.Add(wedges[j]);
                //}
            }
        }
    }
    public void Dock()
    {
        Debug.Log("DOCK");
        //      ASIGNAR NEIGHBORS
        for (int i = 1; i <= outerWedges; i++)
        {
            Wedge wedge = wedges[i].GetComponent<Wedge>();
            wedge.neighbors[3] = wedge.GetAdjacentNeighbor();
            if (wedge.neighbors[3] != null)
            {
                if (wedge.neighbors[3].transform.parent.gameObject != wedge.transform.parent.gameObject)
                    wedge.neighbors[3].GetComponent<Wedge>().neighbors[3] = wedge.gameObject;
                else
                    wedge.neighbors[3] = null;
            }
        }
        //      NIVELAR MESH BORDER
        //Creo BackUp de Meshes del trio de wedges a nivelar y hago cash in de las referencias de dichos wedges
        //luego hago el nivelado
        //BackUpMeshesOnWedges();
        //UnBufferMeshes();
        BufferMeshes();
        for (int i = 1; i <= outerWedges; i++)
        {
            Wedge w1;
            Wedge w11;
            Wedge w2;
            Wedge w22;
            Wedge w0 = wedges[i].GetComponent<Wedge>();
            Wedge w00 = wedges[i].GetComponent<Wedge>().neighbors[2].GetComponent<Wedge>();
            if (w0.neighbors[3] != null)
            {
                w1 = w0.neighbors[3].GetComponent<Wedge>();
                w11 = w0.neighbors[3].GetComponent<Wedge>().neighbors[1].GetComponent<Wedge>();
                //w1.transform.parent.GetComponent<HexTile>().BackUpMeshesOnWedges();
            }
            else
            {
                w1 = null;
                w11 = null;
            }

            if (w0.neighbors[2].GetComponent<Wedge>().neighbors[3] != null)
            {
                w2 = w0.neighbors[2].GetComponent<Wedge>().neighbors[3].GetComponent<Wedge>();
                w22 = w0.neighbors[2].GetComponent<Wedge>().neighbors[3].GetComponent<Wedge>().neighbors[2].GetComponent<Wedge>();
                //w2.transform.parent.GetComponent<HexTile>().BackUpMeshesOnWedges();
            }
            else
            {
                w2 = null;
                w22 = null;
            }
            BlendBordersAndHardCorners(w0, w1, w2,w00,w11,w22);
            w0.gameObject.GetComponent<MeshFilter>().mesh.colors = w0.colors;
            w00.gameObject.GetComponent<MeshFilter>().mesh.colors = w00.colors;
            if (w1 != null)
            {
                w1.gameObject.GetComponent<MeshFilter>().mesh.colors = w1.colors;
                w11.gameObject.GetComponent<MeshFilter>().mesh.colors = w11.colors;
            }
            if (w2 != null)
            {
                w2.gameObject.GetComponent<MeshFilter>().mesh.colors = w2.colors;
                w22.gameObject.GetComponent<MeshFilter>().mesh.colors = w22.colors;
            }
        }  
    }
    private void BlendBordersAndHardCorners(Wedge w0, Wedge w1, Wedge w2, Wedge w00,Wedge w11,Wedge w22)
    {
        if (w1 == null) 
        {
            if (w2 != null) //si en frente es null y al lado hay alguien 
            {
                //nivelar hardcorner entre w0 y w2
                LevelHardCorners(w0, null,w2,w00,null,w22);
            }
        }
        else
        {
            if (w2 != null) //si enfrente hay alguien y al lado hay alguien 
            {
                //nivelar borde y hardcorner entre w0 w1 y w2
                LevelMeshBorders(w0, w1);
                LevelHardCorners(w0,w1,w2,w00,w11,w22);
                
            }
            else 
            {
                //nivelar borde y hardcorner entre w0 y w1
                LevelMeshBorders(w0, w1);
                LevelHardCorners(w0, w1,null,w00,w11,null);
            }
        }
    }
    private void LevelHardCorners(Wedge w0, Wedge w1, Wedge w2, Wedge w00, Wedge w11, Wedge w22)
    {        
        if (w2 == null)
        {
            Vector3[] vertices0 = w0.gameObject.GetComponent<MeshFilter>().mesh.vertices;
            Vector3[] vertices1 = w1.gameObject.GetComponent<MeshFilter>().mesh.vertices;
            Vector3[] vertices00 = w00.gameObject.GetComponent<MeshFilter>().mesh.vertices;
            Vector3[] vertices11 = w11.gameObject.GetComponent<MeshFilter>().mesh.vertices;
            float midPoint = GetVertexMidpoint2(vertices0[w0.hardCorners[0]], vertices1[w1.hardCorners[1]]);
            vertices0[w0.hardCorners[0]] = new Vector3(vertices0[w0.hardCorners[0]].x, midPoint, vertices0[w0.hardCorners[0]].z);
            vertices1[w1.hardCorners[1]] = new Vector3(vertices1[w1.hardCorners[1]].x, midPoint, vertices1[w1.hardCorners[1]].z);
            vertices00[w00.hardCorners[1]] = new Vector3(vertices00[w00.hardCorners[1]].x, midPoint, vertices00[w00.hardCorners[1]].z);
            vertices11[w11.hardCorners[0]] = new Vector3(vertices11[w11.hardCorners[0]].x, midPoint, vertices11[w11.hardCorners[0]].z);
            vertices0[w0.hardCorners[2]] = new Vector3(vertices0[w0.hardCorners[2]].x, midPoint, vertices0[w0.hardCorners[2]].z);
            vertices1[w1.hardCorners[3]] = new Vector3(vertices1[w1.hardCorners[3]].x, midPoint, vertices1[w1.hardCorners[3]].z);
            vertices00[w00.hardCorners[3]] = new Vector3(vertices00[w00.hardCorners[3]].x, midPoint, vertices00[w00.hardCorners[3]].z);
            vertices11[w11.hardCorners[2]] = new Vector3(vertices11[w11.hardCorners[2]].x, midPoint, vertices11[w11.hardCorners[2]].z);
            Color color2 = GradientManager.Evaluate2(w0.gameObject.GetComponent<MeshFilter>().mesh.colors[w0.hardCorners[0]], w1.gameObject.GetComponent<MeshFilter>().mesh.colors[w1.hardCorners[1]]);
            w0.colors[w0.hardCorners[0]] = color2;
            w1.colors[w1.hardCorners[1]] = color2;
            w00.colors[w00.hardCorners[1]] = color2;
            w11.colors[w11.hardCorners[0]] = color2;
            w0.colors[w0.hardCorners[2]] =color2;
            w1.colors[w1.hardCorners[3]] = color2;
            w00.colors[w00.hardCorners[3]] = color2;
            w11.colors[w11.hardCorners[2]] = color2;
            w0.gameObject.GetComponent<MeshFilter>().mesh.vertices = vertices0;
            w1.gameObject.GetComponent<MeshFilter>().mesh.vertices = vertices1;
            w00.gameObject.GetComponent<MeshFilter>().mesh.vertices = vertices00;
            w11.gameObject.GetComponent<MeshFilter>().mesh.vertices = vertices11;
            w0.gameObject.GetComponent<MeshFilter>().mesh.RecalculateNormals();
            w1.gameObject.GetComponent<MeshFilter>().mesh.RecalculateNormals();
            w00.gameObject.GetComponent<MeshFilter>().mesh.RecalculateNormals();
            w11.gameObject.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        }
        else
        {
            if (w1 == null)
            {
                Vector3[] vertices0 = w0.gameObject.GetComponent<MeshFilter>().mesh.vertices;
                Vector3[] vertices2 = w2.gameObject.GetComponent<MeshFilter>().mesh.vertices;
                Vector3[] vertices00 = w00.gameObject.GetComponent<MeshFilter>().mesh.vertices;
                Vector3[] vertices22 = w22.gameObject.GetComponent<MeshFilter>().mesh.vertices;
                float midPoint = GetVertexMidpoint2(vertices0[w0.hardCorners[0]], vertices2[w2.hardCorners[0]]);
                vertices0[w0.hardCorners[0]] = new Vector3(vertices0[w0.hardCorners[0]].x, midPoint, vertices0[w0.hardCorners[0]].z);
                vertices2[w2.hardCorners[0]] = new Vector3(vertices2[w2.hardCorners[0]].x, midPoint, vertices2[w2.hardCorners[0]].z);
                vertices00[w00.hardCorners[1]] = new Vector3(vertices00[w00.hardCorners[1]].x, midPoint, vertices00[w00.hardCorners[1]].z);
                vertices22[w22.hardCorners[1]] = new Vector3(vertices22[w22.hardCorners[1]].x, midPoint, vertices22[w22.hardCorners[1]].z);
                vertices0[w0.hardCorners[2]] = new Vector3(vertices0[w0.hardCorners[2]].x, midPoint, vertices0[w0.hardCorners[2]].z);
                vertices2[w2.hardCorners[2]] = new Vector3(vertices2[w2.hardCorners[2]].x, midPoint, vertices2[w2.hardCorners[2]].z);
                vertices00[w00.hardCorners[3]] = new Vector3(vertices00[w00.hardCorners[3]].x, midPoint, vertices00[w00.hardCorners[3]].z);
                vertices22[w22.hardCorners[3]] = new Vector3(vertices22[w22.hardCorners[3]].x, midPoint, vertices22[w22.hardCorners[3]].z);
                Color color2 = GradientManager.Evaluate2(w0.gameObject.GetComponent<MeshFilter>().mesh.colors[w0.hardCorners[0]], w2.gameObject.GetComponent<MeshFilter>().mesh.colors[w2.hardCorners[0]]);
                w0.colors[w0.hardCorners[0]] = color2;
                w2.colors[w2.hardCorners[0]] = color2;
                w00.colors[w00.hardCorners[1]] = color2;
                w22.colors[w22.hardCorners[1]] = color2;
                w0.colors[w0.hardCorners[2]] = color2;
                w2.colors[w2.hardCorners[2]] = color2;
                w00.colors[w00.hardCorners[3]] = color2;
                w22.colors[w22.hardCorners[3]] = color2;
                w0.gameObject.GetComponent<MeshFilter>().mesh.vertices = vertices0;
                w2.gameObject.GetComponent<MeshFilter>().mesh.vertices = vertices2;
                w00.gameObject.GetComponent<MeshFilter>().mesh.vertices = vertices00;
                w22.gameObject.GetComponent<MeshFilter>().mesh.vertices = vertices22;
                w0.gameObject.GetComponent<MeshFilter>().mesh.RecalculateNormals();
                w2.gameObject.GetComponent<MeshFilter>().mesh.RecalculateNormals();
                w00.gameObject.GetComponent<MeshFilter>().mesh.RecalculateNormals();
                w22.gameObject.GetComponent<MeshFilter>().mesh.RecalculateNormals();
            }
            else
            {
                Vector3[] vertices0 = w0.gameObject.GetComponent<MeshFilter>().mesh.vertices;
                Vector3[] vertices1 = w1.gameObject.GetComponent<MeshFilter>().mesh.vertices;
                Vector3[] vertices2 = w2.gameObject.GetComponent<MeshFilter>().mesh.vertices;
                Vector3[] vertices00 = w00.gameObject.GetComponent<MeshFilter>().mesh.vertices;
                Vector3[] vertices11 = w11.gameObject.GetComponent<MeshFilter>().mesh.vertices;
                Vector3[] vertices22 = w22.gameObject.GetComponent<MeshFilter>().mesh.vertices;
                float midPoint = GetVertexMidPoint3(vertices0[w0.hardCorners[0]], vertices1[w1.hardCorners[1]], vertices2[w2.hardCorners[0]]);
                vertices0[w0.hardCorners[0]] = new Vector3(vertices0[w0.hardCorners[0]].x, midPoint, vertices0[w0.hardCorners[0]].z);
                vertices1[w1.hardCorners[1]] = new Vector3(vertices1[w1.hardCorners[1]].x, midPoint, vertices1[w1.hardCorners[1]].z);
                vertices2[w2.hardCorners[0]] = new Vector3(vertices2[w2.hardCorners[0]].x, midPoint, vertices2[w2.hardCorners[0]].z);
                vertices00[w00.hardCorners[1]] = new Vector3(vertices00[w00.hardCorners[1]].x, midPoint, vertices00[w00.hardCorners[1]].z);
                vertices11[w11.hardCorners[0]] = new Vector3(vertices11[w11.hardCorners[0]].x, midPoint, vertices11[w11.hardCorners[0]].z);
                vertices22[w22.hardCorners[1]] = new Vector3(vertices22[w22.hardCorners[1]].x, midPoint, vertices22[w22.hardCorners[1]].z);
                vertices0[w0.hardCorners[2]] = new Vector3(vertices0[w00.hardCorners[2]].x, midPoint, vertices0[w0.hardCorners[2]].z);
                vertices1[w1.hardCorners[3]] = new Vector3(vertices1[w11.hardCorners[3]].x, midPoint, vertices1[w1.hardCorners[3]].z);
                vertices2[w2.hardCorners[2]] = new Vector3(vertices2[w22.hardCorners[2]].x, midPoint, vertices2[w2.hardCorners[2]].z);
                vertices00[w00.hardCorners[3]] = new Vector3(vertices00[w00.hardCorners[3]].x, midPoint, vertices00[w00.hardCorners[3]].z);
                vertices11[w11.hardCorners[2]] = new Vector3(vertices11[w11.hardCorners[2]].x, midPoint, vertices11[w11.hardCorners[2]].z);
                vertices22[w22.hardCorners[3]] = new Vector3(vertices22[w22.hardCorners[3]].x, midPoint, vertices22[w22.hardCorners[3]].z);
                Color color3 = GradientManager.Evaluate3(w0.gameObject.GetComponent<MeshFilter>().mesh.colors[w0.hardCorners[0]], w1.gameObject.GetComponent<MeshFilter>().mesh.colors[w1.hardCorners[1]], w2.gameObject.GetComponent<MeshFilter>().mesh.colors[w2.hardCorners[0]]);
                w0.colors[w0.hardCorners[0]] = color3;
                w1.colors[w1.hardCorners[1]] = color3;
                w2.colors[w2.hardCorners[0]] = color3;
                w00.colors[w00.hardCorners[1]] = color3;
                w11.colors[w11.hardCorners[0]] = color3;
                w22.colors[w22.hardCorners[1]] = color3;
                w0.colors[w0.hardCorners[2]] = color3;
                w1.colors[w1.hardCorners[3]] = color3;
                w2.colors[w2.hardCorners[2]] = color3;
                w00.colors[w00.hardCorners[3]] = color3;
                w11.colors[w11.hardCorners[2]] = color3;
                w22.colors[w22.hardCorners[3]] = color3;
                w0.gameObject.GetComponent<MeshFilter>().mesh.vertices = vertices0;
                w1.gameObject.GetComponent<MeshFilter>().mesh.vertices = vertices1;
                w2.gameObject.GetComponent<MeshFilter>().mesh.vertices = vertices2;
                w00.gameObject.GetComponent<MeshFilter>().mesh.vertices = vertices00;
                w11.gameObject.GetComponent<MeshFilter>().mesh.vertices = vertices11;
                w22.gameObject.GetComponent<MeshFilter>().mesh.vertices = vertices22;
                w0.gameObject.GetComponent<MeshFilter>().mesh.RecalculateNormals();  
                w1.gameObject.GetComponent<MeshFilter>().mesh.RecalculateNormals();
                w2.gameObject.GetComponent<MeshFilter>().mesh.RecalculateNormals();
                w00.gameObject.GetComponent<MeshFilter>().mesh.RecalculateNormals();
                w11.gameObject.GetComponent<MeshFilter>().mesh.RecalculateNormals();
                w22.gameObject.GetComponent<MeshFilter>().mesh.RecalculateNormals();
            }
        }
    }
    private void LevelMeshBorders(Wedge w0, Wedge w1)
    {
        
        Vector3[] vertices0 = w0.gameObject.GetComponent<MeshFilter>().mesh.vertices;
        Vector3[] vertices1 = w1.gameObject.GetComponent<MeshFilter>().mesh.vertices;
        int wEnd = 7;
        int j = wEnd - 1;
        for (int i = 0; i < w0.border.Count/2; i++)
        {
            float midPoint = (vertices0[w0.border[i]].y + vertices1[w1.border[j]].y) / 2;
            vertices0[w0.border[i]] = new Vector3(vertices0[w0.border[i]].x, midPoint, vertices0[w0.border[i]].z);
            vertices1[w1.border[j]] = new Vector3(vertices1[w1.border[j]].x, midPoint, vertices1[w1.border[j]].z);
            w0.colors[w0.border[i]] = GradientManager.Evaluate2(GradientManager.GetColorById(w0.colorID), GradientManager.GetColorById(w1.colorID));
            w1.colors[w1.border[j]] = GradientManager.Evaluate2(GradientManager.GetColorById(w1.colorID), GradientManager.GetColorById(w0.colorID));
            vertices0[w0.border[wEnd + i]] = new Vector3(vertices0[w0.border[wEnd + i]].x, midPoint, vertices0[w0.border[wEnd + i]].z);
            vertices1[w1.border[wEnd + j]] = new Vector3(vertices1[w1.border[wEnd + j]].x, midPoint, vertices1[w1.border[wEnd + j]].z);
            w0.colors[w0.border[wEnd + i]] = GradientManager.Evaluate2(GradientManager.GetColorById(w0.colorID), GradientManager.GetColorById(w1.colorID));
            w1.colors[w1.border[wEnd + j]] = GradientManager.Evaluate2(GradientManager.GetColorById(w1.colorID), GradientManager.GetColorById(w0.colorID));
            j--;
        }

        //for (int i = 0; i < w0.border.Count; i++)
        //{
        //    Vector3 v0 = w0.gameObject.transform.position + vertices0[w0.border[i]];
        //    for (int j = 0; j < w1.border.Count; j++)
        //    {                
        //        Vector3 v1 = w1.gameObject.transform.position + vertices1[w1.border[j]];
        //        float v0x = (float)Math.Round(v0.x, 4);
        //        float v1x = (float)Math.Round(v1.x, 4);
        //        float v0z = (float)Math.Round(v0.z, 4);
        //        float v1z = (float)Math.Round(v1.z, 4);
        //        if ((v0x == v1x) && (v0z == v1z)) //  (vertices0[w0.border[i]].x == vertices1[w1.border[j]].x) && (vertices0[w0.border[i]].z == vertices1[w1.border[j]].z))
        //        {
        //            float midPoint = (vertices0[w0.border[i]].y + vertices1[w1.border[j]].y) / 2;
        //            vertices0[w0.border[i]] = new Vector3(vertices0[w0.border[i]].x, midPoint, vertices0[w0.border[i]].z);
        //            vertices1[w1.border[j]] = new Vector3(vertices1[w1.border[j]].x, midPoint, vertices1[w1.border[j]].z);
        //            w0.gameObject.GetComponent<MeshFilter>().mesh.colors[w0.border[i]] = GradientManager.Evaluate2(GradientManager.GetColorById(w0.colorID), GradientManager.GetColorById(w1.colorID));
        //            w1.gameObject.GetComponent<MeshFilter>().mesh.colors[w1.border[j]] = GradientManager.Evaluate2(GradientManager.GetColorById(w1.colorID), GradientManager.GetColorById(w0.colorID));
        //            vertices0[w0.border[wEnd + i]] = new Vector3(vertices0[w0.border[wEnd + i]].x, midPoint, vertices0[w0.border[wEnd + i]].z);
        //            vertices1[w1.border[wEnd + j]] = new Vector3(vertices1[w1.border[wEnd + j]].x, midPoint, vertices1[w1.border[wEnd + j]].z);
        //            w0.gameObject.GetComponent<MeshFilter>().mesh.colors[w0.border[wEnd + i]] = GradientManager.Evaluate2(GradientManager.GetColorById(w0.colorID), GradientManager.GetColorById(w1.colorID));
        //            w1.gameObject.GetComponent<MeshFilter>().mesh.colors[w1.border[wEnd + j]] = GradientManager.Evaluate2(GradientManager.GetColorById(w1.colorID), GradientManager.GetColorById(w0.colorID));
        //        } 
        //    }
        //}
        w0.gameObject.GetComponent<MeshFilter>().mesh.vertices = vertices0;
        w1.gameObject.GetComponent<MeshFilter>().mesh.vertices = vertices1;
        w0.gameObject.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        w1.gameObject.GetComponent<MeshFilter>().mesh.RecalculateNormals();
    }
    public void BackUpMeshesOnWedges()
    {
        int auxCount = 0;        
        for (int i = 1; i < wedges.Length; i++)
        {
            auxCount++;
            wedges[i].GetComponent<Wedge>().SetMeshBuffer();
            GameObject wedgeGoNeighbor = wedges[i].GetComponent<Wedge>().neighbors[3];
            if (wedgeGoNeighbor != null)
            {
                HexTile hexTileNeighbor = wedgeGoNeighbor.transform.parent.GetComponent<HexTile>();
                for (int j = 1; j < hexTileNeighbor.wedges.Length; j++)
                {
                    wedges[j].GetComponent<Wedge>().SetMeshBuffer();
                    auxCount++;
                }
            }
        }
        Debug.Log("BackUp Cant = " + auxCount);
    }
    private void DockMeshesOnWedges()
    {
        //para cada wedge dentro de este hex, fijarme si tengo un vecino, SI LO TENGO entonces
        for (int i = 1; i < wedges.Length; i++)
        {            
            if (wedges[i].GetComponent<Wedge>().neighbors[3] != null)
            {
                //wedges[i].GetComponent<Wedge>().LevelMeshVertices();
                Debug.Log("UNDOCK NOT IMPLEMENTED");
            }
        }
    }
    public void Undock()
    {
        //Restaurar Mesh Border        
        //UndockMeshesOnWedges();
        //Desanclar Neighbor
        for (int i = 1; i <= outerWedges; i++)
        {
            if (wedges[i].GetComponent<Wedge>().neighbors[3] != null)
            {
                wedges[i].GetComponent<Wedge>().neighbors[3].GetComponent<Wedge>().neighbors[3] = null;
                wedges[i].GetComponent<Wedge>().neighbors[3] = null;
            }
        }
    }
    public void RestoreBordersAndHardCorners()
    {
        if (bufferedMeshes.Count > 0)
        {
            RestoreBorders();
            RestoreHardCorners();
        }
    }
    private void RestoreBorders()
    {
        //foreach (GameObject wedgeGo in bufferedMeshes)
        //{
        //    //Agarro solo los bordes de 1 y los nivelo a su respectivo COLORID
        //    Wedge wedge = wedgeGo.GetComponent<Wedge>();
        //    Vector3[] vertices = wedgeGo.GetComponent<MeshFilter>().mesh.vertices;
        //    Color[] colors = wedgeGo.GetComponent<MeshFilter>().mesh.colors;
        //    for (int i = 0; i < wedge.border.Count; i++)
        //    {
        //        float defaultPoint = wedge.colorID * wedge.meshHeight;
        //        vertices[wedge.border[i]] = new Vector3(vertices[wedge.border[i]].x,defaultPoint, vertices[wedge.border[i]].z);
        //        colors[wedge.border[i]] = GradientManager.GetColorById(wedge.colorID);
        //    }
        //    wedgeGo.GetComponent<MeshFilter>().mesh.vertices = vertices;
        //    wedgeGo.GetComponent<MeshFilter>().mesh.colors = colors;
        //    wedgeGo.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        //}        
        foreach (GameObject wedgeGo in bufferedMeshes)
        {
            //Agarro solo los bordes de 1 y los nivelo a su respectivo COLORID
            Wedge wedge = wedgeGo.GetComponent<Wedge>();
            Vector3[] vertices = wedgeGo.GetComponent<MeshFilter>().mesh.vertices;
            Color[] colors = wedgeGo.GetComponent<Wedge>().colors;
            for (int i = 0; i < wedge.border.Count; i++)
            {
                float defaultPoint = wedge.colorID * wedge.meshHeight;
                vertices[wedge.border[i]] = new Vector3(vertices[wedge.border[i]].x, defaultPoint, vertices[wedge.border[i]].z);
                colors[wedge.border[i]] = GradientManager.GetColorById(wedge.colorID);
            }
            wedgeGo.GetComponent<MeshFilter>().mesh.vertices = vertices;
            wedgeGo.GetComponent<MeshFilter>().mesh.colors = colors;
            wedgeGo.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        }
    }
    private void RestoreHardCorners()
    {
        foreach (GameObject wedgeGo in bufferedMeshes)
        {
            GameObject w0Go = wedgeGo;
            GoLeft(w0Go);
            GoRight(w0Go);
        }
    }
    private void GoLeft(GameObject w0Go)
    {
        Wedge w0 = w0Go.GetComponent<Wedge>();
        GameObject w1Go = w0.neighbors[1];
        Wedge w1 = w1Go.GetComponent<Wedge>();
        if (w1.neighbors[3] != null)
        {
            GameObject wAux1Go = w1.neighbors[3];
            Wedge wAux1 = wAux1Go.GetComponent<Wedge>();
            GameObject wAux2Go = wAux1.neighbors[1];
            Wedge wAux2 = wAux2Go.GetComponent<Wedge>();
            //Asignar
            List<int> yValue = new List<int>();
            yValue.Add(w0.colorID); yValue.Add(w1.colorID); yValue.Add(wAux1.colorID); yValue.Add(wAux2.colorID);
            float medianValue = GetMedianHeight(yValue,w0.meshHeight);
            Color medianColor = GradientManager.Evaluate4(w0.colorID, w1.colorID, wAux1.colorID, wAux2.colorID);
            Vector3[] v0 = w0Go.GetComponent<MeshFilter>().mesh.vertices;
            Color[] c0 = w0Go.GetComponent<Wedge>().colors;
            v0[w0.hardCorners[1]] = new Vector3(v0[w0.hardCorners[1]].x, medianValue, v0[w0.hardCorners[1]].z);
            v0[w0.hardCorners[3]] = new Vector3(v0[w0.hardCorners[3]].x, medianValue, v0[w0.hardCorners[3]].z);
            c0[w0.hardCorners[1]] = medianColor;
            c0[w0.hardCorners[3]] = medianColor;
            Vector3[] v1 = w1Go.GetComponent<MeshFilter>().mesh.vertices;
            Color[] c1 = w1Go.GetComponent<Wedge>().colors;
            v1[w1.hardCorners[0]] = new Vector3(v1[w1.hardCorners[0]].x, medianValue, v1[w1.hardCorners[0]].z);
            v1[w1.hardCorners[2]] = new Vector3(v1[w1.hardCorners[2]].x, medianValue, v1[w1.hardCorners[2]].z);
            c1[w1.hardCorners[0]] = medianColor;
            c1[w1.hardCorners[2]] = medianColor;
            Vector3[] vAux1 = wAux1Go.GetComponent<MeshFilter>().mesh.vertices;
            Color[] cAux1 = wAux1Go.GetComponent<Wedge>().colors;
            vAux1[wAux1.hardCorners[1]] = new Vector3(vAux1[wAux1.hardCorners[1]].x, medianValue, vAux1[wAux1.hardCorners[1]].z);
            vAux1[wAux1.hardCorners[3]] = new Vector3(vAux1[wAux1.hardCorners[3]].x, medianValue, vAux1[wAux1.hardCorners[3]].z);
            cAux1[wAux1.hardCorners[1]] = medianColor;
            cAux1[wAux1.hardCorners[3]] = medianColor;
            Vector3[] vAux2 = wAux2Go.GetComponent<MeshFilter>().mesh.vertices;
            Color[] cAux2 = wAux2Go.GetComponent<Wedge>().colors;
            vAux2[wAux2.hardCorners[0]] = new Vector3(vAux2[wAux2.hardCorners[0]].x, medianValue, vAux2[wAux2.hardCorners[0]].z);
            vAux2[wAux2.hardCorners[2]] = new Vector3(vAux2[wAux2.hardCorners[2]].x, medianValue, vAux2[wAux2.hardCorners[2]].z);
            cAux2[wAux2.hardCorners[0]] = medianColor;
            cAux2[wAux2.hardCorners[2]] = medianColor;
            //SetMesh
            w0Go.GetComponent<MeshFilter>().mesh.vertices = v0;
            w0Go.GetComponent<MeshFilter>().mesh.colors = c0;
            w0Go.GetComponent<MeshFilter>().mesh.RecalculateNormals();
            w1Go.GetComponent<MeshFilter>().mesh.vertices = v1;
            w1Go.GetComponent<MeshFilter>().mesh.colors = c1;
            w1Go.GetComponent<MeshFilter>().mesh.RecalculateNormals();
            wAux1Go.GetComponent<MeshFilter>().mesh.vertices = vAux1;
            wAux1Go.GetComponent<MeshFilter>().mesh.colors = cAux1;
            wAux1Go.GetComponent<MeshFilter>().mesh.RecalculateNormals();
            wAux2Go.GetComponent<MeshFilter>().mesh.vertices = vAux2;
            wAux2Go.GetComponent<MeshFilter>().mesh.colors = cAux2;
            wAux2Go.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        }
        else
        {
            //Asignar
            List<int> yValue = new List<int>();
            yValue.Add(w0.colorID); yValue.Add(w1.colorID);
            float medianValue = GetMedianHeight(yValue, w0.meshHeight);
            Color medianColor = GradientManager.Evaluate2(GradientManager.GetColorById(w0.colorID), GradientManager.GetColorById(w1.colorID));
            Vector3[] v0 = w0Go.GetComponent<MeshFilter>().mesh.vertices;
            Color[] c0 = w0Go.GetComponent<Wedge>().colors;
            v0[w0.hardCorners[1]] = new Vector3(v0[w0.hardCorners[1]].x, medianValue, v0[w0.hardCorners[1]].z);
            v0[w0.hardCorners[3]] = new Vector3(v0[w0.hardCorners[3]].x, medianValue, v0[w0.hardCorners[3]].z);
            c0[w0.hardCorners[1]] = medianColor;
            c0[w0.hardCorners[3]] = medianColor;
            Vector3[] v1 = w1Go.GetComponent<MeshFilter>().mesh.vertices;
            Color[] c1 = w1Go.GetComponent<Wedge>().colors;
            v1[w1.hardCorners[0]] = new Vector3(v1[w1.hardCorners[0]].x, medianValue, v1[w1.hardCorners[0]].z);
            v1[w1.hardCorners[2]] = new Vector3(v1[w1.hardCorners[2]].x, medianValue, v1[w1.hardCorners[2]].z);
            c1[w1.hardCorners[0]] = medianColor;
            c1[w1.hardCorners[2]] = medianColor;
            //SetMesh
            w0Go.GetComponent<MeshFilter>().mesh.vertices = v0;
            w0Go.GetComponent<MeshFilter>().mesh.colors = c0;
            w0Go.GetComponent<MeshFilter>().mesh.RecalculateNormals();
            w1Go.GetComponent<MeshFilter>().mesh.vertices = v1;
            w1Go.GetComponent<MeshFilter>().mesh.colors = c1;
            w1Go.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        }
    }
    private void GoRight(GameObject w0Go)
    {
        Wedge w0 = w0Go.GetComponent<Wedge>();
        GameObject w1Go = w0.neighbors[2];
        Wedge w1 = w1Go.GetComponent<Wedge>();
        if (w1.neighbors[3] != null)
        {
            GameObject wAux1Go = w1.neighbors[3];
            Wedge wAux1 = wAux1Go.GetComponent<Wedge>();
            GameObject wAux2Go = wAux1.neighbors[2];
            Wedge wAux2 = wAux2Go.GetComponent<Wedge>();
            //Asignar
            List<int> yValue = new List<int>();
            yValue.Add(w0.colorID); yValue.Add(w1.colorID); yValue.Add(wAux1.colorID); yValue.Add(wAux2.colorID);
            float medianValue = GetMedianHeight(yValue, w0.meshHeight);
            Color medianColor = GradientManager.Evaluate4(w0.colorID, w1.colorID, wAux1.colorID, wAux2.colorID);
            Vector3[] v0 = w0Go.GetComponent<MeshFilter>().mesh.vertices;
            Color[] c0 = w0Go.GetComponent<Wedge>().colors;
            v0[w0.hardCorners[0]] = new Vector3(v0[w0.hardCorners[0]].x, medianValue, v0[w0.hardCorners[0]].z);
            v0[w0.hardCorners[2]] = new Vector3(v0[w0.hardCorners[2]].x, medianValue, v0[w0.hardCorners[2]].z);
            c0[w0.hardCorners[0]] = medianColor;
            c0[w0.hardCorners[2]] = medianColor;
            Vector3[] v1 = w1Go.GetComponent<MeshFilter>().mesh.vertices;
            Color[] c1 = w1Go.GetComponent<Wedge>().colors;
            v1[w1.hardCorners[1]] = new Vector3(v1[w1.hardCorners[1]].x, medianValue, v1[w1.hardCorners[1]].z);
            v1[w1.hardCorners[3]] = new Vector3(v1[w1.hardCorners[3]].x, medianValue, v1[w1.hardCorners[3]].z);
            c1[w1.hardCorners[1]] = medianColor;
            c1[w1.hardCorners[3]] = medianColor;
            Vector3[] vAux1 = wAux1Go.GetComponent<MeshFilter>().mesh.vertices;
            Color[] cAux1 = wAux1Go.GetComponent<Wedge>().colors;
            vAux1[wAux1.hardCorners[0]] = new Vector3(vAux1[wAux1.hardCorners[0]].x, medianValue, vAux1[wAux1.hardCorners[0]].z);
            vAux1[wAux1.hardCorners[2]] = new Vector3(vAux1[wAux1.hardCorners[2]].x, medianValue, vAux1[wAux1.hardCorners[2]].z);
            cAux1[wAux1.hardCorners[0]] = medianColor;
            cAux1[wAux1.hardCorners[2]] = medianColor;
            Vector3[] vAux2 = wAux2Go.GetComponent<MeshFilter>().mesh.vertices;
            Color[] cAux2 = wAux2Go.GetComponent<Wedge>().colors;
            vAux2[wAux2.hardCorners[1]] = new Vector3(vAux2[wAux2.hardCorners[1]].x, medianValue, vAux2[wAux2.hardCorners[1]].z);
            vAux2[wAux2.hardCorners[3]] = new Vector3(vAux2[wAux2.hardCorners[3]].x, medianValue, vAux2[wAux2.hardCorners[3]].z);
            cAux2[wAux2.hardCorners[1]] = medianColor;
            cAux2[wAux2.hardCorners[3]] = medianColor;
            //SetMesh
            w0Go.GetComponent<MeshFilter>().mesh.vertices = v0;
            w0Go.GetComponent<MeshFilter>().mesh.colors = c0;
            w0Go.GetComponent<MeshFilter>().mesh.RecalculateNormals();
            w1Go.GetComponent<MeshFilter>().mesh.vertices = v1;
            w1Go.GetComponent<MeshFilter>().mesh.colors = c1;
            w1Go.GetComponent<MeshFilter>().mesh.RecalculateNormals();
            wAux1Go.GetComponent<MeshFilter>().mesh.vertices = vAux1;
            wAux1Go.GetComponent<MeshFilter>().mesh.colors = cAux1;
            wAux1Go.GetComponent<MeshFilter>().mesh.RecalculateNormals();
            wAux2Go.GetComponent<MeshFilter>().mesh.vertices = vAux2;
            wAux2Go.GetComponent<MeshFilter>().mesh.colors = cAux2;
            wAux2Go.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        }
        else
        {
            //Asignar
            List<int> yValue = new List<int>();
            yValue.Add(w0.colorID); yValue.Add(w1.colorID);
            float medianValue = GetMedianHeight(yValue, w0.meshHeight);
            Color medianColor = GradientManager.Evaluate2(GradientManager.GetColorById(w0.colorID), GradientManager.GetColorById(w1.colorID));
            Vector3[] v0 = w0Go.GetComponent<MeshFilter>().mesh.vertices;
            Color[] c0 = w0Go.GetComponent<Wedge>().colors;
            v0[w0.hardCorners[0]] = new Vector3(v0[w0.hardCorners[0]].x, medianValue, v0[w0.hardCorners[0]].z);
            v0[w0.hardCorners[2]] = new Vector3(v0[w0.hardCorners[2]].x, medianValue, v0[w0.hardCorners[2]].z);
            c0[w0.hardCorners[0]] = medianColor;
            c0[w0.hardCorners[2]] = medianColor;
            Vector3[] v1 = w1Go.GetComponent<MeshFilter>().mesh.vertices;
            Color[] c1 = w1Go.GetComponent<Wedge>().colors;
            v1[w1.hardCorners[1]] = new Vector3(v1[w1.hardCorners[1]].x, medianValue, v1[w1.hardCorners[1]].z);
            v1[w1.hardCorners[3]] = new Vector3(v1[w1.hardCorners[3]].x, medianValue, v1[w1.hardCorners[3]].z);
            c1[w1.hardCorners[1]] = medianColor;
            c1[w1.hardCorners[3]] = medianColor;
            //SetMesh
            w0Go.GetComponent<MeshFilter>().mesh.vertices = v0;
            w0Go.GetComponent<MeshFilter>().mesh.colors = c0;
            w0Go.GetComponent<MeshFilter>().mesh.RecalculateNormals();
            w1Go.GetComponent<MeshFilter>().mesh.vertices = v1;
            w1Go.GetComponent<MeshFilter>().mesh.colors = c1;
            w1Go.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        }
    }
    private float GetMedianHeight(List<int> yValues,float height)
    {
        float total = 0;
        foreach (int yValue in yValues)
        {
            total += (yValue * height);
        }
        return total / yValues.Count;
    }
    private void UndockMeshesOnWedges()
    {
        int auxCount = 0;
        for (int i = 1; i < wedges.Length; i++)
        {
            auxCount++;
            wedges[i].GetComponent<Wedge>().RestoreBufferedMesh();
            GameObject wedgeGoNeighbor = wedges[i].GetComponent<Wedge>().neighbors[3];
            if (wedgeGoNeighbor != null)
            {
                HexTile hexTileNeighbor = wedgeGoNeighbor.transform.parent.GetComponent<HexTile>();
                for (int j = 1; j < hexTileNeighbor.wedges.Length; j++)
                {                    
                    hexTileNeighbor.wedges[j].GetComponent<Wedge>().RestoreBufferedMesh();
                    auxCount++;
                }
            }
        }
        Debug.Log("Restore Cant = " + auxCount);
    }    
    private void FillEmptyNeighbors()
    {
        for (int i = 1; i <= outerWedges; i++)
        {
            Wedge wedge = wedges[i].GetComponent<Wedge>();
            if (wedge.CheckAdjacentTile(0) == false) //Si no le pega a nadie -> llenar con blank tile
            {
                CreateBlankNeighbor(new Vector3(wedge.eye.transform.position.x, 0, wedge.eye.transform.position.z));
            }
        }
    }
    private void CreateBlankNeighbor(Vector3 dest)
    {
        string blankPrefabPath = "Prefabs/BlankTile";
        Vector3 pos = (dest - transform.position);
        pos.Normalize();
        pos = pos * neighborDistance;
        pos = transform.position + pos;
        GameObject blankGo = Instantiate(Resources.Load(blankPrefabPath), pos, Quaternion.identity) as GameObject;
        blankGo.GetComponent<HexTile>().currentState = HexTile.TileState.Blank;
        blankGo.transform.parent = this.gameObject.transform.parent;
    }    
    private int[] GenerateWedgeColors()
    {
        randomColorsList.Clear();
        //PASO 1 : INICIALIZAR
        int[] colorCounter = new int[maxColors]; //Contador de ocurrencias de cada color        
        for (int i = 0; i < maxColors; i++) //Inicializa en 0 
        {
            colorCounter[i] = 0;
        }        
        for (int i = 0; i < outerWedges; i++) //Agrega 6 Random int en Lista
        {
            int rnd = UnityEngine.Random.Range(0, 5);  
            randomColorsList.Add(rnd);
            colorCounter[rnd] += 1; //Suma ocurrencia al ARR del Random generado
        }        
        return colorCounter;
    }
    private void FixColors(int rolledColors)
    {
        int i = 1;
        int buffer;
        int j = 1;
        buffer = randomColorsList[j];
        while (i < rolledColors)
        {
            if (randomColorsList[j] == buffer)
            {                
                j++;
            }
            else
            {
                i++;
                buffer = randomColorsList[j];
            }
        }
        j++;
        for (int k = j; k < randomColorsList.Count ; k = j++)
        {
            randomColorsList[k] = randomColorsList[0];
        }
    }
    private int GetAmmountOfColors(int[] colorCounter)
    {
        int ammount = 0;
        for (int i = 0; i < colorCounter.Length; i++)
        {
            if (colorCounter[i] > 0)
            {
                ammount++; 
            }
        }
        return ammount;
    }    
    public void MainGenerate()
    {
        bool success = false;
        int[] colorsRatio = new int[25] { 1, 1, 1, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4, 4, 5, 5, 5 };
        int colorsCantRolled = colorsRatio[UnityEngine.Random.Range(0, 25)];        
        int[] colorCounter;
        colorCounter = GenerateWedgeColors();
        randomColorsList.Insert(0, GetCenterWedgeColor(colorCounter)); //Defino el color del WEDGE 0 y lo agrego al principio
        int currentColors;
        while (success == false)
        { 
            currentColors = GetAmmountOfColors(colorCounter);
            if (currentColors > colorsCantRolled)
            {                
                FixColors(colorsCantRolled);
                success = true;
            }
            else if (currentColors < colorsCantRolled)
            {
                 colorCounter = GenerateWedgeColors();
                randomColorsList.Insert(0, GetCenterWedgeColor(colorCounter)); //Defino el color del WEDGE 0 y lo agrego al principio
            }
            else
            {
                success = true;
            }
        }               
        //PASO 3 : SETEAR COLORES
        for (int i = 0; i < allWedges; i++)
        {
            wedges[i].GetComponent<Wedge>().SetColor(randomColorsList[i]); //CORRECTA
        }        
    }    
    public void AdaptHexMesh()
    {
        //NIVELO CORE VERTEX Y EDGES 
        foreach (GameObject wedgeGo in wedges)
        {
            Wedge wedge = wedgeGo.GetComponent<Wedge>();
            wedge.colors = new Color[wedgeGo.GetComponent<MeshFilter>().mesh.vertices.Length];
            
            wedge.SetMeshHeightAndColor();            
        }
        //AJUSTO AL PUNTO MEDIO EDGES
        foreach (GameObject wedgeGo in wedges)
        {
            Wedge wedge = wedgeGo.GetComponent<Wedge>();            
            
            foreach (GameObject neighbor in wedge.neighbors)
            {
                if (neighbor != null)
                {
                    LevelMeshEdges(wedgeGo, neighbor);
                }
            }
        }
        //AJUSTO AL PUNTO MEDIO CORNERS de WEDGE 0
        LevelMeshCorners(wedges[0]);
        //REFRESH COLORS ARRAY ON MESH
        for (int i = 0; i < wedges.Length; i++)
        {
            Wedge wedge = wedges[i].GetComponent<Wedge>();
            wedges[i].GetComponent<MeshFilter>().mesh.colors = wedge.colors;
        }
    }
    private void LevelMeshEdges(GameObject go1,GameObject go2)
    {
        Vector3[] arr1 = go1.GetComponent<MeshFilter>().mesh.vertices;
        Vector3[] arr2 = go2.GetComponent<MeshFilter>().mesh.vertices;
        Wedge w1 = go1.GetComponent<Wedge>();
        Wedge w2 = go2.GetComponent<Wedge>();
        for (int i = 0; i < w1.edges.Count; i++)
        {
            for (int j = 0; j < w2.edges.Count; j++)
            {
                if ((arr1[w1.edges[i]].x == arr2[w2.edges[j]].x) && (arr1[w1.edges[i]].z == arr2[w2.edges[j]].z))
                {
                    float midPoint = (arr1[w1.edges[i]].y + arr2[w2.edges[j]].y) / 2;
                    arr1[w1.edges[i]] = new Vector3(arr1[w1.edges[i]].x, midPoint, arr1[w1.edges[i]].z);
                    arr2[w2.edges[j]] = new Vector3(arr2[w2.edges[j]].x, midPoint, arr2[w2.edges[j]].z);
                    w1.colors[w1.edges[i]] = GradientManager.Evaluate2(GradientManager.GetColorById(w1.colorID), GradientManager.GetColorById(w2.colorID));
                    w2.colors[w2.edges[j]] = GradientManager.Evaluate2(GradientManager.GetColorById(w2.colorID), GradientManager.GetColorById(w1.colorID));
                }
            }
        }
        go1.GetComponent<MeshFilter>().mesh.vertices = arr1;
        go2.GetComponent<MeshFilter>().mesh.vertices = arr2;
        go1.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        go2.GetComponent<MeshFilter>().mesh.RecalculateNormals();
    }
    private void LevelMeshCorners(GameObject wedge)
    {
        Wedge w1;
        Wedge w2;
        Wedge w0 = wedge.GetComponent<Wedge>();
        Vector3[] arr0 = wedge.GetComponent<MeshFilter>().mesh.vertices;
        Vector3[] arr1;
        Vector3[] arr2;
        //End Indexes
        int w0end = 6;
        int w1end = 2;
        //Recorrer Todos los vecinos Juntando grupos de 3        
        for (int i = 0; i < 6; i++)
        {
            if (i == 5)
            {
                w1 = wedges[i+1].GetComponent<Wedge>();
                arr1 = wedges[i+1].GetComponent<MeshFilter>().mesh.vertices;
                w2 = wedges[1].GetComponent<Wedge>();
                arr2 = wedges[1].GetComponent<MeshFilter>().mesh.vertices;
            }
            else
            {
                w1 = wedges[i + 1].GetComponent<Wedge>();
                arr1 = wedges[i + 1].GetComponent<MeshFilter>().mesh.vertices;
                w2 = wedges[i + 2].GetComponent<Wedge>();
                arr2 = wedges[i + 2].GetComponent<MeshFilter>().mesh.vertices;                              
            }
            //Accedo al corner correspondiente en cada Mesh Vertices Array y le modifico el " .y " asignandole el punto medio entre los 3 VERTICES
            //W0 corner es corners en la pos i | w1 corner es corners en la pos 0 (corner 0)| w2 corner es corners en la pos 1 (corner 3)
            float midPoint = GetVertexMidPoint3(arr0[w0.corners[i]], arr1[w1.corners[0]], arr2[w2.corners[1]]);
            arr0[w0.corners[i]] = new Vector3(arr0[w0.corners[i]].x, midPoint, arr0[w0.corners[i]].z);
            arr1[w1.corners[0]] = new Vector3(arr1[w1.corners[0]].x, midPoint, arr1[w1.corners[0]].z);
            arr2[w2.corners[1]] = new Vector3(arr2[w2.corners[1]].x, midPoint, arr2[w2.corners[1]].z);
            arr0[w0.corners[w0end + i]] = new Vector3(arr0[w0.corners[w0end + i]].x, midPoint, arr0[w0.corners[w0end + i]].z);
            arr1[w1.corners[w1end + 0]] = new Vector3(arr1[w1.corners[w1end + 0]].x, midPoint, arr1[w1.corners[w1end + 0]].z);
            arr2[w2.corners[w1end + 1]] = new Vector3(arr2[w2.corners[w1end + 1]].x, midPoint, arr2[w2.corners[w1end + 1]].z);
            //Asignar color a dicho VERTICE
            Color targetColor = GradientManager.Evaluate3(GradientManager.GetColorById(w0.colorID), GradientManager.GetColorById(w1.colorID), GradientManager.GetColorById(w2.colorID));
            w0.colors[w0.corners[i]] = targetColor;
            w1.colors[w1.corners[0]] = targetColor;
            w2.colors[w2.corners[1]] = targetColor;
            w0.colors[w0.corners[w0end + i]] = targetColor;
            w1.colors[w1.corners[w1end + 0]] = targetColor;
            w1.colors[w2.corners[w1end + 1]] = targetColor;

            w0.gameObject.GetComponent<MeshFilter>().mesh.vertices = arr0;
            w1.gameObject.GetComponent<MeshFilter>().mesh.vertices = arr1;
            w2.gameObject.GetComponent<MeshFilter>().mesh.vertices = arr2;
            w0.gameObject.GetComponent<MeshFilter>().mesh.RecalculateNormals();
            w1.gameObject.GetComponent<MeshFilter>().mesh.RecalculateNormals();
            w2.gameObject.GetComponent<MeshFilter>().mesh.RecalculateNormals();            
        }
    }
    private float GetVertexMidPoint3(Vector3 v0,Vector3 v1, Vector3 v2)
    {
        return (v0.y + v1.y + v2.y) / 3;
    }
    private float GetVertexMidpoint2(Vector3 v0, Vector3 v1)
    {
        return (v0.y + v1.y) / 2;
    }
    private int GetCenterWedgeColor(int[] colorCounter)
    {        
        List<int> candidates = new List<int>();
        for (int i = 0; i < colorCounter.Length; i++)
        {
            if (colorCounter[i] > 1) //Si hay mas de 2 colores iguales Tenerlo en cuenta
            {
                candidates.Add(i);
            }
        }
        int rnd = UnityEngine.Random.Range(0, candidates.Count);
        return candidates[rnd];
    }
    public void RotateHexTile(int rotDirection)
    {
        transform.Rotate(new Vector3(0, 60 * rotDirection, 0));
        UpdateWedgeDirections(rotDirection);
    }
    private void UpdateWedgeDirections(int rotDirection)
    {
        //for (int i = 1; i <= outerWedges; i++)
        //{
        //    List<Vector3> newSet = new List<Vector3>();
        //    for (int j = 0; j < 4; j++)
        //    {
        //        Vector3 aux = wedges[i].GetComponent<Wedge>().directions[j];
        //        aux = Quaternion.AngleAxis(60 * rotDirection, Vector3.up) * aux;
        //        newSet.Add(aux);
        //    }            
        //    wedges[i].GetComponent<Wedge>().directions = newSet;
        //}
    }    
    private float GetNeighborDistance()
    {
        //return 2 * Mathf.Sqrt(Mathf.Pow(tileSize, 2) - Mathf.Pow(tileSize / 2, 2));
        return Mathf.Sqrt(3) * tileSize;
    }
    private void CreateDistributionArray()
    {
        distributions = new List<int[]>[3];
        distributions[0] = new List<int[]> {    new int[] {0,0,0,0,0,0}};
        distributions[1] = new List<int[]> {    new int[] {0,0,0,0,0,1}, new int[] {0,0,0,0,1,1}, new int[] {0,0,0,1,1,1}, new int[] {0,1,0,1,0,1},
                                                new int[] {0,0,1,0,0,1}, new int[] {0,0,0,1,0,1}, new int[] {0,0,1,0,1,1}, new int[] {0,0,1,1,0,1}};
        distributions[2] = new List<int[]> {    new int[] {0,0,0,0,1,2}, new int[] {0,0,0,1,1,2}, new int[] {0,0,0,2,1,1}, new int[] {0,0,0,1,2,1},
                                                new int[] {0,0,1,1,1,2}, new int[] {0,0,1,1,2,2}, new int[] {0,0,1,2,2,2}, new int[] {0,0,1,2,2,1},
                                                new int[] {0,0,1,2,1,1}, new int[] {0,0,1,1,2,1}, new int[] {0,0,2,1,2,1}, new int[] {0,0,1,2,1,2},
                                                new int[] {0,1,2,0,1,2}, new int[] {0,0,1,0,1,2}, new int[] {0,0,1,1,0,2}, new int[] {0,0,1,0,2,1},
                                                new int[] {0,0,1,2,0,1}, new int[] {0,1,2,1,0,2}};
    }
    public void DistributeColors()
    {
        //PASO 1 : Armar los Outer los Wedges
        int[] colorsSet = CreateColorsSet(UnityEngine.Random.Range(0,3));
        int rnd = UnityEngine.Random.Range(0, distributions[colorsSet.Length - 1].Count);
        for (int i = 1; i < wedges.Length; i++)
        {
            wedges[i].GetComponent<Wedge>().SetColor(colorsSet[distributions[colorsSet.Length - 1][rnd][i-1]]) ;//[rnd[i - 1]]]) ; //CORRECTA
            
        }
        //PASO 2 : Meter el center wedge (wedge0)
        int[] colorOcurrences = PopulateColorOcurrences();
        wedges[0].GetComponent<Wedge>().SetColor(GetCenterWedgeColor(colorOcurrences));        
    }
    private int[] PopulateColorOcurrences()
    {
        int[] final = new int[5];
        for (int i = 0; i < final.Length; i++)
        {
            final[i] = 0;
        }
        for (int i = 1; i < wedges.Length; i++)
        {
            final[wedges[i].GetComponent<Wedge>().colorID]++;
        }
        return final;
    }
    private int[] CreateColorsSet(int colorsCant)
    {
        int[] final = new int[colorsCant+1];
        int[] colorPicker = new int[] { 0, 1, 2, 3, 4 };
        bool goodRoll;
        for (int i = 0; i < final.Length; i++)
        {
            goodRoll = false;
            while (goodRoll == false)
            {
                int rnd = UnityEngine.Random.Range(0, 5);
                if (colorPicker[rnd] != -1)
                {
                    final[i] = colorPicker[rnd];
                    colorPicker[rnd] = -1;
                    goodRoll = true;
                }
                else
                {
                    goodRoll = false;
                }
            }
        }
        return final;
    }
    public void ResolvePerfect()
    {
        int status = GetPerfectStatus();
        if (status > 0)
        {
            //perfect
            //StartCoroutine(LaunchPopUp("PERFECT"));            
            onPerfectTile?.Invoke(1,this.gameObject);
        }
        else if (status < 0)
        {
            //not perfect
            StartCoroutine(LaunchFailedTile());
            onPerfectTile?.Invoke(-1, this.gameObject);
        }
        else
        {
            //incomplete status
            onPerfectTile?.Invoke(0, this.gameObject);
        }
    }
    private int GetPerfectStatus() //1 = PERFECT   --   -1 = NOT PERFECT   --    0 = INCOMPLETE
    {
        int perfectStatus = 1;
        for (int i = 1; i < wedges.Length; i++)
        {
            Wedge wedge = wedges[i].GetComponent<Wedge>();
            if (wedge.neighbors[3] != null)
            {
                if (wedge.colorID != wedge.neighbors[3].GetComponent<Wedge>().colorID)
                    perfectStatus = -1;
            }
            else
            {
                return 0;
            }
        }
        return perfectStatus;
    }
    private int GetColorAmmount()
    {
        int[] chances = new int[] { 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3 };
        return chances[UnityEngine.Random.Range(0,chances.Length)];
    }
    public IEnumerator LaunchPopUp(string value)
    {
        float duration = 0.5f;    // <----  SETEAR DURACION    
        GameObject cameraBase = GameObject.Find("Camera Base");
        string path = "Prefabs/PopUpText";
        GameObject popUp = Instantiate(Resources.Load(path), this.transform.position, Quaternion.identity) as GameObject;
        popUp.transform.LookAt(cameraBase.transform);
        popUp.GetComponentInChildren<Canvas>().worldCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        popUp.transform.parent = this.gameObject.transform;
        popUp.GetComponentInChildren<Canvas>().transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = value;
        Vector3 startPosition = popUp.transform.position;
        Vector3 endPositon = popUp.transform.position + new Vector3(0, 1, 0);        
        for (float t = 0f; t < duration; t+= Time.deltaTime)
        {
            popUp.transform.position = Vector3.Lerp(startPosition, endPositon, t/duration);
            yield return null;
        }
        popUp.transform.position = endPositon;
        yield return new WaitForSeconds(0.1f);
        Destroy(popUp);
    }
    public IEnumerator LaunchFailedTile()
    {
        float duration = 0.03f;      // <-----  DURACION
        int steps = 7;              // <-----  VARIACION
        float maxAngle = 2.5f;        // <-----  ROTACION
        float setUpDuration = 0.03f;
        //GENERAR ANIMACION ERRATICA
        Quaternion defaultRotation = this.transform.rotation;
        Quaternion baseRotation = this.transform.rotation;
        for (int i = 0; i < steps; i++)
        {
            float angleX = UnityEngine.Random.Range(0,maxAngle);
            float angleZ = UnityEngine.Random.Range(0,maxAngle);
            Quaternion startRotation = baseRotation * Quaternion.Euler(new Vector3(angleX, 0, angleZ));
            Quaternion endRotation = baseRotation * Quaternion.Euler(new Vector3(-angleX,0,angleZ));
            for (float t = 0f; t < setUpDuration; t+=Time.deltaTime)
            {
                this.transform.rotation = Quaternion.Lerp(baseRotation, startRotation, t / setUpDuration);
                yield return null;
            }
            for (float t = 0f; t < duration; t+=Time.deltaTime)
            {
                this.transform.rotation = Quaternion.Lerp(startRotation, endRotation, t / duration);
                yield return null;
            }
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                this.transform.rotation = Quaternion.Lerp(endRotation, baseRotation, t / duration);
                yield return null;
            }            
        }
        //Quaternion startRot = this.transform.rotation;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            this.transform.rotation = Quaternion.Lerp(baseRotation, defaultRotation, t * duration);
            yield return null;
        }
        this.transform.rotation = defaultRotation;
        yield return null;
    }
    public IEnumerator LaunchMiniHex()
    {
        //EXPLOTAR PARTICULAS
        string explosionPath = "Prefabs/SpawnMiniHexExplosion";
        GameObject explosion = Instantiate(Resources.Load(explosionPath), this.transform.position, Quaternion.identity) as GameObject;
        explosion.GetComponent<ParticleSystem>().Play();
        //INSTANCIAR MINIHEX
        float duration = 50f; 
        string miniHexPath = "Prefabs/MiniHex";
        GameObject miniHex = Instantiate(Resources.Load(miniHexPath), this.transform.position + new Vector3(0,1.01f,0), Quaternion.identity) as GameObject;        
        Vector3 startPosition = this.transform.position;
        Vector3 targetDir = new Vector3(UnityEngine.Random.Range(-3, 3),8f,UnityEngine.Random.Range(-3, 3));
        miniHex.GetComponent<MiniHex>().velocity = targetDir;
        miniHex.transform.rotation = Quaternion.LookRotation(targetDir);
        for (float t = 0; t < duration; t += Time.deltaTime)
        {            
            miniHex.transform.rotation *= Quaternion.AngleAxis(5f, Vector3.right);
            if (miniHex.transform.position.y <= 1)
            {
                miniHex.GetComponent<MiniHex>().velocity = Vector3.zero;
                miniHex.GetComponent<MiniHex>().gravity = 0f;
                miniHex.transform.rotation = Quaternion.identity;
                miniHex.transform.GetChild(7).GetComponent<ParticleSystem>().Stop();
                yield return new WaitForSeconds(1);
                Destroy(miniHex);
                Destroy(explosion);
                break;
            }
            yield return null;            
        }        
        yield return null;
    }
}
