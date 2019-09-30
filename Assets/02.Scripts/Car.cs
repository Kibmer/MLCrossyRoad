using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    public float movementSpeed = 5f;
    private int leftPoint = -11;
    private int rightPoint = 11;

    void Update()
    {
        transform.position += transform.forward * Time.deltaTime * movementSpeed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CARRESET"))
        {
            if(transform.TransformVector(transform.forward).x < 0)
            {
                transform.position = new Vector3(leftPoint + transform.parent.position.x, 0, transform.position.z);
            }
            else
            {
                transform.position = new Vector3(rightPoint + transform.parent.position.x, 0, transform.position.z);
            }
        }
    }
}