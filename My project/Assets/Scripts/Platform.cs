using UnityEngine;
using GorillaLocomotion;

namespace GorillaFanGame
{
    // Scores on first touch by either hand or the body, then goes quiet until PlatformManager
    // recycles it. Touch is detected by proximity rather than trigger callbacks because the
    // hand-follower spheres on the rig currently have no Collider of their own — see PlatformManager notes.
    [RequireComponent(typeof(Collider))]
    public class Platform : MonoBehaviour
    {
        [Tooltip("How close a hand/body reference point needs to be to this platform's surface to count as a touch.")]
        public float touchRadius = 0.25f;

        [SerializeField] private Renderer platformRenderer;
        [SerializeField] private Color touchedColor = Color.green;

        private Collider platformCollider;
        private Color originalColor;
        private bool touched;

        private void Awake()
        {
            platformCollider = GetComponent<Collider>();
            if (platformRenderer != null)
                originalColor = platformRenderer.material.color;
        }

        private void Update()
        {
            if (touched) return;

            Player player = Player.Instance;
            if (player == null) return;

            if (IsTouchingPoint(player.bodyCollider != null ? player.bodyCollider.transform.position : (Vector3?)null) ||
                IsTouchingPoint(player.leftHandFollower != null ? player.leftHandFollower.position : (Vector3?)null) ||
                IsTouchingPoint(player.rightHandFollower != null ? player.rightHandFollower.position : (Vector3?)null))
            {
                RegisterTouch();
            }
        }

        private bool IsTouchingPoint(Vector3? point)
        {
            if (point == null) return false;
            Vector3 closest = platformCollider.ClosestPoint(point.Value);
            return Vector3.Distance(closest, point.Value) <= touchRadius;
        }

        private void RegisterTouch()
        {
            touched = true;
            if (platformRenderer != null)
                platformRenderer.material.color = touchedColor;

            if (GameManager.Instance != null)
                GameManager.Instance.AddScore(1);
        }

        // Called by PlatformManager when this platform is repositioned to the top of the stack.
        public void ResetTouch()
        {
            touched = false;
            if (platformRenderer != null)
                platformRenderer.material.color = originalColor;
        }
    }
}
