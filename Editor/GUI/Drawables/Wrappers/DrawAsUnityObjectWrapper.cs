using Rhinox.GUIUtils.Attributes;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    // TODO: how do we support this
    // public class DrawAsUnityObjectWrapper : BaseWrapperDrawable
    // {
    //     public DrawAsUnityObjectWrapper(IOrderedDrawable drawable) : base(drawable)
    //     {
    //     }
    //
    //     protected override void DrawInner(Rect rect, GUIContent label)
    //     {
    //         EditorGUI.ObjectField(rect, label, (UnityEngine.Object) Host, typeof(UnityEngine.Object));
    //     }
    //     
    //     
    //     [WrapDrawer(typeof(DrawAsUnityObjectAttribute))]
    //     public static BaseWrapperDrawable Create(DrawAsUnityObjectAttribute attr, IOrderedDrawable drawable)
    //     {
    //         return new DrawAsUnityObjectWrapper(drawable)
    //         {
    //         };
    //     }
    // }
}