using System.Collections.Generic;

namespace FMV.Data
{
    public class FMVScene
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsEnding { get; set; }
        public string AutoAdvanceSceneId { get; set; }
        public List<FMVAutoAdvance> AutoAdvances { get; set; } = new List<FMVAutoAdvance>();
        
        public List<string> AutoItems { get; set; } = new List<string>();
        public List<string> DecisionFlags { get; set; } = new List<string>();
        public List<FMVChoice> Choices { get; set; } = new List<FMVChoice>();

        public FMVScene(string id)
        {
            Id = id;
        }
    }
}
