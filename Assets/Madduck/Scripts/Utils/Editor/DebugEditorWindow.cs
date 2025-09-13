using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Madduck.Scripts.Utils.Editor
{
    public interface IDebugData
    {
        public bool ConstantUpdate { get; }
    }
    
    /// <summary>
    /// Debug window for inspecting IDebugData objects.
    /// </summary>
    public class DebugEditorWindow : OdinEditorWindow
    {
        private IDebugData _debugData;
        public static DebugEditorWindow Inspect(IDebugData debugData, string title = "Debug")
        {
            var window = GetWindow<DebugEditorWindow>();
            window._debugData = debugData;
            var inspectWindow = InspectObject(window, debugData);
            inspectWindow.titleContent = new GUIContent(title);
            return window;
        }

        protected void OnInspectorUpdate()
        {
            if (!_debugData.ConstantUpdate) return;
            Repaint();
        }
    }
}