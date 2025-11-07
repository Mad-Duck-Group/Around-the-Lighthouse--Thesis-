using Sirenix.OdinInspector;
using UnityEngine;

namespace Madduck.Fishing.Shared
{
    public class BubbleGizmoDrawer : MonoBehaviour
    {
        [Required, InlineEditor,
         SerializeField] private BubbleManagerConfig config;
        
        #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!config) return;
            var center = Vector3.zero + Vector3.up * config.BubbleYOffset;
            var left = center - Vector3.left * config.BubbleSpawnRange.x;
            var right = center + Vector3.right * config.BubbleSpawnRange.y;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(left, right);
        }
        #endif
    }
}