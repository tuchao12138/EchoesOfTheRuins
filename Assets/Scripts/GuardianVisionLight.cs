using UnityEngine;

namespace EchoesOfTheRuins
{
    [RequireComponent(typeof(Light))]
    public sealed class GuardianVisionLight : MonoBehaviour
    {
        private Light visionLight;
        private GuardianAI guardian;

        private void Awake()
        {
            visionLight = GetComponent<Light>();
            guardian = GetComponentInParent<GuardianAI>();
        }

        private void Update()
        {
            if (guardian == null || visionLight == null) return;
            switch (guardian.CurrentState)
            {
                case GuardianState.Chase:
                case GuardianState.Capture:
                    visionLight.color = new Color(1f, .12f, .06f);
                    visionLight.intensity = 7f;
                    break;
                case GuardianState.Investigate:
                case GuardianState.Search:
                    visionLight.color = new Color(1f, .72f, .12f);
                    visionLight.intensity = 5.5f;
                    break;
                default:
                    visionLight.color = new Color(.45f, .75f, 1f);
                    visionLight.intensity = 3.5f;
                    break;
            }
        }
    }
}
