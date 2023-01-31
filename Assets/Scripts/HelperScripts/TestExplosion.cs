using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

public class TestExplosion : MonoBehaviour
{
    public Mesh mesh;
    private string path;
    private GameObject go;

    private MeshFilter newMesh;
    private void Start()
    {
        //path = "Prefabs/SpawnMiniHexExplosion";
        go = new GameObject("meshy", typeof(MeshRenderer), typeof(MeshFilter));        
        MeshHelper.Subdivide(mesh,2);
        go.GetComponent<MeshFilter>().mesh = mesh;
        SavePrefabAsset(go.GetComponent<MeshFilter>().mesh);
    }
    private void SavePrefabAsset(Mesh newMesh)
    {
        string name = "meshSubd1";
        string meshPath = "Assets/Meshes/" + name + ".asset";
        AssetDatabase.CreateAsset(newMesh, meshPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            this.transform.rotation *= Quaternion.AngleAxis(10, Vector3.right);
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            Destroy(go);
        }
    }
    private void Explode()
    {
        go = Instantiate(Resources.Load(path), new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
        go.GetComponent<ParticleSystem>().Play();
    }
}
