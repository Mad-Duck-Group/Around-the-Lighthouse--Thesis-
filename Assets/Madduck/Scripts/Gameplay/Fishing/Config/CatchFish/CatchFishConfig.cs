using Madduck.Utils;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Fishing.Config
{
    [CreateAssetMenu(fileName = "CatchFishConfig", menuName = "Madduck/Fishing/CatchFishConfig")]
    public class CatchFishConfig : ScriptableObject
    {
        [Title("Settings")]
        [field: SerializeField] public TweenSettings<float> SlowMoSettings { get; private set; }
        [field: SerializeField] public Percentage SlowMoThreshold { get; private set; } = Percentage.FromFraction(0.5f);
    }
}