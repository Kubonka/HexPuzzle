using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class HexTileBrickGenerator : MonoBehaviour
{
    public Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    public float distance;
    void Start()
    {        
        //GenerateOuterWedge();
        GenerateCenterWedge();
    }
    private void GenerateOuterWedge() //outer
    {
        //Levantar los vertices y ver como estan
        //Crear un nuevo array de vertices y replicar el array pero -1 en coord Y
        //Generar los triangulos de los costados sin perder los triangulos anteriores  (TAL VEZ NO)
        Vector3[] vertices1 = mesh.vertices;
        Vector3[] vertices2 = new Vector3[vertices1.Length];
        for (int i = 0; i < vertices1.Length; i++)
        {
            vertices2[i] = new Vector3(vertices1[i].x, - distance, vertices1[i].z);
        }
        //Reordenar
        int j = 0;
        vertices = new Vector3[vertices1.Length * 2];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = vertices1[j];
            vertices[i + 1] = vertices2[j];
            i++;
            j++;
        }
        GenerateOuterTriangles();
        GenerateMesh();
    }
    private void GenerateOuterTriangles()
    {
        List<int> tri = new List<int>();
        //TOP
        tri.Add(0); tri.Add(4); tri.Add(2); tri.Add(4); tri.Add(6); tri.Add(2);
        //BOT
        tri.Add(1); tri.Add(3); tri.Add(5); tri.Add(3); tri.Add(7); tri.Add(5);
        //SIDE
        tri.Add(0); tri.Add(2); tri.Add(1); tri.Add(1); tri.Add(2); tri.Add(3);
        tri.Add(4); tri.Add(0); tri.Add(5); tri.Add(5); tri.Add(0); tri.Add(1);
        tri.Add(6); tri.Add(4); tri.Add(7); tri.Add(7); tri.Add(4); tri.Add(5);
        tri.Add(2); tri.Add(6); tri.Add(3); tri.Add(3); tri.Add(6); tri.Add(7);
        triangles = new int[tri.Count];
        triangles = tri.ToArray();
    }
    private void GenerateMesh()
    {
        Mesh newMesh = new Mesh();
        newMesh.vertices = vertices;
        newMesh.triangles = triangles;

        //asd
        Vector2[] uvs = new Vector2[newMesh.vertices.Length];

        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(newMesh.vertices[i].x, newMesh.vertices[i].z);
        }
        newMesh.uv = uvs;
        //asd
        newMesh.RecalculateNormals();
        newMesh.RecalculateTangents();
        newMesh.RecalculateBounds();
        SavePrefabAsset(newMesh);
        
    }
    private void SavePrefabAsset(Mesh newMesh)
    {
        string name = "Wedge00";
        string meshPath = "Assets/Meshes/" + name + ".asset";
        AssetDatabase.CreateAsset(newMesh, meshPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    // CENTER WEDGE PART
    private void GenerateCenterWedge()
    {
        Vector3[] vertices1 = mesh.vertices;
        Vector3[] vertices2 = new Vector3[vertices1.Length];
        //for (int i = 0; i < vertices1.Length; i++)
        //{
        //    Debug.Log(vertices1[i]);
        //}
        // asd asd asd 
        for (int i = 0; i < vertices1.Length; i++)
        {
            vertices2[i] = new Vector3(vertices1[i].x, -distance, vertices1[i].z);
        }
        int j = 0;
        vertices = new Vector3[vertices1.Length * 2];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = vertices1[j];
            vertices[i + 1] = vertices2[j];
            i++;
            j++;
        }
        GenerateCenterTriangles();
        GenerateMesh();
    }
    private void GenerateCenterTriangles()
    {
        List<int> tri = new List<int>();
        //TOP 
        for (int i = 2; i <= 12; i++)
        {
            if (i == 12)
            {
                tri.Add(0); tri.Add(i); tri.Add(2);
            }
            else
            {
                tri.Add(0); tri.Add(i); tri.Add(i + 2);
            }
            i++;
        }
        //BOT
        for (int i = 2; i <= 12; i++)
        {
            if (i == 12)
            {
                tri.Add(1); tri.Add(3); tri.Add(i+1);
            }
            else
            {
                tri.Add(1); tri.Add(i + 3); tri.Add(i + 1);
            }
            i++;
        }
        //SIDE
        for (int i = 2; i <= 12; i++)
        {
            if (i == 12)
            {
                tri.Add(i); tri.Add(i+1); tri.Add(3);
                tri.Add(3); tri.Add(2); tri.Add(i);
            }
            else
            {
                tri.Add(i); tri.Add(i + 1); tri.Add(i+3);
                tri.Add(i+3); tri.Add(i + 2); tri.Add(i);
            }
            i++;
        }
        triangles = new int[tri.Count];
        triangles = tri.ToArray();
    }

    
    /*
     * private void GenerateWedges(Vector3[] inner, Vector3[] outer)
    {
        float elevation = 1f;
        List<GameObject> wedgesList = new List<GameObject>();
        List<Vector3> combinedVertices = GetCombinedVertices(inner, outer);

        for (int i = 0; i < 14; i++)
        {
            wedgesList.Add(new GameObject("Wedge" + i / 2, typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider)));
            GameObject wedgeGO = wedgesList[i / 2];
            GameObject eyeGO = new GameObject();
            wedgeGO.transform.parent = hexTile.transform;
            //CALCULA CENTRO DE CADA WEDGE y GENERA MESH
            if (i > 0)
            {
                //MESH
                Vector3 newPos = new Vector3(0, 0, 0);
                wedgeGO.transform.position = newPos;
                wedgeGO.GetComponent<MeshFilter>().mesh = GenerateWedgeMesh(combinedVertices, i);
                wedgeGO.GetComponent<MeshCollider>().sharedMesh = wedgeGO.GetComponent<MeshFilter>().mesh;
                //EYE
                eyeGO.transform.parent = wedgeGO.transform;
                eyeGO.name = "eye" + i/2;
                Vector3 pos = GetEyePosition(wedgeGO, i/2);
                eyeGO.transform.position = new Vector3(pos.x, elevation, pos.z);
            }
            else
            {
                //MESH
                wedgeGO.transform.position = this.transform.position; //CORAZON
                wedgeGO.GetComponent<MeshFilter>().mesh = GenerateCenterWedgeMesh(inner);
                wedgeGO.GetComponent<MeshCollider>().sharedMesh = wedgeGO.GetComponent<MeshFilter>().mesh;
                //EYE
                eyeGO.transform.parent = wedgeGO.transform;
                eyeGO.name = "eye" + i/2;
                eyeGO.transform.position = new Vector3(0, elevation, 0);
            }
            i++;
            //go.AddComponent<SerializeMesh>();            
            CreatePrefabAsset(wedgeGO);
        }
    }
     */
}
