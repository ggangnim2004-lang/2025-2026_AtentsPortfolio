using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [Header("Slots")]
    public Transform playerSlot;
    public Transform monsterSlot;

    [Header("Mode")]
    public bool usePrefabs = false; // A Type: false , B Type: true;

    [Header("A Type: Actor Reference (for now)")]
    public Transform playerActor;    // 플레이어 배치용 슬롯
    public Transform monsterActor;   // 몬스터 배치용 슬롯

    [Header("B Type: Battle Prefab")]
    public GameObject playerBattlePrefab;
    public GameObject monsterBattlePrefab;

    private void Start()
    {

        if (!usePrefabs)
        {
            // A Type: 씬에 있는 오브젝트를 슬롯으로 정렬
            PlaceActor(playerActor, playerSlot);
            PlaceActor(monsterActor, monsterSlot);
        }
        else // B Type: 전투용 prefab을 생성에서 슬롯으로 배치
        {
            if (playerBattlePrefab != null)
            {
                var p = Instantiate(playerBattlePrefab);
                PlaceActor(p.transform, playerSlot);
            }

            if (playerBattlePrefab != null)
            {
                var m = Instantiate(monsterBattlePrefab);
                PlaceActor(m.transform, monsterSlot);
            }
        }

    }

    private void PlaceActor(Transform actor, Transform slot)
    {
        if (actor == null || slot == null) return;

        actor.position = slot.position;
        actor.rotation = Quaternion.identity;
    }


}
