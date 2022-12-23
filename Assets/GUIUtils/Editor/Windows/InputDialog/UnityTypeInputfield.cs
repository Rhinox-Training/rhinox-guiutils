using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class BaseUnityObjectInputField<T> : DialogInputField<T> where T : UnityEngine.Object
    {
        protected BaseUnityObjectInputField(string label, string tooltip = null, T initialValue = default(T)) 
            : base(label, tooltip, initialValue)
        {
        }
        
        protected override void DrawFieldValue(Rect rect)
        {
            SmartValue = EditorGUI.ObjectField(rect, SmartValue, typeof(T), true) as T;
        }
    }
    
    public class GenericUnityObjectInputField<T> : BaseUnityObjectInputField<T> where T : UnityEngine.Object
    {
        public GenericUnityObjectInputField(string label, string tooltip = null, T initialValue = default(T)) 
            : base(label, tooltip, initialValue)
        {
        }
    }

    public class TransformInputField : BaseUnityObjectInputField<Transform>
    {
        public TransformInputField(string label, string tooltip = null, Transform initialValue = default(Transform)) 
            : base(label, tooltip, initialValue)
        {
        }
    }

    public class GameObjectInputField : BaseUnityObjectInputField<GameObject>
    {
        public GameObjectInputField(string label, string tooltip = null, GameObject initialValue = default(GameObject)) 
            : base(label, tooltip, initialValue)
        {
        }
    }
    
    public class MaterialInputField : BaseUnityObjectInputField<Material>
    {
        public MaterialInputField(string label, string tooltip = null, Material initialValue = default(Material)) 
            : base(label, tooltip, initialValue)
        {
        }
    }
    
    public class TextureInputField : BaseUnityObjectInputField<Texture>
    {
        public TextureInputField(string label, string tooltip = null, Texture initialValue = default(Texture)) 
            : base(label, tooltip, initialValue)
        {
        }
    }
}