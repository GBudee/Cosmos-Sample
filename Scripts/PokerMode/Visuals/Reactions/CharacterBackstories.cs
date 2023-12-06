using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PokerMode.Visuals.Reactions
{
    [CreateAssetMenu(fileName = "CharacterBackstoriesSO", menuName = "CharacterBackstories", order = 0)]
    public class CharacterBackstories : ScriptableObject
    {
        [SerializeField] private List<Info> _Backstories;
        
        [Serializable]
        public struct Info
        {
            public string Name;
            [Multiline]
            public string Backstory;
        }
        
        public string GetBackstory(string label)
        {
            return _Backstories.FirstOrDefault(x => x.Name == label).Backstory ?? "";
        }
    }
}