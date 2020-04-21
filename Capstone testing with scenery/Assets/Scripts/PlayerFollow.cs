using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerFollow : MonoBehaviour
{
    public GameObject target;
    public double moveSpeed;

    private float speed;

    // Update is called once per frame
    void Update()
    {
        
        speed = (float)(Math.Sqrt((Math.Pow(transform.position[0] - target.transform.position[0], 2) + Math.Pow(transform.position[2] - target.transform.position[2], 2)))) * 5;
        
        transform.position = Vector3.MoveTowards(transform.position, target.transform.position, Time.deltaTime * speed);
        
    }
}
