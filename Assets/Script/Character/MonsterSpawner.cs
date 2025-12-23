using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [Header("References")]
    public MapGenerator mapGenerator;

    [Header("Monster Prefab")]
    public GameObject monsterPrefab;

    [Header("Spawn Setting")]
    public int spawnFromPieceIndex = 1;         // 0 = 1번째 , 1 = 2번째
    public string spawnPointName = "MonsterSpawnPoint";
    public int monsterCount = 1; // 몬스터 숫자
    public float spawnRandomRadius = 0.5f;

    [Header("Fallback")]
    public bool useFallbackIfMissing = true;
    public Vector3 fallbackSpawnPos = new Vector3(0f, 3f, 0f);

    public void SpawnOnce()
    {
        if (mapGenerator == null)
        {
            Debug.LogError("[MonsterSpawn] mapGenerator가 할당되지 않았습니다.");
            return;
        }
        if (monsterCount <= 0)
        {
            Debug.LogWarning("[MonsterSpawn] monsterCount가 0 이하");
            return;
        }

        // 생성된 맵 조각 수 확인
        if (mapGenerator.transform.childCount <= spawnFromPieceIndex)
        {
            Debug.LogWarning("[MonsterSpawner] 지정한 인덱스의 맵 조각이 존재하지 않습니다.");
            if (useFallbackIfMissing)
                SpawnMultipleAt(fallbackSpawnPos);
            return;
        }

        Transform piece = mapGenerator.transform.GetChild(spawnFromPieceIndex);
        Transform spawnPoint = piece.Find(spawnPointName);

        if (spawnPoint == null)
        {
            Debug.LogWarning($"[MonsterSpawner] '{spawnPointName}'를 {piece.name}에서 찾지 못했습니다.");
            if (useFallbackIfMissing)
                SpawnMultipleAt(fallbackSpawnPos);
            return;
        }

        SpawnMultipleAt(spawnPoint.position);
    }

    private void SpawnMultipleAt(Vector3 center)
    {
        for (int i = 0; i < monsterCount; i++)
        {
            Vector3 offset = Random.insideUnitSphere * spawnRandomRadius;
            offset.y = 0f; // 2.5D라서 높이 고정

            Vector3 spawnPos = center + offset;

            GameObject monster;

            if (monsterPrefab != null)
            {
                monster = Instantiate(monsterPrefab, spawnPos, Quaternion.identity);
            }
            else
            {
                // 테스트용 큐브 생성
                monster = GameObject.CreatePrimitive(PrimitiveType.Cube);
                monster.transform.position = spawnPos;

                if (monster.GetComponent<Rigidbody>() == null)
                    monster.AddComponent<Rigidbody>();

                monster.AddComponent<MonsterChaseFSM>();
            }
        }
    }

}
