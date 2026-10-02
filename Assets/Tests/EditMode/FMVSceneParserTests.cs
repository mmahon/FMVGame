using System.Linq;
using FMV.Core;
using FMV.Data;
using NUnit.Framework;
using UnityEngine.TestTools;
using System.Text.RegularExpressions;

namespace FMV.Tests
{
    public class FMVSceneParserTests
    {
        const string Sample = @"===== SCENE: start =====
Title: Start
Description: The beginning.
Ending: No
Auto Items: key, photo
Decision Flags: seen_intro

CHOICES:
- Open the door → hall
  [requires: item:key, !item:bomb, visited:start, !visited:hall, decision:met, !decision:fled] [sets: opened, loud]
- Wait -> wait_scene

===== SCENE: hall =====
Title: Hall
Ending: yes
Auto Advance: end_a [requires: !visited:end_a]
Auto Advance: end_b
";

        [Test]
        public void ParsesSceneProperties()
        {
            var s = FMVSceneParser.ParseScenes(Sample)["start"];
            Assert.AreEqual("Start", s.Title);
            Assert.AreEqual("The beginning.", s.Description);
            Assert.IsFalse(s.IsEnding);
            CollectionAssert.AreEqual(new[] { "key", "photo" }, s.AutoItems);
            CollectionAssert.AreEqual(new[] { "seen_intro" }, s.DecisionFlags);
        }

        [Test]
        public void ParsesAllSixConditionTypesOnAChoice()
        {
            var choice = FMVSceneParser.ParseScenes(Sample)["start"].Choices[0];
            var parsed = choice.Conditions.Select(c => (c.Type, c.TargetId)).ToArray();
            CollectionAssert.AreEqual(new[]
            {
                (ConditionType.HasItem, "key"),
                (ConditionType.DoesNotHaveItem, "bomb"),
                (ConditionType.HasVisited, "start"),
                (ConditionType.HasNotVisited, "hall"),
                (ConditionType.HasMadeDecision, "met"),
                (ConditionType.HasNotMadeDecision, "fled"),
            }, parsed);
        }

        [Test]
        public void ParsesChoiceTargetsAndSetsFlags()
        {
            var choices = FMVSceneParser.ParseScenes(Sample)["start"].Choices;
            Assert.AreEqual("Open the door", choices[0].DisplayText);
            Assert.AreEqual("hall", choices[0].TargetSceneId);
            CollectionAssert.AreEqual(new[] { "opened", "loud" }, choices[0].SetsFlags);
            Assert.AreEqual("wait_scene", choices[1].TargetSceneId);
            Assert.AreEqual(0, choices[1].Conditions.Count);
        }

        [Test]
        public void ParsesAutoAdvanceWithAndWithoutConditions()
        {
            var s = FMVSceneParser.ParseScenes(Sample)["hall"];
            Assert.IsTrue(s.IsEnding);
            Assert.AreEqual(2, s.AutoAdvances.Count);
            Assert.AreEqual("end_a", s.AutoAdvances[0].TargetSceneId);
            Assert.AreEqual(ConditionType.HasNotVisited, s.AutoAdvances[0].Conditions.Single().Type);
            Assert.AreEqual("end_a", s.AutoAdvances[0].Conditions.Single().TargetId);
            Assert.AreEqual("end_b", s.AutoAdvances[1].TargetSceneId);
            Assert.AreEqual(0, s.AutoAdvances[1].Conditions.Count);
            Assert.AreEqual("end_a", s.AutoAdvanceSceneId);
        }

        [Test]
        public void UnrecognizedCondition_LogsWarningAndIsSkipped()
        {
            const string text = "===== SCENE: a =====\nTitle: A\n\nCHOICES:\n- Go -> b\n  [requires: item:x, bogus:y]\n";
            LogAssert.Expect(UnityEngine.LogType.Warning, new Regex("Unrecognized condition: 'bogus:y' in scene 'a'"));
            var choice = FMVSceneParser.ParseScenes(text)["a"].Choices[0];
            Assert.AreEqual(1, choice.Conditions.Count);
            Assert.AreEqual(ConditionType.HasItem, choice.Conditions[0].Type);
        }

        [Test]
        public void UnrecognizedAutoAdvanceCondition_UsesAutoAdvanceWording()
        {
            const string text = "===== SCENE: a =====\nTitle: A\nAuto Advance: b [requires: nope:z]\n";
            LogAssert.Expect(UnityEngine.LogType.Warning, new Regex("Unrecognized auto-advance condition: 'nope:z' in scene 'a'"));
            FMVSceneParser.ParseScenes(text);
        }
    }
}
