using UnityEngine;

namespace EchoesOfTheRuins
{
    public sealed class WorldObjectiveMarker : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField, Min(0f)] private float hideDistance = 8f;
        public Transform Target => target;
        public bool DistanceVisible { get; private set; }
        public bool ArrowVisible { get; private set; }
        public float Distance { get; private set; }

        public void SetTarget(Transform value) => target = value;

        private void Update()
        {
            Camera camera = Camera.main;
            if (target == null || camera == null) { DistanceVisible = false; ArrowVisible = false; return; }
            Distance = Vector3.Distance(camera.transform.position, target.position);
            DistanceVisible = Distance >= hideDistance;
            Vector3 screen = camera.WorldToScreenPoint(target.position);
            ArrowVisible = DistanceVisible && (screen.z < 0f || screen.x < 0f || screen.x > Screen.width || screen.y < 0f || screen.y > Screen.height);
        }

        private void OnGUI()
        {
            if (!DistanceVisible || target == null || Camera.main == null) return;
            Vector3 screen = Camera.main.WorldToScreenPoint(target.position);
            if (ArrowVisible)
            {
                if (screen.z < 0f) screen *= -1f;
                screen.x = Mathf.Clamp(screen.x, 24f, Screen.width - 24f);
                screen.y = Mathf.Clamp(Screen.height - screen.y, 24f, Screen.height - 24f);
                float pulse = 1f + Mathf.Sin(Time.time * 5f) * .15f;
                GUI.Label(new Rect(screen.x - 12f * pulse, screen.y - 12f * pulse, 48f, 24f), "▲");
            }
            GUI.Label(new Rect(Screen.width * .5f - 50f, 50f, 100f, 24f), Mathf.CeilToInt(Distance) + "m");
        }
    }
}
