using System.Collections.Generic;

namespace FMV.Data
{
    public enum ConditionType
    {
        HasItem,
        DoesNotHaveItem,
        HasVisited,
        HasNotVisited,
        HasMadeDecision,
        HasNotMadeDecision
    }

    public class FMVCondition
    {
        public ConditionType Type;
        public string TargetId;

        public FMVCondition(ConditionType type, string targetId)
        {
            Type = type;
            TargetId = targetId;
        }

        public bool Evaluate()
        {
            if (FMV.Core.FMVGameState.Instance == null) return false;

            switch (Type)
            {
                case ConditionType.HasItem:
                    return FMV.Core.FMVGameState.Instance.HasItem(TargetId);
                case ConditionType.DoesNotHaveItem:
                    return !FMV.Core.FMVGameState.Instance.HasItem(TargetId);
                case ConditionType.HasVisited:
                    return FMV.Core.FMVGameState.Instance.HasVisitedScene(TargetId);
                case ConditionType.HasNotVisited:
                    return !FMV.Core.FMVGameState.Instance.HasVisitedScene(TargetId);
                case ConditionType.HasMadeDecision:
                    return FMV.Core.FMVGameState.Instance.HasMadeDecision(TargetId);
                case ConditionType.HasNotMadeDecision:
                    return !FMV.Core.FMVGameState.Instance.HasMadeDecision(TargetId);
                default:
                    return false;
            }
        }
    }
}
