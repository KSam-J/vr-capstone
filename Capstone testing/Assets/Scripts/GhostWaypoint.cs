using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostWaypoint : MonoBehaviour
{
    public GameObject[] waypoints;
    public GameObject player;
    int currentPos = 0;
    float rotSpeed;
    public float moveSpeed;
    double radius = 0.1;


    // Update is called once per frame
    void Update()
    {
        Debug.Log("Shit works 1");
        if (Vector3.Distance(waypoints[currentPos].transform.position, transform.position) < radius)
        {
            currentPos++;
            if(currentPos >= waypoints.Length)
            {
                currentPos = 0;
            }
        }

        //transform.position = Vector3.MoveTowards(transform.position, waypoints[currentPos].transform.position, Time.deltaTime * moveSpeed);
        //transform.position = waypoints[currentPos].transform.position;
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.name == "Player")
        {
            Debug.Log("Shit works");
            transform.position = waypoints[currentPos].transform.position;
            
        }
    }
}
