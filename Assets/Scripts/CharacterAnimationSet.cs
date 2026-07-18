using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EchoesOfTheRuins
{
    public enum AnimationRole { Idle, Walk, Run, Sprint, Crouch, Jump, Fall, Throw, Hit, Attack }

    public sealed class CharacterAnimationSet
    {
        private static readonly IReadOnlyDictionary<AnimationRole, string[]> Aliases =
            new Dictionary<AnimationRole, string[]>
            {
                [AnimationRole.Idle] = new[] { "Idle_A", "Idle" },
                [AnimationRole.Walk] = new[] { "Walking_A", "Walk" },
                [AnimationRole.Run] = new[] { "Running_A", "Run" },
                [AnimationRole.Sprint] = new[] { "Running_B", "Running_A", "Run" },
                [AnimationRole.Crouch] = new[] { "Walking_C", "Sneaking", "Walking_A" },
                [AnimationRole.Jump] = new[] { "Jump_Full_Short", "Jump_Start", "Jump" },
                [AnimationRole.Fall] = new[] { "Jump_Idle", "Falling", "Jump_Full_Short" },
                [AnimationRole.Throw] = new[] { "Throw", "Interact" },
                [AnimationRole.Hit] = new[] { "Hit_A", "Hit_B", "RecieveHit" },
                [AnimationRole.Attack] = new[] { "1H_Melee_Attack_Chop", "Sword_Attack", "Dagger_Attack" }
            };

        private readonly Dictionary<AnimationRole, AnimationClip> clips;
        private CharacterAnimationSet(Dictionary<AnimationRole, AnimationClip> clips) => this.clips = clips;

        public static CharacterAnimationSet Create(CharacterAssetId _, IEnumerable<AnimationClip> source)
        {
            AnimationClip[] available = source.Where(clip => clip != null && !clip.name.Contains("preview", StringComparison.OrdinalIgnoreCase)).ToArray();
            var resolved = new Dictionary<AnimationRole, AnimationClip>();
            foreach (KeyValuePair<AnimationRole, string[]> pair in Aliases)
            {
                resolved[pair.Key] = pair.Value
                    .Select(alias => available.FirstOrDefault(clip =>
                        string.Equals(clip.name, alias, StringComparison.OrdinalIgnoreCase) ||
                        clip.name.EndsWith("|" + alias, StringComparison.OrdinalIgnoreCase)))
                    .FirstOrDefault(clip => clip != null);
            }
            return new CharacterAnimationSet(resolved);
        }

        public AnimationClip Resolve(AnimationRole role) => clips.TryGetValue(role, out AnimationClip clip) ? clip : null;
    }
}
