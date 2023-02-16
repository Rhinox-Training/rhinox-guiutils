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
        
        public static ICollection<IOrderedDrawable> ParseNonUnityObject(object obj)
        {
            if (obj == null)
                return Array.Empty<SimpleDrawable>();
            
            var type = obj.GetType();

            var drawables = CreateDrawableMembersFor(obj, type);

            var buttons = FindButtons(obj);
            drawables.AddRange(buttons);


            Help(ref drawables);
            

            foreach (var drawable in drawables)
            {
                if (drawable is CompositeDrawableMember compositeDrawableMember)
                    compositeDrawableMember.Sort();
            }
            
            var result = drawables.OrderBy(x => x.Order).ToArray();
            return result;
        }

        private static void HandleGrouping(ref List<IOrderedDrawable> drawables)
        {
            var grouping = new Dictionary<PropertyGroupAttribute, List<IOrderedDrawable>>();
            var finalList = new List<IOrderedDrawable>();
            foreach (var drawable in drawables)
            {
                if (drawable == null)
                    continue;

                var groupingAttributes = drawable.GetMemberAttributes<PropertyGroupAttribute>();
                if (groupingAttributes.IsNullOrEmpty())
                {
                    finalList.AddUnique(drawable);
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

            var remainingGroupings = grouping.Keys.ToList();
            remainingGroupings.SortByDescending(x => x.GroupID.Length);
            while (remainingGroupings.Count > 0)
            {
                var target = remainingGroupings.First();
                remainingGroupings.RemoveAt(0);

                var targetDrawables = grouping[target];
                var converted = new CompositeDrawableMember("", target.Order);
                converted.AddRange(targetDrawables);
                converted.GroupBy(target);

                grouping.Remove(target);

                bool someoneContains = false;
                foreach (var entry in grouping)
                {
                    if (entry.Value.ContainsAny(targetDrawables))
                    {
                        entry.Value.RemoveRange(targetDrawables);
                        entry.Value.Add(converted);
                        entry.Value.SortBy(x => x.Order);
                        someoneContains = true;
                    }
                }

                if (!someoneContains)
                {
                    var firstDrawable = converted.FirstOrDefault(x => !(x is CompositeDrawableMember));
                    int index = drawables.IndexOf(firstDrawable);
                    if (index == -1)
                        finalList.Add(converted);
                    else
                        finalList.Insert(index, converted);
                }
            }

            drawables = finalList;
        }
        
        private const string GroupingString = "/";

        private static void Help(ref List<IOrderedDrawable> drawables)
        {
            var rootItems = new List<IOrderedDrawable>();
        
            var lookup = new Dictionary<string, CompositeDrawableMember>();
            foreach (var drawable in drawables)
            {
                
                var groupingAttributes = drawable.GetMemberAttributes<PropertyGroupAttribute>();
                if (groupingAttributes.IsNullOrEmpty())
                {
                    rootItems.Add(drawable);
                    continue;
                }
                
                foreach (var groupingAttribute in groupingAttributes)
                {

                    string itemName = groupingAttribute.GroupID;

                    
                    var splitI = groupingAttribute.GroupID.LastIndexOf(GroupingString, StringComparison.InvariantCulture);
                    if (splitI < 0)
                    {
                        if (!lookup.ContainsKey(itemName))
                        {
                            var grouping = new CompositeDrawableMember(itemName);
                            grouping.GroupBy(groupingAttribute);
                            lookup.Add(itemName, grouping);
                            rootItems.Add(grouping);
                        }

                        lookup[itemName].Add(drawable);
                        continue;
                    }
                    
                    var parts = itemName.Split(new[] {GroupingString}, StringSplitOptions.RemoveEmptyEntries);
                    CompositeDrawableMember hierarchy = null;
                    for (int i = 0; i < parts.Length; ++i)
                    {
                        string full = string.Join(GroupingString, parts.Take(i + 1));
                        if (lookup.ContainsKey(full))
                        {
                            hierarchy = lookup[full];
                            continue;
                        }

                        var next = new CompositeDrawableMember(full);
                        next.GroupBy(groupingAttribute);
                        hierarchy?.Add(next);
                        hierarchy = next;
                        lookup[full] = next;
                    }
                    
                    lookup[itemName].Add(drawable);
                }
            }

            drawables = rootItems;
        }

        private static PropertyGroupAttribute FindKey(Dictionary<PropertyGroupAttribute, List<IOrderedDrawable>> grouping, PropertyGroupAttribute attr)
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
            
            HandleGrouping(ref drawables);


            foreach (var drawable in drawables)
            {
                if (drawable is CompositeDrawableMember compositeDrawableMember)
                    compositeDrawableMember.Sort();
            }
            
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
                        Help(ref subdrawables);
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
    }
}