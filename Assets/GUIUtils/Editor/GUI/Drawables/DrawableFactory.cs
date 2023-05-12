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
            return CreateDrawableForMember(hostInfo, 0);
        }

        public static IOrderedDrawable CreateDrawableFor(object instance)
            => CreateDrawableFor(new RootHostInfo(instance));
        
        public static IOrderedDrawable CreateDrawableFor(SerializedObject obj)
        {
            object instanceVal = obj.targetObject;
            var info = new RootHostInfo(instanceVal);

            var visibleFields = obj.EnumerateEditorVisibleFields();
            var drawable = DrawableMembersForSerializedObject(info, visibleFields, 0);
            return ObjectCompositeDrawableMember.CreateFrom(info, drawable);
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

        public static IOrderedDrawable CreateDrawableForParametersOf(ParameterInfo[] parameters, object[] parameterArray)
        {
            var group = new VerticalGroupDrawable();
            var rootHostInfo = new RootHostInfo(parameterArray);
            for (int i = 0; i < parameters.Length; i++)
            {
                var elementDrawable = CreateDrawableForParameter(parameters[i], i, rootHostInfo);
                group.Add(elementDrawable);
            }

            return group;
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

        private static VerticalGroupDrawable DrawableMembersForSerializedObject(GenericHostInfo hostInfo, IEnumerable<SerializedObjectExtensions.FieldData> visibleFields, int depth)
        {
            var drawable = new VerticalGroupDrawable();
            foreach (var fieldData in visibleFields)
            {
                IOrderedDrawable fieldDrawable = null;
                if (fieldData.OverrideDrawable != null)
                    fieldDrawable = fieldData.OverrideDrawable;
                else if (!fieldData.IsSerialized)
                {
                    var fieldHostInfo = new GenericHostInfo(hostInfo, fieldData.FieldInfo);
                    fieldDrawable = CreateDrawableForMember(fieldHostInfo, depth);
                }
                else
                    fieldDrawable = CreateDrawableForSerializedProperty(fieldData.SerializedProperty);

                if (fieldDrawable == null)
                    continue;
                
                drawable.Add(fieldDrawable);
            }

            foreach (var propertyMember in hostInfo.GetReturnType().GetEditorVisibleProperties())
            {
                var propHostInfo = new GenericHostInfo(hostInfo, propertyMember);
                var propertyDrawable = CreateDrawableForMember(propHostInfo, depth);
                if (propertyDrawable == null)
                    continue;

                drawable.Add(propertyDrawable);
            }

            var buttons = FindCustomDrawables(hostInfo);
            drawable.AddRange(buttons);

            drawable.Sort();

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

                // TODO still needed?
                if (subInstance == null)
                    return new TypePickerDrawable(hostInfo);
                
                resultingMember = CreateCompositeDrawable(hostInfo, depth + 1);
            }

            if (resultingMember == null)
                return null;

            // Check for decorators
            resultingMember = DrawableWrapperFactory.TryWrapDrawable(resultingMember, hostInfo.GetAttributes());
            return resultingMember;
        }
        
        private static IOrderedDrawable CreateDrawableForParameter(ParameterInfo pi, int index, GenericHostInfo arrayHostInfo)
        {
            var hostInfo = new ParameterHostInfo(arrayHostInfo, pi, index);
            if (TryCreateDirect(hostInfo, out IOrderedDrawable drawable))
                return drawable;
            return new UndrawableField(hostInfo);
        }

        private static IOrderedDrawable CreateCompositeDrawable(GenericHostInfo hostInfo, int depth)
        {
            var drawable = new VerticalGroupDrawable();

            var memberEntries = GetEditorVisibleFields(hostInfo);
            var drawables = new List<IOrderedDrawable>();
            foreach (var memberEntry in memberEntries)
            {
                if (memberEntry.MemberInfo is PropertyInfo propertyInfo && !propertyInfo.IsVisibleInEditor())
                    continue;

                IOrderedDrawable resultingMember = CreateDrawableForMember(memberEntry, depth);

                if (resultingMember != null)
                    drawables.Add(resultingMember);
            }

            drawable.AddRange(drawables);
            
            var buttons = FindCustomDrawables(hostInfo);
            drawable.AddRange(buttons);
            
            drawable.Sort();

            // Wrap in object composite
            var resultDrawable = ObjectCompositeDrawableMember.CreateFrom(hostInfo, drawable);
            return resultDrawable;
        }

        private static IOrderedDrawable CreateDrawableForSerializedProperty(SerializedProperty property)
        {
            if (property == null)
                return null;

            IOrderedDrawable drawable = null;
            IEnumerable<Attribute> attributes = null;
            if (CanUnityHandleDrawingProperty(property))
            {
                drawable = new UnityPropertyDrawable(property);
                attributes = property.GetAttributes();
            }
            else
            {
                var hostInfo = property.GetHostInfo();

                attributes = hostInfo.GetAttributes();
                // if (instanceVal == null)
                //     drawable = new NullReferenceDrawable(property);
                // else
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
                            var visibleFields = property.EnumerateEditorVisibleFields();
                            var verticalGroupDrawable = DrawableMembersForSerializedObject(hostInfo, visibleFields, 0);
                            
                            drawable = ObjectCompositeDrawableMember.CreateFrom(hostInfo, verticalGroupDrawable);
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
                drawable = new ListDrawable(property);
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
                drawableMember = new UndrawableField<Type>(hostInfo);
                return true;
            }

            var drawerType = PropertyDrawerHelper.GetDrawerTypeFor(type);
            if (drawerType != null && drawerType.HasInterfaceType<IHostInfoDrawer>())
            {
                drawableMember = new DrawableAsUnityProperty(hostInfo, drawerType);
                return true;
            }

            if (type == typeof(LayerMask))
            {
                drawableMember = new LayerMaskDrawableField(hostInfo);
                return true;
            }

            if (type.InheritsFrom<IList>())
            {
                drawableMember = new ListDrawable(hostInfo);
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
        
        private static ICollection<IOrderedDrawable> FindCustomDrawables(GenericHostInfo info)
        {
            if (info == null) return Array.Empty<IOrderedDrawable>();
            
            var type = info.GetReturnType();

            var drawables = new List<IOrderedDrawable>();
            var buttonMethods = TypeCache.GetMethodsWithAttribute<ButtonAttribute>();
            var drawMethods = TypeCache.GetMethodsWithAttribute<OnInspectorGUIAttribute>();

            for (var i = 0; i < buttonMethods.Count; i++)
            {
                var mi = buttonMethods[i];
                if (!ReflectionUtility.IsMethodOfType(type, ref mi))
                    continue;

                // TypeCache only marks certain methods as using this attribute, now actually fetch it
                var attributes = mi.GetCustomAttributes();
                var attr = attributes.OfType<ButtonAttribute>().First();

                IOrderedDrawable button = new DrawableButton(info, mi)
                {
                    Name = attr.Name,
                    Height = attr.ButtonHeight
                };
                button = DrawableWrapperFactory.TryWrapDrawable(button, attributes);
                drawables.AddUnique(button);
            }

            for (var i = 0; i < drawMethods.Count; i++)
            {
                var mi = drawMethods[i];
                if (!ReflectionUtility.IsMethodOfType(type, ref mi))
                    continue;
                var attributes = mi.GetCustomAttributes();

                IOrderedDrawable drawable = new DrawableMethod(info, mi);
                drawable = DrawableWrapperFactory.TryWrapDrawable(drawable, attributes);
                drawables.AddUnique(drawable);
            }

            return drawables;
        }

        private static IReadOnlyCollection<GenericHostInfo> GetEditorVisibleFields(GenericHostInfo parent)
        {
            var t = parent.GetReturnType();
            
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

            var instance = parent.GetValue();

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

            var type = property.GetHostInfo().GetReturnType();
            var drawerType = PropertyDrawerHelper.GetDrawerTypeFor(type);
            if (drawerType != null)
                return true;
            
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