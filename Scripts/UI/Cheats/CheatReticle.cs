using System;
using System.Collections.Generic;
using BezierSolution;
using DG.Tweening;
using Lean.Pool;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace UI.Cheats
{
    [ExecutionOrder(1000)]
    public class CheatReticle : MonoBehaviour
    {
        [SerializeField] private GameObject prefab_Chevron;
        [SerializeField] private GameObject _ArrowHead;
        [SerializeField] private GameObject _ArrowBase;
        [SerializeField] private CanvasGroup _CanvasGroup;
        [Header("Sprites")]
        [SerializeField] private Sprite _ArrowHead_Valid;
        [SerializeField] private Sprite _ArrowHead_Invalid;
        [SerializeField] private Sprite _ArrowBase_Valid;
        [SerializeField] private Sprite _ArrowBase_Invalid;
        [SerializeField] private Sprite _Chevron_Valid;
        [SerializeField] private Sprite _Chevron_Invalid;
        
        private CheatCardDisplay _target;
        private List<GameObject> _spawnedChevrons = new();
        private bool _targetValid;

        void Awake()
        {
            _CanvasGroup.alpha = 0f;
        }
        
        public void Show(CheatCardDisplay target)
        {
            _target = target;
            
            _CanvasGroup.DOKill();
            _CanvasGroup.DOFade(1f, .3f);
        }
        
        public void Hide()
        {
            _target = null;
            
            _CanvasGroup.DOKill();
            _CanvasGroup.alpha = 0f;
        }

        public void SetTargetValid(bool value)
        {
            _targetValid = value;
        }
        
        private void LateUpdate()
        {
            if (_target != null)
            {
                // Set spline start and end
                const float START_X_OFFSET = -80;
                Vector3 p0 = transform.InverseTransformPoint(_target.transform.position) + Vector3.right * START_X_OFFSET;
                RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, Input.mousePosition, null, out var cursorPos);
                Vector3 p2 = cursorPos;
                
                // Calculate knee pos
                const float KNEE_PROJ = .5f;
                const float KNEE_EXT = .16f;
                var baseVec = p2 - p0;
                var baseDir = baseVec.normalized;
                var kneeProjPoint = baseVec * KNEE_PROJ + p0;
                var horizontalness = Mathf.Abs(Vector3.Dot(baseDir, Vector3.right));
                var kneePos = kneeProjPoint + Vector3.up * horizontalness * baseVec.magnitude * KNEE_EXT * 2f;
                //var kneePos = kneeProjPoint + Vector3.Cross(baseDir, Vector3.forward) * baseVec.magnitude * KNEE_EXT * 2f;
                Vector3 p1 = kneePos;
                
                // Construct spline
                var spline = new QuadraticBezier(p0, p1, p2);
                
                const float SPACING = 35;
                const float HEAD_LENGTH = 40;
                float splineLength = spline.GetLength() - HEAD_LENGTH;
                int chevronIndex = 0;
                
                // Place chevrons
                if (splineLength > 0)
                {
                    float effectiveSpacing = splineLength / Mathf.Floor(splineLength / SPACING);
                    float normalizedPos = 0f;
                    float currentDist = 0f;
                    while (true)
                    {
                        // Spawn or pick next element
                        GameObject element;
                        if (currentDist.GEqual(splineLength))
                        {
                            element = _ArrowHead;
                            normalizedPos = 1f;
                            const float HEAD_OFFSET = 32;
                            spline.MoveAlongSpline(ref normalizedPos, -HEAD_OFFSET);
                            UpdateSprite(element, _ArrowHead_Valid, _ArrowHead_Invalid);
                        }
                        else if (normalizedPos == 0)
                        {
                            element = _ArrowBase;
                            UpdateSprite(element, _ArrowBase_Valid, _ArrowBase_Invalid);
                        }
                        else if (chevronIndex >= _spawnedChevrons.Count)
                        {
                            element = LeanPool.Spawn(prefab_Chevron, transform);
                            _spawnedChevrons.Add(element);
                            UpdateSprite(element, _Chevron_Valid, _Chevron_Invalid);
                        }
                        else
                        {
                            element = _spawnedChevrons[chevronIndex];
                            UpdateSprite(element, _Chevron_Valid, _Chevron_Invalid);
                        }
                        
                        // Position element along spline
                        element.transform.localPosition = spline.GetPoint(normalizedPos);
                        element.transform.localRotation = Quaternion.LookRotation(Vector3.forward, spline.GetTangent(normalizedPos));
                        const float SHADOW_OFFSET = 5f;
                        var shadowTransform = element.transform.GetChild(0);
                        shadowTransform.localPosition = shadowTransform.InverseTransformVector(transform.TransformVector(new Vector3(-1f, -1f, 0f) * SHADOW_OFFSET));

                        // Advance along spline (handling special cases at start and end)
                        if (element == _ArrowHead) break;
                        element.transform.localScale = Vector3.one * DOVirtual.EasedValue(2f, .8f, normalizedPos, Ease.InQuad); // Mathf.Abs(normalizedPos - .5f) * 2f
                        spline.MoveAlongSpline(ref normalizedPos, effectiveSpacing);
                        currentDist += effectiveSpacing;
                        if (element != _ArrowBase) chevronIndex++;
                    }
                }
                else
                {
                    // TODO: Place arrpwhead and arrowbase correctly in the "too close for spline" case
                }
                
                // Despawn any extra chevrons
                while (chevronIndex < _spawnedChevrons.Count)
                {
                    LeanPool.Despawn(_spawnedChevrons[chevronIndex]);
                    _spawnedChevrons.RemoveAt(chevronIndex);
                }
            }
        }
        
        private void UpdateSprite(GameObject element, Sprite validSprite, Sprite invalidSprite)
        {
            // TEMP: Invalid case doesn't look very good yet
            var image = element.transform.GetChild(1).GetComponent<Image>();
            image.sprite = _targetValid ? validSprite : invalidSprite;
        }
    }
}