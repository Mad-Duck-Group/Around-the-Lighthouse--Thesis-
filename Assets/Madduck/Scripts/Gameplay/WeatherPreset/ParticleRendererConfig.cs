using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Madduck.Utils;

namespace Madduck.WeatherPreset
{
    [Serializable]
    public class ParticleEntry
    {
        [HorizontalGroup("Row")]
        public ParticleSystem prefab;

        [HorizontalGroup("Row")]
        public Renderer renderer;

        [HorizontalGroup("Row")]
        public ParticleRendererConfig config;
    }
    
    [System.Serializable]
    public class ParticleRendererConfig
    {
        [HorizontalGroup("Sorting"),
         LabelText("Layer")]
        [ValueDropdown("GetSortingLayers")]
        [SerializeField] private int sortingLayerID;

        [HorizontalGroup("Sorting"),
         LabelText("Order")]
        [SerializeField] private int orderInLayer = 0;

        public void ApplyTo(Renderer renderer)
        {
            if (renderer == null) return;
            renderer.sortingLayerID = sortingLayerID;
            renderer.sortingOrder = orderInLayer;
        }

#if UNITY_EDITOR
        private static IEnumerable<ValueDropdownItem<int>> GetSortingLayers()
        {
            foreach (var layer in SortingLayer.layers)
            {
                yield return new ValueDropdownItem<int>(layer.name, layer.id);
            }
        }
#endif
    }
    
}
