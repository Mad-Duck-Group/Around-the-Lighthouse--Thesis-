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
        [VerticalGroup("Row")]
        public ParticleSystem prefab;

        [VerticalGroup("Row")]
        public Renderer renderer;

        [VerticalGroup("Row"), InlineProperty, HideLabel]
        public ParticleRendererConfig config;
    }
    
    [Serializable]
    public class ParticleRendererConfig
    {
        [HorizontalGroup("Sorting"), 
         LabelText("Layer"),
         SortingLayer,
         SerializeField] private int sortingLayerID;

        [HorizontalGroup("Sorting"),
         LabelText("Order")]
        [SerializeField] private int orderInLayer;

        public void ApplyTo(Renderer renderer)
        {
            if (!renderer) return;
            renderer.sortingLayerID = sortingLayerID;
            renderer.sortingOrder = orderInLayer;
            DebugUtils.Log("Applied Particle Renderer Config");
            
        }
    }
    
}
