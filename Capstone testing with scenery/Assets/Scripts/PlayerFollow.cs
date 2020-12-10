using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerFollow : MonoBehaviour
{
    public GameObject target;

    private float speed;
    //The factor of how fast the average speed is (inc = faster, dec = slower)
    private readonly int AVG_SPEED_FACTOR = 2;
    //Mininum speed to speed up time to reach destination when 2 objects are close
    private readonly int MIN_SPEED = 10;

    // Update is called once per frame
    void Update()
    {
        //speed = (distance between ghost and player) * SPEED_FACTOR
        speed = (float)(Math.Sqrt((Math.Pow(transform.position[0] - target.transform.position[0], 2) + Math.Pow(transform.position[2] - target.transform.position[2], 2)))) * AVG_SPEED_FACTOR;
        if (speed < MIN_SPEED)
            speed = MIN_SPEED;
        transform.position = Vector3.MoveTowards(transform.position, target.transform.position, Time.deltaTime * speed);
        
    }
}
