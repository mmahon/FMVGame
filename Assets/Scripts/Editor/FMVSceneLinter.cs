using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using FMV.Core;
using FMV.Data;

public static class FMVSceneLinter
{
    private const string ResourcesRoot = "Assets/Resources/FMV";

    [MenuItem("FMV/Lint Scene Files")]
    public static void LintAll()
    {
        int errors = 0, warnings = 0, files = 0;

        foreach (string path in Directory.GetFiles(ResourcesRoot, "*.txt", SearchOption.AllDirectories))
        {
            string text = File.ReadAllText(path);
            if (!text.Contains("===== SCENE: ")) continue;

            files++;
            string unityPath = path.Replace('\', '/');
            string dir = Path.GetDirectoryName(unityPath).Replace('\', '/');

            var scenes = FMVSceneParser.ParseScenes(text);
            var issues = Lint(scenes, dir);

            foreach (var issue in issues)
            {
                string msg = $"[FMVLint] {unityPath}: {issue.Message}";
                if (issue.IsError) { Debug.LogError(msg); errors++; }
                else { Debug.LogWarning(msg); warnings++; }
            }
        }

        Debug.Log($"[FMVLint] Checked {files} scene file(s): {errors} error(s), {warnings} warning(s).");
    }

    public struct Issue
    {
        public bool IsError;
        public string Message;
    }

    // dir is the folder holding the scene file; videos/images are expected in dir/Videos and dir/Images.
    public static List<Issue> Lint(Dictionary<string, FMVScene> scenes, string dir)
    {
        var issues = new List<Issue>();
        void Error(string m) => issues.Add(new Issue { IsError = true, Message = m });
        void Warn(string m) => issues.Add(new Issue { IsError = false, Message = m });

        // Everything the story can grant, so requirements can be checked against it.
        var grantedItems = new HashSet<string>();
        var grantedFlags = new HashSet<string>();
        foreach (var s in scenes.Values)
        {
            grantedItems.UnionWith(s.AutoItems);
            grantedFlags.UnionWith(s.DecisionFlags);
            foreach (var c in s.Choices) grantedFlags.UnionWith(c.SetsFlags);
        }

        void CheckConditions(string where, List<FMVCondition> conditions)
        {
            foreach (var cond in conditions)
            {
                switch (cond.Type)
                {
                    case ConditionType.HasItem:
                    case ConditionType.DoesNotHaveItem:
                        if (!grantedItems.Contains(cond.TargetId))
                            Warn($"{where}: item '{cond.TargetId}' is never granted by any scene.");
                        break;
                    case ConditionType.HasVisited:
                    case ConditionType.HasNotVisited:
                        if (!scenes.ContainsKey(cond.TargetId))
                            Error($"{where}: condition references unknown scene '{cond.TargetId}'.");
                        break;
                    case ConditionType.HasMadeDecision:
                    case ConditionType.HasNotMadeDecision:
                        if (!grantedFlags.Contains(cond.TargetId))
                            Warn($"{where}: decision '{cond.TargetId}' is never set by any scene or choice.");
                        break;
                }
            }
        }

        foreach (var scene in scenes.Values)
        {
            var edges = new List<string>();

            foreach (var aa in scene.AutoAdvances)
            {
                if (!scenes.ContainsKey(aa.TargetSceneId))
                    Error($"Scene '{scene.Id}': auto advance to unknown scene '{aa.TargetSceneId}'.");
                CheckConditions($"Scene '{scene.Id}' auto advance", aa.Conditions);
            }

            foreach (var choice in scene.Choices)
            {
                string where = $"Scene '{scene.Id}' choice '{choice.DisplayText}'";
                if (string.IsNullOrEmpty(choice.TargetSceneId))
                    Error($"{where}: has no target scene.");
                else if (!scenes.ContainsKey(choice.TargetSceneId))
                    Error($"{where}: targets unknown scene '{choice.TargetSceneId}'.");
                CheckConditions(where, choice.Conditions);
            }

            bool hasExit = scene.AutoAdvances.Count > 0 || scene.Choices.Count > 0;
            if (!scene.IsEnding && !hasExit)
                Error($"Scene '{scene.Id}': not an ending but has no choices or auto advance (dead end).");
            if (scene.IsEnding && hasExit)
                Warn($"Scene '{scene.Id}': marked as ending but has choices/auto advance (no Restart button will show).");

            if (scene.AutoAdvances.Count > 0 && scene.Choices.Count > 0)
                Warn($"Scene '{scene.Id}': has both auto advance and choices; choices are only shown if no auto advance is available.");

            if (!File.Exists($"{dir}/Videos/{scene.Id}.mp4"))
                Warn($"Scene '{scene.Id}': no video at {dir}/Videos/{scene.Id}.mp4 (3s placeholder will play).");
            if (!HasImage(dir, scene.Id))
                Warn($"Scene '{scene.Id}': no freeze-frame image in {dir}/Images.");
        }

        // Reachability from the first-defined scene (dictionary preserves insertion order for this usage).
        string start = null;
        foreach (var id in scenes.Keys) { start = id; break; }
        if (start != null)
        {
            var seen = new HashSet<string> { start };
            var stack = new Stack<string>();
            stack.Push(start);
            while (stack.Count > 0)
            {
                var s = scenes[stack.Pop()];
                var next = new List<string>();
                foreach (var aa in s.AutoAdvances) next.Add(aa.TargetSceneId);
                foreach (var c in s.Choices) next.Add(c.TargetSceneId);
                foreach (var n in next)
                    if (!string.IsNullOrEmpty(n) && scenes.ContainsKey(n) && seen.Add(n)) stack.Push(n);
            }
            foreach (var id in scenes.Keys)
                if (!seen.Contains(id))
                    Warn($"Scene '{id}': unreachable from start scene '{start}'.");
        }

        return issues;
    }

    private static bool HasImage(string dir, string id)
    {
        foreach (var ext in new[] { ".jpeg", ".jpg", ".png" })
            if (File.Exists($"{dir}/Images/{id}{ext}")) return true;
        return false;
    }
}
