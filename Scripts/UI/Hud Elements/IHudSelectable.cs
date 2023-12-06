using UnityEngine;

namespace UI
{
    public interface IHudSelectable
    {
        public enum State { Normal, Highlighted, Pressed, Selected, Disabled }
        public IHudSelectable.State CurrentState { get; }
    }
}