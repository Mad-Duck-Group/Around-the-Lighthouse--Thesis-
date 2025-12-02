using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.GameData
{
    [CreateAssetMenu(fileName = "GameSettingsManagerConfig", menuName = "Madduck/Settings/Game Settings Manager Config")]
    public class GameSettingsManagerConfig : ScriptableObject
    {
        [Title("Settings"),
            HideLabel,
            ShowInInspector] private InspectorPlaceholder _settingsPlaceholder;
        [field: SerializeField] public Vector2 MouseSensitivityRange { get; private set; } = new(800f, 2000f);
        [field: SerializeField] public Vector2 GamepadSensitivityRange { get; private set; } = new(100f, 1500f);
        //[field: SerializeField] public float SensitivitySliderStep { get; private set; } = 10f;
    }
}