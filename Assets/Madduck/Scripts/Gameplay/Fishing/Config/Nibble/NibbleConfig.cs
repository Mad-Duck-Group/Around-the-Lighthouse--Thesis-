using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Fishing.Config
{
    [CreateAssetMenu(fileName = "Nibble Config", menuName = "Madduck/Fishing/Nibble Config", order = 2)]
    public class NibbleConfig : ScriptableObject
    {
        [field: SerializeField] public Vector2 QteIntervalRange { get; private set; } = new(3, 8);
    }
}