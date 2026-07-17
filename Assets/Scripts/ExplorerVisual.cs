using UnityEngine;

namespace EchoesOfTheRuins
{
    /// <summary>Builds a compact, non-colliding explorer silhouette for the third-person camera.</summary>
    public static class ExplorerVisual
    {
        public static GameObject Create(Transform player, Material cloak, Material trim)
        {
            GameObject explorer = new GameObject("Explorer Visual");
            explorer.transform.SetParent(player, false);
            // Keep the silhouette below one third of the 58-degree third-person frame.
            explorer.transform.localScale = Vector3.one * .78f;

            CreateCloak(explorer.transform, cloak);
            CreatePrimitive("Torso", PrimitiveType.Cube, explorer.transform, new Vector3(0f, .94f, .03f), new Vector3(.45f, .52f, .26f), cloak);
            CreatePrimitive("Head", PrimitiveType.Sphere, explorer.transform, new Vector3(0f, 1.39f, .04f), Vector3.one * .34f, trim);
            CreatePrimitive("Hood Trim", PrimitiveType.Cylinder, explorer.transform, new Vector3(0f, 1.31f, -.12f), new Vector3(.37f, .12f, .37f), cloak);
            CreatePrimitive("Backpack", PrimitiveType.Cube, explorer.transform, new Vector3(0f, .93f, .22f), new Vector3(.34f, .38f, .18f), trim);

            GameObject shoulderLight = new GameObject("Shoulder Light");
            shoulderLight.transform.SetParent(explorer.transform, false);
            shoulderLight.transform.localPosition = new Vector3(-.24f, 1.12f, -.05f);
            Light light = shoulderLight.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(.18f, .90f, 1f);
            light.range = 2.2f;
            light.intensity = .55f;

            return explorer;
        }

        private static void CreateCloak(Transform parent, Material material)
        {
            GameObject cloak = new GameObject("Cloak");
            cloak.transform.SetParent(parent, false);
            cloak.transform.localPosition = new Vector3(0f, .62f, .08f);
            MeshFilter filter = cloak.AddComponent<MeshFilter>();
            filter.sharedMesh = CreateTaperedCloakMesh();
            cloak.AddComponent<MeshRenderer>().sharedMaterial = material;
        }

        private static GameObject CreatePrimitive(string name, PrimitiveType type, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            part.GetComponent<Renderer>().sharedMaterial = material;
            Collider collider = part.GetComponent<Collider>();
            if (collider != null) collider.enabled = false;
            return part;
        }

        private static Mesh CreateTaperedCloakMesh()
        {
            const int sides = 8;
            const float height = 1.18f;
            const float topRadius = .30f;
            const float bottomRadius = .56f;
            Vector3[] vertices = new Vector3[sides * 2 + 2];
            int[] triangles = new int[sides * 12];

            for (int index = 0; index < sides; index++)
            {
                float angle = index * Mathf.PI * 2f / sides;
                float x = Mathf.Sin(angle);
                float z = Mathf.Cos(angle);
                vertices[index] = new Vector3(x * topRadius, height, z * topRadius);
                vertices[index + sides] = new Vector3(x * bottomRadius, 0f, z * bottomRadius);
            }

            vertices[sides * 2] = new Vector3(0f, height, 0f);
            vertices[sides * 2 + 1] = Vector3.zero;
            int triangle = 0;
            for (int index = 0; index < sides; index++)
            {
                int next = (index + 1) % sides;
                triangles[triangle++] = index;
                triangles[triangle++] = next;
                triangles[triangle++] = index + sides;
                triangles[triangle++] = next;
                triangles[triangle++] = next + sides;
                triangles[triangle++] = index + sides;
                triangles[triangle++] = sides * 2;
                triangles[triangle++] = next;
                triangles[triangle++] = index;
                triangles[triangle++] = sides * 2 + 1;
                triangles[triangle++] = index + sides;
                triangles[triangle++] = next + sides;
            }

            Mesh mesh = new Mesh { name = "Explorer Tapered Cloak" };
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
