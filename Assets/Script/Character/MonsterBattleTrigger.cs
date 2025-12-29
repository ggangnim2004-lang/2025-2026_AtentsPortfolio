using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterBattleTrigger : MonoBehaviour
{
    private bool triggered = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (triggered) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            triggered = true;

            BattleTransitionController.Instance
                .StartBattle("BattleScene"); // 전투 씬 이름
        }
    }

}
