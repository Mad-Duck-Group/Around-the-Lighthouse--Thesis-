using Madduck.Shared;
using Madduck.Utils;
using Sirenix.OdinInspector;
using VContainer;

namespace Madduck.GameData
{
    public class NewFishPopUpFactory : PopUpFactory<NewFishPopUpObject>
    {
        [Inject] private readonly IModalManager _modalManager;
        
        [Title("Debug")]
        [HideInEditorMode,
         Button("Test Show")]
        public void TestShow(FishItemData fishItemData)
        {
            var instance = new FishItemInstance(fishItemData, new ModifierSourceMock());
            var popUpObject = new NewFishPopUpObject(instance);
            var popUp = Create();
            popUp.SetPopUpObject(popUpObject);
            _modalManager.Queue(popUp);
        }
    }
}