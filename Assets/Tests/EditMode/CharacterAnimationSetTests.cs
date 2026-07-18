using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace EchoesOfTheRuins.Tests
{
    public sealed class CharacterAnimationSetTests
    {
        [TestCase(CharacterAssetId.Explorer)]
        [TestCase(CharacterAssetId.Guardian)]
        public void ProductionCharacter_ResolvesEveryRequiredRole(CharacterAssetId id)
        {
            AnimationClip[] clips = Resources.LoadAll<AnimationClip>(CharacterAssetCatalog.GetPath(id));
            CharacterAnimationSet set = CharacterAnimationSet.Create(id, clips);
            AnimationRole[] required = id == CharacterAssetId.Explorer
                ? new[] { AnimationRole.Idle, AnimationRole.Walk, AnimationRole.Run, AnimationRole.Sprint,
                    AnimationRole.Crouch, AnimationRole.Jump, AnimationRole.Throw, AnimationRole.Hit }
                : new[] { AnimationRole.Idle, AnimationRole.Walk, AnimationRole.Run,
                    AnimationRole.Attack, AnimationRole.Hit };

            Assert.That(required.All(role => set.Resolve(role) != null), Is.True,
                string.Join(", ", required.Where(role => set.Resolve(role) == null)));
        }
    }
}
