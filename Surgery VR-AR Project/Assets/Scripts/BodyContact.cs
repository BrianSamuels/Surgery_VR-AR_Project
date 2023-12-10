using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyContact : MonoBehaviour
{
    public List<AudioSource> heartbeats = new List<AudioSource>();
    //public AudioSource heartbeat;
    public GameObject timer;
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
            timer.GetComponent<Timer>().StartTimer();
            Debug.Log("music playing");
            heartbeats[0].loop = true;
            heartbeats[0].Play();
            //heartbeat.loop = true;
            //heartbeat.Play();
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
            timer.GetComponent<Timer>().StopTimer();
            Debug.Log("music stopped");
            heartbeats[0].loop = false;
            heartbeats[0].Stop();
            //heartbeat.loop = false;
            //heartbeat.Stop();
            //print(bpm);
            //Debug.Log("scapel destroyed");
            //Destroy(collision.gameObject);
        }
    }
}
