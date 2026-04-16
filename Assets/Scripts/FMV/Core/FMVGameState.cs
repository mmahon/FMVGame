using System.Collections.Generic;

namespace FMV.Core
{
    public class FMVGameState
    {
        private static FMVGameState _instance;
        public static FMVGameState Instance
        {
            get
            {
                if (_instance == null) _instance = new FMVGameState();
                return _instance;
            }
        }

        private HashSet<string> _visitedScenes = new HashSet<string>();
        private List<string> _inventory = new List<string>();
        private Dictionary<string, bool> _decisions = new Dictionary<string, bool>();

        // Resets the state for a new playthrough
        public void Reset()
        {
            _visitedScenes.Clear();
            _inventory.Clear();
            _decisions.Clear();
        }

        // --- Writers ---
        public void VisitScene(string sceneId)
        {
            if (string.IsNullOrEmpty(sceneId)) return;
            _visitedScenes.Add(sceneId);
        }

        public void AddItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return;
            if (!_inventory.Contains(itemId))
            {
                _inventory.Add(itemId);
            }
        }

        public void SetDecision(string decisionId, bool value = true)
        {
            if (string.IsNullOrEmpty(decisionId)) return;
            _decisions[decisionId] = value;
        }

        // --- Readers ---
        public bool HasVisitedScene(string sceneId)
        {
            return _visitedScenes.Contains(sceneId);
        }

        public bool HasItem(string itemId)
        {
            return _inventory.Contains(itemId);
        }

        public bool HasMadeDecision(string decisionId)
        {
            if (_decisions.TryGetValue(decisionId, out bool made))
            {
                return made;
            }
            return false;
        }

        // Expose state for debug UI / systems
        public HashSet<string> GetVisitedScenes() => _visitedScenes;
        public List<string> GetInventory() => _inventory;
        public Dictionary<string, bool> GetDecisions() => _decisions;
    }
}
