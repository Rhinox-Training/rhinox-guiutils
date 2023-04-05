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
        public static IOrderedDrawable CreateDrawableFor(GenericMemberEntry entry)
        {
            return CreateDrawableFor(entry.GetValue(), entry.GetReturnType(), entry);
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
            
            if (type == typeof(UnityEngine.Object))
                return new DrawableUnityObject((UnityEngine.Object)instanceVal);
            
            var visibleFields = obj.EnumerateEditorVisibleFields();
            return DrawableMembersForSerializedObject(instanceVal, type, visibleFields, 0);
        }
        
        public static IOrderedDrawable CreateDrawableFor(SerializedProperty property, Type type)
        {
            if (property == null)
                return null;
            
            object instanceVal = property.GetValue();

            if (CanUnityHandleDrawingProperty(property))
            {
                IOrderedDrawable drawable = new DrawableUnityProperty(property, property.FindFieldInfo());
                drawable = DrawableWrapperFactory.TryWrapDrawable(drawable, property.GetAttributes().Append(new HideLabelAttribute()));
                return drawable;
            }

            if (instanceVal == null)
                return new NullReferenceDrawable(property);

            var visibleFields = property.EnumerateEditorVisibleFields();
            return DrawableMembersForSerializedObject(instanceVal, type, visibleFields, 0);
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

        private static IOrderedDrawable CreateDrawableFor(object instance, Type type, GenericMemberEntry parent)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            
            if (parent == null && type.InheritsFrom<UnityEngine.Object>())
                return new DrawableUnityObject((UnityEngine.Object) instance);

            if (parent != null)
                return CreateDrawableForMember(parent, 0);
            
            return CreateCompositeDrawable(instance, type, 0, parent);
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
                    var stack = new GenericMemberEntry(instanceVal, fieldData.FieldInfo);
                    fieldDrawable = CreateDrawableForMember(stack, depth);
                }
                else
                    fieldDrawable = CreateDrawableForSerializedProperty(fieldData.FieldInfo, fieldData.SerializedProperty);

                if (fieldDrawable == null)
                    continue;
                
                drawable.Add(fieldDrawable);
            }

            foreach (var propertyMember in type.GetEditorVisibleProperties())
            {
                var entry = new GenericMemberEntry(instanceVal, propertyMember);
                var propertyDrawable = CreateDrawableForMember(entry, depth);
                if (propertyDrawable == null)
                    continue;

                drawable.Add(propertyDrawable);
            }

            var buttons = FindButtons(instanceVal);
            drawable.AddRange(buttons);

            return drawable;
        }

        private static IOrderedDrawable CreateDrawableForMember(GenericMemberEntry entry, int depth)
        {
            IOrderedDrawable resultingMember;
            if (TryCreateDirect(entry, out var drawableMember) || depth >= MAX_DEPTH)
                resultingMember = drawableMember;
            else
            {
                var subInstance = entry.GetValue();
                var subtype = entry.GetReturnType();
                
                if (subInstance != null)
                    subtype = subInstance.GetType();

                resultingMember = CreateCompositeDrawable(subInstance, subtype, depth + 1, entry);
            }

            if (resultingMember == null)
                return null;

            // Check for decorators
            resultingMember = DrawableWrapperFactory.TryWrapDrawable(resultingMember, entry.GetAttributes());
            return resultingMember;
        }

        private static IOrderedDrawable CreateCompositeDrawable(object instance, Type t, int depth, GenericMemberEntry upperEntry = null)
        {
            CompositeDrawableMember drawable = new VerticalGroupDrawable();

            var entries = GetEditorVisibleFields(instance, t, upperEntry);
            var drawables = new List<IOrderedDrawable>();
            foreach (var entry in entries)
            {
                if (entry.Info is PropertyInfo propertyInfo && !propertyInfo.IsVisibleInEditor())
                    continue;

                var resultingMember = CreateDrawableForMember(entry, depth);

                if (resultingMember != null)
                    drawables.Add(resultingMember);
            }

            drawable.AddRange(drawables);
            drawable.Sort();
            
            var buttons = FindButtons(instance);
            drawable.AddRange(buttons);

            if (upperEntry != null)
            {
                drawable = new ObjectCompositeDrawableMember(upperEntry, drawable);
                foreach (var attr in upperEntry.GetAttributes())
                    drawable.AddAttribute(attr);
            }
            else
                drawable = new ObjectCompositeDrawableMember(string.Empty, drawable);
            


            return drawable;
        }

        private static IOrderedDrawable CreateDrawableForSerializedProperty(FieldInfo field, SerializedProperty prop)
        {
            IOrderedDrawable drawable;
            if (field.FieldType.InheritsFrom(typeof(IList)))
                drawable = new DrawableList(prop);
            else
                drawable = new DrawableUnityProperty(prop, field);
            var propOrder = field.GetCustomAttribute<PropertyOrderAttribute>();
            if (propOrder != null)
                drawable.Order = propOrder.Order;
            
            
            AttributeParser.Parse(field, ref drawable);
            // Check for decorators
            drawable = DrawableWrapperFactory.TryWrapDrawable(drawable, field.GetCustomAttributes());
            
            return drawable;
        }
        
        private static bool TryCreateDirect(GenericMemberEntry entry, out IOrderedDrawable drawableMember)
        {
            var type = entry.GetReturnType();

            if (type == null)
            {
                drawableMember = null;
                return false;
            }

            if (type == typeof(string))
            {
                drawableMember = new StringDrawableField(entry);
                return true;
            }

            if (type.IsEnum)
            {
                drawableMember = new EnumDrawableField(entry);
                return true;
            }

            if (type == typeof(int))
            {
                drawableMember = new IntDrawableField(entry);
                return true;
            }

            if (type == typeof(float))
            {
                drawableMember = new FloatDrawableField(entry);
                return true;
            }

            if (type == typeof(bool))
            {
                drawableMember = new BoolDrawableField(entry);
                return true;
            }
            
            if (type == typeof(Type))
            {
                drawableMember = new UndrawableField<Type>(entry);
                return true;
            }

            if (type.InheritsFrom<IList>())
            {
                drawableMember = new DrawableList(entry);
                return true;
            }

            if (type.InheritsFrom<Texture>())
            {
                drawableMember = new TextureDrawableField(entry);
                return true;
            }

            if (type.InheritsFrom<UnityEngine.Object>())
            {
                drawableMember = new UnityObjectDrawableField(entry);
                return true;
            }

            drawableMember = null;
            return false;
        }

        // =============================================================================================================
        // Searcher methods (fields, properties, buttons, methods, ...)
        
        private static ICollection<IOrderedDrawable> FindButtons(object instance)
        {
            if (instance == null) return Array.Empty<IOrderedDrawable>();
            
            var type = instance.GetType();

            var buttons = new List<IOrderedDrawable>();
            var types = TypeCache.GetMethodsWithAttribute<ButtonAttribute>();
            foreach (var mi in types)
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
                buttons.AddUnique(button);
            }

            return buttons;
        }

        private static IReadOnlyCollection<GenericMemberEntry> GetEditorVisibleFields(object instance, Type t, GenericMemberEntry parent = null)
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

            var list = new List<GenericMemberEntry>();
            foreach (var member in publicFields)
                list.Add(new GenericMemberEntry(instance, member, parent));
            foreach (var member in publicProperties)
                list.Add(new GenericMemberEntry(instance, member, parent));
            foreach (var member in serializedMembers)
                list.Add(new GenericMemberEntry(instance, member, parent));
            
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
                case SerializedPropertyType.LayerMask:
                case SerializedPropertyType.Enum:
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