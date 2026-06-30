using System.Collections.Generic;

namespace FMV.Data
{
    public class FMVAutoAdvance
    {
        public string TargetSceneId { get; set; }
        public List<FMVCondition> Conditions { get; set; } = new List<FMVCondition>();

        public bool IsAvailable()
        {
            if (Conditions == null) return true;
            foreach (var condition in Conditions)
            {
                if (!condition.Evaluate()) return false;
            }
            return true;
        }
    }
}
