using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Interaction : MonoBehaviour
{
    Vector3 touchStart;
    public Transform target;
    public float distance = 20.0f;
    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;
    public float distanceMin = .5f;
    public float distanceMax = 15f;
    public float zoomOutMin = 2;
    public float zoomoutMax = 43.5369f;
    public Vector3 delta = Vector3.zero;
    private Vector3 lastPos = Vector3.zero;
    float x = 0.0f;
    float y = 0.0f;
    GameObject[] array;
    private void Awake()
    {
        Input.multiTouchEnabled = true;
    }
    void Update()
    {
        // record first touch position
        if (Input.touchCount > 0)
        {
            Debug.Log("One touch");
            touchStart = Input.GetTouch(0).deltaPosition;
            //touchStart = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).deltaPosition);
            //lastPos = Input.GetTouch(0).deltaPosition;
        }

        // move camera
        if (Input.touchCount == 1)
        {
            
            //Tracks if touch is being moved on the screen
            if (Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                Debug.Log("Holding Touch");
                //Updating camera position
                transform.position += new Vector3(touchStart.x * 10 * Time.deltaTime, 0, touchStart.y * 10 * Time.deltaTime);
            }
        }

        /*
        if (Input.GetTouch(0).phase == TouchPhase.Stationary)
        {
            Debug.Log("Holding touch");
            Vector3 direction = touchStart - Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
            Camera.main.transform.position += direction;
        
        */
        
        // zoom
        //Zoom(Input.GetAxis("Mouse ScrollWheel"));
        if (Input.touchCount == 2)
        {
            Debug.Log("Two touch zoom");
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float currentMag = (touchZero.position - touchOne.position).magnitude;

            float difference = prevMag - currentMag;

            Zoom(difference * 0.01f);
        }

        // rotation
        /*
        if (Input.GetButton("Fire2"))
        {
            x += Input.GetAxis("Mouse X") * xSpeed * distance * 0.02f;
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
            Quaternion rotation = Quaternion.Euler(y, x, 0);
            target.transform.rotation = rotation;
        }
        */
        // For mobile devices, you will need to rewrite the above move, zoom and rotation
    // interactions with touch inputs.
    /*
    if (Input.touchCount == 2)
        {
            Debug.Log("Two touches");
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;
            float prevMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float currentMag = (touchZero.position - touchOne.position).magnitude;
            float difference = currentMag - prevMag;
            Zoom(difference * 0.01f);
        }
    */
    }
    
    void Zoom(float increment)
    {
        //Camera.main.orthographicSize += increment;
        //Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, zoomOutMin, zoomoutMax);
        Camera.main.fieldOfView += increment;
        Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView, zoomOutMin, zoomoutMax);
   
    }

}