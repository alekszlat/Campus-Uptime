using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class shootingScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    

    [SerializeField] List<GameObject> bulletTypes = new List<GameObject>();
    [SerializeField] ObjectPooling objPool;

    int currentBulletType = 0;

    float shootingPerSec = 0.5f;
    TimerUtil shootingTimer;
    public enum ShootingStates {inactive,isShoting};

    private ShootingStates currentShootingState=ShootingStates.inactive;

    void Start()
    {
       
        shootingTimer = new TimerUtil(shootingPerSec, true);

    }

    // Update is called once per frame
    void Update()
    {
        if (currentShootingState == ShootingStates.isShoting)
        {
         
     
            shotingState();

            if (Input.GetKeyUp(KeyCode.Space))
            {
                switchStates(ShootingStates.inactive);
                shootingTimer.ResetTimer();
            }

        }else if (currentShootingState == ShootingStates.inactive)
        {
         
            if (Input.GetKeyDown(KeyCode.Space))
            {
                switchStates(ShootingStates.isShoting);

            }

        }
        
    }

    public void shotingState()
    {
        if (shootingTimer.UpdateTimer(Time.deltaTime))
        {
            spawnBullet();
        }
    }

    public void spawnBullet()
    {
        Vector3 newPos = new Vector3(transform.position.x, transform.position.y, transform.position.z + 1);
        GameObject bulletObj = objPool.getObject(bulletTypes[currentBulletType]);
        BaseBullet bullet = bulletObj.GetComponent<BaseBullet>();
        bullet.setDir(transform.rotation*Vector3.forward);
        bullet.transform.rotation = transform.rotation;
        bullet.transform.position = new Vector3(transform.position.x,transform.position.y,transform.position.z+2);
    }



    public void switchStates(ShootingStates currentShootingState)
    {
        this.currentShootingState=currentShootingState;
    }
}
