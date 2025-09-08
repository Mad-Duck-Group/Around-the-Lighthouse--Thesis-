using System;
using Madduck.Scripts.FishingBoard.UI.Model;
using Madduck.Scripts.FishingBoard.UI.ViewModel;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace Madduck.Scripts.FishingBoard.Editor
{
    [Serializable]
    public struct FishingBoardDebugData
    {
        [ShowInInspector] private FishingBoardState _fishingBoardState;
        [ShowInInspector] private FishingBoardModel _fishingBoardModel;
        [ShowInInspector] private FishingBoardController _fishingBoardController;
        
        public FishingBoardDebugData(
            FishingBoardState fishingBoardState, 
            FishingBoardModel fishingBoardModel, 
            FishingBoardController fishingBoardController)
        {
            _fishingBoardState = fishingBoardState;
            _fishingBoardModel = fishingBoardModel;
            _fishingBoardController = fishingBoardController;
        }
    }
    public class FishingBoardDebugEditorWindow : OdinEditorWindow
    {
        public static FishingBoardDebugEditorWindow Inspect(object obj)
        {
            var window = GetWindow<FishingBoardDebugEditorWindow>();
            var inspectWindow = InspectObject(window, obj);
            inspectWindow.titleContent = new GUIContent("Fishing Board Debug");
            return window;
        }

        protected void OnInspectorUpdate()
        {
            Repaint();
        }
    }
}