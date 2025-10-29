using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Madduck.Utils
{
    public class VerletFishingLine2D : MonoBehaviour
    {
        [Title("References")]
        [Required, 
         SerializeField] private LineRenderer lineRenderer;
        [field: SerializeField] public Transform RodTip { get; set; }
        [field: SerializeField] public Transform Hook { get; set; }
        
        [Title("Settings")]
        [SerializeField] private int segmentCount = 15;
        [SerializeField] private int constraintIterations = 3;
        [SerializeField] private float looseness = 1;
        [SerializeField] private TweenSettings forwardSettings;
        [SerializeField] private TweenSettings backwardSettings;
        [SerializeField] private float constraintDifferenceThreshold = 0.01f;
    
        [Title("Physics")]
        [SerializeField] private float gravity = 2f;
        [SerializeField] private float friction = 0.98f;
        
        private Vector2[] _segments;
        private Vector2[] _previousSegments;
        private float _segmentLength;

        private void Awake()
        {
            if (!RodTip || !Hook) return;
            InitializeRope();
        }

        public void SetUp(Transform rodTip, Transform hook)
        {
            RodTip = rodTip;
            Hook = hook;
            InitializeRope();
        }
        
        public void CastLine(
            float flightTime, 
            float distance,
            bool forward)
        {
            var startLength = 0;
            var targetLength = distance / segmentCount;
            if (forward)
            {
                forwardSettings.duration = flightTime;
                Tween.Custom(startLength, targetLength, forwardSettings, x =>
                {
                    _segmentLength = x * looseness;
                });
            }
            else
            {
                backwardSettings.duration = flightTime;
                Tween.Custom(targetLength, startLength, backwardSettings, x =>
                {
                    _segmentLength = x * looseness;
                });
            }
        }

        public void SetLength(float distance)
        {
            var targetLength = distance / segmentCount ;
            _segmentLength = targetLength * looseness;
        }

        private void InitializeRope()
        {
            _segments = new Vector2[segmentCount];
            _previousSegments = new Vector2[segmentCount];
            lineRenderer.positionCount = segmentCount;
        
            // Initialize positions
            for (int i = 0; i < segmentCount; i++)
            {
                Vector2 pos = Vector2.Lerp(RodTip.position, Hook.position, i / (float)(segmentCount - 1));
                _segments[i] = pos;
                _previousSegments[i] = pos;
            }
        }

        private void Update()
        {
            SimulateRope();
            UpdateLineRenderer();
        }

        private void SimulateRope()
        {
            // Apply verlet integration
            for (int i = 0; i < segmentCount; i++)
            {
                if (i == 0) continue; // Skip first point (rod tip)
            
                Vector2 velocity = (_segments[i] - _previousSegments[i]) * friction;
                _previousSegments[i] = _segments[i];
            
                // Apply gravity
                _segments[i] += velocity + Vector2.down * (gravity * Time.deltaTime * Time.deltaTime);
            }
        
            // Apply constraints
            for (int iteration = 0; iteration < constraintIterations; iteration++)
            {
                // Constrain to rod tip
                _segments[0] = RodTip.position;
            
                // Constrain segment distances
                for (int i = 0; i < segmentCount - 1; i++)
                {
                    Vector2 delta = _segments[i + 1] - _segments[i];
                    float distance = delta.magnitude;
                    float difference = (_segmentLength - distance) / distance;
                    if (Mathf.Abs(difference) < constraintDifferenceThreshold) continue;
                    if (i > 0) _segments[i] -= delta * (0.5f * difference);
                    _segments[i + 1] += delta * (0.5f * difference);
                }
            
                // Constrain to hook
                _segments[segmentCount - 1] = Hook.position;
            }
        }

        private void UpdateLineRenderer()
        {
            Vector3[] positions = new Vector3[segmentCount];
            for (int i = 0; i < segmentCount; i++)
            {
                positions[i] = _segments[i];
            }
            lineRenderer.SetPositions(positions);
        }
    }
}