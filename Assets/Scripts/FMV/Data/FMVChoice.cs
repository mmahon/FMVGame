using System.Collections.Generic;

namespace FMV.Data
{
    public class FMVChoice
    {
        public string DisplayText;
        public string TargetSceneId;
        public List<FMVCondition> Conditions = new List<FMVCondition>();
        
        public List<string> SetsFlags = new List<string>();

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
