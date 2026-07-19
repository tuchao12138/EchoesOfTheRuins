using System.Collections.Generic;
using UnityEngine;

namespace EchoesOfTheRuins
{
    public sealed class AudioDirector : MonoBehaviour
    {
        private readonly List<GuardianAI> guardians = new List<GuardianAI>();
        private AudioSource ambience;
        private AudioSource cue;
        private AudioClip pickup;
        private AudioClip alert;
        private AudioClip unlock;
        private AudioClip wind;
        private AudioClip telegraph;
        private AudioClip strike;
        private AudioClip playerHit;
        private AudioClip objectivePulse;
        private float nextAlertCue;

        public void Configure(IEnumerable<GuardianAI> configuredGuardians)
        {
            guardians.Clear();
            if (configuredGuardians != null) guardians.AddRange(configuredGuardians);
        }

        private void Awake()
        {
            ambience = gameObject.AddComponent<AudioSource>();
            ambience.loop = true;
            ambience.playOnAwake = false;
            ambience.spatialBlend = 0f;
            ambience.volume = .14f;
            cue = gameObject.AddComponent<AudioSource>();
            cue.playOnAwake = false;
            cue.spatialBlend = 0f;
            cue.volume = .66f;
            pickup = ProceduralAudioLibrary.CreatePickup();
            alert = ProceduralAudioLibrary.CreateAlert();
            unlock = ProceduralAudioLibrary.CreateUnlock();
            wind = ProceduralAudioLibrary.CreateWind();
            telegraph = ProceduralAudioLibrary.CreateAttackTelegraph();
            strike = ProceduralAudioLibrary.CreateSwordStrike();
            playerHit = ProceduralAudioLibrary.CreatePlayerHit();
            objectivePulse = ProceduralAudioLibrary.CreateObjectivePulse();
        }

        private void Start()
        {
            ambience.clip = wind;
            ambience.Play();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CoreCountChanged += OnCoreCountChanged;
                GameManager.Instance.RelicCountChanged += OnRelicCountChanged;
                GameManager.Instance.ExitUnlocked += OnExitUnlocked;
                GameManager.Instance.EscapeStarted += OnEscapeStarted;
                GameManager.Instance.Victory += OnVictory;
            }
            foreach (GuardianAI guardian in guardians) if (guardian != null) { guardian.AlertStateChanged += OnGuardianAlert; guardian.GuardianAttackChanged += OnGuardianAttack; guardian.PlayerStruck += OnPlayerStruck; }
        }

        private void OnCoreCountChanged(int _, int __) => cue.PlayOneShot(pickup);
        private void OnRelicCountChanged(int _, int __) => cue.PlayOneShot(pickup, .7f);
        private void OnExitUnlocked() { cue.PlayOneShot(unlock); cue.PlayOneShot(objectivePulse, .55f); }
        private void OnEscapeStarted() { cue.PlayOneShot(alert, .8f); cue.PlayOneShot(objectivePulse); }
        private void OnVictory() => cue.PlayOneShot(unlock, 1f);
        private void OnGuardianAlert(GuardianState state, float _)
        {
            if (state != GuardianState.Chase || Time.unscaledTime < nextAlertCue) return;
            cue.PlayOneShot(alert);
            nextAlertCue = Time.unscaledTime + 1.4f;
        }

        private void OnGuardianAttack(string _, GuardianAttackPhase phase)
        {
            if (phase == GuardianAttackPhase.Telegraph) cue.PlayOneShot(telegraph);
            else if (phase == GuardianAttackPhase.Strike) cue.PlayOneShot(strike);
        }

        private void OnPlayerStruck(string _) => cue.PlayOneShot(playerHit);

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CoreCountChanged -= OnCoreCountChanged;
                GameManager.Instance.RelicCountChanged -= OnRelicCountChanged;
                GameManager.Instance.ExitUnlocked -= OnExitUnlocked;
                GameManager.Instance.EscapeStarted -= OnEscapeStarted;
                GameManager.Instance.Victory -= OnVictory;
            }
            foreach (GuardianAI guardian in guardians) if (guardian != null) { guardian.AlertStateChanged -= OnGuardianAlert; guardian.GuardianAttackChanged -= OnGuardianAttack; guardian.PlayerStruck -= OnPlayerStruck; }
        }
    }
}
