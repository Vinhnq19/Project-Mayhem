using UnityEngine;

namespace ProjectMayhem.Player
{
    /// <summary>
    /// Debug gizmos for ground detection
    /// </summary>
    [RequireComponent(typeof(CapsuleCollider2D))]
    public class GroundDetectionGizmos : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] private bool showGizmos = true;
        [SerializeField] private Color groundedColor = Color.green;
        [SerializeField] private Color notGroundedColor = Color.red;
        [SerializeField] private LayerMask groundLayerMask = 1;

        private CapsuleCollider2D capsuleCollider;

        private void Awake()
        {
            capsuleCollider = GetComponentInChildren<CapsuleCollider2D>();
        }

        private void OnDrawGizmos()
        {
            if (!showGizmos || capsuleCollider == null) return;

            Vector2 boxSize = new Vector2(capsuleCollider.size.x * 0.9f, 0.1f);
            Vector2 boxCenter = new Vector2(transform.position.x, transform.position.y - capsuleCollider.size.y / 2f);

            // Check if grounded
            bool isGrounded = Physics2D.OverlapBox(boxCenter, boxSize, 0f, groundLayerMask);

            // Draw ground detection box
            Gizmos.color = isGrounded ? groundedColor : notGroundedColor;
            Gizmos.DrawWireCube(boxCenter, boxSize);

            // Draw player collider outline
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.position, capsuleCollider.size);

            // Draw multiple raycasts for better debugging
            Vector2 rayStart = new Vector2(transform.position.x, transform.position.y - capsuleCollider.size.y / 2f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(rayStart, Vector2.down * 0.3f); // Normal raycast distance
            
            // Draw additional raycasts (same as in CheckGrounded)
            Gizmos.color = Color.cyan;
            for (int i = -1; i <= 1; i++)
            {
                Vector2 multiRayStart = new Vector2(transform.position.x + i * capsuleCollider.size.x * 0.3f, transform.position.y - capsuleCollider.size.y / 2f);
                Gizmos.DrawRay(multiRayStart, Vector2.down * 0.3f);
            }
            
            // Draw debug raycasts (longer distance for debugging)
            Gizmos.color = Color.magenta;
            for (int i = -1; i <= 1; i++)
            {
                Vector2 multiRayStart = new Vector2(transform.position.x + i * capsuleCollider.size.x * 0.3f, transform.position.y - capsuleCollider.size.y / 2f);
                Gizmos.DrawRay(multiRayStart, Vector2.down * 2.0f);
            }

            // Draw text
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, 
                $"Grounded: {isGrounded}\nLayerMask: {groundLayerMask.value}\nRayDistance: 0.3f");
            #endif
        }
    }
}
