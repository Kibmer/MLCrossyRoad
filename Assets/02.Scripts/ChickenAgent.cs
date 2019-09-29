using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class ChickenAgent : Agent
{
    //로드 블럭 하나의 정보를 모아놓은 스트럭쳐
    public struct RoadBlock
    {
        public Transform roadTr;
        public Vector3 roadFirstPos;
        private List<GameObject> ChickList;

        public RoadBlock(Transform roadTr){
            this.roadTr = roadTr;
            roadFirstPos = roadTr.position;
            ChickList = new List<GameObject>();
        }
    }

    //에이전트가 움직이는 방향  X, Z
    private int directionX = 0; 
    private int directionZ = 0;
    
    
    private SkinnedMeshRenderer chickenSkin; //에이전트가 죽었을 때 색을 바꿔주기 위한 스킨 렌더러

    private RoadBlock[] roadBlocks;          //로드 블럭 하나의 정보를 모아놓은 스트럭쳐의 리스트
    public Transform[] roadBlockTransforms;     //유니티 에디터에서 실제 로드 블럭의 위치들을 가지고 오기 위한 public 변수

    private Vector3 chickenFirstPos;
    private Transform chickenTr;


    public override void InitializeAgent()
    {
        chickenSkin = GetComponentInChildren<SkinnedMeshRenderer>();
        chickenTr = GetComponent<Transform>();

        roadBlocks = new RoadBlock[]{
            new RoadBlock(roadBlockTransforms[0]),
            new RoadBlock(roadBlockTransforms[1]),
            new RoadBlock(roadBlockTransforms[2])
        };

        chickenFirstPos = chickenTr.position;
    }

    public override void CollectObservations()
    {

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
        if (other.CompareTag("CHICK"))
        {
            AddReward(+1.0f);
        }
        if (other.CompareTag("DEAD_ZONE"))
        {
            chickenSkin.material.color = Color.red;
            AddReward(-1.0f);
            Done();
        }
        if (other.CompareTag("CAR"))
        {
            chickenSkin.material.color = Color.red;
            AddReward(-1.0f);
            Done();
        }
    }

    public override void AgentReset()
    {
        ResetStage();
    }

    void ResetStage()
    {
        //에이전트의 위치를 초기화
        chickenTr.position = chickenFirstPos;
        Invoke("SkinReset", 0.3f);

        //길을 초기화
        foreach (RoadBlock roadBlock in roadBlocks)
        {
            roadBlock.roadTr.position = roadBlock.roadFirstPos;
        }
    }

    private void SkinReset()
    {
        chickenSkin.material.color = Color.white;
    }

}
