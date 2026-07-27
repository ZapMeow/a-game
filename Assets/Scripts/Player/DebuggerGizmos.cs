using System;
using UnityEngine;

public class DebuggerGizmos : MonoBehaviour
{

    [SerializeField] private float radius;
    [SerializeField] private PlayerMovement playerMovement;

    [SerializeField] private BoxCollider2D boxCollider2D;
    [SerializeField] private Transform wallChecker;
    [SerializeField] private Transform groundChecker;

    private void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        DrawHitboxes();
        DrawGroundChecker();
        DrawWallChecker();
    }


    private void DrawHitboxes()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(boxCollider2D.size.x, boxCollider2D.size.y, 0));
    }

    private void DrawGroundChecker()
    {
        if (playerMovement.GetIsGrounded())
        {
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.color = Color.red;
        }
        Gizmos.DrawWireSphere(groundChecker.position, radius);
    }

    private void DrawWallChecker()
    {
        if (playerMovement.GetIsWalled())
        {
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.color = Color.red;
        }
        Gizmos.DrawWireSphere(wallChecker.position, radius);
    }
}
