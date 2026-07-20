using UnityEngine;

namespace EchoesOfTheRuins
{
[RequireComponent(typeof(BoxCollider))]
    public sealed class ExitGate : MonoBehaviour, IInteractable, IInteractionPromptProvider
    {
        [SerializeField] private GameObject lockedVisual;
        [SerializeField] private GameObject unlockedVisual;
        [SerializeField] private Light beacon;
        private bool unlocked;
        private GameManager gameManager;

        public bool IsUnlocked => unlocked;
        public bool CanEscape => unlocked && GameManager.Instance != null && !GameManager.Instance.HasWon;
        public string InteractionPrompt
        {
            get
            {
                int remaining = GameManager.Instance == null ? RuinGameState.RequiredCoreCount :
                    Mathf.Max(0, GameManager.Instance.RequiredCoreCount - GameManager.Instance.GameState.CollectedCoreCount);
                return ExitGatePresentation.GetPrompt(unlocked, remaining);
            }
        }

        public void Configure(GameObject lockedGate, GameObject openGate, Light exitBeacon = null)
        {
            lockedVisual = lockedGate;
            unlockedVisual = openGate;
            beacon = exitBeacon;
        }

        private void Reset() => GetComponent<Collider>().isTrigger = true;

        private void OnEnable()
        {
            BindGameManager();
        }

        private void Start()
        {
            BindGameManager();
            if (beacon == null)
            {
                GameObject beaconObject = GameObject.Find("Exit Beacon");
                if (beaconObject != null) beacon = beaconObject.GetComponent<Light>();
            }
            if (GameManager.Instance != null && GameManager.Instance.GameState.IsExitUnlocked) Unlock();
            RefreshVisuals();
        }

        private void OnDisable()
        {
            if (gameManager != null) gameManager.ExitUnlocked -= Unlock;
            gameManager = null;
        }

        private void BindGameManager()
        {
            if (gameManager == GameManager.Instance) return;
            if (gameManager != null) gameManager.ExitUnlocked -= Unlock;
            gameManager = GameManager.Instance;
            if (gameManager != null) gameManager.ExitUnlocked += Unlock;
        }

        private void Unlock()
        {
            unlocked = true;
            RefreshVisuals();
        }

        private void RefreshVisuals()
        {
            if (lockedVisual != null) lockedVisual.SetActive(!unlocked);
            if (unlockedVisual != null) unlockedVisual.SetActive(unlocked);
            if (beacon != null)
            {
                beacon.color = unlocked ? new Color(.08f, .9f, 1f) : new Color(1f, .45f, .12f);
                beacon.range = unlocked ? 30f : 10f;
                beacon.intensity = unlocked ? 6.5f : 1.5f;
            }
        }

        private void Update()
        {
            if (!unlocked || beacon == null) return;
            beacon.intensity = 6f + Mathf.Sin(Time.time * 3.5f) * 1.1f;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) other.GetComponent<PlayerInteractor>()?.Register(this);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player")) other.GetComponent<PlayerInteractor>()?.Unregister(this);
        }

        public void Interact(PlayerInteractor player)
        {
            if (CanEscape) GameManager.Instance.CompleteEscape();
        }
    }
}
