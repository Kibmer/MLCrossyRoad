﻿using System.Collections;
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

        public RoadBlock(Transform roadTr)
        {
            this.roadTr = roadTr;
            roadFirstPos = roadTr.position;
            chickList = new List<GameObject>();
            carList = new List<GameObject>();
        }

        public void CreateChick(GameObject chick)
        {
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
            for (int i = 0; i < carLinePerBlock; i++)
            {
                int zPos = Random.Range(1, 31);
                for (int j = 0; j < carPerLineMax; j++)
                {
                    int xPos = Random.Range(-10, 10);
                    Quaternion rotation = Quaternion.identity;
                    bool rotationChange = 1 == Random.Range(0, 1) ? true : false;
                    if (rotationChange)
                    {
                        rotation.eulerAngles = new Vector3(0, 90, 0);
                    }
                    else
                    {
                        rotation.eulerAngles = new Vector3(0, -90, 0);
                    }

                    Vector3 carPosition = new Vector3(xPos + roadTr.position.x, 0, zPos + roadTr.position.z);
                    carList.Add(Instantiate(car, carPosition, rotation, roadTr));
                }
            }
        }

        public void ShuffleChick()
        {
            foreach (GameObject chick in chickList)
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

        public void ShuffleCar()
        {
            int carShuffleProgress = 0;
            for (int i = 0; i < carLinePerBlock; i++)
            {
                int zPos = Random.Range(1, 31);
                for (int j = 0; j < carPerLineMax; j++)
                {
                    int xPos = Random.Range(-10, 10);
                    Quaternion rotation = Quaternion.identity;
                    bool rotationChange = 1 == Random.Range(0, 1) ? true : false;
                    if (rotationChange)
                    {
                        rotation.eulerAngles = new Vector3(0, 90, 0);
                    }
                    else
                    {
                        rotation.eulerAngles = new Vector3(0, -90, 0);
                    }

                    Vector3 carPosition = new Vector3(xPos + roadTr.position.x, 0, zPos + roadTr.position.z);

                    carList[carShuffleProgress].transform.position = carPosition;
                    carList[carShuffleProgress].transform.rotation = rotation;
                    carShuffleProgress++;
                }
            }
        }
    }

    //에이전트가 움직이는 방향  X, Z
    private int directionX = 0;
    private int directionZ = 0;

    private SkinnedMeshRenderer chickenSkin; //에이전트가 죽었을 때 색을 바꿔주기 위한 스킨 렌더러

    private RoadBlock[] roadBlocks;          //로드 블럭 하나의 정보를 모아놓은 스트럭쳐의 리스트
    public Transform[] roadBlockTransforms;  //유니티 에디터에서 실제 로드 블럭의 위치들을 가지고 오기 위한 public 변수

    public ArrayList roadShuffArrayList;

    private GameObject chickPrefab;
    private GameObject carPrefab;

    private Vector3 chickenFirstPos;
    private Transform chickenTr;
    private GameObject camera;

    private Collider[] carCollBuffer;
    private Collider[] chickCollBuffer;

    private RayPerception3D ray;
    //광선의 거리
    public float rayDistance = 20.0f;
    //광선의 발사 각도 {7개의 광선}
    public float[] rayAngles = { 20.0f, 45.0f, 70.0f, 90.0f, 110.0f, 135.0f, 160.0f };
    //광선의 검출 대상 (4개의 검출 대상}
    public string[] detectObjects = { "CAR", "CHICK", "DEAD_ZONE" };

    public static int chickPerBlock = 13;
    public static int carPerLineMax = 1;
    public static int carLinePerBlock = 4;

    public override void InitializeAgent()
    {
        chickenSkin = GetComponentInChildren<SkinnedMeshRenderer>();
        chickenTr = GetComponent<Transform>();
        carCollBuffer = new Collider[16];
        chickCollBuffer = new Collider[16];

        ray = GetComponent<RayPerception3D>();

        roadShuffArrayList = new ArrayList();
        roadShuffArrayList.Add(0);
        roadShuffArrayList.Add(1);
        roadShuffArrayList.Add(2);

        chickPrefab = Resources.Load("Chick") as GameObject;
        carPrefab = Resources.Load("Car") as GameObject;
        camera = transform.parent.Find("Camera").gameObject;

        roadBlocks = new RoadBlock[]{
            new RoadBlock(roadBlockTransforms[0]),
            new RoadBlock(roadBlockTransforms[1]),
            new RoadBlock(roadBlockTransforms[2])
        };

        Debug.Log(roadBlocks);

        chickenFirstPos = chickenTr.position;
        foreach (RoadBlock roadBlock in roadBlocks)
        {
            roadBlock.CreateChick(chickPrefab);
            roadBlock.CreateCar(carPrefab);
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


        float z = roadBlocks[(int)roadShuffArrayList[2]].roadTr.position.z;
        if (chickenTr.position.z > z)
        {
            roadBlocks[(int)roadShuffArrayList[0]].roadTr.Translate(0, 0, 96);
            roadBlocks[(int)roadShuffArrayList[0]].ShuffleChick();
            roadBlocks[(int)roadShuffArrayList[0]].ShuffleCar();

            int firstObjIndex = (int)roadShuffArrayList[0];
            roadShuffArrayList.RemoveAt(0);
            roadShuffArrayList.Add(firstObjIndex);
        }

        camera.transform.position = new Vector3(camera.transform.parent.position.x, 2, chickenTr.position.z + 3);
        transform.Translate(directionX, 0, directionZ);
        AddReward(-0.002f);
    }

    public override void CollectObservations()
    {
        //int bufferCount = Physics.OverlapBoxNonAlloc(transform.position + new Vector3(0, 0, 6)
        //                                            , new Vector3(16, 3, 16)
        //                                            , carCollBuffer
        //                                            , Quaternion.identity
        //                                            , LayerMask.GetMask("CAR"));
        //float carLength = 20;
        //for (int i = 0; i < bufferCount; i++)
        //{
        //    float dist = Vector3.Distance(carCollBuffer[i].transform.position, transform.position);
        //    if (dist < carLength)
        //    {
        //        carLength = dist;
        //    }
        //}

        //int chickBufferCount = Physics.OverlapBoxNonAlloc(transform.position + new Vector3(0, 0, 6)
        //                                            , new Vector3(16, 3, 16)
        //                                            , chickCollBuffer
        //                                            , Quaternion.identity
        //                                            , LayerMask.GetMask("CHICK"));
        //float chickLength = 20;
        //for (int i = 0; i < bufferCount; i++)
        //{
        //    float dist = Vector3.Distance(chickCollBuffer[i].transform.position, transform.position);
        //    if (dist < chickLength)
        //    {
        //        chickLength = dist;
        //    }
        //}
        //AddVectorObs(carLength);
        //AddVectorObs(chickLength);

        //광선 7, 대상 (3 + 2)
        //Observation Size = 7 * 5 = 35
        AddVectorObs(ray.Perceive(rayDistance, rayAngles, detectObjects, 0.5f, 0.5f));

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CHICK"))
        {
            AddReward(+1.5f);
            other.gameObject.SetActive(false);
        }
        if (other.CompareTag("DEAD_ZONE"))
        {
            chickenSkin.material.color = Color.red;
            foreach (RoadBlock roadBlock in roadBlocks)
            {
                roadBlock.ShuffleChick();
                roadBlock.ShuffleCar();
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
                roadBlock.ShuffleCar();
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

        roadShuffArrayList.Clear();
        roadShuffArrayList.Add(0);
        roadShuffArrayList.Add(1);
        roadShuffArrayList.Add(2);

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
