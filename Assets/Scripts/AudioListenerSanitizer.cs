using UnityEngine;

namespace EchoesOfTheRuins
{
    /// <summary>Keeps runtime scenes to one listener when a prototype scene still has its default camera.</summary>
    public sealed class AudioListenerSanitizer : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Schedule()
        {
            if (FindFirstObjectByType<AudioListenerSanitizer>() == null)
                new GameObject("Audio Listener Sanitizer").AddComponent<AudioListenerSanitizer>();
        }

        private void Start()
        {
            AudioListener[] listeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
            if (listeners.Length < 2) { Destroy(gameObject); return; }

            Camera preferredCamera = Camera.main;
            AudioListener keep = preferredCamera == null ? listeners[0] : preferredCamera.GetComponent<AudioListener>();
            if (keep == null) keep = listeners[0];
            foreach (AudioListener listener in listeners)
                if (listener != keep) listener.enabled = false;
            Destroy(gameObject);
        }
    }
}
