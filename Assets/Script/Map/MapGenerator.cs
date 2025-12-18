using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("Map Prefabs")]
    public GameObject mapPieceA;
    public GameObject mapPieceB;
    public GameObject mapPieceC;
    public GameObject mapPieceD;
    public GameObject mapPieceE;

    [Header("Spawn settings")] // 라인(맵)에 몇개의 맵 조각이 들어가는지
    public int piecesToSpawn = 3;


    // 스폰의 Y 위치
    public float spawnY = 3;
    public float fallbackSpacing = 5;

    [Header("Chance (Lower = rarer")] // 맵 조각 생성 확률 (적을 수록 나올 확률이 낮다)
    public float ChanceA = 0.2f;
    public float ChanceB = 1f;
    public float ChanceC = 1f;
    public float ChanceD = 1f;
    public float ChanceE = 1f;

    [Header("Generation Reference")]
    public Transform generationOrigin; // 플레이어

    private List<GameObject> _prefabs;
    private List<float> _chance;

    public Transform firstSpawnPoint { get; private set; }

    private void Awake()
    {
        _prefabs = new List<GameObject> { mapPieceA, mapPieceB, mapPieceC, mapPieceD, mapPieceE };
        _chance = new List<float> { ChanceA, ChanceB, ChanceC, ChanceD, ChanceE };
    }

    private void Start()
    {

    }

    public void GenerateLine()
    {
        
        if (generationOrigin == null)
        {
            var go = GameObject.Find("MapDirectionAnchor");
            if (go != null) generationOrigin = go.transform;
        }
        if (generationOrigin == null)
        {
            Debug.LogError("[MapGeneration] generationOrigin이 할당되지 않았습니다.");
            return;
        }

        firstSpawnPoint = null;

        // 기존 맵 조각 삭제
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        List<int> pickdIndices = PickUniqueChanceIndices(_prefabs.Count, piecesToSpawn, _chance);

        // 뽑힌 3개를 랜덤으로 섞기
        Shuffle(pickdIndices);

        // 첫 시작 위치
        Vector3 forward = generationOrigin.forward;
        forward.y = 0f;
        forward = forward.sqrMagnitude < 0.001f ? Vector3.right : forward.normalized;

        forward = -forward;

        Quaternion rot = Quaternion.LookRotation(forward, Vector3.up);

        Vector3 cursorPos = generationOrigin.position + forward * 0.1f;
        cursorPos.y = spawnY;

        // 일자로 스폰
        for (int i = 0; i < pickdIndices.Count; i++)
        {
            GameObject prefab = _prefabs[pickdIndices[i]];
            if (prefab == null) continue;

            GameObject piece = Instantiate(prefab, cursorPos, rot, transform);

            MapPiecesAnchor anchors = piece.GetComponent<MapPiecesAnchor>();
            if (anchors == null || anchors.startAnchor == null || anchors.endAnchor == null)
            {
                Debug.LogError($"[MapGenerator] Anchor가 {piece.name}에 존재하지 않는다.");
                continue;
            }

            // 1) StartAnchor를 cursorPos에 맞추기
            Vector3 delta = cursorPos - anchors.startAnchor.position;
            delta.y = 0.0f; // 높이 고정
            piece.transform.position += delta;

            // 2) 첫번째 맵 조각이라면 SpawnPoint 기억
            if (firstSpawnPoint == null)
            {
                var sp = piece.transform.Find("PlayerSpawnPoint");
                if (sp != null) firstSpawnPoint = sp;
            }

            // 3) 다음 맵 조각은 EndAnchor부터 시작
            cursorPos = anchors.endAnchor.position;
            cursorPos.y = spawnY;

        }

    }

    // 가중치를 기반으로 중복없이 n개의 인덱스를 뽑는다
    private List<int> PickUniqueChanceIndices(int totalCount, int pickCount, List<float> chances)
    {
        pickCount = Mathf.Clamp(pickCount, 0, totalCount);

        List<int> candidates = new List<int>(totalCount);
        List<float> candidateChances = new List<float>(totalCount);
        
        for (int i = 0; i < totalCount; i++)
        {
            candidates.Add(i);
            candidateChances.Add(Mathf.Max(0f, chances[i]));
        }

        List<int> picked = new List<int>(pickCount);

        for (int j = 0; j < pickCount; j++)
        {
            int chosenCandidateIndex = ChanceChoiceIndex(candidateChances);
            int chosen = candidates[chosenCandidateIndex];

            picked.Add(chosen);

            // 생성에 쓰인 맵 조각은 후보에서 제거함으로써 중복을 방지
            candidates.RemoveAt(chosenCandidateIndex);
            candidateChances.RemoveAt(chosenCandidateIndex);

        }

        return picked;
    }

    // Chance 리스트에서 가중치 랜덤 선택 (리턴은 chance 내부 인덱스)
    private int ChanceChoiceIndex(List<float> chances)
    {
        float sum = 0f;
        
        for (int i = 0; i < chances.Count; i++)
        {
            sum += chances[i];
        }

        // 전부 0이면 균등하게 랜덤
        if (sum <= 0.0001f) return Random.Range(0, chances.Count);

        float _random = Random.value * sum;
        float acc = 0f;

        for (int i = 0; i < chances.Count; i++)
        {
            acc += chances[i];
            if (_random <= acc) return i;
        }
        return chances.Count - 1;
    }

    private void Shuffle(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    /*
    private float GetPrefabWidth(GameObject instance)
    {
        var renderers = instance.GetComponentsInChildren<Renderer>();
        if (renderers == null || renderers.Length == 0) return 0f;

        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            b.Encapsulate(renderers[i].bounds);

        return b.size.x;
    }
    */

}
