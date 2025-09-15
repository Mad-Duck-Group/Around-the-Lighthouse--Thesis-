using UnityEngine;

namespace Madduck.Scripts.Game.Weather
{
    public enum WeatherType
    {
        Clear,
        Rain,
        Fog,
    }
    
    [CreateAssetMenu(fileName = "New Weather Weight Table", menuName = "Madduck/Weather/Weather Weight Table")]
    public class WeatherWeightTable : ScriptableObject
    {
        
    }
}