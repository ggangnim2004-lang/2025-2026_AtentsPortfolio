using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPiecesAnchor : MonoBehaviour
{
    public Transform startAnchor;
    public Transform endAnchor;

    private void OnValidate()
    {
        if (startAnchor == null)
        {
            var t = transform.Find("StartAnchor");
            if (t != null) startAnchor = t;
        }
        if (endAnchor == null)
        {
            var t = transform.Find("EndAnchor");
            if (t != null) endAnchor = t;
        }
    }
}
