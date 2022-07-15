using System;
using System.Collections;
using System.Collections.Generic;
using Rhinox.GUIUtils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
#endif

public abstract class BaseEditor
#if ODIN_INSPECTOR
    : OdinEditor
#else
    : Editor
#endif
{
#if !ODIN_INSPECTOR // OdinEditor implements these, to allow easy override make stubs
    protected virtual void OnEnable() { }
    protected virtual void OnDisable() { }
#endif
}

public abstract class BaseEditor<T> : BaseEditor
    where T : UnityEngine.Object
{
    protected T Target => ConvertObject(target);
    protected T[] Targets => Array.ConvertAll(targets, ConvertObject);


    protected void Each(Action<T> update, bool dirty = false)
    {
        foreach (var t in Targets)
        {
            update(t);

            if (dirty)
            {
                EditorUtility.SetDirty(t);
            }
        }
    }

    protected bool Any(Func<T, bool> check)
    {
        foreach (var t in Targets)
        {
            if (check(t))
            {
                return true;
            }
        }

        return false;
    }

    protected bool All(Func<T, bool> check)
    {
        foreach (var t in Targets)
        {
            if (check(t) == false)
            {
                return false;
            }
        }

        return true;
    }
    
    // Method to prevent lambda alloc
    protected virtual T ConvertObject(Object o) => o as T;
}
