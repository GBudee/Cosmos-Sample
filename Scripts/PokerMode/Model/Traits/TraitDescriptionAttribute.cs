using System;

namespace PokerMode.Traits
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TraitDescriptionAttribute : Attribute
    {
        public readonly string Name;
        public readonly string Contents;
        public readonly string Tournament;
        
        public TraitDescriptionAttribute(string name, string contents)
        {
            Name = name;
            Contents = contents;
        }
        
        public TraitDescriptionAttribute(string name, string contents, string tournament)
        {
            Name = name;
            Contents = contents;
            Tournament = tournament;
        }
    }
}