using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public MapGenerator mapGenerator;
    public PlayerMovement player;
    public Camera mainCamera;


    [Header("FallBack Spawn")] // 정상적인 스폰 위치를 찾지 못하였을 때
    public Vector3 fallbackSpawnPos = new Vector3(0f, 10f, 0f);

    public Transform mapDirectionAnchor;

    public MonsterSpawner monsterSpawner;

    // Start is called before the first frame update
    private void Start()
    {
        mapGenerator.generationOrigin = mapDirectionAnchor;
        mapGenerator.GenerateLine();

        if (mainCamera == null) mainCamera = Camera.main;

        mapGenerator.generationOrigin = mainCamera.transform;

        // 1) 맵 생성
        mapGenerator.GenerateLine();

        // 2) 스폰 위치
        Vector3 spawnPos = mapGenerator.firstSpawnPoint != null ? mapGenerator.firstSpawnPoint.position : fallbackSpawnPos;

        // 3) 플레이어 이동
        player.transform.position = spawnPos;

        // 4) 물리 초기화
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        monsterSpawner.SpawnOnce();
    }


}
