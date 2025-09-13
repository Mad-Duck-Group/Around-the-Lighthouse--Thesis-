using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace MadDuck.Scripts.Utils.Inspectors
{
    /// <summary>
    /// Used as a placeholder in the inspector when no value is needed.
    /// </summary>
    [Serializable]
    public struct InspectorVoid {}

    /// <summary>
    /// Percentage multiplier ranging from 0 to 1.
    /// </summary>
    [Serializable]
    public struct PercentageMultiplier
    {
        [MinValue(0), MaxValue(1)] public float percentage;
    }

    /// <summary>
    /// Struct to define insets for a RectTransform.
    /// </summary>
    [Serializable]
    public struct RectTransformInset
    {
        [HorizontalGroup("LeftTop")] public float left;
        [HorizontalGroup("LeftTop")] public float top;
        [HorizontalGroup("RightBottom")] public float right;
        [HorizontalGroup("RightBottom")] public float bottom;
        public Vector2 OffsetMin => new(left, bottom);
        public Vector2 OffsetMax => new(-right, -top);
    }
}