using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public static class DrawableFactory
    {
        private const int MAX_DEPTH = 10;
        
        public static IOrderedDrawable CreateDrawableForProperty(FieldInfo field, SerializedProperty prop)
        {
            IOrderedDrawable drawable;
            if (field.FieldType.InheritsFrom(typeof(IList)))
                drawable = new DrawableList(prop);
            else
                drawable = new DrawableUnityProperty(prop, field);
            var propOrder = field.GetCustomAttribute<PropertyOrderAttribute>();
            if (propOrder != null)
                drawable.Order = propOrder.Order;
            
            // Check for decorators
            foreach (var attr in field.GetCustomAttributes())
            {
                if (DrawableWrapperFactory.TryCreateWrapper(attr, drawable, out WrapperDrawable wrappedDrawable))
                    drawable = wrappedDrawable;
            }

            return drawable;
        }

        public static void SortDrawables(this List<IOrderedDrawable> drawables)
        {
            foreach (var drawable in drawables)
            {
                if (drawable is CompositeDrawableMember compositeDrawableMember)
                    compositeDrawableMember.Sort();
            }

            drawables.SortBy(x => x.Order);
        }

        public static ICollection<IOrderedDrawable> FindButtons(object instance)
        {
            if (instance == null) return Array.Empty<IOrderedDrawable>();
            
            var type = instance.GetType();

            var buttons = new List<IOrderedDrawable>();
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var method in methods)
            {
                var buttonAttr = method.GetCustomAttribute<ButtonAttribute>();
                if (buttonAttr == null)
                    continue;

                var button = new DrawableButton(instance, method, buttonAttr);
                buttons.AddUnique(button);
            }

            return buttons;
        }

        public static List<IOrderedDrawable> CreateDrawableMembersFor(SerializedProperty property, Type type)
        {
            return CreateDrawableMembersFor(property, type, 0);
        }


        private static List<IOrderedDrawable> CreateDrawableMembersFor(SerializedProperty property, Type type, int depth)
        {
            if (property == null)
                return null;
            
            object instanceVal = property.GetValue();
            
            if (type == typeof(GameObject))
                return new List<IOrderedDrawable>() {new DrawableUnityObject(instanceVal, property.FindFieldInfo())};
            if (type.InheritsFrom<UnityEngine.Object>())
                return new List<IOrderedDrawable>() {new DrawableUnityProperty(property, property.FindFieldInfo())};
            
            var visibleFields = property.EnumerateEditorVisibleFields();
            return DrawableMembersFor(instanceVal, type, visibleFields, depth);
        }

        private static List<IOrderedDrawable> DrawableMembersFor(object instanceVal, Type type, IEnumerable<SerializedObjectExtensions.FieldData> visibleFields, int depth)
        {
            var drawables = new List<IOrderedDrawable>();
            foreach (var fieldData in visibleFields)
            {
                IOrderedDrawable fieldDrawable = null;
                if (fieldData.OverrideDrawable != null)
                {
                    fieldDrawable = fieldData.OverrideDrawable;
                }
                else if (!fieldData.IsSerialized)
                {
                    var val = fieldData.FieldInfo.GetValue(instanceVal);
                    fieldDrawable = CreateCompositeMemberForInstance(val, depth, fieldData.FieldInfo);
                }
                else
                {
                    fieldDrawable = CreateDrawableForProperty(fieldData.FieldInfo, fieldData.SerializedProperty);
                    AttributeParser.Parse(fieldData.FieldInfo, ref fieldDrawable);
                }

                if (fieldDrawable != null)
                    drawables.Add(fieldDrawable);
            }

            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .ToList();
            foreach (var propertyMember in properties)
            {
                var showInInspector = propertyMember.GetCustomAttribute<ShowInInspectorAttribute>();
                if (showInInspector != null)
                {
                    var propVal = propertyMember.GetValue(instanceVal);
                    var propertyDrawable = CreateCompositeMemberForInstance(propVal, depth, propertyMember);
                    if (propertyDrawable != null)
                        drawables.Add(propertyDrawable);
                }
            }

            var buttons = FindButtons(instanceVal);
            drawables.AddRange(buttons);

            return drawables;
        }
        
        public static List<IOrderedDrawable> CreateDrawableMembersFor(SerializedObject obj, Type type)
        {
            object instanceVal = obj.targetObject;
            
            if (type == typeof(GameObject))
                return new List<IOrderedDrawable>() {new DrawableUnityObject(instanceVal, null)};
            
            var visibleFields = obj.EnumerateEditorVisibleFields();
            return DrawableMembersFor(instanceVal, type, visibleFields, 0);
        }

        public static List<IOrderedDrawable> CreateDrawableMembersFor(object instance, Type t)
        {
            if (t == typeof(GameObject))
                return new List<IOrderedDrawable>() {new DrawableUnityObject(instance, null)};
            return CreateDrawableMembersFor(instance, t, 0);
        }

        private static List<IOrderedDrawable> CreateDrawableMembersFor(object instance, Type t, int depth)
        {
            var members = GetEditorVisibleFields(instance, t);
            var drawableMembers = new List<IOrderedDrawable>();
            foreach (var memberInfo in members)
            {
                if (memberInfo == null)
                    continue;

                if (memberInfo is PropertyInfo propertyInfo)
                {
                    // If the property has no getter
                    if (propertyInfo.GetGetMethod(false) == null)
                        continue;

                    if (memberInfo.GetCustomAttribute<SerializeField>() == null &&
                        memberInfo.GetCustomAttribute<ShowInInspectorAttribute>() == null)
                        continue;
                }

                IOrderedDrawable resultingMember = null;
                if (TryCreate(instance, memberInfo, out var drawableMember) || depth >= MAX_DEPTH)
                    resultingMember = drawableMember;
                else
                    resultingMember = CreateCompositeMemberForInstance(instance, depth, memberInfo);

                if (resultingMember == null)
                    continue;

                // Check for decorators
                foreach (var attr in memberInfo.GetCustomAttributes())
                {
                    if (DrawableWrapperFactory.TryCreateWrapper(attr, resultingMember, out WrapperDrawable wrappedDrawable))
                        resultingMember = wrappedDrawable;
                }

                drawableMembers.Add(resultingMember);
            }
            
            
            var buttons = FindButtons(instance);
            drawableMembers.AddRange(buttons);


            return drawableMembers;
        }

        private static IReadOnlyCollection<MemberInfo> GetEditorVisibleFields(object instance, Type t)
        {
            // All public members
            var publicMembers = t.GetMembers(BindingFlags.Instance | BindingFlags.Public |
                                                BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.FlattenHierarchy);
            publicMembers = publicMembers
                .Where(x => !(x is MethodInfo))
                .ToArray();
            
            // All non-publics that serialize or are visible
            var serializedMembers = t.GetMembers(BindingFlags.Instance | BindingFlags.NonPublic |
                                                    BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.FlattenHierarchy);
            serializedMembers = serializedMembers
                .Where(x => !(x is MethodInfo))
                .Where(x => x.IsSerialized() || x.GetCustomAttribute<ShowInInspectorAttribute>() != null)
                .ToArray();

            var list = new List<MemberInfo>();
            list.AddRange(publicMembers);
            list.AddRange(serializedMembers);
            
            return list;
        }

        private static IOrderedDrawable CreateCompositeMemberForInstance(object instance, int depth, MemberInfo memberInfo)
        {
            IOrderedDrawable resultingMember;
            try
            {
                var subInstance = memberInfo.GetValue(instance);
                var subtype = memberInfo.GetReturnType();
                var subdrawables = CreateDrawableMembersFor(subInstance, subtype, depth + 1);
                DrawableGroupingHelper.Process(ref subdrawables);
                var composite = new CompositeDrawableMember();
                var attributes = memberInfo.GetCustomAttributes<Attribute>();

                foreach (var attr in attributes)
                {
                    if (attr is PropertyGroupAttribute groupAttribute)
                        composite.Order = groupAttribute.Order;
                    composite.AddAttribute(attr);
                }

                composite.AddRange(subdrawables);
                resultingMember = composite;
            }
            catch (Exception /*e*/)
            {
                resultingMember = null;
            }

            return resultingMember;
        }

        private static bool TryCreate(object instance, MemberInfo info, out IOrderedDrawable drawableMember)
        {
            var type = info.GetReturnType();

            if (type == null)
            {
                drawableMember = null;
                return false;
            }

            if (type == typeof(string))
            {
                drawableMember = new StringDrawableField(instance, info);
                return true;
            }

            if (type.IsEnum)
            {
                drawableMember = new EnumDrawableField(instance, info);
                return true;
            }

            if (type == typeof(int))
            {
                drawableMember = new IntDrawableField(instance, info);
                return true;
            }

            if (type == typeof(float))
            {
                drawableMember = new FloatDrawableField(instance, info);
                return true;
            }

            if (type == typeof(bool))
            {
                drawableMember = new BoolDrawableField(instance, info);
                return true;
            }
            
            if (type == typeof(Type))
            {
                drawableMember = new UndrawableField<Type>(instance, info);
                return true;
            }

            if (type.InheritsFrom<IList>())
            {
                drawableMember = new DrawableList(instance, info);
                return true;
            }

            if (type.InheritsFrom<Texture>())
            {
                drawableMember = new TextureDrawableField(instance, info);
                return true;
            }

            if (type.InheritsFrom<UnityEngine.Object>())
            {
                drawableMember = new UnityObjectDrawableField(instance, info);
                return true;
            }

            drawableMember = null;
            return false;
        }
    }
}