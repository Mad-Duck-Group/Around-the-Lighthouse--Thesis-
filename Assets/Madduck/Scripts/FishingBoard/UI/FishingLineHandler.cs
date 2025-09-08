using PrimeTween;
using Shapes2D;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Scripts.FishingBoard.UI
{
    public enum FishLineTension
    {
        Normal,
        Low,
        Medium,
        High
    }
    public class FishingLineHandler : MonoBehaviour
    {
        #region Inspector
        [Title("Settings")]
        [SerializeField] private Shape[] shapes;
        [SerializeField] private Color32 noTensionColor = Color.white;
        [SerializeField] private Color32 lowTensionColor;
        [SerializeField] private Color32 mediumTensionColor;
        [SerializeField] private Color32 highTensionColor;
        [SerializeField] private float colorLerpSpeed = 2f;
        [SerializeField] private Vector2 offset;
        
        [Title("Debug")]
        [DisplayAsString]
        [ShowInInspector] private FishLineTension _fishlineTension = FishLineTension.Normal;
        [ReadOnly]
        [ShowInInspector] private Vector3[] _fishLine;
        [ReadOnly]
        [ShowInInspector] private Vector3[] _hookLine;
        #endregion

        #region Fields
        private Color _currentColor;
        private Tween _colorTween;
        private FishLineTension _previousTension = FishLineTension.Normal;
        private float _width, _height, _unitWidth, _unitHeight;
        #endregion

        public void Reset()
        {
            _currentColor = noTensionColor;
            foreach (var s in shapes)
            {
                s.settings.fillColor = noTensionColor;
            }
            _fishlineTension = FishLineTension.Normal;
            _previousTension = FishLineTension.Normal;
            _colorTween.Stop();
        }

        /// <summary>
        /// Set the points of the fishing line.
        /// </summary>
        /// <param name="hook"></param>
        /// <param name="center"></param>
        /// <param name="fish"></param>
        public void SetPoints(Transform hook, Transform center, Transform fish)
        {
            var directionHook = (hook.position - center.position).normalized;
            var directionFish = (fish.position - center.position).normalized;
            
            Vector2 finalHookPoint = hook.position;
            Vector2 finalFishPoint = fish.position;
            
            Vector2 perpendicularHook = new Vector2(-directionHook.y, directionHook.x) * (offset.magnitude * 0.5f);
            Vector2 perpendicularFish = new Vector2(-directionFish.y, directionFish.x) * (offset.magnitude * 0.5f);

            var finalHookPoint1 = finalHookPoint - perpendicularHook;
            var finalFishPoint1 = finalFishPoint - perpendicularFish;

            var finalHookPoint2 = finalHookPoint + perpendicularHook;
            var finalFishPoint2 = finalFishPoint + perpendicularFish;
            

            _fishLine = new Vector3[3];
            _fishLine[0] = center.position;
            _fishLine[1] = finalHookPoint2;
            _fishLine[2] =  finalHookPoint1;
            
            _hookLine = new Vector3[3];
            _hookLine[0] = center.position;
            _hookLine[1] = finalFishPoint2;
            _hookLine[2] =  finalFishPoint1;
            SetVertexPosition();
        }
        
        /// <summary>
        /// Set the vertex positions of the fishing line shapes.
        /// </summary>
        private void SetVertexPosition()
        {
            foreach (var shape in shapes)
            {
                var rectTransform = (RectTransform)shape.transform;
                rectTransform.sizeDelta = new Vector2(_width, _height);
            }
            shapes[0].SetPolygonWorldVertices(_fishLine);
            shapes[1].SetPolygonWorldVertices(_hookLine);
        }
        
        /// <summary>
        /// Get the width and height of the fishing line area.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void GetWidthHeight(float width, float height)
        {
            _width = width;
            _height = height;
        }

        /// <summary>
        /// Handle the tension of the fishing line based on durability percentage.
        /// </summary>
        /// <param name="durabilityPercent"></param>
        public void HandleTension(float durabilityPercent)
        {
            switch (durabilityPercent)
            {
                case <= 0.3f:
                    _fishlineTension = FishLineTension.High;
                    break;
                case <= 0.5f:
                    _fishlineTension = FishLineTension.Medium;
                    break;
                case <= 0.7f:
                    _fishlineTension = FishLineTension.Low;
                    break;
                default:
                    _fishlineTension = FishLineTension.Normal;
                    break;
            }
            if (_fishlineTension == _previousTension) return;
            var targetColor = noTensionColor;
            switch (_fishlineTension)
            {
                case FishLineTension.Normal:
                    targetColor = noTensionColor;
                    break;
                case FishLineTension.Low:
                    targetColor = lowTensionColor;
                    break;
                case FishLineTension.Medium:
                    targetColor = mediumTensionColor;
                    break;
                case FishLineTension.High:
                    targetColor = highTensionColor;
                    break;
            }
            _previousTension = _fishlineTension;
            var currentColor = _currentColor;
            _colorTween.Stop();
            _colorTween = Tween.Custom(currentColor, targetColor, colorLerpSpeed, color =>
            {
                _currentColor = color;
                foreach (var s in shapes)
                {
                    s.settings.fillColor = color;
                }
            });
        }
    
    }
}