using UnityEngine;
using System.Collections;

public class DoneWayPointGizmo : MonoBehaviour
{
    public GameObject[] linkedWaypoints;

    void OnDrawGizmos()
    {
        Gizmos.DrawIcon(transform.position, "wayPoint.png", true);
    }
}
