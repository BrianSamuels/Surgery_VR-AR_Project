using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyContact : MonoBehaviour
{
    public List<AudioSource> heartbeats = new List<AudioSource>();
    public int bpm = 0;
    private void Start()
    {
        bpm = 0;
    }

    private void Update()
    {   /*
        if (heartbeats[0].isPlaying)
        {
            bpm += 1;
        }
        */
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Stethoscope"))
        {
            Debug.Log("music playing");
            heartbeats[0].loop = true;
            heartbeats[0].Play();
            //Debug.Log("scapel destroyed");
            //Destroy(collision.gameObject);
        }
        /*
        while (heartbeats[0].isPlaying)
        {
            bpm += 1;
        
        */
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Stethoscope"))
        {
            Debug.Log("music stopped");
            heartbeats[0].loop = false;
            heartbeats[0].Stop();
            print(bpm);
            //Debug.Log("scapel destroyed");
            //Destroy(collision.gameObject);
        }
    }
}
