using UnityEngine;
using GorillaLocomotion;

namespace GorillaFanGame
{
    // A large trigger volume that only ever rises: it stays a fixed distance below the player
    // and never drops back down, so falling behind means falling into it.
    [RequireComponent(typeof(Collider))]
    public class DeathZone : MonoBehaviour
    {
        public float riseSpeed = 1f;

        [Tooltip("The DeathZone's target height is always this far below the player's current height.")]
        public float followOffsetBelowPlayer = 5f;

        private void Reset()
        {
            Collider col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        private void Update()
        {
            Player player = Player.Instance;
            if (player == null) return;

            float targetY = player.transform.position.y - followOffsetBelowPlayer;
            if (targetY <= transform.position.y) return; // ratchet: never move back down

            Vector3 pos = transform.position;
            pos.y = Mathf.MoveTowards(pos.y, targetY, riseSpeed * Time.deltaTime);
            transform.position = pos;
        }

        private void OnTriggerEnter(Collider other)
        {
            Player player = Player.Instance;
            if (player == null) return;

            if (other == player.bodyCollider || other == player.headCollider)
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.HandlePlayerDeath();
            }
        }
    }
}
