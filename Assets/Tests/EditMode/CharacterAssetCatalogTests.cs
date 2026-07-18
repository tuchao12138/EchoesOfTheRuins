using NUnit.Framework;
using UnityEngine;

namespace EchoesOfTheRuins.Tests
{
    public sealed class CharacterAssetCatalogTests
    {
        [TestCase(CharacterAssetId.Explorer, "Characters/Explorer")]
        [TestCase(CharacterAssetId.Guardian, "Characters/Guardian")]
        public void Resources_UseStablePathsForRiggedCharacters(CharacterAssetId assetId, string expectedPath)
        {
            Assert.That(CharacterAssetCatalog.GetPath(assetId), Is.EqualTo(expectedPath));
        }

        [TestCase(CharacterAssetId.Explorer)]
        [TestCase(CharacterAssetId.Guardian)]
        public void EveryCharacterRole_HasAModelAndTexturePath(CharacterAssetId assetId)
        {
            Assert.That(CharacterAssetCatalog.GetPath(assetId), Is.Not.Empty);
            Assert.That(CharacterAssetCatalog.GetTexturePath(assetId), Is.Not.Empty);
        }

        [TestCase(CharacterAssetId.Explorer)]
        [TestCase(CharacterAssetId.Guardian)]
        public void ProductionCharacter_ContainsAtLeastOneEmbeddedAnimation(CharacterAssetId assetId)
        {
            AnimationClip[] clips = Resources.LoadAll<AnimationClip>(CharacterAssetCatalog.GetPath(assetId));
            Assert.That(clips.Length, Is.GreaterThan(0));
        }
    }
}
