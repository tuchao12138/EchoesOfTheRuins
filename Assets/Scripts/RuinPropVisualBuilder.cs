using System.Collections.Generic;
using UnityEngine;

namespace EchoesOfTheRuins
{
    /// <summary>Small authored-looking props that replace debug primitives in the production route.</summary>
    public static class RuinPropVisualBuilder
    {
        public static GameObject CreateRitualDais(string name, Vector3 position, float radius, Material stone)
        {
            GameObject root = new GameObject(name);
            root.transform.position = position;
            CreatePrism(root.transform, "Lower Carved Step", new Vector3(0f, .16f, 0f), radius, .32f, 10, stone, 0f);
            CreatePrism(root.transform, "Middle Carved Step", new Vector3(0f, .43f, 0f), radius * .82f, .24f, 10, stone, 18f);
            CreatePrism(root.transform, "Rune Altar Cap", new Vector3(0f, .68f, 0f), radius * .62f, .28f, 8, stone, 22.5f);
            return root;
        }

        public static GameObject CreateBrazier(string name, Vector3 position, Material stone, Material fire)
        {
            GameObject root = new GameObject(name);
            root.transform.position = position;
            CreatePrism(root.transform, "Carved Foot", new Vector3(0f, .18f, 0f), .48f, .36f, 8, stone, 22.5f);
            CreatePrism(root.transform, "Brazier Stem", new Vector3(0f, .65f, 0f), .20f, .62f, 8, stone, 0f);
            CreatePrism(root.transform, "Iron Fire Bowl", new Vector3(0f, 1.02f, 0f), .56f, .22f, 12, stone, 15f);
            CreatePrism(root.transform, "Glowing Coals", new Vector3(0f, 1.16f, 0f), .39f, .08f, 12, fire, 0f);

            Light light = new GameObject("Warm Flame Light").AddComponent<Light>();
            light.transform.SetParent(root.transform, false);
            light.transform.localPosition = new Vector3(0f, 1.38f, 0f);
            light.type = LightType.Point;
            light.color = new Color(1f, .48f, .15f);
            light.range = 9f;
            light.intensity = 2.4f;
            light.shadows = LightShadows.Soft;
            return root;
        }

        private static GameObject CreatePrism(Transform parent, string name, Vector3 localPosition, float radius, float height, int sides, Material material, float yaw)
        {
            GameObject part = new GameObject(name);
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            part.AddComponent<MeshFilter>().sharedMesh = BuildPrismMesh(name + " Mesh", radius, height, sides);
            part.AddComponent<MeshRenderer>().sharedMaterial = material;
            return part;
        }

        private static Mesh BuildPrismMesh(string name, float radius, float height, int sides)
        {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();
            float halfHeight = height * .5f;

            for (int side = 0; side < sides; side++)
            {
                float a0 = side * Mathf.PI * 2f / sides;
                float a1 = (side + 1) * Mathf.PI * 2f / sides;
                Vector3 p0 = new Vector3(Mathf.Cos(a0) * radius, -halfHeight, Mathf.Sin(a0) * radius);
                Vector3 p1 = new Vector3(Mathf.Cos(a1) * radius, -halfHeight, Mathf.Sin(a1) * radius);
                Vector3 p2 = new Vector3(p1.x, halfHeight, p1.z);
                Vector3 p3 = new Vector3(p0.x, halfHeight, p0.z);
                int start = vertices.Count;
                Vector3 normal = Vector3.Cross(p1 - p0, p3 - p0).normalized;
                vertices.Add(p0); vertices.Add(p1); vertices.Add(p2); vertices.Add(p3);
                normals.Add(normal); normals.Add(normal); normals.Add(normal); normals.Add(normal);
                uvs.Add(Vector2.zero); uvs.Add(Vector2.right); uvs.Add(Vector2.one); uvs.Add(Vector2.up);
                triangles.Add(start); triangles.Add(start + 2); triangles.Add(start + 1);
                triangles.Add(start); triangles.Add(start + 3); triangles.Add(start + 2);
            }

            AddCap(vertices, normals, uvs, triangles, radius, halfHeight, sides, true);
            AddCap(vertices, normals, uvs, triangles, radius, -halfHeight, sides, false);
            Mesh mesh = new Mesh { name = name };
            mesh.SetVertices(vertices); mesh.SetNormals(normals); mesh.SetUVs(0, uvs); mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds(); mesh.RecalculateTangents();
            return mesh;
        }

        private static void AddCap(List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> triangles, float radius, float y, int sides, bool top)
        {
            int center = vertices.Count;
            vertices.Add(new Vector3(0f, y, 0f));
            normals.Add(top ? Vector3.up : Vector3.down);
            uvs.Add(new Vector2(.5f, .5f));
            for (int side = 0; side < sides; side++)
            {
                float angle = side * Mathf.PI * 2f / sides;
                vertices.Add(new Vector3(Mathf.Cos(angle) * radius, y, Mathf.Sin(angle) * radius));
                normals.Add(top ? Vector3.up : Vector3.down);
                uvs.Add(new Vector2(Mathf.Cos(angle) * .5f + .5f, Mathf.Sin(angle) * .5f + .5f));
            }
            for (int side = 0; side < sides; side++)
            {
                int current = center + 1 + side;
                int next = center + 1 + (side + 1) % sides;
                if (top) { triangles.Add(center); triangles.Add(next); triangles.Add(current); }
                else { triangles.Add(center); triangles.Add(current); triangles.Add(next); }
            }
        }
    }
}
