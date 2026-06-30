using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using FMV.Data;

namespace FMV.Core
{
    public static class FMVSceneParser
    {
        private static readonly Regex RequiresRegex = new Regex(@"\[\s*requires\s*:\s*(.+?)\s*\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex SetsRegex = new Regex(@"\[\s*sets\s*:\s*(.+?)\s*\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static Dictionary<string, FMVScene> ParseScenes(string text)
        {
            var scenes = new Dictionary<string, FMVScene>();
            if (string.IsNullOrEmpty(text)) return scenes;

            string[] blocks = text.Split(new[] { "===== SCENE: " }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var block in blocks)
            {
                string rawBlock = block.Trim();
                int endIdIndex = rawBlock.IndexOf("=====");
                if (endIdIndex == -1) continue;

                string sceneId = rawBlock.Substring(0, endIdIndex).Trim();
                string content = rawBlock.Substring(endIdIndex + 5).Trim();

                FMVScene scene = new FMVScene(sceneId);

                var lines = new List<string>(content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries));
                int choicesIndex = -1;

                for (int i = 0; i < lines.Count; i++)
                {
                    string line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line)) continue;

                    if (line.Equals("CHOICES:", StringComparison.OrdinalIgnoreCase))
                    {
                        choicesIndex = i;
                        break;
                    }

                    int colonIdx = line.IndexOf(':');
                    if (colonIdx > 0)
                    {
                        string key = line.Substring(0, colonIdx).Trim().ToLower();
                        string val = line.Substring(colonIdx + 1).Trim();

                        switch (key)
                        {
                            case "title":
                                scene.Title = val;
                                break;
                            case "description":
                                scene.Description = val;
                                break;
                            case "ending":
                                scene.IsEnding = (val.Equals("yes", StringComparison.OrdinalIgnoreCase) || val.Equals("true", StringComparison.OrdinalIgnoreCase));
                                break;
                            case "auto advance":
                                {
                                    string targetSceneId = val;
                                    var conditions = new List<FMVCondition>();

                                    var reqMatch = RequiresRegex.Match(val);
                                    if (reqMatch.Success)
                                    {
                                        targetSceneId = val.Replace(reqMatch.Value, "").Trim();

                                        string reqs = reqMatch.Groups[1].Value;
                                        var condTokens = reqs.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                        foreach (var cond in condTokens)
                                        {
                                            string c = cond.Trim();
                                            string cLower = c.ToLower();
                                            if (cLower.StartsWith("item:"))
                                                conditions.Add(new FMVCondition(ConditionType.HasItem, c.Substring(5).Trim()));
                                            else if (cLower.StartsWith("!item:"))
                                                conditions.Add(new FMVCondition(ConditionType.DoesNotHaveItem, c.Substring(6).Trim()));
                                            else if (cLower.StartsWith("visited:"))
                                                conditions.Add(new FMVCondition(ConditionType.HasVisited, c.Substring(8).Trim()));
                                            else if (cLower.StartsWith("!visited:"))
                                                conditions.Add(new FMVCondition(ConditionType.HasNotVisited, c.Substring(9).Trim()));
                                            else if (cLower.StartsWith("decision:"))
                                                conditions.Add(new FMVCondition(ConditionType.HasMadeDecision, c.Substring(9).Trim()));
                                            else if (cLower.StartsWith("!decision:"))
                                                conditions.Add(new FMVCondition(ConditionType.HasNotMadeDecision, c.Substring(10).Trim()));
                                            else
                                                Debug.LogWarning($"[FMVParser] Unrecognized auto-advance condition: '{c}' in scene '{sceneId}'");
                                        }
                                    }

                                    scene.AutoAdvances.Add(new FMVAutoAdvance
                                    {
                                        TargetSceneId = targetSceneId,
                                        Conditions = conditions
                                    });

                                    // Keep AutoAdvanceSceneId updated for fallback/compatibility (use first parsed target)
                                    if (string.IsNullOrEmpty(scene.AutoAdvanceSceneId))
                                    {
                                        scene.AutoAdvanceSceneId = targetSceneId;
                                    }
                                }
                                break;
                            case "auto items":
                                foreach (var item in val.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    if (!string.IsNullOrWhiteSpace(item)) scene.AutoItems.Add(item.Trim());
                                }
                                break;
                            case "decision flags":
                                foreach (var flag in val.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    if (!string.IsNullOrWhiteSpace(flag)) scene.DecisionFlags.Add(flag.Trim());
                                }
                                break;
                        }
                    }
                }

                if (choicesIndex != -1)
                {
                    FMVChoice currentChoice = null;

                    for (int i = choicesIndex + 1; i < lines.Count; i++)
                    {
                        string line = lines[i].Trim();
                        if (string.IsNullOrEmpty(line)) continue;

                        if (line.StartsWith("-"))
                        {
                            currentChoice = new FMVChoice();
                            string choiceLine = line.Substring(1).Trim();
                            
                            int arrowIdx = choiceLine.IndexOf("→");
                            if (arrowIdx == -1) arrowIdx = choiceLine.IndexOf("->");

                            if (arrowIdx != -1)
                            {
                                currentChoice.DisplayText = choiceLine.Substring(0, arrowIdx).Trim();
                                int arrowLen = choiceLine.IndexOf("→") != -1 ? 1 : 2;
                                currentChoice.TargetSceneId = choiceLine.Substring(arrowIdx + arrowLen).Trim();
                            }
                            else
                            {
                                currentChoice.DisplayText = choiceLine;
                                currentChoice.TargetSceneId = "";
                            }

                            scene.Choices.Add(currentChoice);
                        }
                        else if (line.StartsWith("[") && line.EndsWith("]") && currentChoice != null)
                        {
                            var reqMatch = RequiresRegex.Match(line);
                            if (reqMatch.Success)
                            {
                                string reqs = reqMatch.Groups[1].Value;
                                var conditions = reqs.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                foreach(var cond in conditions)
                                {
                                    string c = cond.Trim();
                                    string cLower = c.ToLower();
                                    if (cLower.StartsWith("item:"))
                                        currentChoice.Conditions.Add(new FMVCondition(ConditionType.HasItem, c.Substring(5).Trim()));
                                    else if (cLower.StartsWith("!item:"))
                                        currentChoice.Conditions.Add(new FMVCondition(ConditionType.DoesNotHaveItem, c.Substring(6).Trim()));
                                    else if (cLower.StartsWith("visited:"))
                                        currentChoice.Conditions.Add(new FMVCondition(ConditionType.HasVisited, c.Substring(8).Trim()));
                                    else if (cLower.StartsWith("!visited:"))
                                        currentChoice.Conditions.Add(new FMVCondition(ConditionType.HasNotVisited, c.Substring(9).Trim()));
                                    else if (cLower.StartsWith("decision:"))
                                        currentChoice.Conditions.Add(new FMVCondition(ConditionType.HasMadeDecision, c.Substring(9).Trim()));
                                    else if (cLower.StartsWith("!decision:"))
                                        currentChoice.Conditions.Add(new FMVCondition(ConditionType.HasNotMadeDecision, c.Substring(10).Trim()));
                                    else
                                        Debug.LogWarning($"[FMVParser] Unrecognized condition: '{c}' in scene '{sceneId}'");
                                }
                            }

                            var setMatch = SetsRegex.Match(line);
                            if (setMatch.Success)
                            {
                                string setsStr = setMatch.Groups[1].Value;
                                var setsTokens = setsStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                foreach(var t in setsTokens)
                                {
                                    if (!string.IsNullOrWhiteSpace(t)) currentChoice.SetsFlags.Add(t.Trim());
                                }
                            }
                        }
                    }
                }

                scenes[sceneId] = scene;
            }

            return scenes;
        }
    }
}
