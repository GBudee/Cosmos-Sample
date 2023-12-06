using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PokerMode.Visuals.Reactions
{
    [System.Serializable]
    public class DialogueLookup
    {
        public List<Action> Actions;

        public string GetDialogue(string actionName, string archetype, params (string id, string value)[] replacementValues)
        {
            // Find the appropriate action
            var options = Actions.FirstOrDefault(x => x.ActionName == actionName);
            if (options == null) return null;
            
            // Grab generic and character-specific dialogue option lists
            var genericOptions = options.Options.FirstOrDefault(x => x.Archetype == "Generic");
            var genericCount = genericOptions?.Text.Count ?? 0;
            var characterOptions = options.Options.FirstOrDefault(x => x.Archetype == archetype);
            var characterCount = characterOptions?.Text.Count ?? 0;
            int totalCount = genericCount + characterCount;
            if (totalCount == 0) return null;
            
            // Randomize dialogue
            int randomOption = UnityEngine.Random.Range(0, totalCount);
            string result = randomOption < genericCount
                ? genericOptions.Text[randomOption]
                : characterOptions.Text[randomOption - genericCount];
            
            // Replace any custom values in the dialogue
            if (replacementValues.Length > 0)
            {
                StringBuilder sb = new StringBuilder (result);
                foreach (var replacer in replacementValues)
                    sb.Replace(replacer.id, replacer.value);
                result = sb.ToString();
            }
            return result;
        }
    }
    
    [System.Serializable]
    public class Action
    {
        public string ActionName;
        public List<Option> Options;
    }
    
    [System.Serializable]
    public class Option
    {
        public string Archetype;
        public List<string> Text;
    }
}