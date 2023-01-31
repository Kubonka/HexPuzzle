using System;
using System.Collections;
using System.Collections.Generic;
using TheTide.utils;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


/*
 *  Object originalPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject));
    GameObject objSource = PrefabUtility.InstantiatePrefab(originalPrefab) as GameObject;    
    GameObject prefabVariant = PrefabUtility.SaveAsPrefabAsset(objSource, localPath);
 * */
public class HexTilePrefabGenerator : MonoBehaviour
{
    public float tileSize;
    private GameObject hexTile;
    void Start()
    {
        hexTile = new GameObject("BlankHexTilePRE", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
        hexTile.transform.position = new Vector3(0, 0, 0);
        //GENERAR GAME OBJECT Y MESH
        //GenerateBlankHexTile();
        GenerateHexTile();
        SavePrefab();
    }

    private void GenerateHexTile()
    {
        List<Vector3> corner1 = new List<Vector3>();
        List<Vector3> corner2 = new List<Vector3>();
        //CREO inner
        corner1.Add(transform.position);
        for (int i = 0; i < 6; i++)
        {
            corner1.Add(HexCorner(transform.position, tileSize/3, i));
        }
        //RE ORDENO inner
        Vector3[] inner = new Vector3[corner1.Count];
        inner[0] = corner1[0];
        for (int i = 1; i <= 6; i++)
        {
            inner[i] = corner1[corner1.Count - i];
        }
        //CREO outer
        for (int i = 0; i < 6; i++)
        {
            corner2.Add(HexCorner(transform.position, tileSize, i));
        }
        //RE ORFENO outer
        Vector3[] outer = new Vector3[corner2.Count];
        for (int i = 0; i < 6; i++)
        {
            outer[i] = corner2[corner2.Count - 1 - i];
        }
        GenerateWedges(inner, outer);
    }
    private void GenerateWedges(Vector3[] inner, Vector3[] outer)
    {
        float elevation = 10f;
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
                //wedgeGO.GetComponent<MeshFilter>().mesh = GenerateWedgeMesh(combinedVertices, i);
                //wedgeGO.GetComponent<MeshCollider>().sharedMesh = wedgeGO.GetComponent<MeshFilter>().mesh;
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
                //wedgeGO.GetComponent<MeshFilter>().mesh = GenerateCenterWedgeMesh(inner);
                //wedgeGO.GetComponent<MeshCollider>().sharedMesh = wedgeGO.GetComponent<MeshFilter>().mesh;
                //EYE
                eyeGO.transform.parent = wedgeGO.transform;
                eyeGO.name = "eye" + i/2;
                eyeGO.transform.position = new Vector3(0, elevation, 0);
            }
            i++;
            //go.AddComponent<SerializeMesh>();            
            //CreatePrefabAsset(wedgeGO);        //TOGGLE ACA !!
        }
    }
    private Vector3 GetEyePosition(GameObject wedge,int wedgeNumber)
    {
        float angle = 60 * wedgeNumber - 60;
        GameObject cursor = new GameObject();
        cursor.transform.position = new Vector3(0, 0, 0);
        cursor.transform.Rotate(new Vector3(0, angle, 0));
        cursor.transform.position += cursor.transform.forward * (tileSize * 1.2f); //tileSize / 2; <- OLD
        return cursor.transform.position;
    }
    private Mesh GenerateCenterWedgeMesh(Vector3[] inner)
    {
        Mesh mesh = new Mesh();
        List<int> triangles = new List<int>();
        int center = 0;

        for (int i = 1; i < inner.Length; i++)
        {
            triangles.Add(i);
            if (i == 6)
                triangles.Add(1);
            else
                triangles.Add(i + 1);
            triangles.Add(center);
        }
        mesh.vertices = inner;
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        return mesh;
    }
    private Mesh GenerateWedgeMesh(List<Vector3> combined, int pos) //pos arranca en 1
    {
        Mesh mesh = new Mesh();
        List<int> triangles = new List<int>();
        List<Vector3> vertices = new List<Vector3>();
        if (pos == 12)
        {
            vertices.Add(combined[pos - 2]);
            vertices.Add(combined[pos - 1]);
            vertices.Add(combined[0]);
            vertices.Add(combined[1]);
        }
        else
        {
            vertices.Add(combined[pos - 2]);
            vertices.Add(combined[pos - 1]);
            vertices.Add(combined[pos    ]);
            vertices.Add(combined[pos + 1]);
        }
        triangles.Add(0);
        triangles.Add(2);
        triangles.Add(1);
        triangles.Add(2);
        triangles.Add(3);
        triangles.Add(1);

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        return mesh;
    }
    private List<Vector3> GetCombinedVertices(Vector3[] inner, Vector3[] outer)
    {
        List<Vector3> vertices = new List<Vector3>();  
        Vector3[] aux = new Vector3[inner.Length + outer.Length];
        int i = 0;
        while (i < inner.Length - 1)
        {
            aux[i] = inner[i + 1];
            i++;
        }
        int j = 0;
        while (j < outer.Length)
        {
            aux[i] = outer[j];
            j++;
            i++;
        }
        vertices = ReArrangeVertices(aux);
        return vertices;
    }
    private List<Vector3> ReArrangeVertices(Vector3[] aux)
    {
        List<Vector3> vertices = new List<Vector3>();
        vertices.Add(aux[9]);
        vertices.Add(aux[3]);
        vertices.Add(aux[10]);
        vertices.Add(aux[4]);
        vertices.Add(aux[11]);
        vertices.Add(aux[5]);
        vertices.Add(aux[6]);
        vertices.Add(aux[0]);
        vertices.Add(aux[7]);
        vertices.Add(aux[1]);
        vertices.Add(aux[8]);
        vertices.Add(aux[2]);
        return vertices;
    }
    private Vector3 GetWedgePosition(Vector3[] inner,Vector3[] outer,int pos)
    {
        //CALCULA PROMEDIO DE CADA COMPONENTE 
        Vector3 i, j, k;
        i = outer[pos - 1];
        j = outer[pos];
        k = inner[0];
        return new Vector3((i.x+j.x+k.x)/3, (i.y+j.y+k.y)/3, (i.z+j.z+k.z)/3);
    }
    private Vector3 HexCorner(Vector3 center,float size, int i)
    {
        /*
         function flat_hex_corner(center, size, i):
        var angle_deg = 60 * i
        var angle_rad = PI / 180 * angle_deg
        return Point(center.x + size * cos(angle_rad), center.y + size * sin(angle_rad))
         */

        float angle = 60 * i;
        float angle_rad = Mathf.PI / 180 * angle;
        return new Vector3(center.x + size * Mathf.Cos(angle_rad), 0, center.z + size * Mathf.Sin(angle_rad));
    }
    private void SavePrefab()
    {
        string localPath = "Assets/Resources/Prefabs/" + hexTile.name + ".prefab";
        localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);
        PrefabUtility.SaveAsPrefabAssetAndConnect(hexTile, localPath,InteractionMode.UserAction);
    }
    private void CreatePrefabAsset(GameObject go)
    {
        // MANEJO DEL PREFAB Y DE LOS ASSETS
        //string colliderPath = "Assets/Colliders/PaddleCollider.asset";
        string meshPath = "Assets/Meshes/" + go.name + ".asset";
        AssetDatabase.CreateAsset(go.GetComponent<MeshFilter>().mesh, meshPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();        
        /*AssetDatabase.CreateAsset(gameObject.GetComponent<MeshFilter>().mesh, colliderPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();*/
        /*string localPath = "Assets/Meshes/PaddleMesh";
        PrefabUtility.SaveAsPrefabAsset(gameObject, localPath);*/
    }
    // BLANK MESH AREA
    private void GenerateMesh(List<Vector3> corners)
    {        
        Mesh mesh = new Mesh();
        hexTile.GetComponent<MeshFilter>().mesh = mesh;
        MeshCollider collider = new MeshCollider();        
        List<int> triangles = new List<int>(); // !!
        int center = 0;

        for (int i = 1; i < corners.Count; i++)
        {
            triangles.Add(i);
            if (i == 6)
                triangles.Add(1);
            else
                triangles.Add(i + 1);
            triangles.Add(center);
        }
        Vector3[] arr = new Vector3[corners.Count];
        arr[0] = corners[0];
        for (int i = 1; i <= 6; i++)
        {
            arr[i] = corners[corners.Count - i];
        }
        mesh.vertices = arr;
        mesh.triangles = triangles.ToArray();
        
        hexTile.GetComponent<MeshCollider>().sharedMesh = mesh;
    }
    private void GenerateBlankHexTile()
    {
        List<Vector3> corners = new List<Vector3>();
        corners.Add(transform.position);
        for (int i = 0; i < 6; i++)
        {
            corners.Add(HexCorner(transform.position, tileSize, i));
        }
        GenerateMesh(corners);
        //CreatePrefabAsset();
    }
}
