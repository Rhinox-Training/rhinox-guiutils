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
        private const string GroupingString = "/";

        public static ICollection<IOrderedDrawable> ParseNonUnityObject(object obj)
        {
            if (obj == null)
                return Array.Empty<SimpleDrawable>();

            var type = obj.GetType();

            var drawables = CreateDrawableMembersFor(obj, type);
            
            if (drawables.Count == 0 && obj is UnityEngine.Object unityObj)
                drawables.Add(new DrawableUnityObject(unityObj));

            var buttons = FindButtons(obj);
            drawables.AddRange(buttons);

            HandleGrouping(ref drawables);

            drawables.SortDrawables();
            return drawables;
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

            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .ToList();
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

            HandleGrouping(ref drawables);

            drawables.SortDrawables();
            return drawables;
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

        private static void HandleGrouping(ref List<IOrderedDrawable> drawables)
        {
            var drawablesByGroup = CreateLookupByGroupAttribute(drawables);

            var remainingGroupings = drawablesByGroup.Keys.ToList();
            remainingGroupings.SortByDescending(x => x.GroupID.Length);

            // Create composites (unconnected)
            var idLookup = new Dictionary<string, CompositeDrawableMember>();
            while (remainingGroupings.Count > 0)
            {
                var currentGroupAttr = remainingGroupings.First();
                remainingGroupings.RemoveAt(0);

                var compositeMember = CompositeDrawableMember.CreateFrom(currentGroupAttr);
                var childDrawables = drawablesByGroup[currentGroupAttr];

                compositeMember.AddRange(childDrawables);

                // Clean entries from higher located composite groups
                foreach (var key in drawablesByGroup.Keys)
                {
                    var curDrawables = drawablesByGroup[key];
                    curDrawables.RemoveRange(childDrawables);
                }

                // Store in lookup
                idLookup.Add(currentGroupAttr.GroupID, compositeMember);
            }

            var finalList = new List<IOrderedDrawable>();
            // Create tree structure
            foreach (var drawable in drawables)
            {
                var groupingAttributes = drawable.GetMemberAttributes<PropertyGroupAttribute>();
                if (groupingAttributes.IsNullOrEmpty())
                {
                    finalList.Add(drawable);
                    continue;
                }

                foreach (var groupingAttribute in groupingAttributes.OrderByDescending(x => x.GroupID.Length))
                {
                    string groupId = groupingAttribute.GroupID;

                    var parts = groupId.Split(new[] {GroupingString}, StringSplitOptions.RemoveEmptyEntries);
                    CompositeDrawableMember curParent = null;
                    for (int i = 0; i < parts.Length; ++i)
                    {
                        string idAtCurrentDepth = string.Join(GroupingString, parts.Take(i + 1));
                        if (idLookup.ContainsKey(idAtCurrentDepth))
                        {
                            if (curParent != null)
                            {
                                if (!curParent.Children.Contains(idLookup[idAtCurrentDepth]))
                                    curParent.Add(idLookup[idAtCurrentDepth]);
                            }

                            curParent = idLookup[idAtCurrentDepth];
                        }
                        else
                        {
                            var next = new CompositeDrawableMember(idAtCurrentDepth, groupingAttribute.Order);
                            if (curParent == null)
                                finalList.Add(next);
                            else
                                curParent.Add(next);
                            curParent = next;
                            idLookup.Add(idAtCurrentDepth, next);
                        }
                    }
                }
            }

            // Add root most groupings to finalList
            foreach (var entry in idLookup.Keys.ToArray())
            {
                var group = idLookup[entry];
                bool isTopLevel = true;
                foreach (var otherGroup in idLookup.Values)
                {
                    if (otherGroup == group)
                        continue;

                    if (otherGroup.Children.Contains(group))
                    {
                        isTopLevel = false;
                        break;
                    }
                }

                if (isTopLevel)
                    finalList.Add(group);
            }

            // Return list
            drawables = finalList;
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
                // var buttonGroup = method.GetCustomAttribute<PropertyGroupAttribute>() ?? method.GetCustomAttribute<ButtonGroupAttribute>();
                // if (buttonGroup != null)
                // {
                //     var group = buttonGroups.FirstOrDefault(x => x.ID == buttonGroup.GroupID);
                //     if (group == null)
                //     {
                //         group = new DrawableButtonGroup(obj, buttonGroup.GroupID);
                //         group.Order = buttonGroup.Order;
                //         buttonGroups.Add(group);
                //     }
                //     
                //     group.AddButton(button);
                // }
                // else
                {
                    buttons.AddUnique(button);
                }
            }

            buttons.AddRange(buttonGroups);

            return buttons;
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
                    try
                    {
                        var subInstance = memberInfo.GetValue(instance);
                        var subtype = memberInfo.GetReturnType();
                        var subdrawables = CreateDrawableMembersFor(subInstance, subtype, depth + 1);
                        HandleGrouping(ref subdrawables);
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

        private static Dictionary<PropertyGroupAttribute, List<IOrderedDrawable>> CreateLookupByGroupAttribute(
            List<IOrderedDrawable> drawables)
        {
            var grouping = new Dictionary<PropertyGroupAttribute, List<IOrderedDrawable>>();
            foreach (var drawable in drawables)
            {
                if (drawable == null)
                    continue;

                var groupingAttributes = drawable.GetMemberAttributes<PropertyGroupAttribute>();
                if (groupingAttributes.IsNullOrEmpty())
                {
                    continue;
                }

                foreach (var attr in groupingAttributes)
                {
                    var key = FindKey(grouping, attr);
                    if (key == null)
                    {
                        grouping.Add(attr, new List<IOrderedDrawable>());
                        key = attr;
                    }

                    var list = grouping[key];
                    list.AddUnique(drawable);
                    grouping[key] = list;
                }
            }

            return grouping;
        }

        private static PropertyGroupAttribute FindKey(
            Dictionary<PropertyGroupAttribute, List<IOrderedDrawable>> grouping, PropertyGroupAttribute attr)
        {
            foreach (var group in grouping)
            {
                if (group.Key.GetType() != attr.GetType())
                    continue;

                if (group.Key.GroupID != null && group.Key.GroupID.Equals(attr.GroupID))
                {
                    return group.Key;
                }
            }

            return null;
        }
    }
}