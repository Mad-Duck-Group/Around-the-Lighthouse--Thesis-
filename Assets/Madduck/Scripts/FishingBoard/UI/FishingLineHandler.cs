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
        [SerializeField] private Color32 lowTensionColor;
        [SerializeField] private Color32 mediumTensionColor;
        [SerializeField] private Color32 highTensionColor;
        [SerializeField] private float colorLerpSpeed = 2f; // Speed of color transition
        [SerializeField] private Vector2 offset;
        
        [Title("Debug")]
        [DisplayAsString]
        [ShowInInspector] public FishLineTension FishLineTension { get; set; } = FishLineTension.Normal;
        [ReadOnly]
        [ShowInInspector] private Vector3[] _fishLine;
        [ReadOnly]
        [ShowInInspector] private Vector3[] _hookLine;
        #endregion

        #region Fields
        private Color _currentColor;
        private Color _targetColor;
        private FishLineTension _previousTension;
        private float _width, _height, _unitWidth, _unitHeight;
        #endregion

        private void Start()
        {
            _currentColor = Color.white; // Default color
            _targetColor = _currentColor;
            _previousTension = FishLineTension;
        }

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
        
        private void SetVertexPosition()
        {
            foreach (var shape in shapes)
            {
                var rectTransform = (RectTransform)shape.transform;
                rectTransform.sizeDelta = new Vector2(_width, _height);
            }
            shapes[0].SetPolygonWorldVertices(_fishLine);
            shapes[1].SetPolygonWorldVertices(_hookLine);
            foreach (var s in shapes)
            {
                s.settings.fillColor = _currentColor;
            }
        }
        
        public void GetWidthHeight(float width, float height)
        {
            _width = width;
            _height = height;
        }

        private void Update()
        {
            if (_previousTension != FishLineTension)
            {
                HandleTension(); // Call only when tension changes
                _previousTension = FishLineTension; // Update last known state
            }
            _currentColor = Color.Lerp(_currentColor, _targetColor, Time.deltaTime * colorLerpSpeed);
            
        }

        public void SetColor(Color color)
        {
            _targetColor = color;
            
        }
        
        private void HandleTension()
        {
            switch (FishLineTension)
            {
                case FishLineTension.Normal:
                    SetColor(Color.white); // Example color
                    break;
                case FishLineTension.Low:
                    SetColor(lowTensionColor);
                    break;
                case FishLineTension.Medium:
                    SetColor(mediumTensionColor);
                    break;
                case FishLineTension.High:
                    SetColor(highTensionColor);
                    break;
            }
        }
    
    }
}