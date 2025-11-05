using System;
using Cysharp.Threading.Tasks;
using Madduck.Shared;
using Madduck.Utils;
using Sirenix.OdinInspector;
using VContainer;

namespace Madduck.GameData
{
    [Serializable]
    public class FishItemPopUpFactory : PopUpFactory<FishItemPopUpObject>
    {
        [Inject] private readonly IModalManager _modalManager;
        
        [Title("Debug")]
        [HideInEditorMode,
         Button("Test Show")]
        public void TestShow(FishItemData fishItemData)
        {
            var instance = new FishItemInstance(fishItemData, new ModifierSourceMock());
            var popUpObject = new FishItemPopUpObject(instance);
            var popUp = Create();
            popUp.SetPopUpObject(popUpObject);
            _modalManager.Queue(popUp);
        }
    }
}