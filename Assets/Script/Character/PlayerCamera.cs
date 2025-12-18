using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Runtime.ConstrainedExecution;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header ("Target")]
    public Transform _target; // 플레이어

    [Header("Direction Basis")]
    public Transform directionBasis;  // MapDirectionAnchor

    [Header("Quater View Offset (relative to direction)")]
    [Tooltip("x=우측(대각선), y=위, z=뒤")]
    public Vector3 offset = new Vector3(0.7f, 0.7f, 2.0f);

    [Header("smooth)")]
    public float positionSmoothTime = 0.2f;
    private Vector3 _posVel;

    [Header("Look At")]
    public float rotationSpeed = 12.0f; // 카메라가 타겟을 바라보는 회전 속도 (0 = 즉시 회전)
    public bool lockRoll = true; // 카메라 기울기 방지

    [Header("Auto Find Anchor")] // directionBasis가 비어있다면 이름으로 자동 탐색
    public string directionAnchorName = "MapDirectionAnchor";

    private void Awake()
    {
        // 타겟 자동 연결
        if (_target == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) _target = p.transform;
        }
        // directionBasis 자동 탐색
        if (directionBasis == null && !string.IsNullOrEmpty(directionAnchorName))
        {
            var go = GameObject.Find(directionAnchorName);
            if (go != null) directionBasis = go.transform;
        }
    }

    private void LateUpdate()
    {
        if (_target == null) return;

        // 1) 진행 방향 기준 forward 계산
        Vector3 fwd = GetForwardBasis();
        Vector3 right = Vector3.Cross(Vector3.up, fwd).normalized;

        // 2) 우측 대각선 위로 카메라 위치 계산
        Vector3 desiredPos = _target.position + right * offset.x + Vector3.up * offset.y - fwd * offset.z;

        // 3) 위치 추적 (부드러움)
        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref _posVel, positionSmoothTime);

        // 4) 플레이어 바라보기
        Vector3 dir = _target.position - transform.position;
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);

        if (rotationSpeed <= 0.0f)
        {
            transform.rotation = targetRot;
        }
        else
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
        }

        // 카메라 기울어짐 방지
        if (lockRoll)
        {
            Vector3 e = transform.eulerAngles;
            transform.rotation = Quaternion.Euler(e.x, e.y, 0.0f);
        }
    }

    private Vector3 GetForwardBasis()
    {
        Transform basis = directionBasis != null ? directionBasis : _target;

        Vector3 fwd = basis.forward;
        fwd.y = 0.0f;

        // forward가 너무 작으면 월드 +z를 기본으로
        if (fwd.sqrMagnitude < 0.0001f)
        {
            fwd = Vector3.forward;
        }
        return fwd.normalized;
    }

    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_target == null) return;

        Vector3 fwd = GetForwardBasis();
        Vector3 right = Vector3.Cross(Vector3.up, fwd).normalized;

        Vector3 desiredPos = _target.position + right * offset.x + Vector3.up * offset.y - fwd * offset.z;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(_target.position, desiredPos);
        Gizmos.DrawSphere(desiredPos, 0.15f);
    }
    #endif

}
