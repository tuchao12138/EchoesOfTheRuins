using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace EchoesOfTheRuins
{
    /// <summary>Builds one draw-call wall from individually offset stone blocks.</summary>
    public static class MasonryWallBuilder
    {
        private const float CourseHeight = .68f;
        private const float NominalStoneLength = 1.45f;

        public static GameObject Create(string name, Vector3 position, Vector3 size, Material material)
        {
            GameObject wall = new GameObject(name);
            wall.transform.position = position;
            Mesh mesh = BuildMesh(name + " Mesh", size);
            wall.AddComponent<MeshFilter>().sharedMesh = mesh;
            wall.AddComponent<MeshRenderer>().sharedMaterial = material;
            return wall;
        }

        public static Mesh BuildMesh(string name, Vector3 size)
        {
            bool alongX = size.x >= size.z;
            float length = alongX ? size.x : size.z;
            float thickness = Mathf.Max(.38f, alongX ? size.z : size.x);
            int courses = Mathf.Max(1, Mathf.CeilToInt(size.y / CourseHeight));
            int columns = Mathf.Max(1, Mathf.CeilToInt(length / NominalStoneLength));
            float stoneLength = length / columns;
            float stoneHeight = size.y / courses;

            var vertices = new List<Vector3>(courses * columns * 24);
            var normals = new List<Vector3>(courses * columns * 24);
            var uvs = new List<Vector2>(courses * columns * 24);
            var triangles = new List<int>(courses * columns * 36);

            for (int row = 0; row < courses; row++)
            {
                float stagger = row % 2 == 0 ? 0f : stoneLength * .5f;
                for (int column = -1; column <= columns; column++)
                {
                    float longPosition = -length * .5f + (column + .5f) * stoneLength + stagger;
                    float min = longPosition - stoneLength * .5f;
                    float max = longPosition + stoneLength * .5f;
                    if (max <= -length * .5f || min >= length * .5f) continue;

                    float clippedMin = Mathf.Max(min, -length * .5f);
                    float clippedMax = Mathf.Min(max, length * .5f);
                    float clippedLength = clippedMax - clippedMin;
                    float centerLong = (clippedMin + clippedMax) * .5f;
                    int seed = row * 97 + column * 37;
                    float depthOffset = Mathf.Sin(seed * 1.73f) * .035f;
                    float yOffset = Mathf.Cos(seed * 2.11f) * .018f;
                    Vector3 center = alongX
                        ? new Vector3(centerLong, -size.y * .5f + (row + .5f) * stoneHeight + yOffset, depthOffset)
                        : new Vector3(depthOffset, -size.y * .5f + (row + .5f) * stoneHeight + yOffset, centerLong);
                    Vector3 blockSize = alongX
                        ? new Vector3(Mathf.Max(.2f, clippedLength - .055f), stoneHeight - .045f, thickness)
                        : new Vector3(thickness, stoneHeight - .045f, Mathf.Max(.2f, clippedLength - .055f));
                    Quaternion rotation = Quaternion.Euler(0f, Mathf.Sin(seed) * .7f, Mathf.Cos(seed) * .28f);
                    AddBox(vertices, normals, uvs, triangles, Matrix4x4.TRS(center, rotation, blockSize));
                }
            }

            Mesh mesh = new Mesh { name = name, indexFormat = IndexFormat.UInt32 };
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            return mesh;
        }

        private static void AddBox(List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> triangles, Matrix4x4 matrix)
        {
            Vector3[] faceNormals = { Vector3.forward, Vector3.back, Vector3.right, Vector3.left, Vector3.up, Vector3.down };
            Vector3[,] faceVertices =
            {
                { new Vector3(-.5f,-.5f,.5f), new Vector3(.5f,-.5f,.5f), new Vector3(.5f,.5f,.5f), new Vector3(-.5f,.5f,.5f) },
                { new Vector3(.5f,-.5f,-.5f), new Vector3(-.5f,-.5f,-.5f), new Vector3(-.5f,.5f,-.5f), new Vector3(.5f,.5f,-.5f) },
                { new Vector3(.5f,-.5f,.5f), new Vector3(.5f,-.5f,-.5f), new Vector3(.5f,.5f,-.5f), new Vector3(.5f,.5f,.5f) },
                { new Vector3(-.5f,-.5f,-.5f), new Vector3(-.5f,-.5f,.5f), new Vector3(-.5f,.5f,.5f), new Vector3(-.5f,.5f,-.5f) },
                { new Vector3(-.5f,.5f,.5f), new Vector3(.5f,.5f,.5f), new Vector3(.5f,.5f,-.5f), new Vector3(-.5f,.5f,-.5f) },
                { new Vector3(-.5f,-.5f,-.5f), new Vector3(.5f,-.5f,-.5f), new Vector3(.5f,-.5f,.5f), new Vector3(-.5f,-.5f,.5f) }
            };
            Vector2[] faceUvs = { Vector2.zero, Vector2.right, Vector2.one, Vector2.up };

            for (int face = 0; face < 6; face++)
            {
                int start = vertices.Count;
                Vector3 normal = matrix.MultiplyVector(faceNormals[face]).normalized;
                for (int corner = 0; corner < 4; corner++)
                {
                    vertices.Add(matrix.MultiplyPoint3x4(faceVertices[face, corner]));
                    normals.Add(normal);
                    uvs.Add(faceUvs[corner]);
                }
                triangles.Add(start); triangles.Add(start + 1); triangles.Add(start + 2);
                triangles.Add(start); triangles.Add(start + 2); triangles.Add(start + 3);
            }
        }
    }
}
