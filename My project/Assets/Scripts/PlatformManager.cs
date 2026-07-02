using System.Collections.Generic;
using UnityEngine;
using GorillaLocomotion;

namespace GorillaFanGame
{
    // Pools a fixed number of Platform instances instead of destroying/instantiating: whichever
    // platform falls furthest below the player gets teleported to the top of the stack and re-armed.
    public class PlatformManager : MonoBehaviour
    {
        public static PlatformManager Instance { get; private set; }

        public GameObject platformPrefab;
        public int initialPlatformCount = 5;
        public float verticalSpacing = 2.5f;
        public float horizontalRange = 2f;

        [Tooltip("Once a platform is this far below the player, it gets recycled to the top.")]
        public float recycleMarginBelowPlayer = 4f;

        private readonly List<Platform> platforms = new List<Platform>();
        private float highestY;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (platformPrefab == null)
            {
                Debug.LogError("PlatformManager: platformPrefab is not assigned.");
                return;
            }

            for (int i = 0; i < initialPlatformCount; i++)
            {
                GameObject instance = Instantiate(platformPrefab, transform);
                Platform platform = instance.GetComponent<Platform>();
                if (platform == null)
                    platform = instance.AddComponent<Platform>();
                platforms.Add(platform);
            }

            ArrangeInitialStack();
        }

        private void Update()
        {
            Player player = Player.Instance;
            if (player == null || platforms.Count == 0) return;

            float playerY = player.transform.position.y;

            foreach (Platform platform in platforms)
            {
                if (platform.transform.position.y < playerY - recycleMarginBelowPlayer)
                    RecycleToTop(platform);
            }
        }

        private void ArrangeInitialStack()
        {
            Player player = Player.Instance;
            Vector3 basePosition = player != null ? player.transform.position : Vector3.zero;

            highestY = basePosition.y;

            for (int i = 0; i < platforms.Count; i++)
            {
                highestY += verticalSpacing;
                platforms[i].transform.position = RandomizedPosition(highestY);
                platforms[i].ResetTouch();
            }
        }

        private void RecycleToTop(Platform platform)
        {
            highestY += verticalSpacing;
            platform.transform.position = RandomizedPosition(highestY);
            platform.ResetTouch();
        }

        private Vector3 RandomizedPosition(float y)
        {
            Player player = Player.Instance;
            Vector3 origin = player != null ? player.transform.position : Vector3.zero;
            float x = origin.x + Random.Range(-horizontalRange, horizontalRange);
            float z = origin.z + Random.Range(-horizontalRange, horizontalRange);
            return new Vector3(x, y, z);
        }

        // Used by InGameGachaManager's "pull the next platform closer" buff.
        public Platform GetNextPlatformAbovePlayer()
        {
            Player player = Player.Instance;
            if (player == null || platforms.Count == 0) return null;

            float playerY = player.transform.position.y;
            Platform closest = null;
            float closestY = float.MaxValue;

            foreach (Platform platform in platforms)
            {
                float y = platform.transform.position.y;
                if (y > playerY && y < closestY)
                {
                    closest = platform;
                    closestY = y;
                }
            }

            return closest;
        }

        // Called by GameManager on death: puts every pooled platform back into its starting stack.
        public void ResetPlatforms()
        {
            ArrangeInitialStack();
        }
    }
}
