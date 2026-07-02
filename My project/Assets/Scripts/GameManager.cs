using System;
using UnityEngine;
using GorillaLocomotion;

namespace GorillaFanGame
{
    // Central score/run-state authority. Platform, DeathZone and the gacha managers
    // all report to this instead of talking to each other directly.
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Tooltip("Where the player is teleported to on death. Should match the rig's original spawn pose.")]
        public Transform respawnPoint;

        public int Score { get; private set; }
        public bool IsGameOver { get; private set; }

        public event Action<int> OnScoreChanged;
        public event Action<int> OnGameOver;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void AddScore(int amount = 1)
        {
            if (IsGameOver) return;

            Score += amount;
            OnScoreChanged?.Invoke(Score);

            if (InGameGachaManager.Instance != null)
                InGameGachaManager.Instance.TryRollBuff(Score);
        }

        // Called by DeathZone (or anything else that should end the run) when the player falls out.
        public void HandlePlayerDeath()
        {
            if (IsGameOver) return;
            IsGameOver = true;

            if (CosmeticGachaManager.Instance != null)
                CosmeticGachaManager.Instance.GrantRolls(Score);

            OnGameOver?.Invoke(Score);

            ResetGame();
        }

        public void ResetGame()
        {
            Score = 0;
            IsGameOver = false;
            OnScoreChanged?.Invoke(Score);

            if (InGameGachaManager.Instance != null)
                InGameGachaManager.Instance.ResetThresholds();

            RespawnPlayer();

            if (PlatformManager.Instance != null)
                PlatformManager.Instance.ResetPlatforms();
        }

        private void RespawnPlayer()
        {
            Player player = Player.Instance;
            if (player == null || respawnPoint == null) return;

            // The Player component sits on a child of the rig root; move the whole rig, not just that child.
            Transform rigRoot = player.transform.root;
            rigRoot.position = respawnPoint.position;
            rigRoot.rotation = respawnPoint.rotation;

            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
}
