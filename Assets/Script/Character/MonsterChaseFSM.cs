using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]

public class MonsterChaseFSM : MonoBehaviour
{
    public enum State
    {
        Idle,
        Chase
    }

    [Header("Target")]
    public Transform player; 
    public string playerTag = "Player";

    [Header("Chase Setting")]
    public float detectRadius = 6.0f; // 추적 시작 반경
    public float stopDistance = 1.2f; // 너무 붙으면 멈춤
    public float moveSpeed = 3.0f;
    public float turnSpeed = 10f;

    [Header("Debug")]
    public bool drawGizmos = true;

    private Rigidbody rb;
    private State state = State.Idle;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // 큐브가 안 굴러가게
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindWithTag(playerTag);
            if (p != null) player = p.transform;
        }
    }
    private void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(GetFlatPos(transform.position), GetFlatPos(player.position));

        // 상태 전환
        if (dist <= detectRadius) state = State.Chase;
        else state = State.Idle;
    }

    private void FixedUpdate()
    {
        if (player == null) return;
        switch (state)
        {
            case State.Idle:
                StopMovement();
                break;

            case State.Chase:
                ChasePlayer();
                break;
        }
    }

    private void ChasePlayer()
    {
        Vector3 myPos = rb.position;
        Vector3 targetPos = player.position;

        // 2.5D 상하(Y)는 무시하고 XZ 평면에서만 추적
        myPos.y = 0f;
        targetPos.y = 0f;

        Vector3 toTarget = targetPos - myPos;
        float dist = toTarget.magnitude;

        // 너무 가까우면 멈춤
        if (dist <= stopDistance)
        {
            StopMovement();
            return;
        }

        Vector3 dir = toTarget.normalized;

        // 회전
        if (dir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
            Quaternion newRot = Quaternion.Slerp(rb.rotation, targetRot, Time.fixedDeltaTime * turnSpeed);
            rb.MoveRotation(newRot);
        }

        // 이동
        Vector3 nextPos = rb.position + dir * (moveSpeed * Time.deltaTime);
        rb.MovePosition(nextPos);
    }

    private void StopMovement()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    private Vector3 GetFlatPos(Vector3 p)
    {
        p.y = 0f;
        return p;
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }

}
