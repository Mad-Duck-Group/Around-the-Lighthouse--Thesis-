using System.Collections.Generic;
using Madduck.Input;
using UnityEngine;

public class InputUIIconRegistry : MonoBehaviour
{
    private static readonly List<InputUIIconView> _views = new();

    public static IReadOnlyList<InputUIIconView> Views => _views;

    public static void Register(InputUIIconView view)
    {
        if (!_views.Contains(view))
            _views.Add(view);
    }

    public static void Unregister(InputUIIconView view)
    {
        _views.Remove(view);
    }
}
