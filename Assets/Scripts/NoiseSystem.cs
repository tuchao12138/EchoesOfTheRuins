using System;
using UnityEngine;

namespace EchoesOfTheRuins
{
    public static class NoiseSystem
    {
        public static event Action<Vector3, float> NoiseCreated;

        public static void Emit(Vector3 position, float radius)
        {
            NoiseCreated?.Invoke(position, radius);
        }
    }
}
