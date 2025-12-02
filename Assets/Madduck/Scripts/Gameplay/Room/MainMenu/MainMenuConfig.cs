using System.Collections.Generic;
using FMODUnity;
using Madduck.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Room
{
    [CreateAssetMenu(fileName = "MainMenuConfig", menuName = "Madduck/Room/MainMenu/MainMenuConfig")]
    public class MainMenuConfig : ScriptableObject
    {
        [Title("Audio"),
         HideLabel,
         ShowInInspector] private InspectorPlaceholder _audioTitle;
        [field: SerializeField] public List<EventReference> MainMenuBGMPlaylist { get; private set; } = new();
    }
}