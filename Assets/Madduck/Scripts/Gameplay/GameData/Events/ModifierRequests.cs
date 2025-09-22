using System;
using System.Collections.Generic;
using System.Linq;

namespace Madduck.GameData
{
    public readonly struct ModifierRequest
    {
         public Type ModifierType { get; }

         private ModifierRequest(Type modifierType)
         {
              ModifierType = modifierType;
         }
         
         public static ModifierRequest For<T>() where T : BaseModifierData
         {
              return new ModifierRequest(typeof(T));
         }
    }

    public readonly struct ModifierResponse
    {
        private List<BaseModifierData> Modifiers { get; }

        public ModifierResponse(List<BaseModifierData> modifiers)
        {
             Modifiers = modifiers;
        }

        public List<T> As<T>() where T : BaseModifierData
        {
            return Modifiers.Cast<T>().ToList();
        }
    }
}