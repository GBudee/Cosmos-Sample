using System.Collections.Generic;
using MEC;
using TMPro;
using UnityEngine;

namespace UI.Menus
{
    public class ConfirmationMenu : MonoBehaviour
    {
        [SerializeField] private TMP_Text _Label;
        [SerializeField] private HudButton _Confirm;
        [SerializeField] private HudButton _Cancel;
        
        public void Show(string label, System.Action confirm = null, System.Action cancel = null)
        {
            _Label.text = label;
            _Confirm.gameObject.SetActive(confirm != null);
            _Confirm.SetListener(() =>
            {
                Hide();
                confirm?.Invoke();
            });
            _Cancel.SetListener(() =>
            {
                Hide();
                cancel?.Invoke();
            });
            
            _Cancel.Text.text = "CANCEL";
            
            gameObject.SetActive(true);
        }
        
        // Extends Show with the option for an automatic countdown
        public void Show(string label, System.Action confirm, System.Action cancel, float autoCancelTimer)
        {
            CoroutineHandle timer = default;
            
            Show(label, confirm: () =>
            {
                Timing.KillCoroutines(timer);
                confirm?.Invoke();
            }, cancel: () =>
            {
                Timing.KillCoroutines(timer);
                cancel?.Invoke();
            });
            
            timer = Timing.RunCoroutine(Timer());
            IEnumerator<float> Timer()
            {
                while (true)
                {
                    autoCancelTimer -= Time.deltaTime;
                    _Cancel.Text.text = $"CANCEL ({Mathf.CeilToInt(autoCancelTimer)})";
                    if (autoCancelTimer <= 0) break;
                    yield return Timing.WaitForOneFrame;
                }
                
                // Timer expired, auto-cancel
                Hide();
                cancel?.Invoke();
            }
        }
        
        private void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}