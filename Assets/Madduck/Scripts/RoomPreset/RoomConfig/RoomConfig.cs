using UnityEngine;

namespace RoomConfig
{
    [CreateAssetMenu(fileName = "RoomConfig", menuName = "ScriptableObjects/RoomConfig", order = 1)]
    public class RoomConfig : ScriptableObject
    {
        public Sprite skySprite;
        public Sprite rockSprite;
        public Sprite[] waveSprites;
    }
}
