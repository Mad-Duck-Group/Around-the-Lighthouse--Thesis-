using Madduck.Utils;

namespace Madduck.GameData
{
    /// <summary>
    /// Event that is sent out by ModifierSource to let subscribers subscribe to the modifiers.
    /// </summary>
    public readonly struct ModifierSourceEvent
    {
        public IModifierSource ModiferSource { get; }

        public ModifierSourceEvent(IModifierSource source)
        {
            ModiferSource = source;
        }
    }
}