using UnityEngine;

namespace EchoesOfTheRuins
{
    /// <summary>Builds readable magical landmarks without exposing primitive debug meshes.</summary>
    public static class LandmarkVisualBuilder
    {
        private const int RingSegments = 48;

        public static void CreateCore(Transform root, Material crystalMaterial, Material runeMaterial)
        {
            GameObject crystal = new GameObject("Crystal");
            crystal.transform.SetParent(root, false);
            crystal.transform.localPosition = new Vector3(0f, .2f, 0f);
            crystal.transform.localScale = new Vector3(.72f, 1.25f, .72f);
            crystal.AddComponent<MeshFilter>().sharedMesh = CreateCrystalMesh();
            crystal.AddComponent<MeshRenderer>().sharedMaterial = crystalMaterial;

            LineRenderer ringA = CreateRing("Rune Ring A", root, runeMaterial, .72f, .02f);
            ringA.transform.localPosition = new Vector3(0f, -.18f, 0f);
            ringA.transform.localRotation = Quaternion.Euler(68f, 0f, 12f);

            LineRenderer ringB = CreateRing("Rune Ring B", root, runeMaterial, .56f, .018f);
            ringB.transform.localPosition = new Vector3(0f, .30f, 0f);
            ringB.transform.localRotation = Quaternion.Euler(74f, 42f, 0f);

            Light glow = new GameObject("Core Glow").AddComponent<Light>();
            glow.transform.SetParent(root, false);
            glow.transform.localPosition = new Vector3(0f, .25f, 0f);
            glow.type = LightType.Point;
            glow.color = new Color(.12f, .86f, 1f);
            glow.range = 5.5f;
            glow.intensity = 2.4f;
        }

        public static void CreateCheckpoint(Transform root, Material runeMaterial)
        {
            LineRenderer outer = CreateRing("Outer Rune", root, runeMaterial, 1.15f, .035f);
            outer.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            outer.transform.localPosition = new Vector3(0f, .035f, 0f);

            LineRenderer inner = CreateRing("Inner Rune", root, runeMaterial, .72f, .022f);
            inner.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            inner.transform.localPosition = new Vector3(0f, .04f, 0f);

            for (int index = 0; index < 4; index++)
            {
                LineRenderer glyph = new GameObject($"Rune Mark {index + 1}").AddComponent<LineRenderer>();
                glyph.transform.SetParent(root, false);
                glyph.transform.localPosition = new Vector3(0f, .045f, 0f);
                glyph.useWorldSpace = false;
                glyph.sharedMaterial = runeMaterial;
                glyph.widthMultiplier = .03f;
                glyph.positionCount = 3;
                glyph.SetPositions(new[]
                {
                    Quaternion.Euler(0f, index * 90f, 0f) * new Vector3(.76f, 0f, 0f),
                    Quaternion.Euler(0f, index * 90f, 0f) * new Vector3(.94f, 0f, .12f),
                    Quaternion.Euler(0f, index * 90f, 0f) * new Vector3(1.08f, 0f, 0f)
                });
            }
        }

        public static void CreateGateBarrier(Transform root, Material runeMaterial)
        {
            for (int index = 0; index < 5; index++)
            {
                float x = -1.8f + index * .9f;
                CreateRuneLine($"Seal Column {index + 1}", root, runeMaterial,
                    new Vector3(x, -2f, 0f), new Vector3(x, 2f, 0f), .055f);
            }
            CreateRuneLine("Seal Diagonal A", root, runeMaterial,
                new Vector3(-2.2f, -1.8f, 0f), new Vector3(2.2f, 1.8f, 0f), .04f);
            CreateRuneLine("Seal Diagonal B", root, runeMaterial,
                new Vector3(-2.2f, 1.8f, 0f), new Vector3(2.2f, -1.8f, 0f), .04f);
            LineRenderer crown = CreateRing("Seal Crown Rune", root, runeMaterial, .7f, .045f);
            crown.transform.localPosition = new Vector3(0f, 1.35f, 0f);
        }

        private static LineRenderer CreateRuneLine(string name, Transform parent, Material material, Vector3 from, Vector3 to, float width)
        {
            LineRenderer line = new GameObject(name).AddComponent<LineRenderer>();
            line.transform.SetParent(parent, false);
            line.useWorldSpace = false;
            line.sharedMaterial = material;
            line.widthMultiplier = width;
            line.numCapVertices = 3;
            line.positionCount = 2;
            line.SetPosition(0, from);
            line.SetPosition(1, to);
            return line;
        }

        private static LineRenderer CreateRing(string name, Transform parent, Material material, float radius, float width)
        {
            LineRenderer line = new GameObject(name).AddComponent<LineRenderer>();
            line.transform.SetParent(parent, false);
            line.useWorldSpace = false;
            line.loop = true;
            line.sharedMaterial = material;
            line.widthMultiplier = width;
            line.numCornerVertices = 3;
            line.numCapVertices = 3;
            line.positionCount = RingSegments;
            for (int index = 0; index < RingSegments; index++)
            {
                float angle = index * Mathf.PI * 2f / RingSegments;
                line.SetPosition(index, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f));
            }
            return line;
        }

        private static Mesh CreateCrystalMesh()
        {
            Vector3[] vertices =
            {
                new Vector3(0f, 1f, 0f),
                new Vector3(.56f, .18f, 0f),
                new Vector3(0f, .18f, .56f),
                new Vector3(-.56f, .18f, 0f),
                new Vector3(0f, .18f, -.56f),
                new Vector3(0f, -1f, 0f)
            };
            int[] triangles =
            {
                0, 2, 1, 0, 3, 2, 0, 4, 3, 0, 1, 4,
                5, 1, 2, 5, 2, 3, 5, 3, 4, 5, 4, 1
            };
            Mesh mesh = new Mesh { name = "Energy Core Crystal" };
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
