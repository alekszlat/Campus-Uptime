using UnityEngine;

public class BaseBullet : MonoBehaviour
{
    [SerializeField] float damage;
    [SerializeField] float speed;
    private Vector3 dir;
    TimerUtil shootingTimer;
    ObjectPooling objPool;

    private void Awake()
    {
        shootingTimer = new TimerUtil(1, true);
    }
    void Start()
    {
        objPool = GameObject.FindWithTag("ObjPool").GetComponent<ObjectPooling>();
      
    }

    private void OnEnable()
    {
        shootingTimer.ResetTimer();
    }
    // Update is called once per frame
    void Update()
    {
        move();

        if (shootingTimer.UpdateTimer(Time.deltaTime))
        {
            objPool.returnObject(gameObject);
        }
    }

    public virtual void move()
    {
        transform.position += dir * speed * Time.deltaTime;
    }

    public void setDir(Vector3 dir)
    {
        this.dir = dir.normalized;
    }
  
}
