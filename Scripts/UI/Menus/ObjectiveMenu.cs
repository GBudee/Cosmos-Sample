using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using PokerMode;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI.Menus
{
    public class ObjectiveMenu : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _CanvasGroup;
        [SerializeField] private List<GameObject> _Icons;
        [SerializeField] private TMP_Text _Title;
        [SerializeField] private TMP_Text _Subtitle;
        [SerializeField] private HudButton _ExitButton;
        [Header("General Content")] 
        [SerializeField] private GameObject _GeneralContainer;
        [SerializeField] private TMP_Text _GeneralHeader;
        [SerializeField] private TMP_Text _GeneralFooter;
        [SerializeField] private List<GameObject> _HintVisuals;
        [Header("Table Preview")]
        [SerializeField] private GameObject _TablePreviewContainer;
        [SerializeField] private RectTransform _Carousel;
        [SerializeField] private List<RectTransform> _TableInfos;
        [FormerlySerializedAs("_Footer")] [SerializeField] private TMP_Text _TablePreviewFooter;
        [SerializeField] private HudButton _LeftArrow;
        [SerializeField] private HudButton _RightArrow;
        
        private Tween _fader;
        
        public enum Mode { TablePreview, NextObjective, FinalObjective }
        public bool IsActive => gameObject.activeSelf;
        
        public static int ObjectiveCredits(int objectiveIndex)
        {
            return objectiveIndex switch
            {
                0 => 400,
                1 => 1200,
                2 => 5000,
                3 => 15000,
                4 => 45000,
                5 => 200000,
                6 => 600000,
                7 => 1800000,
                8 => 5000000,
                9 => Int32.MaxValue,
                _ => 0,
            };
        }
        
        public const int MAX_OBJECTIVE = 10;
        
        void Awake()
        {
            gameObject.SetActive(false);
            _fader = _CanvasGroup.DOFade(1f, .3f).From(0f)
                .OnPlay(() => gameObject.SetActive(true))
                .OnRewind(() => gameObject.SetActive(false))
                .SetAutoKill(false).Pause();
        }
        
        public void Show(Mode mode, int objectiveIndex, System.Action onExit = null)
        {
            if (objectiveIndex >= MAX_OBJECTIVE) mode = Mode.FinalObjective;
            
            // Set icon
            int sceneIndex = SceneManager.GetActiveScene().name switch
            {
                "BuzzGazz" => 0,
                "TheBlackHole" => 1,
                "FarOutDiner" => 2,
                "NewRenoStation" => 3,
            };
            for (int i = 0; i < _Icons.Count; i++) _Icons[i].SetActive(i == sceneIndex);
            
            // Set Title
            _Title.text = mode switch
            {
                Mode.TablePreview => "Objective",
                Mode.NextObjective => "Objective Complete!",
                Mode.FinalObjective => "Objective Complete!"
            };
            int objectiveBuyIn = ObjectiveCredits(mode == Mode.TablePreview ? objectiveIndex : objectiveIndex - 1);
            _Subtitle.text = sceneIndex == 3 ? "Be the last cheater standing." : $"Earn {CreditVisuals.CREDIT_SYMBOL}{objectiveBuyIn}";
            
            // Set Contents
            _GeneralContainer.SetActive(mode != Mode.TablePreview);
            _TablePreviewContainer.SetActive(mode == Mode.TablePreview);
            if (mode == Mode.TablePreview)
            {
                GoToTableIndex(0);
            }
            else if (mode == Mode.NextObjective)
            {
                for (int i = 0; i < _HintVisuals.Count; i++) _HintVisuals[i].SetActive(i == objectiveIndex - 1);
                _GeneralHeader.text = objectiveIndex switch
                {
                    3 or 6 => "Congrats! You earned enough credits to travel to a new planet. Go to your truck when you're ready!",
                    9 => "You've been invited to New Reno Station to participate in the 78th Annual Asteroid County Truck Pull and Poker Tournament!",
                    _ => "Congrats! You earned enough credits to buy into the next table!"
                };
                int nextBuyIn = ObjectiveCredits(objectiveIndex);
                _GeneralFooter.text = $"<color=white><font=\"Oldtimer SDF\" material=\"Oldtimer Atlas Material - drop shadow\"><smallcaps>New Objective:</smallcaps></font></color>  " +
                                      (objectiveIndex == 9 ? "Win the tournament!" : $"Earn {CreditVisuals.CREDIT_SYMBOL}{NumberDisplay.FormattedValue(nextBuyIn)}.");
                _GeneralFooter.gameObject.SetActive(true);
            }
            else if (mode == Mode.FinalObjective)
            {
                foreach (var visual in _HintVisuals) visual.SetActive(false);
                _GeneralHeader.text = "Congrats! You won the 78th Annual Asteroid County Truck Pull and Poker Tournament." +
                                      "\n\n<color=white><font=\"Oldtimer SDF\" material=\"Oldtimer Atlas Material - drop shadow\"><smallcaps>A special thanks to all the participants for their honesty and good sportsmanship. Hope to see you next year!";
                _GeneralFooter.gameObject.SetActive(false);
            }
            
            _ExitButton.SetListener(() =>
            {
                Hide();
                onExit?.Invoke();
            });
            
            gameObject.SetActive(true);
            _fader.PlayForward();
        }
        
        public void Hide()
        {
            _fader.Rewind();
        }
        
        private void GoToTableIndex(int index)
        {
            (int lowestIndex, int highestIndex) = SceneManager.GetActiveScene().name switch
            {
                "BuzzGazz" => (0, 2),
                "TheBlackHole" => (3, 5),
                "FarOutDiner" => (6, 8),
                "NewRenoStation" => (9, 9),
            };
            for (int i = 0; i < _TableInfos.Count; i++)
            {
                _TableInfos[i].gameObject.SetActive(i >= lowestIndex && i <= highestIndex);
            }
            
            _LeftArrow.interactable = index > 0;
            _RightArrow.interactable = index < highestIndex - lowestIndex;
            _LeftArrow.SetListener(() => GoToTableIndex(index - 1));
            _RightArrow.SetListener(() => GoToTableIndex(index + 1));
            
            _Carousel.DOKill();
            _Carousel.DOAnchorPosX(-_TableInfos[lowestIndex].rect.width * index, .4f).SetEase(Ease.OutBack, 1.15f);
            
            _TablePreviewFooter.text = $"{index + 1}/{highestIndex - lowestIndex + 1}";
        }
    }
}