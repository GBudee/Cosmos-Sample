using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class HudDropdown : MonoBehaviour, IHudSelectable
    {
        [SerializeField] private HudButton _ActivationButton;
        [SerializeField] private VerticalLayoutGroup _DropContainer;
        [SerializeField] private HudToggle _EntryTemplate;
        [SerializeField] private Image _Arrow;
        [SerializeField] private Color _ActivatedArrow;
        [SerializeField] private Color _DeactivatedArrow;
        [SerializeField] private List<string> _EntryValues;
        
        public IHudSelectable.State CurrentState { get; private set; }
        
        private List<(object value, System.Action onValueChanged)> _entries;
        private string _currentValue;
        private bool _activated;
        
        private List<HudToggle> _activeEntries = new();
        
        void Awake()
        {
            // Default visuals
            _entries ??= _EntryValues.Select(x => ((object)x, (System.Action) null)).ToList();
            if (_currentValue == null) SetDisplayedValue(_ActivationButton.Text.text);
            
            // Set up interactivity
            _EntryTemplate.gameObject.SetActive(false);
            _ActivationButton.SetListener(Activate);
        }
        
        void Update()
        {
            if (!_activated) return;

            var rectTransform = _DropContainer.transform as RectTransform;
            bool mouseOver = RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, null);
            if (!mouseOver && Input.GetMouseButtonDown(0))
            {
                Deactivate();
            }
        }
        
        public void SetEntries<T>(IEnumerable<(T value, System.Action onValueChanged)> entries, T current)
        {
            _entries = entries.Select(x => ((object)x.value, x.onValueChanged)).ToList();
            SetDisplayedValue(current.ToString());
        }
        
        private void SetDisplayedValue(string text)
        {
            _currentValue = text;
            _ActivationButton.Text.text = text;
            Deactivate();
        }
        
        private void Activate()
        {
            Debug.Assert(!_activated, "Cannot activate a dropdown twice!");
            
            // Toggle visuals
            _ActivationButton.gameObject.SetActive(false);
            _DropContainer.gameObject.SetActive(true);
            _Arrow.color = _ActivatedArrow;
            
            // Populate entries
            foreach (var (value, onValueChanged) in _entries)
            {
                // Create new entry
                var newItem = Instantiate(_EntryTemplate, _DropContainer.transform);
                newItem.gameObject.SetActive(true);
                newItem.Text.text = value.ToString();
                if (value.ToString().Equals(_currentValue)) newItem.isOn = true;
                _activeEntries.Add(newItem);
                
                // OnValueChanged
                var localValue = value;
                newItem.onValueChanged.AddListener(isOn =>
                {
                    SetDisplayedValue(localValue.ToString());
                    onValueChanged?.Invoke();
                });
            }
            
            CurrentState = IHudSelectable.State.Selected;
            _activated = true;
        }
        
        private void Deactivate()
        {
            // Toggle visuals
            _ActivationButton.gameObject.SetActive(true);
            _DropContainer.gameObject.SetActive(false);
            _Arrow.color = _DeactivatedArrow;
            
            // Destroy entries
            foreach (var entry in _activeEntries) Destroy(entry.gameObject);
            _activeEntries.Clear();
            
            CurrentState = IHudSelectable.State.Normal;
            _activated = false;
        }
    }
}