using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


public class ObjectPooling : MonoBehaviour
{

    private Dictionary<GameObject, Queue<GameObject>> objPool=new Dictionary<GameObject, Queue<GameObject>>();


    public GameObject getObject(GameObject prefab)
    {
        if (!objPool.ContainsKey(prefab))
        {
            objPool[prefab] = new Queue<GameObject>();
        }

        if (objPool[prefab].Count > 0)
        {
            GameObject currentObject = objPool[prefab].Dequeue();
            currentObject.SetActive(true);
            return currentObject;
        }
        else
        {
            return Instantiate(prefab);
        }
    }

    public void returnObject(GameObject obj)
    {
        obj.SetActive(false);
        var prefab = obj.GetComponent<PooledPrefab>()?.getPrefab();
        if (prefab != null)
        {
            objPool[prefab].Enqueue(obj);
        }
        else Destroy(obj); // fallback
    }
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
