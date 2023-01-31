using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Wedge : MonoBehaviour
{
    public Vector3 realPivot;
    public bool centerWedge;
    public int colorID;
    private float maxDist;
    public GameObject eye;
    public bool checkedWedge;
    private Material[] materials;
    private bool highlight;
    public GameObject[] neighbors;
    public Vector3 adjacentWedgeDirection;
    private Timer timer1;
    private int maxColors = 5;

    // MESH VERTEX DATA
    private float minHeight;
    private float maxHeight;
    private List<Vector3> lastMeshVertices;
    private int lastIndex;
    public float meshHeight;
    private int edgeLenght;
    public List<int> corners;
    public List<int> edges;
    public List<int> border;
    public List<int> hardCorners;
    public Color[] colors;
    public Mesh meshBuffer;
    public GameObject meshBufferGo;
    public List<Vector3> verticesBuffer;
    public List<Color> colorsBuffer;


    //MMM
    public List<Vector3> directions;

    private void Awake()
    {
        //meshBufferGo = new GameObject("aux", typeof(MeshFilter));
        meshBuffer = new Mesh();
        verticesBuffer = new List<Vector3>();
        colorsBuffer = new List<Color>();
        maxColors = 5;
        lastMeshVertices = new List<Vector3>();
        materials = new Material[2];
        meshHeight = 0.6f;
        minHeight = 0;
        maxHeight = 5 * meshHeight;
        SetUpMeshData();
    }
    private void Start()
    {
        //PREPARAR MESH DATA

        meshHeight = 0.6f;
        //SetUpMeshData();
        timer1 = new Timer(0.05f);
        // materials[0] setear los 2 materiales
        // materials[1]
        highlight = false;
        maxDist = 100;
        checkedWedge = false;

    }

    private void SetUpMeshData()
    {
        corners = new List<int>();
        edges = new List<int>();
        border = new List<int>();
        hardCorners = new List<int>();
        if (centerWedge)
        {   //CENTER WEDGE (W0)
            corners.Add(2); corners.Add(1); corners.Add(6); corners.Add(5); corners.Add(4); corners.Add(3);
            corners.Add(101); corners.Add(61); corners.Add(69); corners.Add(77); ; corners.Add(85); corners.Add(93);

            edges.Add(57); edges.Add(18); edges.Add(60);
            edges.Add(51); edges.Add(17); edges.Add(55);
            edges.Add(44); edges.Add(15); edges.Add(48);
            edges.Add(37); edges.Add(13); edges.Add(41);
            edges.Add(30); edges.Add(11); edges.Add(34);
            edges.Add(22); edges.Add(8); edges.Add(27);

            edges.Add(63); edges.Add(65); edges.Add(67);
            edges.Add(71); edges.Add(73); edges.Add(75);
            edges.Add(79); edges.Add(81); edges.Add(83);
            edges.Add(87); edges.Add(89); edges.Add(91);
            edges.Add(95); edges.Add(97); edges.Add(99);
            edges.Add(103); edges.Add(105); edges.Add(107);

            edgeLenght = 18;
        }
        else
        {   //OUTER WEDGE (W1..W6)
            corners.Add(1); corners.Add(3);
            corners.Add(43); corners.Add(51);

            edges.Add(0); edges.Add(12); edges.Add(5); edges.Add(17);
            edges.Add(21); edges.Add(8); edges.Add(25);
            edges.Add(30); edges.Add(10); edges.Add(34); edges.Add(4);
            edges.Add(32); edges.Add(1); edges.Add(29); edges.Add(2);
            edges.Add(18); edges.Add(7); edges.Add(14);

            edges.Add(35); edges.Add(37); edges.Add(39); edges.Add(41);
            edges.Add(45); edges.Add(47); edges.Add(49);
            edges.Add(53); edges.Add(55); edges.Add(57); edges.Add(59);
            edges.Add(61); edges.Add(63); edges.Add(65); edges.Add(67);
            edges.Add(69); edges.Add(71); edges.Add(73);

            /*border.Add(4);*/
            border.Add(32); border.Add(11); border.Add(29); border.Add(2); border.Add(18); border.Add(7); border.Add(14); //border.Add(0);
            /*border.Add(57); border.Add(59);*/
            border.Add(61); border.Add(63); border.Add(65); border.Add(67); border.Add(69); border.Add(71); border.Add(73);

            hardCorners.Add(0); hardCorners.Add(4);
            hardCorners.Add(35); hardCorners.Add(59);

            edgeLenght = 18;
        }
    }
    public void Highlight(int mod)
    {
        if (mod < 0) //-1
        {
            GetComponent<Renderer>().material = materials[0];
        }
        else    // +1
        {
            GetComponent<Renderer>().material = materials[1];
        }
    }
    public bool CheckAdjacentTile(int pos)
    {
        Ray ray = new Ray();
        RaycastHit hit;
        ray.origin = eye.transform.position;
        ray.direction = directions[pos];
        if (Physics.Raycast(ray, out hit, maxDist))
            return true;
        else
            return false;
    }
    public GameObject GetAdjacentNeighbor()
    {
        Ray ray = new Ray();
        RaycastHit hit;
        ray.origin = eye.transform.position;
        ray.direction = directions[0];
        if (Physics.Raycast(ray, out hit, maxDist))
        {
            if (hit.collider.gameObject.tag == "Tile")
            {
                return hit.collider.gameObject;
            }
            else
                return null;
        }
        else
            return null;
    }
    public void GetAllNeighbors(List<GameObject> list)
    {
        if (!checkedWedge)
        {
            if (list.Count > 0)
            { //Demas casos
                if (list[0].GetComponent<Wedge>().colorID == colorID)
                {
                    list.Add(this.gameObject);
                    checkedWedge = true;
                    for (int i = 0; i < neighbors.Length; i++)
                    {
                        if (neighbors[i] != null)
                            neighbors[i].GetComponent<Wedge>().GetAllNeighbors(list);
                    }
                }
            }
            else
            { //Primer caso
                list.Add(this.gameObject);
                checkedWedge = true;
                for (int i = 0; i < neighbors.Length; i++)
                {
                    if (neighbors[i] != null)
                        neighbors[i].GetComponent<Wedge>().GetAllNeighbors(list);
                }
            }
        }
    }
    public void SetMeshBuffer()
    {
        verticesBuffer.Clear();
        colorsBuffer.Clear();
        Vector3[] vert = this.gameObject.GetComponent<MeshFilter>().mesh.vertices;
        Color[] col = this.gameObject.GetComponent<MeshFilter>().mesh.colors;
        verticesBuffer = vert.ToList();
        colorsBuffer = col.ToList();

    }
    public void RestoreBufferedMesh()
    {
        if (verticesBuffer.Count > 0)
        {
            Debug.Log("ENTRA RESTORE " + this.gameObject.name);
            this.gameObject.GetComponent<MeshFilter>().mesh.vertices = verticesBuffer.ToArray();
            this.gameObject.GetComponent<MeshFilter>().mesh.colors = colorsBuffer.ToArray();
            this.gameObject.GetComponent<MeshFilter>().mesh.RecalculateNormals();
            this.gameObject.GetComponent<MeshFilter>().mesh.RecalculateBounds();
            //verticesBuffer.Clear();
            //colorsBuffer.Clear();
        }
    }
    public void SetMeshHeightAndColor()
    {
        // Definir Altura
        meshHeight = meshHeight * colorID;
        //Usando el color ID poner todos los vertices de TOP y de edgeRepetidos a la altura correspondiente (id * HEIGHT)        
        lastIndex = (centerWedge) ? 81 : 55; //81 para W0 (Top+EdgeExtra) (61+24)|| 55 para W1..W6 (Top+EdgeExtra) (35+20)
        int topEnd = (centerWedge) ? 61 : 35;
        Mesh mesh = this.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < topEnd; i++)
        {
            vertices[i] = new Vector3(vertices[i].x, vertices[i].y + meshHeight, vertices[i].z);
            colors[i] = GradientManager.GetColorById(colorID);
        }
        for (int i = edgeLenght; i < edges.Count; i++)
        {
            vertices[edges[i]] = new Vector3(vertices[edges[i]].x, vertices[edges[i]].y + meshHeight, vertices[edges[i]].z);
            colors[edges[i]] = GradientManager.GetColorById(colorID);
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }
    //private Color GetColorById()
    //{
    //    switch (colorID)
    //    {
    //        case 0:
    //            return Color.blue;                
    //        case 1:
    //            return Color.green;                
    //        case 2:
    //            return Color.red;                
    //        case 3:
    //            return Color.magenta;                
    //        case 4:
    //            return Color.yellow;                
    //    }
    //    return Color.black;
    //}
    //public void LevelMeshVertices()
    //{
    //    Wedge w2 = this.neighbors[3].GetComponent<Wedge>();
    //    Vector3[] vertices1 = this.GetComponent<MeshFilter>().mesh.vertices;
    //    Vector3[] vertices2 = this.neighbors[3].GetComponent<MeshFilter>().mesh.vertices;
    //    //Reseteo Buffer
    //    lastMeshVertices.Clear();
    //    w2.lastMeshVertices.Clear();        
    //    //Agrego Vertex Border al buffer
    //    foreach (int i in this.border)
    //    {
    //        this.lastMeshVertices.Add(vertices1[this.border[i]]);
    //        w2.lastMeshVertices.Add(vertices2[w2.border[i]]);
    //    }
    //    //Leveleo Mesh Border
    //    for (int i = 0; i < border.Count; i++)
    //    {
    //        for (int j = 0; j < w2.border.Count; j++)
    //        {
    //            if ((vertices1[this.border[i]].x == vertices2[w2.border[j]].x) && (vertices1[this.border[i]].z == vertices2[w2.border[j]].z))
    //            {
    //                float midPoint = (vertices1[this.border[i]].y + vertices2[w2.border[j]].y) / 2;
    //                vertices1[this.border[i]] = new Vector3(vertices1[this.border[i]].x, midPoint, vertices1[this.border[i]].z);
    //                vertices2[w2.border[j]] = new Vector3(vertices2[w2.border[j]].x, midPoint, vertices2[w2.border[j]].z);
    //            }
    //        }
    //    }
    //    this.GetComponent<MeshFilter>().mesh.vertices = vertices1;
    //    this.neighbors[3].GetComponent<MeshFilter>().mesh.vertices = vertices2;
    //    this.GetComponent<MeshFilter>().mesh.RecalculateNormals();
    //    this.neighbors[3].GetComponent<MeshFilter>().mesh.RecalculateNormals();
    //    //UpdateVertexColors();
    //}
    //public void RestoreMeshBorder()
    //{
    //    Wedge w2 = this.neighbors[3].GetComponent<Wedge>();
    //    Vector3[] vertices1 = this.GetComponent<MeshFilter>().mesh.vertices;
    //    Vector3[] vertices2 = this.neighbors[3].GetComponent<MeshFilter>().mesh.vertices;
    //    foreach (int i in this.border)
    //    {
    //        vertices1[this.border[i]] = this.lastMeshVertices[i];
    //        vertices2[w2.border[i]] = w2.lastMeshVertices[i];
    //    }
    //    lastMeshVertices.Clear();
    //    w2.lastMeshVertices.Clear();
    //    this.GetComponent<MeshFilter>().mesh.vertices = vertices1;
    //    this.neighbors[3].GetComponent<MeshFilter>().mesh.vertices = vertices2;
    //    this.GetComponent<MeshFilter>().mesh.RecalculateNormals();
    //    this.neighbors[3].GetComponent<MeshFilter>().mesh.RecalculateNormals();
    //    //UpdateVertexColors();
    //}
    public void SetColor(int id)
    {
        colorID = id;
        materials[0] = Resources.Load("Materials/MeshMaterial") as Material;
        materials[1] = Resources.Load("Materials/MeshMaterial1") as Material;
        this.gameObject.GetComponent<Renderer>().material = materials[0];

        //SETEA LA ALTURA DE LA MESH        
        //SETEA EL MATERIAL
        //switch (colorID)
        //{
        //    case 0:
        //        materials[0] = Resources.Load("Materials/RedWedgeMaterial") as Material;
        //        materials[1] = Resources.Load("Materials/RedWedgeMaterial 1") as Material;
        //        break;
        //    case 1:
        //        materials[0] = Resources.Load("Materials/GreenWedgeMaterial") as Material;
        //        materials[1] = Resources.Load("Materials/GreenWedgeMaterial 1") as Material;
        //        break;
        //    case 2:
        //        materials[0] = Resources.Load("Materials/BlueWedgeMaterial") as Material;
        //        materials[1] = Resources.Load("Materials/BlueWedgeMaterial 1") as Material;
        //        break;
        //    case 3:
        //        materials[0] = Resources.Load("Materials/MagentaWedgeMaterial") as Material;
        //        materials[1] = Resources.Load("Materials/MagentaWedgeMaterial 1") as Material;
        //        break;
        //    case 4:
        //        materials[0] = Resources.Load("Materials/YellowWedgeMaterial") as Material;
        //        materials[1] = Resources.Load("Materials/YellowWedgeMaterial 1") as Material;
        //        break;            
        //}
        //this.gameObject.GetComponent<Renderer>().material = materials[0];
    }

    //public void UpdateVertexColors()
    //{
    //    Mesh mesh = this.GetComponent<MeshFilter>().mesh;
    //    Vector3[] vertices = mesh.vertices;
    //    Color[] colors = new Color[mesh.vertices.Length];
    //    float height;
    //    for (int i = 0; i < colors.Length; i++)
    //    {
    //        height = Mathf.InverseLerp(minHeight,maxHeight,vertices[i].y);
    //        colors[i] = gradient.Evaluate(height);
    //    }
    //    mesh.colors = colors;
    //}

    private void HighlightOn()
    {
        if (checkedWedge)
            GetComponent<Renderer>().material = materials[1];
    }
    private void Refresh()
    {
        Ray ray = new Ray();
        RaycastHit hit;
        ray.origin = eye.transform.position;
        ray.direction = directions[0];
        if (Physics.Raycast(ray, out hit, maxDist))
        {
            if (hit.collider.gameObject.tag == "Tile")
            {
                neighbors[3] = hit.collider.gameObject;
            }
            else
            {
                neighbors[3] = null;
            }
        }
    }
    private void HighlightOff()
    {
        if (checkedWedge)
        {
            GetComponent<Renderer>().material = materials[0];
            checkedWedge = false;
        }
    }
    public void ToggleHighlight()
    {
        //if (highlight)
        //{
        //    //setear material 1
        //    GetComponent<Renderer>().material = materials[0];
        //    highlight = !highlight;
        //}
        //else
        //{
        //    //setear material 2
        //    GetComponent<Renderer>().material = materials[1];
        //    highlight = !highlight;
        //}
    }
    private void DrawRay(Ray ray)
    {
        Debug.DrawRay(ray.origin, ray.direction, Color.red, 15f);        
    }
    public void SetAdjacentNeighbor()
    {
        if (!centerWedge)
        {
            Ray ray = new Ray();
            RaycastHit hit;
            ray.origin = eye.transform.position;
            ray.direction = directions[0];
            if (Physics.Raycast(ray, out hit, maxDist))
            {
                if (hit.collider.gameObject.tag == "Tile")
                {
                    neighbors[3] = hit.collider.gameObject;                    
                }
                else
                {
                    neighbors[3] = null;
                }
            }
        }
    }
    public List<GameObject> GetNeighbors()
    {
        List<GameObject> list = new List<GameObject>();
        foreach (GameObject neighbor in neighbors)
        {
            if (neighbor.GetComponent<Wedge>().colorID == this.colorID && neighbor.GetComponent<Wedge>().checkedWedge == false)
            {
                list.Add(neighbor);
            }
        }
        return list;
    }
}
