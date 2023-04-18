using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    public Camera cam;

    public float smoothTime = 0.3f;

    private Vector3 velocity = Vector3.zero;

    private Vector3 tempPos = Vector3.zero;

    // Use this for initialization
    void Start()
    {
        tempPos = cam.transform.position;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetTargetPos(float posX, float posY)
    {
        tempPos.x = posX;
        tempPos.y = posY;

        cam.transform.position = Vector3.SmoothDamp(cam.transform.position, tempPos, ref velocity, smoothTime);
    }
}
