using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[ExecuteInEditMode]
public class SimpleGizmos : MonoBehaviour
{
    public bool onlyDrawIfSelected;
    [Max(0)]
    public float axisLength = 1;
    [Max(0)]
    public float sphereRadius = 1;
    private void OnDrawGizmosSelected()
    {
        if (onlyDrawIfSelected)
            DrawGizmos();
    }
    private void OnDrawGizmos()
    {
        if (!onlyDrawIfSelected)
            DrawGizmos();
    }
    public void DrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + transform.right * axisLength, transform.position - transform.right * axisLength);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position + transform.up * axisLength, transform.position - transform.up * axisLength);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position + transform.forward * axisLength, transform.position - transform.forward * axisLength);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sphereRadius);
    }
}
