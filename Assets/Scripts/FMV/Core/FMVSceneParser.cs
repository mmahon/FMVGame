using System;
using System.Collections.Generic;
using FMV.Data;

namespace FMV.Core
{
    public static class FMVSceneParser
    {
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
                    if (line == "CHOICES:")
                    {
                        choicesIndex = i;
                        break;
                    }

                    if (line.StartsWith("Title:", StringComparison.OrdinalIgnoreCase))
                    {
                        scene.Title = line.Substring(6).Trim();
                    }
                    else if (line.StartsWith("Description:", StringComparison.OrdinalIgnoreCase))
                    {
                        scene.Description = line.Substring(12).Trim();
                    }
                    else if (line.StartsWith("Ending:", StringComparison.OrdinalIgnoreCase))
                    {
                        string val = line.Substring(7).Trim().ToLower();
                        scene.IsEnding = (val == "yes" || val == "true");
                    }
                    else if (line.StartsWith("Auto Items:", StringComparison.OrdinalIgnoreCase))
                    {
                        string itemsStr = line.Substring(11).Trim();
                        var items = itemsStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach(var item in items)
                        {
                            if (!string.IsNullOrWhiteSpace(item)) scene.AutoItems.Add(item.Trim());
                        }
                    }
                    else if (line.StartsWith("Decision Flags:", StringComparison.OrdinalIgnoreCase))
                    {
                        string flagsStr = line.Substring(15).Trim();
                        var flags = flagsStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach(var flag in flags)
                        {
                            if (!string.IsNullOrWhiteSpace(flag)) scene.DecisionFlags.Add(flag.Trim());
                        }
                    }
                }

                if (choicesIndex != -1)
                {
                    FMVChoice currentChoice = null;

                    for (int i = choicesIndex + 1; i < lines.Count; i++)
                    {
                        string line = lines[i].TrimStart();

                        if (line.StartsWith("- "))
                        {
                            currentChoice = new FMVChoice();
                            string choiceLine = line.Substring(2).Trim();
                            
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
                        else if (line.StartsWith("[") && currentChoice != null)
                        {
                            string modifier = line.Trim();
                            if (modifier.StartsWith("[requires: "))
                            {
                                string reqs = modifier.Substring(11, modifier.Length - 12);
                                var conditions = reqs.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                foreach(var cond in conditions)
                                {
                                    string c = cond.Trim();
                                    if (c.StartsWith("item:"))
                                        currentChoice.Conditions.Add(new FMVCondition(ConditionType.HasItem, c.Substring(5).Trim()));
                                    else if (c.StartsWith("!item:"))
                                        currentChoice.Conditions.Add(new FMVCondition(ConditionType.DoesNotHaveItem, c.Substring(6).Trim()));
                                    else if (c.StartsWith("visited:"))
                                        currentChoice.Conditions.Add(new FMVCondition(ConditionType.HasVisited, c.Substring(8).Trim()));
                                    else if (c.StartsWith("!visited:"))
                                        currentChoice.Conditions.Add(new FMVCondition(ConditionType.HasNotVisited, c.Substring(9).Trim()));
                                    else if (c.StartsWith("decision:"))
                                        currentChoice.Conditions.Add(new FMVCondition(ConditionType.HasMadeDecision, c.Substring(9).Trim()));
                                    else if (c.StartsWith("!decision:"))
                                        currentChoice.Conditions.Add(new FMVCondition(ConditionType.HasNotMadeDecision, c.Substring(10).Trim()));
                                }
                            }
                            else if (modifier.StartsWith("[sets: "))
                            {
                                string setsStr = modifier.Substring(7, modifier.Length - 8);
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
