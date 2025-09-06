using System;
using UnityEngine;

namespace Madduck.Scripts.FishingBoard.UI
{
    [Serializable]
    public struct CircleBoard 
    {
        [field: SerializeField] public RectTransform Circle { get; private set; }
        [field: SerializeField] public float Radius { get; private set; }
        [field: SerializeField] public Vector2 MultiplierRange { get; private set; }
    }
}
