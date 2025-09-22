using Madduck.Utils;

namespace Madduck.GameData
{
    public readonly struct ModifierUpdatedEvent
    {
        public IModifierProvider ModifierProvider { get; }

        public ModifierUpdatedEvent(IModifierProvider modifierProvider)
        {
             ModifierProvider = modifierProvider;
        }
    }
}