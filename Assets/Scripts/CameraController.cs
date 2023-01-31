using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;

public class CameraController : MonoBehaviour
{
    private Timer timer;
    public float minZoom;
    public float maxZoom;
    public float zoomSpeed;
    public float rotSpeed;
    public float moveSpeed;
    public GameObject pivot;
    public Camera cam;
    public GameObject cameraBase;
    
    void Start()
    {
        //Ajustar camara
        cam.transform.LookAt(pivot.transform);
        
    }

    
    void Update()
    {
        
        //cameraBase movement
        cameraBase.transform.position = new Vector3(cam.transform.position.x, 0, cam.transform.position.z);

                    //ROTACION
        if (Input.GetKey(KeyCode.Z))
        {
            RotateCamera(1);
        }
        else if (Input.GetKey(KeyCode.C))
        {
            RotateCamera(-1);            
        }

                    //ZOOM
        if (Input.GetKey(KeyCode.R))
        {
            //ZOOM IN
            ZoomCamera(1);
        }
        else if (Input.GetKey(KeyCode.F))
        {
            //ZOOM OUT
            ZoomCamera(-1);
        }

                    //MOVIMIENTO
        if (Input.GetKey(KeyCode.W))
        {
            //MOVE FORWARD
            MoveForward(1);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            //MOVE RIGHT
            MoveSide(1);            
        }
        else if (Input.GetKey(KeyCode.S))
        {
            //MOVE BACK
            MoveForward(-1);
        }
        else if (Input.GetKey(KeyCode.A))
        {
            //MOVE LEFT
            MoveSide(-1);            
        }
    }

    private void RotateCamera(int direction)
    {
        pivot.transform.Rotate(Vector3.up, Time.deltaTime * rotSpeed * direction);
    }
    private void ZoomCamera(int direction)
    {
        //si la distancia con el pivot es menor que minZoom no hacer nada
        //si la distancia con el pivot es mayor que maxZoom no hacer nada
        float distance = GetZoomDistance(cam.transform.position, pivot.transform.position);
        if (direction > 0 && distance > minZoom)
            cam.transform.position = cam.transform.position + (cam.transform.forward * Time.deltaTime * direction * zoomSpeed);
        if ( direction < 0 && distance < maxZoom )
            cam.transform.position = cam.transform.position + (cam.transform.forward * Time.deltaTime * direction * zoomSpeed);
    }
    private void MoveForward(int direction)
    {
        pivot.transform.position += pivot.transform.forward * Time.deltaTime * direction * moveSpeed;
    }
    private void MoveSide(int direction)
    {
        pivot.transform.position += pivot.transform.right * Time.deltaTime * direction * moveSpeed;
    }
    private float GetZoomDistance(Vector3 c, Vector3 p)
    {
        return Vector3.Distance(c, p);
    }
}
