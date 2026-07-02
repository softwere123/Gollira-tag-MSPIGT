using System;
using System.Collections.Generic;
using UnityEngine;

namespace GorillaFanGame
{
    // GameManager.HandlePlayerDeath() converts the run's final score into rolls here (1 point = 1 roll).
    // Actually spending the rolls (RollOnce/RollAll) is left to whatever UI presents the reward screen.
    public class CosmeticGachaManager : MonoBehaviour
    {
        public static CosmeticGachaManager Instance { get; private set; }

        [Tooltip("Pool of cosmetic prefabs a roll can award. Assign prefab 1, 2, 3, etc. here.")]
        public List<GameObject> cosmeticPrefabs = new List<GameObject>();

        public int RollsAvailable { get; private set; }

        public event Action<int> OnRollsChanged;
        public event Action<GameObject> OnCosmeticAwarded;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        // 1 score point = 1 gacha roll, granted right after a run ends.
        public void GrantRolls(int scoreThisRun)
        {
            if (scoreThisRun <= 0) return;
            RollsAvailable += scoreThisRun;
            OnRollsChanged?.Invoke(RollsAvailable);
        }

        public GameObject RollOnce()
        {
            if (RollsAvailable <= 0 || cosmeticPrefabs.Count == 0) return null;

            RollsAvailable--;
            OnRollsChanged?.Invoke(RollsAvailable);

            GameObject awarded = cosmeticPrefabs[UnityEngine.Random.Range(0, cosmeticPrefabs.Count)];
            OnCosmeticAwarded?.Invoke(awarded);
            return awarded;
        }

        public List<GameObject> RollAll()
        {
            List<GameObject> results = new List<GameObject>();
            while (RollsAvailable > 0)
            {
                GameObject awarded = RollOnce();
                if (awarded == null) break;
                results.Add(awarded);
            }
            return results;
        }
    }
}
