using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyContact : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("scapel"))
        {
            Debug.Log("scapel destroyed");
            Destroy(collision.gameObject);
        }
    }
}
