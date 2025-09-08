using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Madduck.Scripts.Utils.Editor
{
    public interface IDebugData
    {
        public bool ConstantUpdate { get; }
    }
    public class DebugEditorWindow : OdinEditorWindow
    {
        private IDebugData _debugData;
        public static DebugEditorWindow Inspect(IDebugData debugData)
        {
            var window = GetWindow<DebugEditorWindow>();
            window._debugData = debugData;
            var inspectWindow = InspectObject(window, debugData);
            inspectWindow.titleContent = new GUIContent("Fishing Board Debug");
            return window;
        }

        protected void OnInspectorUpdate()
        {
            if (!_debugData.ConstantUpdate) return;
            Repaint();
        }
    }
}