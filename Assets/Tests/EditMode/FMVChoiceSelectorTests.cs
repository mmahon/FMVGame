using System.Collections.Generic;
using System.Linq;
using FMV.Core;
using FMV.Data;
using NUnit.Framework;

namespace FMV.Tests
{
    public class FMVChoiceSelectorTests
    {
        [SetUp]
        public void SetUp() => FMVGameState.Instance.Reset();

        [TearDown]
        public void TearDown() => FMVGameState.Instance.Reset();

        static FMVChoice Plain(string name) => new FMVChoice { DisplayText = name, TargetSceneId = name };

        static FMVChoice Conditional(string name, ConditionType type, string id)
        {
            var c = Plain(name);
            c.Conditions.Add(new FMVCondition(type, id));
            return c;
        }

        // Keeps file order so tests can assert on selection without depending on the shuffle.
        static List<string> Names(IEnumerable<FMVChoice> selected, List<FMVChoice> source) =>
            selected.OrderBy(source.IndexOf).Select(c => c.DisplayText).ToList();

        [Test]
        public void FewerThanThree_ShowsAllValid()
        {
            var all = new List<FMVChoice> { Plain("a"), Plain("b") };
            var result = FMVChoiceSelector.SelectForDisplay(all);
            CollectionAssert.AreEquivalent(new[] { "a", "b" }, Names(result, all));
        }

        [Test]
        public void UnavailableChoices_AreExcluded()
        {
            var all = new List<FMVChoice> { Conditional("locked", ConditionType.HasItem, "key"), Plain("a") };
            var result = FMVChoiceSelector.SelectForDisplay(all);
            CollectionAssert.AreEqual(new[] { "a" }, Names(result, all));
        }

        [Test]
        public void MoreThanThreeUnconditional_KeepsFirstThreeInFileOrder()
        {
            var all = new List<FMVChoice> { Plain("a"), Plain("b"), Plain("c"), Plain("d"), Plain("e") };
            var result = FMVChoiceSelector.SelectForDisplay(all);
            CollectionAssert.AreEqual(new[] { "a", "b", "c" }, Names(result, all));
        }

        [Test]
        public void ValidConditionalChoice_BeatsEarlierUnconditionalChoices()
        {
            FMVGameState.Instance.AddItem("key");
            var all = new List<FMVChoice>
            {
                Plain("a"), Plain("b"), Plain("c"), Plain("d"),
                Conditional("unlocked", ConditionType.HasItem, "key"),
            };
            var result = FMVChoiceSelector.SelectForDisplay(all);
            CollectionAssert.AreEqual(new[] { "a", "b", "unlocked" }, Names(result, all));
        }

        [Test]
        public void NegativeCondition_CountsAsConditional()
        {
            var all = new List<FMVChoice>
            {
                Plain("a"), Plain("b"), Plain("c"),
                Conditional("no_bomb", ConditionType.DoesNotHaveItem, "bomb"),
            };
            var result = FMVChoiceSelector.SelectForDisplay(all);
            CollectionAssert.AreEqual(new[] { "a", "b", "no_bomb" }, Names(result, all));
        }

        [Test]
        public void MoreThanThreeConditional_CapsAtThreeAndDropsUnconditional()
        {
            var all = new List<FMVChoice>
            {
                Plain("plain"),
                Conditional("c1", ConditionType.HasNotVisited, "x"),
                Conditional("c2", ConditionType.HasNotVisited, "x"),
                Conditional("c3", ConditionType.HasNotVisited, "x"),
                Conditional("c4", ConditionType.HasNotVisited, "x"),
            };
            var result = FMVChoiceSelector.SelectForDisplay(all);
            CollectionAssert.AreEqual(new[] { "c1", "c2", "c3" }, Names(result, all));
        }

        [Test]
        public void Shuffle_AppliesAfterSelection()
        {
            var all = new List<FMVChoice> { Plain("a"), Plain("b"), Plain("c"), Plain("d") };
            // Reverse key: if the shuffle ran before the cap, "d" would be selected instead of dropped.
            var result = FMVChoiceSelector.SelectForDisplay(all, 3, c => -all.IndexOf(c));
            CollectionAssert.AreEqual(new[] { "c", "b", "a" }, result.Select(c => c.DisplayText).ToList());
        }
    }
}
