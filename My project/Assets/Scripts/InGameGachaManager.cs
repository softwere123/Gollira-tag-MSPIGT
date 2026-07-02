using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GorillaLocomotion;

namespace GorillaFanGame
{
    // GameManager calls TryRollBuff() every time score changes; this decides whether that score
    // crossed a trigger threshold and, if so, rolls one of the two buffs.
    public class InGameGachaManager : MonoBehaviour
    {
        public static InGameGachaManager Instance { get; private set; }

        [Tooltip("Scores that trigger a buff roll, each only once per run.")]
        public List<int> triggerScores = new List<int> { 2, 4, 8 };

        public float jumpBuffDuration = 8f;
        public float jumpBuffMultiplierFactor = 2f;
        public float platformPullAmount = 1.5f;

        private readonly HashSet<int> consumedThresholds = new HashSet<int>();

        private float baseJumpMultiplier;
        private bool baseJumpMultiplierCaptured;
        private Coroutine jumpBuffRoutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void TryRollBuff(int currentScore)
        {
            if (!triggerScores.Contains(currentScore)) return;
            if (!consumedThresholds.Add(currentScore)) return; // already rolled for this score this run

            RollBuff();
        }

        public void ResetThresholds()
        {
            consumedThresholds.Clear();

            if (jumpBuffRoutine != null)
            {
                StopCoroutine(jumpBuffRoutine);
                jumpBuffRoutine = null;
            }
            RestoreJumpMultiplier();
        }

        private void RollBuff()
        {
            if (Random.Range(0, 2) == 0)
                ApplyJumpBuff();
            else
                PullNextPlatformCloser();
        }

        private void ApplyJumpBuff()
        {
            Player player = Player.Instance;
            if (player == null) return;

            if (!baseJumpMultiplierCaptured)
            {
                baseJumpMultiplier = player.jumpMultiplier;
                baseJumpMultiplierCaptured = true;
            }

            if (jumpBuffRoutine != null)
                StopCoroutine(jumpBuffRoutine);

            jumpBuffRoutine = StartCoroutine(JumpBuffRoutine());
        }

        private IEnumerator JumpBuffRoutine()
        {
            Player player = Player.Instance;
            player.jumpMultiplier = baseJumpMultiplier * jumpBuffMultiplierFactor;

            yield return new WaitForSeconds(jumpBuffDuration);

            RestoreJumpMultiplier();
            jumpBuffRoutine = null;
        }

        private void RestoreJumpMultiplier()
        {
            Player player = Player.Instance;
            if (player != null && baseJumpMultiplierCaptured)
                player.jumpMultiplier = baseJumpMultiplier;
        }

        private void PullNextPlatformCloser()
        {
            if (PlatformManager.Instance == null) return;

            Platform next = PlatformManager.Instance.GetNextPlatformAbovePlayer();
            if (next == null) return;

            Player player = Player.Instance;
            float minY = player != null ? player.transform.position.y + 1f : next.transform.position.y;

            Vector3 pos = next.transform.position;
            pos.y = Mathf.Max(minY, pos.y - platformPullAmount);
            next.transform.position = pos;
        }
    }
}
