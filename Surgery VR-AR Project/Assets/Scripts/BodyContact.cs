using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyContact : MonoBehaviour
{
    public List<AudioSource> heartbeats = new List<AudioSource>();
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("scapel"))
        {
            Debug.Log("music playing");
            //heartbeats[0].loop = true;
            heartbeats[0].Play();
            //Debug.Log("scapel destroyed");
            //Destroy(collision.gameObject);
        }
    }
}
