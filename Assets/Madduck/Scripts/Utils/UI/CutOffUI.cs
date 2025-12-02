using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Madduck.Utils
{
    public class CutOffUI : MonoBehaviour, IMaterialModifier
    {
        private static readonly int StencilComp = Shader.PropertyToID("_StencilComp");
        private Material _resultMaterial;
        public Material GetModifiedMaterial(Material baseMaterial)
        {
            _resultMaterial = new Material(baseMaterial);
            _resultMaterial.SetFloat(StencilComp, Convert.ToSingle(CompareFunction.NotEqual));
            return _resultMaterial;
        }
    }
}
