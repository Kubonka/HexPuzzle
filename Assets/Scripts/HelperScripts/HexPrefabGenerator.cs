using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEditor;

public class HexPrefabGenerator : MonoBehaviour
{
    public float tileSize;
    private List<GameObject> tiles;
    

    void Start()
    {
        tiles = new List<GameObject>();
        //Instanciar Primero
        CrearHexTile(new Vector3(0, 0, 0));
        //Instanciar Siguientes 6
        for (int i = 0; i < 7; i++)
        {
            Vector3 position = GetPosition(i);
            CrearHexTile(position);
        }
        //AGREGAR DATOS AL "eye" de cada WEDGE y generar nueva Prefab
        CreateDirectionsSet(); //ATENTI CON ESTE
        //Testear
        //TestDirections();
    }
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.A))
        //{
        //    rotargo();            
        //}
        //if (Input.GetKeyDown(KeyCode.S))
        //{
        //    DrawRay();
        //}
        //testGO.transform.Rotate(new Vector3(0, angle * Time.deltaTime, 0));
    }
    private void CrearHexTile(Vector3 pos)
    {
        GameObject go;
        string stringPath = "Prefabs/HexTile";
        go = Instantiate(Resources.Load(stringPath), pos, Quaternion.identity) as GameObject;
        tiles.Add(go);
    }
    private Vector3 GetPosition(int i)
    {
        float angle = 60 * i;
        GameObject cursor = new GameObject();
        cursor.transform.position = new Vector3(0, 0, 0);
        cursor.transform.Rotate(new Vector3(0, angle, 0));
        cursor.transform.position += cursor.transform.forward * GetDistance();
        Vector3 position = cursor.transform.position;
        Destroy(cursor);
        return position;
    }
    private float GetDistance()
    {
        return 2 * Mathf.Sqrt(Mathf.Pow(tileSize,2)-Mathf.Pow(tileSize/2,2));
    }
    private void TestDirections()
    {        
        GameObject go1 = tiles[0];
        GameObject go2 = tiles[1];
        Vector3 pos1 = go1.GetComponent<HexTile>().wedges[1].GetComponent<Wedge>().eye.transform.position;
        Vector3 pos2 = go2.GetComponent<HexTile>().wedges[4].GetComponent<Wedge>().eye.transform.position;
        Vector3 newPos2 = new Vector3(pos2.x, 0, pos2.z);
        Vector3 dir = newPos2 - pos1;        
        dir.Normalize();
        go1.GetComponent<HexTile>().wedges[1].GetComponent<Wedge>().directions.Add(dir);
        //foreach (Vector3 dire in go1.GetComponent<HexTile>().wedges[1].GetComponent<Wedge>().directions)
        //{
        //    Debug.Log(dire.x + " ," + dire.y + ", " + dire.z);
        //}        
        
    }    
    private void CreateDirectionsSet()
    {
        List<Vector3> centerSet = new List<Vector3>();
        List<Vector3> edgeSet = new List<Vector3>();
        Vector3 dir;
        //EDGE SET
        Vector3 pos1 = tiles[0].GetComponent<HexTile>().wedges[1].GetComponent<Wedge>().eye.transform.position;
        Vector3 pos2 = tiles[1].GetComponent<HexTile>().wedges[4].GetComponent<Wedge>().eye.transform.position;        
        dir = GetDirectionVector(pos1,pos2);
        edgeSet.Add(dir);
        pos2 = tiles[0].GetComponent<HexTile>().wedges[2].GetComponent<Wedge>().eye.transform.position;
        dir = GetDirectionVector(pos1, pos2);
        edgeSet.Add(dir);
        pos2 = tiles[0].GetComponent<HexTile>().wedges[0].GetComponent<Wedge>().eye.transform.position;
        dir = GetDirectionVector(pos1, pos2);
        edgeSet.Add(dir);
        pos2 = tiles[0].GetComponent<HexTile>().wedges[6].GetComponent<Wedge>().eye.transform.position;
        dir = GetDirectionVector(pos1, pos2);
        edgeSet.Add(dir);
        //CENTER SET
        pos1 = tiles[0].GetComponent<HexTile>().wedges[0].GetComponent<Wedge>().eye.transform.position;
        for (int i = 1; i < 7; i++)
        {
            pos2 = tiles[0].GetComponent<HexTile>().wedges[i].GetComponent<Wedge>().eye.transform.position;
            dir = GetDirectionVector(pos1, pos2);
            centerSet.Add(dir);
        }
        //INSERTAR CADA SET EN CADA WEDGE
        for (int i = 0; i < 7; i++)
        {
            if (i > 0)
            { //EDGE SET
                tiles[0].GetComponent<HexTile>().wedges[i].GetComponent<Wedge>().directions = edgeSet;
                edgeSet = RotateEdgeSet(edgeSet);
            }
            else
            { //CENTER SET
                tiles[0].GetComponent<HexTile>().wedges[i].GetComponent<Wedge>().directions = centerSet;
            }
        }
        //GUARDAR PREFAB 
        SavePrefab(tiles[0]);
    }
    private List<Vector3> RotateEdgeSet(List<Vector3> set)
    {
        List<Vector3> newSet = new List<Vector3>();
        //vb = Quaternion.AngleAxis(60, Vector3.up) * vb;
        //vb.Normalize();
        
        for (int i = 0; i < 4 ; i++)
        {
            Vector3 aux = set[i];
            aux = Quaternion.AngleAxis(60, Vector3.up) * aux;
            newSet.Add(aux);
        }
        return newSet;
    }
    private Vector3 GetDirectionVector(Vector3 v1, Vector3 v2)
    {
        Vector3 newPos = new Vector3(v2.x, 0, v2.z);

        return (newPos - v1).normalized;
    }
    private void SavePrefab(GameObject hexTile)
    {
        string localPath = "Assets/Resources/Prefabs/" + hexTile.name + ".prefab";
        localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);
        PrefabUtility.SaveAsPrefabAssetAndConnect(hexTile, localPath, InteractionMode.UserAction);
    }
    private void DrawRay()
    {
        //Debug.DrawRay(new Vector3(0, 0, 0), new Vector3(0, 10, 0), Color.cyan, 5f);
        Vector3 dir = tiles[0].GetComponent<HexTile>().wedges[1].GetComponent<Wedge>().directions[0];
        Debug.DrawRay(tiles[0].GetComponent<HexTile>().wedges[1].GetComponent<Wedge>().eye.transform.position, dir * 10,Color.cyan, 5f);
        //testGO.GetComponent<HexTile>().wedges[1].GetComponent<Wedge>().directions[0]
    }
    private void rotargo()
    {
        tiles[0].GetComponent<HexTile>().RotateHexTile(1);
    }

}
