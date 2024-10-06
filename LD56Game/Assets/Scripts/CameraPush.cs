using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraPush : MonoBehaviour
{
    Camera camera;
    // Start is called before the first frame update
    void Awake()
    {
        camera = GetComponent<Camera>();
    }


    public void Push()
    {

    }

    public void Frame(Transform target, float size = 5)
    {

    }

}
