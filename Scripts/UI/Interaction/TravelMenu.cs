using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI.Interaction
{
    public class TravelMenu : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _CanvasGroup;
        [SerializeField] private List<PlanetButton> _PlanetButtons;

        public string Selected => PlanetLabel(_selectedPlanet);
        public System.Action OnSelectionChanged;
        
        private int _selectedPlanet;
        
        public void Show(System.Func<string, bool> isUnlockedPlanet)
        {
            gameObject.SetActive(true);
            const float DURATION = .5f;
            DOTween.Sequence().SetTarget(this)
                .Join(_CanvasGroup.DOFade(1f, DURATION).From(0f))
                .Join(transform.DOScale(Vector3.one, DURATION).From(new Vector3(.1f, 1f, 1f)).SetEase(Ease.OutBack));
            
            for (int i = 0; i < _PlanetButtons.Count; i++)
            {
                int localIndex = i;
                _PlanetButtons[i].SetListener(() => SelectPlanet(localIndex));
                if (isUnlockedPlanet(PlanetLabel(i))) _PlanetButtons[i].ShowUnlockedDescription();
            }
            
            SelectPlanet(PlanetIndex(SceneManager.GetActiveScene().name));
        }
        
        public void Hide()
        {
            OnSelectionChanged = null;
            gameObject.SetActive(false);
        }

        public void DisableSelection()
        {
            foreach (var button in _PlanetButtons)
                button.ClearListener();
        }
        
        private void SelectPlanet(int index)
        {
            for (int i = 0; i < _PlanetButtons.Count; i++) _PlanetButtons[i].SetSelected(i == index);
            _selectedPlanet = index;
            OnSelectionChanged?.Invoke();
        }

        private static int PlanetIndex(string label)
        {
            return label switch
            {
                "BuzzGazz" => 0,
                "TheBlackHole" => 1,
                "FarOutDiner" => 2,
                "NewRenoStation" => 3,
                _ => 0
            };
        }
        
        private static string PlanetLabel(int index)
        {
            return index switch
            {
                0 => "BuzzGazz",
                1 => "TheBlackHole",
                2 => "FarOutDiner",
                3 => "NewRenoStation",
            };
        }
    }
}