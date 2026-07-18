using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace EchoesOfTheRuins.Tests
{
    public sealed class CharacterMotionAnimatorTests
    {
        [TestCase(CharacterAssetId.Explorer, AnimationRole.Idle)]
        [TestCase(CharacterAssetId.Explorer, AnimationRole.Run)]
        [TestCase(CharacterAssetId.Explorer, AnimationRole.Sprint)]
        [TestCase(CharacterAssetId.Guardian, AnimationRole.Attack)]
        public void Configure_ProvidesNamedRole(CharacterAssetId id, AnimationRole role)
        {
            GameObject root = new GameObject("Character Root");
            GameObject visual = Object.Instantiate(Resources.Load<GameObject>(CharacterAssetCatalog.GetPath(id)), root.transform);
            CharacterMotionAnimator driver = root.AddComponent<CharacterMotionAnimator>();
            try
            {
                driver.Configure(visual.transform, id);
                Assert.That(driver.HasRole(role), Is.True);
                driver.Play(role, .1f, true);
                Assert.That(driver.CurrentRole, Is.EqualTo(role));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Configure_DisablesRootMotionAndStartsIdle()
        {
            GameObject root = new GameObject("Character Root");
            GameObject visual = Object.Instantiate(Resources.Load<GameObject>(CharacterAssetCatalog.GetPath(CharacterAssetId.Explorer)), root.transform);
            CharacterMotionAnimator driver = root.AddComponent<CharacterMotionAnimator>();
            try
            {
                driver.Configure(visual.transform, CharacterAssetId.Explorer);

                Animator animator = visual.GetComponent<Animator>();
                Assert.That(animator, Is.Not.Null);
                Assert.That(animator.applyRootMotion, Is.False);
                Assert.That(driver.CurrentRole, Is.EqualTo(AnimationRole.Idle));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Play_UnavailableRoleKeepsCurrentRole()
        {
            GameObject root = new GameObject("Character Root");
            GameObject visual = Object.Instantiate(Resources.Load<GameObject>(CharacterAssetCatalog.GetPath(CharacterAssetId.Explorer)), root.transform);
            CharacterMotionAnimator driver = root.AddComponent<CharacterMotionAnimator>();
            try
            {
                driver.Configure(visual.transform, CharacterAssetId.Explorer);

                const AnimationRole unavailableRole = (AnimationRole)999;
                Assert.That(driver.HasRole(unavailableRole), Is.False);
                driver.Play(unavailableRole, 0f, true);
                Assert.That(driver.CurrentRole, Is.EqualTo(AnimationRole.Idle));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void DestroyingComponent_DestroysPlayableGraph()
        {
            GameObject root = new GameObject("Character Root");
            GameObject visual = Object.Instantiate(Resources.Load<GameObject>(CharacterAssetCatalog.GetPath(CharacterAssetId.Explorer)), root.transform);
            CharacterMotionAnimator driver = root.AddComponent<CharacterMotionAnimator>();
            driver.Configure(visual.transform, CharacterAssetId.Explorer);
            FieldInfo graphField = typeof(CharacterMotionAnimator).GetField("graph", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(graphField, Is.Not.Null);
            PlayableGraph graph = (PlayableGraph)graphField.GetValue(driver);

            Object.DestroyImmediate(root);

            Assert.That(graph.IsValid(), Is.False);
        }

        [Test]
        public void Configure_CanRebuildGraphAndRestartCurrentRole()
        {
            GameObject root = new GameObject("Character Root");
            GameObject visual = Object.Instantiate(Resources.Load<GameObject>(CharacterAssetCatalog.GetPath(CharacterAssetId.Explorer)), root.transform);
            CharacterMotionAnimator driver = root.AddComponent<CharacterMotionAnimator>();
            try
            {
                driver.Configure(visual.transform, CharacterAssetId.Explorer);
                driver.Play(AnimationRole.Run, 0f, true);
                driver.Play(AnimationRole.Run, .1f, true);
                driver.Configure(visual.transform, CharacterAssetId.Explorer);

                Assert.That(driver.CurrentRole, Is.EqualTo(AnimationRole.Idle));
                Assert.That(driver.HasRole(AnimationRole.Run), Is.True);
            }
            finally
            {
                Assert.DoesNotThrow(() => Object.DestroyImmediate(root));
            }
        }

        [Test]
        public void Play_ZeroBlendImmediatelySelectsTargetMixerInput()
        {
            GameObject root = CreateConfiguredDriver(CharacterAssetId.Explorer, out CharacterMotionAnimator driver);
            try
            {
                driver.Play(AnimationRole.Run, 0f);

                Assert.That(GetWeight(driver, AnimationRole.Idle), Is.EqualTo(0f).Within(.001f));
                Assert.That(GetWeight(driver, AnimationRole.Run), Is.EqualTo(1f).Within(.001f));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Play_ShortBlendCrossfadesAndCompletesAtTargetWeights()
        {
            GameObject root = CreateConfiguredDriver(CharacterAssetId.Explorer, out CharacterMotionAnimator driver);
            try
            {
                driver.Play(AnimationRole.Run, .1f);

                Assert.That(GetWeight(driver, AnimationRole.Idle), Is.EqualTo(1f).Within(.001f));
                Assert.That(GetWeight(driver, AnimationRole.Run), Is.EqualTo(0f).Within(.001f));
                AdvanceBlend(driver, .05f);
                Assert.That(GetWeight(driver, AnimationRole.Idle), Is.EqualTo(.5f).Within(.001f));
                Assert.That(GetWeight(driver, AnimationRole.Run), Is.EqualTo(.5f).Within(.001f));
                AdvanceBlend(driver, .05f);
                Assert.That(GetWeight(driver, AnimationRole.Idle), Is.EqualTo(0f).Within(.001f));
                Assert.That(GetWeight(driver, AnimationRole.Run), Is.EqualTo(1f).Within(.001f));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Play_RestartResetsCurrentPlayableTime()
        {
            GameObject root = CreateConfiguredDriver(CharacterAssetId.Explorer, out CharacterMotionAnimator driver);
            try
            {
                driver.Play(AnimationRole.Run, 0f, true);
                AnimationClipPlayable run = GetClip(driver, AnimationRole.Run);
                run.SetTime(.4d);
                Assert.That(run.GetTime(), Is.EqualTo(.4d).Within(.001d));

                driver.Play(AnimationRole.Run, .1f, true);

                Assert.That(run.GetTime(), Is.EqualTo(0d).Within(.001d));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void DisableStopsGraphAndEnableResumesSameGraph()
        {
            GameObject root = CreateConfiguredDriver(CharacterAssetId.Explorer, out CharacterMotionAnimator driver);
            try
            {
                PlayableGraph graph = GetGraph(driver);
                Assert.That(graph.IsValid(), Is.True);
                Assert.That(graph.IsPlaying(), Is.True);

                driver.enabled = false;

                Assert.That(graph.IsValid(), Is.True);
                Assert.That(graph.IsPlaying(), Is.False);

                driver.enabled = true;

                Assert.That(graph.IsValid(), Is.True);
                Assert.That(graph.IsPlaying(), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [TestCase(LocomotionState.Idle, AnimationRole.Idle)]
        [TestCase(LocomotionState.Walk, AnimationRole.Walk)]
        [TestCase(LocomotionState.Run, AnimationRole.Run)]
        [TestCase(LocomotionState.Sprint, AnimationRole.Sprint)]
        [TestCase(LocomotionState.Crouch, AnimationRole.Crouch)]
        [TestCase(LocomotionState.Jump, AnimationRole.Jump)]
        [TestCase(LocomotionState.Fall, AnimationRole.Fall)]
        [TestCase(LocomotionState.Throw, AnimationRole.Throw)]
        [TestCase(LocomotionState.Hit, AnimationRole.Hit)]
        [TestCase(LocomotionState.Locked, AnimationRole.Idle)]
        public void PlayerController_MapsLocomotionStateToSemanticRole(LocomotionState state, AnimationRole expected)
        {
            Assert.That(InvokeRoleMapping(typeof(PlayerController), state), Is.EqualTo(expected));
        }

        [TestCase(GuardianState.Patrol, AnimationRole.Walk)]
        [TestCase(GuardianState.Investigate, AnimationRole.Walk)]
        [TestCase(GuardianState.Search, AnimationRole.Walk)]
        [TestCase(GuardianState.Chase, AnimationRole.Run)]
        [TestCase(GuardianState.Capture, AnimationRole.Attack)]
        public void GuardianAI_MapsStateToSemanticRole(GuardianState state, AnimationRole expected)
        {
            Assert.That(InvokeRoleMapping(typeof(GuardianAI), state), Is.EqualTo(expected));
        }

        private static AnimationRole InvokeRoleMapping<TState>(System.Type owner, TState state)
        {
            MethodInfo method = owner.GetMethod("ToAnimationRole", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(method, Is.Not.Null, $"{owner.Name} must provide the semantic ToAnimationRole mapping.");
            return (AnimationRole)method.Invoke(null, new object[] { state });
        }

        private static GameObject CreateConfiguredDriver(CharacterAssetId assetId, out CharacterMotionAnimator driver)
        {
            GameObject root = new GameObject("Character Root");
            GameObject visual = Object.Instantiate(Resources.Load<GameObject>(CharacterAssetCatalog.GetPath(assetId)), root.transform);
            driver = root.AddComponent<CharacterMotionAnimator>();
            driver.Configure(visual.transform, assetId);
            return root;
        }

        private static PlayableGraph GetGraph(CharacterMotionAnimator driver)
        {
            FieldInfo field = typeof(CharacterMotionAnimator).GetField("graph", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(field, Is.Not.Null);
            return (PlayableGraph)field.GetValue(driver);
        }

        private static AnimationMixerPlayable GetMixer(CharacterMotionAnimator driver)
        {
            FieldInfo field = typeof(CharacterMotionAnimator).GetField("mixer", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(field, Is.Not.Null);
            return (AnimationMixerPlayable)field.GetValue(driver);
        }

        private static AnimationClipPlayable GetClip(CharacterMotionAnimator driver, AnimationRole role)
        {
            FieldInfo field = typeof(CharacterMotionAnimator).GetField("clips", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(field, Is.Not.Null);
            var clips = (Dictionary<AnimationRole, AnimationClipPlayable>)field.GetValue(driver);
            return clips[role];
        }

        private static float GetWeight(CharacterMotionAnimator driver, AnimationRole role)
        {
            FieldInfo field = typeof(CharacterMotionAnimator).GetField("inputs", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(field, Is.Not.Null);
            var inputs = (Dictionary<AnimationRole, int>)field.GetValue(driver);
            return GetMixer(driver).GetInputWeight(inputs[role]);
        }

        private static void AdvanceBlend(CharacterMotionAnimator driver, float deltaTime)
        {
            MethodInfo method = typeof(CharacterMotionAnimator).GetMethod("AdvanceBlend", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(method, Is.Not.Null, "CharacterMotionAnimator must expose a deterministic blend step internally.");
            method.Invoke(driver, new object[] { deltaTime });
        }
    }
}
