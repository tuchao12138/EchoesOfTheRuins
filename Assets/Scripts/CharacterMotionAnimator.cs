using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace EchoesOfTheRuins
{
    /// <summary>Blends semantic animation roles while gameplay remains authoritative for movement.</summary>
    [ExecuteAlways]
    public sealed class CharacterMotionAnimator : MonoBehaviour
    {
        private readonly Dictionary<AnimationRole, int> inputs = new Dictionary<AnimationRole, int>();
        private readonly Dictionary<AnimationRole, AnimationClipPlayable> clips = new Dictionary<AnimationRole, AnimationClipPlayable>();
        private PlayableGraph graph;
        private AnimationMixerPlayable mixer;
        private float[] blendStartWeights = Array.Empty<float>();
        private int targetInput = -1;
        private float blendDuration;
        private float blendElapsed;

        public AnimationRole CurrentRole { get; private set; } = AnimationRole.Idle;
        public Animator TargetAnimator { get; private set; }

        public void Configure(Transform targetVisual, CharacterAssetId assetId)
        {
            DestroyGraph();
            inputs.Clear();
            clips.Clear();
            targetInput = -1;
            blendDuration = 0f;
            blendElapsed = 0f;
            CurrentRole = AnimationRole.Idle;
            if (targetVisual == null) return;

            Animator animator = targetVisual.GetComponentInChildren<Animator>(true);
            if (animator == null) animator = targetVisual.gameObject.AddComponent<Animator>();
            if (animator.avatar == null)
            {
                foreach (Avatar avatar in Resources.LoadAll<Avatar>(CharacterAssetCatalog.GetPath(assetId)))
                {
                    if (avatar == null || !avatar.isValid) continue;
                    animator.avatar = avatar;
                    break;
                }
                if (animator.avatar == null)
                {
                    string rootMotionBone = targetVisual.childCount > 0 ? targetVisual.GetChild(0).name : string.Empty;
                    Avatar generatedAvatar = AvatarBuilder.BuildGenericAvatar(targetVisual.gameObject, rootMotionBone);
                    if (generatedAvatar != null && generatedAvatar.isValid) animator.avatar = generatedAvatar;
                }
            }
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            TargetAnimator = animator;

            string resourcesPath = CharacterAssetCatalog.GetPath(assetId);
            CharacterAnimationSet animationSet = CharacterAnimationSet.Create(
                assetId,
                Resources.LoadAll<AnimationClip>(resourcesPath));

            var resolved = new List<KeyValuePair<AnimationRole, AnimationClip>>();
            foreach (AnimationRole role in Enum.GetValues(typeof(AnimationRole)))
            {
                AnimationClip clip = animationSet.Resolve(role);
                if (clip != null) resolved.Add(new KeyValuePair<AnimationRole, AnimationClip>(role, clip));
            }

            graph = PlayableGraph.Create($"{name} Semantic Animation");
            graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            mixer = AnimationMixerPlayable.Create(graph, resolved.Count);
            blendStartWeights = new float[resolved.Count];

            for (int index = 0; index < resolved.Count; index++)
            {
                KeyValuePair<AnimationRole, AnimationClip> entry = resolved[index];
                AnimationClipPlayable playable = AnimationClipPlayable.Create(graph, entry.Value);
                graph.Connect(playable, 0, mixer, index);
                mixer.SetInputWeight(index, 0f);
                inputs.Add(entry.Key, index);
                clips.Add(entry.Key, playable);
            }

            AnimationPlayableOutput output = AnimationPlayableOutput.Create(graph, "Semantic Animation", animator);
            output.SetSourcePlayable(mixer);

            if (inputs.TryGetValue(AnimationRole.Idle, out targetInput))
                mixer.SetInputWeight(targetInput, 1f);

            if (isActiveAndEnabled) graph.Play();
        }

        // Compatibility for the pre-existing scene bootstrap while it migrates to CharacterAssetId.
        public void Configure(Transform targetVisual, string resourcesPath)
        {
            CharacterAssetId assetId = string.Equals(
                resourcesPath,
                CharacterAssetCatalog.GetPath(CharacterAssetId.Guardian),
                StringComparison.OrdinalIgnoreCase)
                ? CharacterAssetId.Guardian
                : CharacterAssetId.Explorer;
            Configure(targetVisual, assetId);
        }

        public bool HasRole(AnimationRole role) => inputs.ContainsKey(role);

        public void Play(AnimationRole role, float blendSeconds = .12f, bool restart = false)
        {
            if (!graph.IsValid() || !inputs.TryGetValue(role, out int nextInput)) return;

            if (restart && clips.TryGetValue(role, out AnimationClipPlayable playable))
            {
                playable.SetTime(0d);
                playable.SetDone(false);
            }

            if (role == CurrentRole && nextInput == targetInput) return;

            CurrentRole = role;
            targetInput = nextInput;
            blendElapsed = 0f;
            blendDuration = Mathf.Max(0f, blendSeconds);
            for (int index = 0; index < blendStartWeights.Length; index++)
                blendStartWeights[index] = mixer.GetInputWeight(index);

            if (blendDuration <= 0f) ApplyBlendWeights(1f);
        }

        private void Update()
        {
            AdvanceBlend(Time.deltaTime);
        }

        private void AdvanceBlend(float deltaTime)
        {
            if (!graph.IsValid() || blendDuration <= 0f || targetInput < 0) return;
            blendElapsed += Mathf.Max(0f, deltaTime);
            float progress = Mathf.Clamp01(blendElapsed / blendDuration);
            ApplyBlendWeights(progress);
            if (progress >= 1f) blendDuration = 0f;
        }

        private void ApplyBlendWeights(float progress)
        {
            for (int index = 0; index < blendStartWeights.Length; index++)
            {
                float desiredWeight = index == targetInput ? 1f : 0f;
                mixer.SetInputWeight(index, Mathf.Lerp(blendStartWeights[index], desiredWeight, progress));
            }
        }

        private void OnEnable()
        {
            if (graph.IsValid()) graph.Play();
        }

        private void OnDisable()
        {
            if (graph.IsValid()) graph.Stop();
        }

        private void OnDestroy() => DestroyGraph();

        private void DestroyGraph()
        {
            if (graph.IsValid()) graph.Destroy();
            graph = default;
            mixer = default;
            TargetAnimator = null;
        }
    }
}
