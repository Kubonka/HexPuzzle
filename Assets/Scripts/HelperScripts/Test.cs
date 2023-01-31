using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    Material[] materials;
    private Mesh mesh;
    public GameObject testGO;
    float angle = 60f;
    private Vector3[] arr;
    private int index;
    private float minHeight;
    private float maxHeight;
    public Gradient gradient;
    // Start is called before the first frame update
    void Start()
    {
        //mesh = testGO.GetComponent<MeshFilter>().mesh;
        ////testear();
        //List<Vector3> arr = new List<Vector3>();
        //for (int i = 0; i < 6; i++)
        //{
        //    arr.Add(Testear1(new Vector3(0, 0, 0), 1,i));
        //}        
    }

    private void Testear2()
    {
        
    }

    private Vector3 Testear1(Vector3 center, float size, int i)
    {
        float angle = 60 * i;
        float angle_rad = Mathf.PI / 180 * angle;
        return new Vector3(center.x + size * Mathf.Cos(angle_rad), 0, center.z + size * Mathf.Sin(angle_rad));
    }
    private void testear()
    {
        Vector3[] vertices = mesh.vertices;
        Color[] colors = new Color[vertices.Length];
        foreach (Vector3 v in vertices)
        {
            if (v.y < minHeight)
                minHeight = v.y;
            if (v.y > maxHeight)
                maxHeight = v.y;
        }
        float height;
        for (int i = 0; i < colors.Length; i++)
        {
            height = Mathf.InverseLerp(minHeight, maxHeight, vertices[i].y);
            colors[i] = gradient.Evaluate(height);
        }
        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.RecalculateNormals();
    }

    // Update is called once per frame
    void Update()
    {
        RefreshMesh();
        if (Input.GetKeyDown(KeyCode.K))
        {
            // testGO.transform.Rotate(new Vector3(0, angle, 0));
            //GetComponent<Renderer>().material = materials[1];
            index--;
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            DrawRayFromVector(index);
            index++;
        }
        //testGO.transform.Rotate(new Vector3(0, angle * Time.deltaTime, 0));      
    }
    private void RefreshMesh()
    {
        
    }
    private void DrawRayFromVector(int index)
    {
        //Vector3 origin = testGO.GetComponent<MeshFilter>().mesh.vertices[index];
        //Vector3 direction = Vector3.up;
        //Debug.DrawLine(origin, origin + Vector3.up * 3, Color.red,2f);
        Debug.Log(index);
        HexTile hexTile = GameObject.Find("HexTile 1").GetComponent<HexTile>();
        Wedge wedge = hexTile.wedges[1].GetComponent<Wedge>();
        Vector3 origin = hexTile.wedges[1].GetComponent<MeshFilter>().mesh.vertices[wedge.hardCorners[index]];
        Vector3 direction = Vector3.up;
        Debug.DrawLine(origin, origin + Vector3.up * 4, Color.cyan, 2f);
        Debug.Log(hexTile.wedges[1].GetComponent<MeshFilter>().mesh.vertices[wedge.hardCorners[index]]);
    }
    private void TestMaterial(int mod)
    {
        switch (mod)
        {
            case 0:
                materials[0] = Resources.Load("Materials/RedWedgeMaterial") as Material;
                materials[1] = Resources.Load("Materials/BlueWedgeMaterial") as Material;
                break;
            case 1:
                materials[0] = Resources.Load("Materials/YellowWedgeMaterial") as Material;
                materials[1] = Resources.Load("Materials/GreenWedgeMaterial") as Material;
                break;
        }
        GetComponent<Renderer>().material = materials[0];
    }
    private void DrawRay()
    {
        Vector3 origin = new Vector3(0, 10, 0);
        Vector3 direction = new Vector3(0, -1, 0);
        Ray ray = new Ray();
        ray.origin = origin;
        ray.direction = direction;
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 30))
        {
            Debug.Log("ENTRA");
        }
        else
        {
            Debug.Log("NO ENTRA");
        }
    }
}
