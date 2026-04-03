using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private GameObject target;
    [SerializeField] float speed = 1.2f;
    [SerializeField] Vector2 CameraLimit = new Vector2(3,3);
    private Vector2 startPosition;
    private Vector3 velocity = Vector3.zero;
    public static float LeftCameraBorder;
    public static float RightCameraBorder;
    public static float UpCameraBorder;
    public static float DownCameraBorder;
   
    
    void Start()
    {
        
        startPosition = transform.position;
        target = GameObject.FindWithTag("Player");
   


        LeftCameraBorder = startPosition.x - CameraLimit.x;
        RightCameraBorder = startPosition.x + CameraLimit.x;
        DownCameraBorder = startPosition.y - CameraLimit.y;
        UpCameraBorder = startPosition.y + CameraLimit.y;
    }

    // Update is called once per frame

    private void FixedUpdate()
    {
       
    }
    void Update()
    {



        float camHalfHeight = Camera.main.orthographicSize;
        float camHalfWidth = camHalfHeight * Camera.main.aspect;


        Vector3 newTarget = new Vector3(target.transform.position.x, target.transform.position.y,transform.position.z);
        Vector3 smoothPosition = Vector3.SmoothDamp(transform.position, newTarget, ref velocity, speed);


        Vector3 clampedPosition = new Vector3(
           Mathf.Clamp(smoothPosition.x, LeftCameraBorder+1, RightCameraBorder-1),
           Mathf.Clamp(smoothPosition.y, DownCameraBorder+1, UpCameraBorder-1),
           smoothPosition.z
       );
    


        
        transform.position = clampedPosition;

       
        
    }
 

}
