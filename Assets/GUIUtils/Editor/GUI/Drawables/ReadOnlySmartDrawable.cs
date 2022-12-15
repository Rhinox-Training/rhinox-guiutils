using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class ReadOnlySmartDrawable : SimpleDrawable
    {
        private readonly FieldInfo _info;
        private IDrawableMember _drawable;

        public ReadOnlySmartDrawable(SerializedObject obj, FieldInfo info, int order = 0) 
            : base(obj, order)
        {
            _info = info;
            _drawable = DrawableMemberFactory.Create(_info);
        }

        protected override void Draw(Object target)
        {
            EditorGUI.BeginDisabledGroup(true);
            _drawable.Draw(target);
            EditorGUI.EndDisabledGroup();
        }
    }
    
    public class ReadOnlySmartPropertyDrawable : SimpleDrawable
    {
        private readonly PropertyInfo _info;
        private IDrawableMember _drawable;

        public ReadOnlySmartPropertyDrawable(SerializedObject obj, PropertyInfo info, int order = 0) 
            : base(obj, order)
        {
            _info = info;
            _drawable = DrawableMemberFactory.Create(_info);
        }

        protected override void Draw(Object target)
        {
            EditorGUI.BeginDisabledGroup(true);
            _drawable.Draw(target);
            EditorGUI.EndDisabledGroup();
        }
    }
}