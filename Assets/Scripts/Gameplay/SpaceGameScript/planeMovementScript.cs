using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class planeScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    
    private float verticalInput;
    private float horizontalInput;

    private float xRotation = 0;
    private float yRotation = 0;
    private float zRotation = 0;

    [SerializeField] private int verticalTilt=15;
    [SerializeField] private int horizontalTilt=15;
    [SerializeField] private int sideTilt = 30;
    int currentRollRot = 0;
   
    [SerializeField] private int rotationSpeedX = 15;
    [SerializeField] private int rotationSpeedY = 15;
    [SerializeField] private int rotationSpeedZ = 15;
 

    Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    
    }

    // Update is called once per frame
    void Update()
    {
       
       //Horizontal и Vertical input плавно стават от 0->1,което позволява на Lerp да отиде от първоначална ротация до желаната плавно
       horizontalInput = Input.GetAxis("Horizontal");//float
       verticalInput = Input.GetAxis("Vertical");//float



        //Lerp само отива от сегашната ротация до желаната за определено време.
        //пример
        //-verticalInput * verticalTilt - понеже verticalInput първоначално е 0.1 и бавно става 1, това позволява на 
        //xRotation със скорост Time.deltaTime * rotationSpeedX, да отива към ротацията -verticalInput * verticalTilt
        //дори и verticalInput да се променя, Lerp просто следва новата ротация
        //За всяка ротация имаме отделна променлива вместо всичко в един lerp,за да имаме отделна скорост на ротация

        //Плавни ротации
        xRotation = Mathf.Lerp(xRotation, -verticalInput * verticalTilt, Time.deltaTime * rotationSpeedX);
        yRotation = Mathf.Lerp(yRotation, horizontalInput * horizontalTilt, Time.deltaTime * rotationSpeedY);
        zRotation = Mathf.Lerp(zRotation, currentRollRot+(-horizontalInput * sideTilt), Time.deltaTime * rotationSpeedZ);
    
        //от сегашната родтация плавно се движи към избраната от нас ротация
        //дори и targetRotation да се промени lerp просто пак ще си работи нормално и ще отива към следващия таргет
        
        Quaternion targetRotation = Quaternion.Euler(xRotation, yRotation, zRotation);
        transform.rotation = targetRotation;

        if (Input.GetKeyDown(KeyCode.E))
        {
            currentRollRot -= 90;
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            currentRollRot += 90;
        }

        rb.AddForce(new Vector3(horizontalInput, verticalInput), ForceMode.Force);

        pushIfOutOfBBounds();




    }

    public void pushIfOutOfBBounds()
    {
        Vector2 pushDir = Vector2.zero;

        if (transform.position.x < CameraController.LeftCameraBorder)
        {
            pushDir = Vector2.right;
        }
        if (transform.position.x > CameraController.RightCameraBorder)
        {
            pushDir = Vector2.left;
        }
        if (transform.position.y > CameraController.UpCameraBorder)
        {
            pushDir = Vector2.down;
        }
        if (transform.position.y < CameraController.DownCameraBorder)
        {
            pushDir = Vector2.up;
        }
        rb.AddForce(pushDir*5, ForceMode.Force);
    }
  
}
