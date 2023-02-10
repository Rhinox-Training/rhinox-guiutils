using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;

namespace Rhinox.GUIUtils.Editor
{
    public static class DrawableFactory
    {
        private const int MAX_DEPTH = 10;
        
        public static ICollection<IOrderedDrawable> ParseNonUnityObject(object obj)
        {
            if (obj == null)
                return Array.Empty<SimpleDrawable>();
            
            var type = obj.GetType();

            var drawables = CreateDrawableMembersFor(obj, type);

            var buttons = FindButtons(obj);
            drawables.AddRange(buttons);

            var result = drawables.OrderBy(x => x.Order).ToArray();
            return result;
        }
        
        public static ICollection<IOrderedDrawable> ParseSerializedObject(SerializedObject obj)
        {
            if (obj == null || obj.targetObject == null)
                return Array.Empty<SimpleDrawable>();
            
            var type = obj.targetObject.GetType();

            var drawables = new List<IOrderedDrawable>();
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).ToList();
            foreach (var field in fields)
            {
                var prop = obj.FindProperty(field.Name);
                if (prop == null)
                {
                    if (field.IsPrivate)
                    {
                        var showInInspector = field.GetCustomAttribute<ShowInInspectorAttribute>();
                        if (showInInspector != null)
                        {
                            var readonlyDraw = new ReadOnlySmartDrawable(obj, field);
                            var propOrder2 = field.GetCustomAttribute<PropertyOrderAttribute>();
                            if (propOrder2 != null)
                                readonlyDraw.Order = propOrder2.Order;
                            drawables.Add(readonlyDraw);
                        }
                    }
                    continue;
                }

                IOrderedDrawable drawable = null;

                if (field.FieldType.InheritsFrom(typeof(IList)))
                    drawable = new DrawableList(prop);
                else
                    drawable = new UnityDrawableProperty(prop);
                
                var propOrder = field.GetCustomAttribute<PropertyOrderAttribute>();
                if (propOrder != null)
                    drawable.Order = propOrder.Order;

                drawables.Add(drawable);
            }
            
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).ToList();
            foreach (var property in properties)
            { 
                var showInInspector = property.GetCustomAttribute<ShowInInspectorAttribute>();
                if (showInInspector != null)
                {
                    var readonlyDraw = new ReadOnlySmartPropertyDrawable(obj, property);
                    var propOrder2 = property.GetCustomAttribute<PropertyOrderAttribute>();
                    if (propOrder2 != null)
                        readonlyDraw.Order = propOrder2.Order;
                    drawables.Add(readonlyDraw);
                }
            }

            var buttons = FindButtons(obj);
            drawables.AddRange(buttons);

            var result = drawables.OrderBy(x => x.Order).ToArray();
            return result;
        }
        
        private static ICollection<IOrderedDrawable> FindButtons(object obj)
        {
            var type = obj.GetType();
            
            var buttons = new List<IOrderedDrawable>();
            var buttonGroups = new List<DrawableButtonGroup>();
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var method in methods)
            {
                var buttonAttr = method.GetCustomAttribute<ButtonAttribute>();
                if (buttonAttr == null)
                    continue;

                var button = new DrawableButton(obj, method, buttonAttr);
                
                
                var propOrderAttr = method.GetCustomAttribute<PropertyOrderAttribute>();
                if (propOrderAttr != null)
                    button.Order = propOrderAttr.Order;

                var colour = method.GetCustomAttribute<GUIColorAttribute>();
                if (colour != null)
                    button.Colour = colour.Color;
                
                // Group or not
                var buttonGroup = method.GetCustomAttribute<PropertyGroupAttribute>() ?? method.GetCustomAttribute<ButtonGroupAttribute>();
                if (buttonGroup != null)
                {
                    var group = buttonGroups.FirstOrDefault(x => x.ID == buttonGroup.GroupID);
                    if (group == null)
                    {
                        group = new DrawableButtonGroup(obj, buttonGroup.GroupID);
                        group.Order = buttonGroup.Order;
                        buttonGroups.Add(group);
                    }
                    
                    group.AddButton(button);
                }
                else
                {
                    buttons.AddUnique(button);
                }
            }

            buttons.AddRange(buttonGroups);

            return buttons;
        }
        
        public static ICollection<IOrderedDrawable> CreateDrawableMembersFor(object instance, Type t)
        {
            return CreateDrawableMembersFor(instance, t, 0);
        }

        private static ICollection<IOrderedDrawable> CreateDrawableMembersFor(object instance, Type t, int depth)
        {
            var publicAndSerializedMembers = SerializeHelper.GetPublicAndSerializedMembers(t);
            var drawableMembers = new List<IOrderedDrawable>();
            foreach (var memberInfo in publicAndSerializedMembers)
            {
                if (memberInfo == null)
                    continue;

                IOrderedDrawable resultingMember = null;
                if (!TryCreate(instance, memberInfo, out var drawableMember) && depth < MAX_DEPTH)
                {
                    var subInstance = memberInfo.GetValue(instance);
                    var subtype = memberInfo.GetReturnType();
                    var subdrawables = CreateDrawableMembersFor(subInstance, subtype, depth + 1);
                    resultingMember = new CompositeDrawableMember(subdrawables);
                }
                else
                    resultingMember = drawableMember;

                if (resultingMember != null)
                    drawableMembers.Add(resultingMember);
            }

            return drawableMembers;
        }

        private static bool TryCreate(object instance, MemberInfo info, out IOrderedDrawable drawableMember)
        {
            var type = info.GetReturnType();

            if (type == typeof(string))
            {
                drawableMember = new StringDrawableField(instance, info);
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
                drawableMember = new DrawableList(instance as IList);
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