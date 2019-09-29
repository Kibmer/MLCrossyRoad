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
        private List<GameObject> chickList;
        private List<GameObject> carList;

        public RoadBlock(Transform roadTr){
            this.roadTr = roadTr;
            roadFirstPos = roadTr.position;
            chickList = new List<GameObject>();
            carList = new List<GameObject>();
        }

        public void CreateChick(GameObject chick){
            for (int i = 0; i < ChickenAgent.chickPerBlock; i++)
            {
                int xPos = Random.Range(-8, 8);
                int zPos = Random.Range(1, 31);
                Quaternion rotation = Quaternion.identity;
                rotation.eulerAngles = new Vector3(0, 180, 0);
                
                Vector3 chickPosition = new Vector3(xPos + roadTr.position.x, 0, zPos + roadTr.position.z);
                chickList.Add(Instantiate(chick, chickPosition, rotation, roadTr));
            }
        }

        public void CreateCar(GameObject car)
        {

        }

        public void ShuffleChick()
        {
            foreach(GameObject chick in chickList)
            {
                chick.SetActive(true);
                int xPos = Random.Range(-8, 8);
                int zPos = Random.Range(1, 31);
                Quaternion rotation = Quaternion.identity;
                rotation.eulerAngles = new Vector3(0, 180, 0);

                Vector3 chickPosition = new Vector3(xPos + roadTr.position.x, 0, zPos + roadTr.position.z);
                chick.transform.position = chickPosition;
            }
        }
    }

    //에이전트가 움직이는 방향  X, Z
    private int directionX = 0; 
    private int directionZ = 0;
    
    private SkinnedMeshRenderer chickenSkin; //에이전트가 죽었을 때 색을 바꿔주기 위한 스킨 렌더러

    private RoadBlock[] roadBlocks;          //로드 블럭 하나의 정보를 모아놓은 스트럭쳐의 리스트
    public Transform[] roadBlockTransforms;  //유니티 에디터에서 실제 로드 블럭의 위치들을 가지고 오기 위한 public 변수

    public ArrayList roadSwipArrayList;
    
    private GameObject chickPrefab;
    private GameObject carPrefab;

    private Vector3 chickenFirstPos;
    private Transform chickenTr;
    private GameObject camera;

    public static int chickPerBlock = 10;

    public override void InitializeAgent()
    {
        chickenSkin = GetComponentInChildren<SkinnedMeshRenderer>();
        chickenTr = GetComponent<Transform>();
        roadSwipArrayList = new ArrayList();
        roadSwipArrayList.Add(0);
        roadSwipArrayList.Add(1);
        roadSwipArrayList.Add(2);

        chickPrefab = Resources.Load("Chick") as GameObject;
        camera = transform.Find("Camera").gameObject;

        roadBlocks = new RoadBlock[]{
            new RoadBlock(roadBlockTransforms[0]),
            new RoadBlock(roadBlockTransforms[1]),
            new RoadBlock(roadBlockTransforms[2])
        };

        Debug.Log(roadBlocks);

        chickenFirstPos = chickenTr.position;
        foreach(RoadBlock roadBlock in roadBlocks)
        {
            roadBlock.CreateChick(chickPrefab);
        }
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

        
        float z = roadBlocks[(int)roadSwipArrayList[2]].roadTr.position.z;
        if (chickenTr.position.z > z)
        {
            roadBlocks[(int)roadSwipArrayList[0]].roadTr.Translate(0, 0, 96);
            roadBlocks[(int)roadSwipArrayList[0]].ShuffleChick();

            int firstObjIndex = (int)roadSwipArrayList[0];
            roadSwipArrayList.RemoveAt(0);
            roadSwipArrayList.Add(firstObjIndex);
        }

        camera.transform.position = new Vector3(camera.transform.parent.parent.position.x, 2, camera.transform.position.z);
        transform.Translate(directionX, 0, directionZ);
        AddReward(-0.001f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CHICK"))
        {
            AddReward(+1.0f);
            other.gameObject.SetActive(false);
        }
        if (other.CompareTag("DEAD_ZONE"))
        {
            chickenSkin.material.color = Color.red;
            foreach (RoadBlock roadBlock in roadBlocks)
            {
                roadBlock.ShuffleChick();
            }
            AddReward(-1.0f);
            Done();
        }
        if (other.CompareTag("CAR"))
        {
            chickenSkin.material.color = Color.red;
            foreach (RoadBlock roadBlock in roadBlocks)
            {
                roadBlock.ShuffleChick();
            }
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

        roadSwipArrayList.Clear();
        roadSwipArrayList.Add(0);
        roadSwipArrayList.Add(1);
        roadSwipArrayList.Add(2);

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
