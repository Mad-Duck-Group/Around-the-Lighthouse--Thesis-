using System;
using UnityEngine;
using UnityEngine.UI;

namespace Madduck.Utils
{
    [RequireComponent(typeof(Image))]
    public class AlphaHitTest : MonoBehaviour
    {
        [SerializeField] private float threshold = 0.5f;
        public void Awake()
        {
            var image = GetComponent<Image>();
            image.alphaHitTestMinimumThreshold = threshold;
        }
    }
}