using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class ChickenAgent : Agent
{
    private int directionX = 0;
    private int directionZ = 0;

    private SkinnedMeshRenderer chickenSkin;

    private Transform chickenTr;
    public List<Transform> roadTrGroup;

    private Vector3 chickenFirstPos;
    private List<Vector3> roadFirstPosGroup;

    public override void InitializeAgent()
    {
        chickenSkin = GetComponentInChildren<SkinnedMeshRenderer>();
        chickenTr = GetComponent<Transform>();

        roadTrGroup = new List<Transform>();
        roadFirstPosGroup = new List<Vector3>();

        chickenFirstPos = chickenTr.position;
        for(int i = 0; i<roadTrGroup.Count; i++){
            roadFirstPosGroup[i] = roadTrGroup[i].position;
        }
    }

    public override void CollectObservations(){
        
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        int movement = Mathf.FloorToInt(vectorAction[0]);
        directionX = 0;
        directionZ = 0;
        if (movement == 1) { directionX = -1; }
        if (movement == 2) { directionX = 1; }
        if (movement == 3) { directionZ = -1; }
        if (movement == 4) { directionZ = 1; }

        transform.Translate(directionX, 0, directionZ);
        AddReward(-0.001f);
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("CHICK"))
        {
            AddReward(+1.0f);
        }
        if (other.CompareTag("DEAD_ZONE"))
        {
            chickenSkin.material.color = Color.red;
            AddReward(-1.0f);
            Done();
        }
        if(other.CompareTag("CAR"))
        {
            chickenSkin.material.color = Color.red;
            AddReward(-1.0f);
            Done();
        }
    }

    public override void AgentReset(){
        ResetStage();
    }

    void ResetStage()
    {
        //치킨의 위치를 초기화
        chickenTr.position = chickenFirstPos;
        Invoke("SkinReset", 0.3f);

        //길을 초기화
        for (int i = 0; i < roadTrGroup.Count; i++)
        {
            roadTrGroup[i].position = roadFirstPosGroup[i];
        }
    }

    private void SkinReset()
    {
        chickenSkin.material.color = Color.white;
    }
}
