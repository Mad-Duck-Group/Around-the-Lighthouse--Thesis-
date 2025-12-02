using Madduck.Save;
using UnityEngine;

namespace Madduck.Audio
{
    [CreateAssetMenu(fileName = "AudioSaveObject", menuName = "Madduck/Audio/AudioSaveObject", order = 0)]
    public class AudioSaveObject : MessagePackSaveObject<AudioSaveData>
    {
        
    }
}