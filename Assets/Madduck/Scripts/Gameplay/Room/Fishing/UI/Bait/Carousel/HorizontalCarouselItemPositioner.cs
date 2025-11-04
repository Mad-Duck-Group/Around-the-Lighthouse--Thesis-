#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections;
using Madduck.Utils;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace HasanSadikin.Carousel
{
    public class HorizontalCarouselItemPositioner : MonoBehaviour, ICarouselItemPositioner
    {        
        [SerializeField] bool _isStatic = false;
        [SerializeField] float _duration = .25f;
        [SerializeField] float _offsetX;
        [SerializeField] float _gap = 100;
        [SerializeField] int _visibleItem = 3;
        [SerializeField] Ease _ease;


        [Header("For Debugging")]
        [SerializeField] bool _debugCarouselArea;
        
        Image _image;
        bool _realIsStatic = false;
        private readonly SerializableDictionary<RectTransform, Sequence> _seqLookup = new();

        private IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();

            _realIsStatic = _isStatic;
        }


        private void OnValidate()
        {
            
            if(_isStatic)
            {
                return;
            }

            EditorApplication.delayCall += UpdateSizeDelta;
        }

        public void SetPosition(RectTransform rectTransform, int index)
        {
             if(_realIsStatic) return;

            float endValue = index * _gap + _offsetX;

            float duration = Mathf.Abs(endValue - rectTransform.anchoredPosition.x) > _gap * _visibleItem ? 0 : _duration;

            if (_seqLookup.TryGetValue(rectTransform, out var oldSeq) && oldSeq.IsAlive) {
                oldSeq.Stop();
            }
            Vector2 targetPos = new Vector2(endValue, rectTransform.anchoredPosition.y);
            
            var tween = Tween.UIAnchoredPosition(rectTransform, targetPos, duration, _ease);
            var seq = Sequence.Create().Group(tween);
            
            _seqLookup[rectTransform] = seq;
        }

        public bool IsItemAfter(RectTransform a, RectTransform b)
        {
            return a.anchoredPosition.x > b.anchoredPosition.x;
        }

        private void UpdateSizeDelta()
        {
            if (_image != null && _image.rectTransform != null)
            {
                Vector2 newSize = new Vector2(_visibleItem * _gap, _image.rectTransform.sizeDelta.y);

                if (_image.rectTransform.sizeDelta != newSize)
                {
#if UNITY_EDITOR
                    Undo.RecordObject(_image.rectTransform, "Update RectTransform SizeDelta");
#endif
                     _image.rectTransform.sizeDelta = newSize;

#if UNITY_EDITOR
                    EditorUtility.SetDirty(_image.rectTransform);
#endif
                }
            }

            EditorApplication.delayCall -= UpdateSizeDelta;
        }
    }
}