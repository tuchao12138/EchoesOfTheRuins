using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoesOfTheRuins
{
    /// <summary>Aggregates every guardian into one stable, player-facing threat signal.</summary>
    public sealed class ThreatCoordinator : MonoBehaviour
    {
        [SerializeField] private GuardianAI[] guardians;
        [SerializeField] private Transform player;
        private readonly ThreatModel model = new ThreatModel();
        private readonly Dictionary<GuardianAI, Action<GuardianState, float>> alertHandlers = new Dictionary<GuardianAI, Action<GuardianState, float>>();
        private readonly Dictionary<GuardianAI, Action<string, GuardianAttackPhase>> attackHandlers = new Dictionary<GuardianAI, Action<string, GuardianAttackPhase>>();

        public ThreatSnapshot Current => model.Current;
        public event Action<ThreatSnapshot> Changed;

        private void Awake()
        {
            model.Changed += snapshot => Changed?.Invoke(snapshot);
            if (guardians == null || guardians.Length == 0) guardians = FindObjectsByType<GuardianAI>(FindObjectsSortMode.None);
            if (player == null && GameManager.Instance != null) player = GameManager.Instance.PlayerTransform;
            foreach (GuardianAI guardian in guardians) Subscribe(guardian);
        }

        private void Subscribe(GuardianAI guardian)
        {
            if (guardian == null || alertHandlers.ContainsKey(guardian)) return;
            Action<GuardianState, float> alert = (_, __) => Publish(guardian);
            Action<string, GuardianAttackPhase> attack = (_, __) => Publish(guardian);
            alertHandlers.Add(guardian, alert);
            attackHandlers.Add(guardian, attack);
            guardian.AlertStateChanged += alert;
            guardian.GuardianAttackChanged += attack;
            Publish(guardian);
        }

        private void Publish(GuardianAI guardian)
        {
            if (guardian == null) return;
            if (player == null && GameManager.Instance != null) player = GameManager.Instance.PlayerTransform;
            Vector3 direction = player == null ? guardian.transform.forward : player.InverseTransformDirection(guardian.transform.position - player.position);
            model.Update(guardian.gameObject.name, guardian.CurrentState, guardian.AlertLevel, guardian.AttackPhase, direction);
        }

        private void OnDestroy()
        {
            foreach (KeyValuePair<GuardianAI, Action<GuardianState, float>> pair in alertHandlers)
                if (pair.Key != null) pair.Key.AlertStateChanged -= pair.Value;
            foreach (KeyValuePair<GuardianAI, Action<string, GuardianAttackPhase>> pair in attackHandlers)
                if (pair.Key != null) pair.Key.GuardianAttackChanged -= pair.Value;
        }
    }
}
