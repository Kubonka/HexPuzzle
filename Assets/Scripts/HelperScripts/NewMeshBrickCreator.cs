using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using MiscUtil.Collections.Extensions;
using UnityEditor;

public class NewMeshBrickCreator : MonoBehaviour
{
    public float sideHeight;
    public GameObject wedge;
    private List<int> corners = new List<int>();
    private List<int> edges = new List<int>();
    private List<int> border = new List<int>();
    private bool centerWedge;
    void Start()
    {
        //ShowVertexData();
        ShowVertexData2();
        centerWedge = false;
        corners = new List<int>();
        edges = new List<int>();
        border = new List<int>();

        //SetUpMeshData();
        //centerWedge = true;           //TOGGLE ACA <- 1
        //CreateCenterWedge();          //TOGGLE ACA <- 1
        //CreateOuterWedge();       //TOGGLE ACA <- 2
    }

    private void CreateCenterWedge()
    {
        Mesh mesh = wedge.GetComponent<MeshFilter>().mesh;        
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        triangles.AddRange(mesh.triangles.ToList());
        vertices.AddRange(mesh.vertices.ToList());
        int index1 = vertices.Count;
        //SIDE
        for (int i = 0; i < edges.Count; i++)
        {
            vertices.Add(vertices[edges[i]]);
            vertices.Add(new Vector3(vertices[edges[i]].x, vertices[edges[i]].y - sideHeight, vertices[edges[i]].z));
        }
        int index2 = vertices.Count;        
        //BOT
        for (int i = 0; i < corners.Count; i++)
        {
            vertices.Add(new Vector3(vertices[corners[i]].x, vertices[corners[i]].y - sideHeight, vertices[corners[i]].z));
        }
        vertices.Add(new Vector3(vertices[0].x,vertices[0].y - sideHeight,vertices[0].z)); //Center Vertex
        //Mesh Triangles
        CreateSideTriangles(index1, index2, triangles);        
        CreateCenterBottomTriangles(index2,vertices.Count-1, triangles);
        //Preparar NEW MESH para generar asset        
        GenerateMeshAsset(vertices,triangles);
    }
    private void CreateOuterWedge()
    {
        Mesh mesh = wedge.GetComponent<MeshFilter>().mesh;
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        triangles.AddRange(mesh.triangles.ToList());
        vertices.AddRange(mesh.vertices.ToList());
        int index1 = vertices.Count;
        //SIDE
        for (int i = 0; i < edges.Count; i++)
        {
            vertices.Add(vertices[edges[i]]);
            vertices.Add(new Vector3(vertices[edges[i]].x, vertices[edges[i]].y - sideHeight, vertices[edges[i]].z));
        }
        int index2 = vertices.Count;
        //BOT
        for (int i = 0; i < corners.Count; i++)
        {
            vertices.Add(new Vector3(vertices[corners[i]].x, vertices[corners[i]].y - sideHeight, vertices[corners[i]].z));
        }
        vertices.Add(new Vector3(vertices[26].x, vertices[26].y - sideHeight, vertices[26].z)); //Center Vertex
        //Mesh Triangles
        CreateSideTriangles(index1, index2, triangles);
        CreateOuterBottomTriangles(index2, vertices.Count - 1, triangles);
        //Preparar NEW MESH para generar asset        
        GenerateMeshAsset(vertices, triangles);
    }
    private void GenerateMeshAsset(List<Vector3> vertices, List<int> triangles)
    {
        GameObject go = new GameObject("W66", typeof(MeshFilter), typeof(MeshRenderer));
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        go.GetComponent<MeshFilter>().mesh = mesh;
        CreatePrefabAsset(go);
    }
    
    private void CreateSideTriangles(int start, int end, List<int> triangles)
    {
        for (int i = start;  i < end; i++)
        {
            if (i == end - 2)
            {
                triangles.Add(i); triangles.Add(i + 1); triangles.Add(start+1);   //Primer SideTriangle
                triangles.Add(start+1); triangles.Add(start); triangles.Add(i);       //Segundo SideTriangle
            }
            else
            {
                triangles.Add(i); triangles.Add(i + 1); triangles.Add(i + 3);   //Primer SideTriangle
                triangles.Add(i + 3); triangles.Add(i + 2); triangles.Add(i);       //Segundo SideTriangle
            }
            i++;
        }
    }
    private void CreateOuterBottomTriangles(int start, int end, List<int> triangles)
    {
        for (int i = start; i < end; i++)
        {
            if (i == end - 1)
            {
                triangles.Add(i); triangles.Add(end); triangles.Add(start);
            }
            else
            {
                triangles.Add(i); triangles.Add(end); triangles.Add(i+1);
            }
        }
    }
    private void CreateCenterBottomTriangles(int start,int end, List<int> triangles)
    {
        for (int i = start; i < end; i++)
        {
            if (i == end - 1)
            {
                triangles.Add(i); triangles.Add(start); triangles.Add(end);
            }
            else
            {
                triangles.Add(i); triangles.Add(i+1); triangles.Add(end);
            }
        }
    }

    private void SetUpMeshData()
    {
        
        if (centerWedge)
        {   //CENTER WEDGE (W0)
            corners.Add(1); corners.Add(2); corners.Add(3); corners.Add(4); corners.Add(5); corners.Add(6);
            edges.Add(1); edges.Add(57); edges.Add(18); edges.Add(60); edges.Add(6);
            edges.Add(51); edges.Add(17); edges.Add(55); edges.Add(5);
            edges.Add(44); edges.Add(15); edges.Add(48); edges.Add(4);
            edges.Add(37); edges.Add(13); edges.Add(41); edges.Add(3);
            edges.Add(30); edges.Add(11); edges.Add(34); edges.Add(2);
            edges.Add(22); edges.Add(8); edges.Add(27);
        }
        else
        {   //OUTER WEDGE (W1..W6)
            corners.Add(0); corners.Add(1); corners.Add(3); corners.Add(4);
            edges.Add(0); edges.Add(12); edges.Add(5); edges.Add(17); edges.Add(1);
            edges.Add(21); edges.Add(8); edges.Add(25); edges.Add(3);
            edges.Add(30); edges.Add(10); edges.Add(34); edges.Add(4);
            edges.Add(32); edges.Add(11); edges.Add(29); edges.Add(2);
            edges.Add(18); edges.Add(7); edges.Add(14);
            border.Add(32); border.Add(11); border.Add(29); border.Add(2); border.Add(18); border.Add(7); border.Add(14);
        }
    }
    private void ShowVertexData()
    {
        string path = "Prefabs/PopUpText";
        for (int i = 0; i < wedge.GetComponent<MeshFilter>().mesh.vertices.Length; i++)
        {            
            GameObject popUp = Instantiate(Resources.Load(path), wedge.GetComponent<MeshFilter>().mesh.vertices[i], Quaternion.AngleAxis(180f,Vector3.up)) as GameObject;
            popUp.GetComponentInChildren<Canvas>().worldCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
            popUp.GetComponentInChildren<Canvas>().transform.GetChild(0).GetComponent<TextMeshProUGUI>().fontSize = 0.1f;
            popUp.GetComponentInChildren<Canvas>().transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = i.ToString();
        }
    }
    private void ShowVertexData2()
    {
        string path = "Prefabs/PopUpText";
        for (int i = 35; i < 75; i++)
        {
            GameObject popUp = Instantiate(Resources.Load(path), wedge.GetComponent<MeshFilter>().mesh.vertices[i], Quaternion.AngleAxis(180f, Vector3.up)) as GameObject;
            popUp.GetComponentInChildren<Canvas>().worldCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
            popUp.GetComponentInChildren<Canvas>().transform.GetChild(0).GetComponent<TextMeshProUGUI>().fontSize = 0.1f;
            popUp.GetComponentInChildren<Canvas>().transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = i.ToString();
        }
    }
    private void CreatePrefabAsset(GameObject go)
    {    
        string meshPath = "Assets/Meshes/" + go.name + ".asset";
        AssetDatabase.CreateAsset(go.GetComponent<MeshFilter>().mesh, meshPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();        
    }
}
