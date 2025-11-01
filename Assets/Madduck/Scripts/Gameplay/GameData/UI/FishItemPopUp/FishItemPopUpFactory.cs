using System;
using Cysharp.Threading.Tasks;
using Madduck.Utils;
using Sirenix.OdinInspector;

namespace Madduck.GameData
{
    [Serializable]
    public class FishItemPopUpFactory : PopUpFactory<FishItemPopUpObject>
    {
        [Title("Debug")]
        [HideInEditorMode,
         Button("Test Show")]
        public void TestShow(FishItemData fishItemData)
        {
            var instance = new FishItemInstance(fishItemData, new ModifierSourceMock());
            var popUpObject = new FishItemPopUpObject(instance);
            var popUp = Create();
            popUp.SetPopUpObject(popUpObject);
            popUp.Show().Forget();
        }
    }
}