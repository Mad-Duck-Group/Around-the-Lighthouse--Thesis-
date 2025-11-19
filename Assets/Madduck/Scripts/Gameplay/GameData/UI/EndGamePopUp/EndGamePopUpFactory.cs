using System;
using Madduck.Shared;
using Madduck.Utils;
using Sirenix.OdinInspector;
using VContainer;

namespace Madduck.GameData
{
    [Serializable]
    public class EndGamePopUpFactory : PopUpFactory<EndGamePopUpObject>
    {
        [Inject] private readonly IModalManager _modalManager;
        
        [Title("Debug")]
        [HideInEditorMode, HideReferenceObjectPicker,
         Button("Test Show")]
        public void TestShow()
        {
            var popUpObject = new EndGamePopUpObject();
            var popUp = Create();
            popUp.SetPopUpObject(popUpObject);
            _modalManager.Queue(popUp);
        }
    }
}