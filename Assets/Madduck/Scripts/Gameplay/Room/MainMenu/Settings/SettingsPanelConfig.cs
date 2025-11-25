using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Room
{
    [CreateAssetMenu(fileName = "SettingsPanelConfig", menuName = "Madduck/Room/SettingsPanelConfig")]
    public class SettingsPanelConfig : ScriptableObject
    {
        [Title("Settings"),
            HideLabel,
            ShowInInspector] private InspectorPlaceholder _settingsPlaceholder;
        [field: SerializeField] public Vector2 MouseSensitivityRange { get; private set; } = new(800f, 2000f);
        [field: SerializeField] public Vector2 GamepadSensitivityRange { get; private set; } = new(100f, 1500f);
        //[field: SerializeField] public float SensitivitySliderStep { get; private set; } = 10f;
    }
}