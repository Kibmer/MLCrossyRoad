using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sphere : MonoBehaviour
{
    public GameObject[] cube;
    public ArrayList cubeIndexList;
    void Start()
    {
        cubeIndexList = new ArrayList();
        cubeIndexList.Add(0);
        cubeIndexList.Add(1);
        cubeIndexList.Add(2);
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(cube[(int)cubeIndexList[2]].transform.position.z);
        if (transform.position.z > cube[(int)cubeIndexList[2]].transform.position.z)
        {
            cube[(int)cubeIndexList[0]].transform.Translate(0, 0, 3);
            int temp = (int)cubeIndexList[0];
            cubeIndexList.RemoveAt(0);
            cubeIndexList.Add(temp);
        }
    }
}
