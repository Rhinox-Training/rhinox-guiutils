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
        
        //==============================================================================================================
        // Public API
        public static IOrderedDrawable CreateDrawableFor(GenericHostInfo hostInfo)
        {
            return CreateDrawableFor(hostInfo.GetValue(), hostInfo.GetReturnType(), hostInfo);
        }

        public static IOrderedDrawable CreateDrawableFor(object instance, Type type)
        {
            return CreateDrawableFor(instance, type, null);
        }
        
        public static IOrderedDrawable CreateDrawableFor(SerializedObject obj, Type type)
        {
            object instanceVal = obj.targetObject;

            if (type == null)
                throw new ArgumentNullException(nameof(type));
            
            var visibleFields = obj.EnumerateEditorVisibleFields();
            var drawable = DrawableMembersForSerializedObject(instanceVal, type, visibleFields, 0);
            return new ObjectCompositeDrawableMember(instanceVal, type, drawable, string.Empty);
        }
        
        public static IOrderedDrawable CreateDrawableFor(SerializedProperty property)
        {
            if (property == null)
                return null;

            var drawable = CreateDrawableForSerializedProperty(property);
            if (drawable == null)
                return null;
            
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

        //==============================================================================================================
        // Helper methods

        private static IOrderedDrawable CreateDrawableFor(object instance, Type type, GenericHostInfo parent)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (parent != null)
                return CreateDrawableForMember(parent, 0);
            
            return CreateCompositeDrawable(instance, type, 0);
        }
        
        private static IOrderedDrawable DrawableMembersForSerializedObject(object instanceVal, Type type, IEnumerable<SerializedObjectExtensions.FieldData> visibleFields, int depth)
        {
            var drawable = new VerticalGroupDrawable();
            foreach (var fieldData in visibleFields)
            {
                IOrderedDrawable fieldDrawable = null;
                if (fieldData.OverrideDrawable != null)
                    fieldDrawable = fieldData.OverrideDrawable;
                else if (!fieldData.IsSerialized)
                {
                    var hostInfo = new GenericHostInfo(instanceVal, fieldData.FieldInfo);
                    fieldDrawable = CreateDrawableForMember(hostInfo, depth);
                }
                else
                    fieldDrawable = CreateDrawableForSerializedProperty(fieldData.SerializedProperty, new GenericHostInfo(instanceVal, fieldData.FieldInfo));

                if (fieldDrawable == null)
                    continue;
                
                drawable.Add(fieldDrawable);
            }

            foreach (var propertyMember in type.GetEditorVisibleProperties())
            {
                var hostInfo = new GenericHostInfo(instanceVal, propertyMember);
                var propertyDrawable = CreateDrawableForMember(hostInfo, depth);
                if (propertyDrawable == null)
                    continue;

                drawable.Add(propertyDrawable);
            }

            var buttons = FindCustomDrawables(instanceVal);
            drawable.AddRange(buttons);

            return drawable;
        }

        private static IOrderedDrawable CreateDrawableForMember(GenericHostInfo hostInfo, int depth)
        {
            IOrderedDrawable resultingMember;
            if (TryCreateDirect(hostInfo, out var drawableMember) || depth >= MAX_DEPTH)
                resultingMember = drawableMember;
            else
            {
                var subInstance = hostInfo.GetValue();
                var subtype = hostInfo.GetReturnType();
                
                if (subInstance != null)
                    subtype = subInstance.GetType();

                resultingMember = CreateCompositeDrawable(subInstance, subtype, depth + 1, hostInfo);
            }

            if (resultingMember == null)
                return null;

            // Check for decorators
            resultingMember = DrawableWrapperFactory.TryWrapDrawable(resultingMember, hostInfo.GetAttributes());
            return resultingMember;
        }

        private static IOrderedDrawable CreateCompositeDrawable(object instance, Type t, int depth, GenericHostInfo hostInfo = null)
        {
            CompositeDrawableMember drawable = new VerticalGroupDrawable();

            var memberEntries = GetEditorVisibleFields(instance, t, hostInfo);
            var drawables = new List<IOrderedDrawable>();
            foreach (var memberEntry in memberEntries)
            {
                if (memberEntry.MemberInfo is PropertyInfo propertyInfo && !propertyInfo.IsVisibleInEditor())
                    continue;

                var resultingMember = CreateDrawableForMember(memberEntry, depth);

                if (resultingMember != null)
                    drawables.Add(resultingMember);
            }

            drawable.AddRange(drawables);
            drawable.Sort();
            
            var buttons = FindCustomDrawables(instance);
            drawable.AddRange(buttons);

            // Wrap in object composite
            if (hostInfo != null)
                drawable = ObjectCompositeDrawableMember.CreateFrom(hostInfo, drawable);
            else
                drawable = new ObjectCompositeDrawableMember(instance, t, drawable);
            
            return drawable;
        }

        private static IOrderedDrawable CreateDrawableForSerializedProperty(SerializedProperty property, GenericHostInfo upperHostInfo = null) // TODO: upperHostInfo naming and abstraction is confusing
        {
            if (property == null)
                return null;

            IOrderedDrawable drawable = null;
            IEnumerable<Attribute> attributes = null;
            if (CanUnityHandleDrawingProperty(property))
            {
                drawable = new DrawableUnityProperty(property);
                attributes = property.GetAttributes();
            }
            else
            {
                var hostInfo = property.GetHostInfo();
                object instanceVal = hostInfo.GetValue();

                attributes = hostInfo.GetAttributes();
                if (instanceVal == null)
                    drawable = new NullReferenceDrawable(property);
                else
                {
                    if (AttributeParser.ParseDrawAsUnity(hostInfo.MemberInfo))
                        drawable = new UnityObjectDrawableField(hostInfo);
                    else
                    {
                        if (CheckOverrideDrawer(property, hostInfo, out IOrderedDrawable overrideDrawer))
                        {
                            drawable = overrideDrawer;
                        }
                        else
                        {
                            var returnType = hostInfo.GetReturnType();
                            var visibleFields = property.EnumerateEditorVisibleFields();
                            drawable = DrawableMembersForSerializedObject(instanceVal, returnType, visibleFields, 0);
                            if (upperHostInfo != null)
                                drawable = ObjectCompositeDrawableMember.CreateFrom(upperHostInfo, drawable);
                            else
                                drawable = new ObjectCompositeDrawableMember(instanceVal, returnType, drawable);
                        }
                    }
                }
            }

            // Check for decorators
            drawable = DrawableWrapperFactory.TryWrapDrawable(drawable, attributes);
            
            return drawable;
        }

        private static bool CheckOverrideDrawer(SerializedProperty property, HostInfo hostInfo, out IOrderedDrawable drawable)
        {
            var returnType = hostInfo.GetReturnType();
            if (returnType.InheritsFrom(typeof(IList)))
            {
                drawable = new DrawableList(property);
                return true;
            }

            // TODO: enable this once we support EnumToggleButtonsAttribute
            // if (returnType.IsEnum)
            // {
            //     drawable = new EnumDrawableField(new GenericMemberEntry(hostInfo.GetHost(), hostInfo.FieldInfo));
            //     return true;
            // }

            drawable = null;
            return false;
        }

        private static bool TryCreateDirect(GenericHostInfo hostInfo, out IOrderedDrawable drawableMember)
        {
            var type = hostInfo.GetReturnType();

            if (type == null)
            {
                drawableMember = null;
                return false;
            }

            if (type == typeof(string))
            {
                drawableMember = new StringDrawableField(hostInfo);
                return true;
            }

            if (type.IsEnum)
            {
                drawableMember = new EnumDrawableField(hostInfo);
                return true;
            }

            if (type == typeof(int))
            {
                drawableMember = new IntDrawableField(hostInfo);
                return true;
            }

            if (type == typeof(float))
            {
                drawableMember = new FloatDrawableField(hostInfo);
                return true;
            }

            if (type == typeof(bool))
            {
                drawableMember = new BoolDrawableField(hostInfo);
                return true;
            }
            
            if (type == typeof(Type))
            {
                drawableMember = new UndrawableField(hostInfo);
                return true;
            }

            if (type == typeof(LayerMask))
            {
                drawableMember = new LayerMaskDrawableField(hostInfo);
                return true;
            }

            if (type.InheritsFrom<IList>())
            {
                drawableMember = new DrawableList(hostInfo);
                return true;
            }

            if (type.InheritsFrom<Texture>())
            {
                drawableMember = new TextureDrawableField(hostInfo);
                return true;
            }

            if (type.InheritsFrom<UnityEngine.Object>())
            {
                drawableMember = new UnityObjectDrawableField(hostInfo);
                return true;
            }

            drawableMember = null;
            return false;
        }

        // =============================================================================================================
        // Searcher methods (fields, properties, buttons, methods, ...)
        private static ICollection<IOrderedDrawable> FindCustomDrawables(object instance)
        {
            if (instance == null) return Array.Empty<IOrderedDrawable>();
            
            var type = instance.GetType();

            var drawables = new List<IOrderedDrawable>();
            var buttonMethods = TypeCache.GetMethodsWithAttribute<ButtonAttribute>();
            var drawMethods = TypeCache.GetMethodsWithAttribute<OnInspectorGUIAttribute>();

            foreach (var mi in buttonMethods)
            {
                if (mi.DeclaringType != type) continue;
                var attributes = mi.GetCustomAttributes();
                var attr = attributes.OfType<ButtonAttribute>().First();
                
                IOrderedDrawable button = new DrawableButton(instance, mi)
                {
                    Name = attr.Name,
                    Height = attr.ButtonHeight
                };
                button = DrawableWrapperFactory.TryWrapDrawable(button, attributes);
                drawables.AddUnique(button);
            }
            
            foreach (var mi in drawMethods)
            {
                if (mi.DeclaringType != type) continue;
                var attributes = mi.GetCustomAttributes();
                
                IOrderedDrawable drawable = new DrawableMethod(instance, mi);
                drawable = DrawableWrapperFactory.TryWrapDrawable(drawable, attributes);
                drawables.AddUnique(drawable);
            }

            return drawables;
        }

        private static IReadOnlyCollection<GenericHostInfo> GetEditorVisibleFields(object instance, Type t, GenericHostInfo parent = null)
        {
            // All public members
            var publicFields = t.GetFields(BindingFlags.Instance | BindingFlags.Public |
                                             BindingFlags.GetField | BindingFlags.FlattenHierarchy);
            var publicProperties = t.GetProperties(BindingFlags.Instance | BindingFlags.Public |
                                           BindingFlags.GetProperty | BindingFlags.FlattenHierarchy);

            // All non-publics that serialize or are visible
            var serializedMembers = t.GetMembers(BindingFlags.Instance | BindingFlags.NonPublic |
                                                    BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.FlattenHierarchy);
            serializedMembers = serializedMembers
                .Where(x => !(x is MethodBase))
                .Where(x => x.IsSerialized() || x.GetCustomAttribute<ShowInInspectorAttribute>() != null)
                .ToArray();

            var list = new List<GenericHostInfo>();
            foreach (var member in publicFields)
                list.Add(new GenericHostInfo(parent, instance, member));
            foreach (var member in publicProperties)
                list.Add(new GenericHostInfo(parent, instance, member));
            foreach (var member in serializedMembers)
                list.Add(new GenericHostInfo(parent, instance, member));
            
            return list;
        }
        
        
        
        // =============================================================================================================
        // Generic Helper methods (TODO: move these to helper classes?)
        private static bool IsVisibleInEditor(this PropertyInfo propertyInfo)
        {
            if (propertyInfo.GetGetMethod(false) == null)
                return false;

            if (propertyInfo.GetCustomAttribute<SerializeField>() == null &&
                propertyInfo.GetCustomAttribute<ShowInInspectorAttribute>() == null)
                return false;
            return true;
        }

        private static IEnumerable<PropertyInfo> GetEditorVisibleProperties(this Type type)
        {
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .ToList();
            foreach (var propertyMember in properties)
            {
                if (!propertyMember.IsVisibleInEditor())
                    continue;
                yield return propertyMember;
            }
        }

        private static bool CanUnityHandleDrawingProperty(SerializedProperty property)
        {
            if (property == null)
                return false;
            switch (property.propertyType)
            {
                case SerializedPropertyType.ExposedReference:
                // case SerializedPropertyType.LayerMask: 
                // case SerializedPropertyType.Enum:// TODO: Reenable this once we support EnumToggleButtonsAttribute
                case SerializedPropertyType.ManagedReference:
                case SerializedPropertyType.ArraySize:
                //case SerializedPropertyType.ObjectReference:
                case SerializedPropertyType.Generic:
                    return false;
                default:
                    return true;
            }
        }
    }
}