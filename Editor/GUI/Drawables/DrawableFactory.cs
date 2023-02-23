using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework.Internal;
using Rhinox.GUIUtils.Attributes;
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

        public static ICollection<IOrderedDrawable> ParseNonUnityObject(object obj)
        {
            if (obj == null)
                return Array.Empty<BaseEntityDrawable>();

            var type = obj.GetType();

            var drawables = CreateDrawableMembersFor(obj, type);

            if (drawables.Count == 0 && obj is UnityEngine.Object unityObj)
                drawables.Add(new DrawableUnityObject(unityObj));

            var buttons = FindButtons(obj);
            drawables.AddRange(buttons);

            DrawableGroupingHelper.Process(ref drawables);

            drawables.SortDrawables();
            return drawables;
        }

        public static ICollection<IOrderedDrawable> ParseSerializedProperty(SerializedProperty property)
        {
            if (property == null)
                return Array.Empty<IOrderedDrawable>();

            var hostInfo = property.GetHostInfo();
            var type = hostInfo.GetReturnType();

            if (AttributeParser.ParseDrawAsUnity(hostInfo.FieldInfo))
                return new[] {new DrawableUnityObject(property.GetValue())};

            var drawables = CreateDrawableMembersFor(property, type);

            DrawableGroupingHelper.Process(ref drawables);

            drawables.SortDrawables();

            return drawables;
        }

        public static ICollection<IOrderedDrawable> ParseSerializedObject(SerializedObject obj)
        {
            if (obj == null || obj.targetObject == null)
                return Array.Empty<IOrderedDrawable>();

            var type = obj.targetObject.GetType();

            var drawables = CreateDrawableMembersFor(obj, type);

            DrawableGroupingHelper.Process(ref drawables);

            drawables.SortDrawables();
            return drawables;
        }

        private static IOrderedDrawable CreateDrawableForProperty(FieldInfo field, SerializedProperty prop)
        {
            IOrderedDrawable drawable;
            if (field.FieldType.InheritsFrom(typeof(IList)))
                drawable = new DrawableList(prop);
            else
                drawable = new DrawableUnityProperty(prop);

            var propOrder = field.GetCustomAttribute<PropertyOrderAttribute>();
            if (propOrder != null)
                drawable.Order = propOrder.Order;
            return drawable;
        }

        private static void SortDrawables(this List<IOrderedDrawable> drawables)
        {
            foreach (var drawable in drawables)
            {
                if (drawable is CompositeDrawableMember compositeDrawableMember)
                    compositeDrawableMember.Sort();
            }

            drawables.SortBy(x => x.Order);
        }

        private static ICollection<IOrderedDrawable> FindButtons(object instance)
        {
            var type = instance.GetType();

            var buttons = new List<IOrderedDrawable>();
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var method in methods)
            {
                var buttonAttr = method.GetCustomAttribute<ButtonAttribute>();
                if (buttonAttr == null)
                    continue;

                var button = new DrawableButton(instance, method, buttonAttr);

                var propOrderAttr = method.GetCustomAttribute<PropertyOrderAttribute>();
                if (propOrderAttr != null)
                    button.Order = propOrderAttr.Order;

                var colour = method.GetCustomAttribute<GUIColorAttribute>();
                if (colour != null)
                    button.Colour = colour.Color;

                buttons.AddUnique(button);
            }

            return buttons;
        }

        private static List<IOrderedDrawable> CreateDrawableMembersFor(SerializedProperty property, Type type)
        {
            return CreateDrawableMembersFor(property, type, 0);
        }


        private static List<IOrderedDrawable> CreateDrawableMembersFor(SerializedProperty property, Type type, int depth)
        {
            var visibleFields = property.EnumerateEditorVisibleFields();
            object instanceVal = property.GetValue();
            
            return DrawableMembersFor(instanceVal, type, visibleFields, depth);
        }

        private static List<IOrderedDrawable> DrawableMembersFor(object instanceVal, Type type, IEnumerable<SerializedObjectExtensions.FieldData> visibleFields, int depth)
        {
            var drawables = new List<IOrderedDrawable>();
            foreach (var fieldData in visibleFields)
            {
                IOrderedDrawable fieldDrawable = null;
                if (!fieldData.IsSerialized)
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


        private static List<IOrderedDrawable> CreateDrawableMembersFor(SerializedObject obj, Type t)
        {
            return CreateDrawableMembersFor(obj, t, 0);
        }

        private static List<IOrderedDrawable> CreateDrawableMembersFor(SerializedObject obj, Type type, int depth)
        {
            var visibleFields = obj.EnumerateEditorVisibleFields();
            object instanceVal = obj.targetObject;
            
            return DrawableMembersFor(instanceVal, type, visibleFields, depth);
        }

        private static List<IOrderedDrawable> CreateDrawableMembersFor(object instance, Type t)
        {
            return CreateDrawableMembersFor(instance, t, 0);
        }

        private static List<IOrderedDrawable> CreateDrawableMembersFor(object instance, Type t, int depth)
        {
            var publicAndSerializedMembers = SerializeHelper.GetPublicAndSerializedMembers(t);
            var drawableMembers = new List<IOrderedDrawable>();
            foreach (var memberInfo in publicAndSerializedMembers)
            {
                if (memberInfo == null)
                    continue;

                if (memberInfo is PropertyInfo propertyInfo)
                {
                    if (propertyInfo.GetGetMethod(false) == null)
                        continue;

                    if (memberInfo.GetCustomAttribute<SerializeField>() == null &&
                        memberInfo.GetCustomAttribute<ShowInInspectorAttribute>() == null)
                        continue;
                }

                IOrderedDrawable resultingMember = null;
                if (!TryCreate(instance, memberInfo, out var drawableMember) && depth < MAX_DEPTH)
                {
                    resultingMember = CreateCompositeMemberForInstance(instance, depth, memberInfo);
                }
                else
                    resultingMember = drawableMember;

                if (resultingMember != null)
                    drawableMembers.Add(resultingMember);
            }

            return drawableMembers;
        }

        private static IOrderedDrawable CreateCompositeMemberForInstance(object instance, int depth,
            MemberInfo memberInfo)
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
                if (attributes != null)
                {
                    foreach (var attr in attributes)
                    {
                        if (attr is PropertyGroupAttribute groupAttribute)
                            composite.Order = groupAttribute.Order;
                        composite.AddAttribute(attr);
                    }
                }

                composite.AddRange(subdrawables);
                resultingMember = composite;
            }
            catch (Exception e)
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

            if (type.InheritsFrom<IList>())
            {
                drawableMember = new DrawableList(instance, info);
                return true;
            }

            if (type.InheritsFrom<UnityEngine.Object>())
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