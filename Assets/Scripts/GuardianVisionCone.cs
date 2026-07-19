using UnityEngine;
using UnityEngine.Rendering;

namespace EchoesOfTheRuins
{
    public sealed class GuardianVisionCone : MonoBehaviour
    {
        private const int DefaultSegments = 28;
        private static readonly Color PatrolColor = new Color(.18f, .55f, 1f);
        private static readonly Color InvestigateColor = new Color(1f, .58f, .08f);
        private static readonly Color ChaseColor = new Color(1f, .12f, .08f);

        [SerializeField, Min(.1f)] private float visualRange = 8f;
        [SerializeField, Range(10f, 180f)] private float visualAngle = 100f;
        [SerializeField, Range(3, 64)] private int segments = DefaultSegments;
        [SerializeField] private LayerMask obstacleMask = ~0;
        private Mesh coneMesh;
        private Material coneMaterial;

        public void Configure(float range, float angle, LayerMask walls)
        {
            visualRange = Mathf.Max(.1f, range);
            visualAngle = Mathf.Clamp(angle, 10f, 180f);
            obstacleMask = walls;
        }

        private void Awake()
        {
            MeshFilter filter = GetComponent<MeshFilter>();
            if (filter == null) filter = gameObject.AddComponent<MeshFilter>();
            MeshRenderer renderer = GetComponent<MeshRenderer>();
            if (renderer == null) renderer = gameObject.AddComponent<MeshRenderer>();
            coneMesh = new Mesh { name = "Guardian Vision Cone" };
            filter.sharedMesh = coneMesh;
            coneMaterial = RuntimeMaterialLibrary.Create("Guardian Vision Cone", ColorForState(GuardianState.Patrol, 0f), false);
            ConfigureTransparentMaterial(coneMaterial);
            renderer.sharedMaterial = coneMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        private void LateUpdate()
        {
            Vector3[] vertices = BuildConeVertices(visualRange, visualAngle, segments);
            Vector3 origin = transform.position + Vector3.up * .05f;
            for (int index = 1; index < vertices.Length; index++)
            {
                Vector3 localDirection = vertices[index].normalized;
                Vector3 worldDirection = transform.TransformDirection(localDirection);
                if (Physics.Raycast(origin, worldDirection, out RaycastHit hit, visualRange,
                        obstacleMask, QueryTriggerInteraction.Ignore))
                    vertices[index] = localDirection * hit.distance;
                vertices[index].y = .05f;
            }

            int[] triangles = new int[segments * 3];
            for (int segment = 0; segment < segments; segment++)
            {
                int triangle = segment * 3;
                triangles[triangle] = 0;
                triangles[triangle + 1] = segment + 1;
                triangles[triangle + 2] = segment + 2;
            }
            coneMesh.Clear();
            coneMesh.vertices = vertices;
            coneMesh.triangles = triangles;
            coneMesh.RecalculateBounds();
        }

        public void SetState(GuardianState state, float suspicion)
        {
            if (coneMaterial == null) return;
            Color color = ColorForState(state, suspicion);
            color.a = Mathf.Lerp(.14f, .32f, Mathf.Clamp01(suspicion));
            coneMaterial.color = color;
            if (coneMaterial.HasProperty("_BaseColor")) coneMaterial.SetColor("_BaseColor", color);
        }

        public static Color ColorForState(GuardianState state, float suspicion)
        {
            switch (state)
            {
                case GuardianState.Investigate:
                case GuardianState.Search:
                    return InvestigateColor;
                case GuardianState.Chase:
                case GuardianState.AttackTelegraph:
                case GuardianState.Strike:
                case GuardianState.Recovery:
                case GuardianState.Capture:
                    return ChaseColor;
                default:
                    return PatrolColor;
            }
        }

        public static Vector3[] BuildConeVertices(float range, float angle, int segmentCount)
        {
            int safeSegments = Mathf.Max(3, segmentCount);
            float safeRange = Mathf.Max(0f, range);
            float safeAngle = Mathf.Clamp(angle, 0f, 180f);
            var vertices = new Vector3[safeSegments + 2];
            for (int index = 0; index <= safeSegments; index++)
            {
                float rayAngle = Mathf.Lerp(-safeAngle * .5f, safeAngle * .5f, index / (float)safeSegments);
                vertices[index + 1] = Quaternion.Euler(0f, rayAngle, 0f) * Vector3.forward * safeRange;
            }
            return vertices;
        }

        public static void ConfigureTransparentMaterial(Material material)
        {
            if (material == null) return;
            material.renderQueue = (int)RenderQueue.Transparent;
            material.SetOverrideTag("RenderType", "Transparent");
            if (material.HasProperty("_Surface")) material.SetFloat("_Surface", 1f);
            if (material.HasProperty("_Blend")) material.SetFloat("_Blend", 0f);
            if (material.HasProperty("_Mode")) material.SetFloat("_Mode", 2f);
            if (material.HasProperty("_SrcBlend")) material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            if (material.HasProperty("_DstBlend")) material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            if (material.HasProperty("_ZWrite")) material.SetFloat("_ZWrite", 0f);
            material.DisableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        }

        private void OnDestroy()
        {
            if (coneMesh != null) Destroy(coneMesh);
            if (coneMaterial != null) Destroy(coneMaterial);
        }
    }
}
