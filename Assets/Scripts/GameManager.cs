using System;
using UnityEngine;

namespace EchoesOfTheRuins
{
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public event Action<int, int> CoreCountChanged;
        public event Action ExitUnlocked;
        public event Action<string> PlayerReset;
        public event Action Victory;

        public RuinGameState GameState { get; } = new RuinGameState();
        public int RequiredCoreCount => RuinGameState.RequiredCoreCount;
        public bool HasWon { get; private set; }
        public Transform PlayerTransform => player;

        [SerializeField] private Transform player;
        [SerializeField] private Transform initialCheckpoint;
        private Transform currentCheckpoint;
        private bool exitWasUnlocked;

        public void Configure(Transform playerTransform, Transform startingCheckpoint)
        {
            player = playerTransform;
            initialCheckpoint = startingCheckpoint;
            currentCheckpoint = startingCheckpoint;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            currentCheckpoint = initialCheckpoint;
        }

        public bool CollectCore(string coreId)
        {
            if (!GameState.CollectCore(coreId)) return false;

            CoreCountChanged?.Invoke(GameState.CollectedCoreCount, RequiredCoreCount);
            if (GameState.IsExitUnlocked && !exitWasUnlocked)
            {
                exitWasUnlocked = true;
                ExitUnlocked?.Invoke();
            }
            return true;
        }

        public void SetCheckpoint(Transform checkpoint)
        {
            if (checkpoint != null) currentCheckpoint = checkpoint;
        }

        public void ResetPlayerToCheckpoint(string reason = "Caught by guardian")
        {
            if (player != null && currentCheckpoint != null)
            {
                var controller = player.GetComponent<CharacterController>();
                if (controller != null) controller.enabled = false;
                player.SetPositionAndRotation(currentCheckpoint.position, currentCheckpoint.rotation);
                if (controller != null) controller.enabled = true;
            }
            PlayerReset?.Invoke(reason);
        }

        public void CompleteEscape()
        {
            if (HasWon || !GameState.IsExitUnlocked) return;
            HasWon = true;
            Victory?.Invoke();
        }
    }
}
