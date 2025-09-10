using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Scripts.Fishing.UI.FishingBoard
{
    [Serializable]
    public struct CircleBoard 
    {
        [field: SerializeField] public RectTransform Circle { get; private set; }
        [PropertyTooltip("Radius of the circle area.")]
        [field: SerializeField] public float Radius { get; private set; }
        [PropertyTooltip("Range of the power multiplier of this zone based on distance from center.")]
        [field: SerializeField] public Vector2 MultiplierRange { get; private set; }
    }

    public struct CircleBoardState
    {
        public Vector2 Center { get; private set; }
        public float Radius { get; private set; }
        public Vector2 MultiplierRange { get; private set; }
        
        public CircleBoardState(CircleBoard circleBoard)
        {
            Center = circleBoard.Circle.localPosition;
            Radius = circleBoard.Radius;
            MultiplierRange = circleBoard.MultiplierRange;
        }
        public CircleBoardState(Vector2 center, float radius, Vector2 multiplierRange)
        {
            Center = center;
            Radius = radius;
            MultiplierRange = multiplierRange;
        }
    }
}
