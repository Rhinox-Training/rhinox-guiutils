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
using UnityEngineInternal;

namespace Rhinox.GUIUtils.Editor
{
    public static class DrawableFactory
    {
        private const int MAX_DEPTH = 10;
        
        //==============================================================================================================
        // Public API
        public static List<IOrderedDrawable> CreateDrawableMembersFor(GenericMemberEntry entry)
            => CreateDrawableMembersFor(entry.GetValue(), entry.GetReturnType(), entry);
        
        public static List<IOrderedDrawable> CreateDrawableMembersFor(object instance, Type type, GenericMemberEntry parent = null)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            
            if (type.InheritsFrom<UnityEngine.Object>())
                return new List<IOrderedDrawable>() {new DrawableUnityObject((UnityEngine.Object) instance)};
            return CreateDrawableMembersFor(instance, type, 0, parent);
        }
        
        public static List<IOrderedDrawable> CreateDrawableMembersFor(SerializedObject obj, Type type)
        {
            object instanceVal = obj.targetObject;
            
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            
            if (type.InheritsFrom<UnityEngine.Object>())
                return new List<IOrderedDrawable>() {new DrawableUnityObject((UnityEngine.Object)instanceVal)};
            
            var visibleFields = obj.EnumerateEditorVisibleFields();
            return DrawableMembersFor(instanceVal, type, visibleFields, 0);
        }
        
        public static List<IOrderedDrawable> CreateDrawableMembersFor(SerializedProperty property, Type type)
        {
            return CreateDrawableMembersFor(property, type, 0);
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

        private static List<IOrderedDrawable> CreateDrawableMembersFor(SerializedProperty property, Type type, int depth)
        {
            if (property == null)
                return null;
            
            object instanceVal = property.GetValue();
            
            if (type.InheritsFrom<UnityEngine.Object>())
                return new List<IOrderedDrawable>() { new DrawableUnityProperty(property, property.FindFieldInfo()) };

            if (instanceVal == null)
                return new List<IOrderedDrawable>() { new NullReferenceDrawable(property) };

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
                    var stack = new GenericMemberEntry(instanceVal, fieldData.FieldInfo);
                    if (TryCreate(stack, out var drawableMember) || depth >= MAX_DEPTH)
                        fieldDrawable = drawableMember;
                    else
                        fieldDrawable = CreateCompositeMemberForInstance(stack, depth);
                }
                else
                {
                    fieldDrawable = CreateDrawableForProperty(fieldData.FieldInfo, fieldData.SerializedProperty);
                    AttributeParser.Parse(fieldData.FieldInfo, ref fieldDrawable);
                }

                if (fieldDrawable == null)
                    continue;
                
                // Check for decorators
                fieldDrawable = DrawableWrapperFactory.TryWrapDrawable(fieldDrawable, fieldData.FieldInfo.GetCustomAttributes());

                drawables.Add(fieldDrawable);
            }

            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .ToList();
            foreach (var propertyMember in properties)
            {
                var showInInspector = propertyMember.GetCustomAttribute<ShowInInspectorAttribute>();
                if (showInInspector == null) continue;
                
                var entry = new GenericMemberEntry(instanceVal, propertyMember);
                var propertyDrawable = CreateCompositeMemberForInstance(entry, depth);
                if (propertyDrawable == null)
                    continue;
                    
                // Check for decorators
                propertyDrawable = DrawableWrapperFactory.TryWrapDrawable(propertyDrawable, propertyMember.GetCustomAttributes());

                drawables.Add(propertyDrawable);
            }

            var buttons = FindButtons(instanceVal);
            drawables.AddRange(buttons);

            return drawables;
        }

        private static List<IOrderedDrawable> CreateDrawableMembersFor(object instance, Type t, int depth, GenericMemberEntry upperEntry = null)
        {
            var entries = GetEditorVisibleFields(instance, t, upperEntry);
            var drawableMembers = new List<IOrderedDrawable>();
            foreach (var entry in entries)
            {
                if (entry.Info is PropertyInfo propertyInfo)
                {
                    // If the property has no getter
                    if (propertyInfo.GetGetMethod(false) == null)
                        continue;

                    if (propertyInfo.GetCustomAttribute<SerializeField>() == null &&
                        propertyInfo.GetCustomAttribute<ShowInInspectorAttribute>() == null)
                        continue;
                }

                IOrderedDrawable resultingMember = null;
                if (TryCreate(entry, out var drawableMember) || depth >= MAX_DEPTH)
                    resultingMember = drawableMember;
                else
                    resultingMember = CreateCompositeMemberForInstance(entry, depth);

                if (resultingMember == null)
                    continue;

                // Check for decorators
                resultingMember = DrawableWrapperFactory.TryWrapDrawable(resultingMember, entry.GetAttributes());

                drawableMembers.Add(resultingMember);
            }
            
            
            var buttons = FindButtons(instance);
            drawableMembers.AddRange(buttons);


            return drawableMembers;
        }
        
        private static IOrderedDrawable CreateDrawableForProperty(FieldInfo field, SerializedProperty prop)
        {
            IOrderedDrawable drawable;
            if (field.FieldType.InheritsFrom(typeof(IList)))
                drawable = new DrawableList(prop);
            else
                drawable = new DrawableUnityProperty(prop, field);
            var propOrder = field.GetCustomAttribute<PropertyOrderAttribute>();
            if (propOrder != null)
                drawable.Order = propOrder.Order;
            
            return drawable;
        }

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
            var publicMembers = t.GetMembers(BindingFlags.Instance | BindingFlags.Public |
                                                BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.FlattenHierarchy);
            publicMembers = publicMembers
                .Where(x => !(x is MethodBase))
                .ToArray();
            
            // All non-publics that serialize or are visible
            var serializedMembers = t.GetMembers(BindingFlags.Instance | BindingFlags.NonPublic |
                                                    BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.FlattenHierarchy);
            serializedMembers = serializedMembers
                .Where(x => !(x is MethodBase))
                .Where(x => x.IsSerialized() || x.GetCustomAttribute<ShowInInspectorAttribute>() != null)
                .ToArray();

            var list = new List<GenericMemberEntry>();
            foreach (var member in publicMembers)
                list.Add(new GenericMemberEntry(instance, member, parent));
            foreach (var member in serializedMembers)
                list.Add(new GenericMemberEntry(instance, member, parent));
            
            return list;
        }

        private static IOrderedDrawable CreateCompositeMemberForInstance(GenericMemberEntry entry, int depth)
        {
            IOrderedDrawable resultingMember;
            try
            {
                var subInstance = entry.GetValue();
                var subtype = entry.GetReturnType();
                
                if (subInstance != null)
                    subtype = subInstance.GetType();

                var subdrawables = CreateDrawableMembersFor(subInstance, subtype, depth + 1, entry);
                var composite = new ObjectCompositeDrawableMember(entry.Info.Name);
                var attributes = entry.GetAttributes();

                foreach (var attr in attributes)
                {
                    if (attr is PropertyGroupAttribute groupAttribute)
                        composite.Order = groupAttribute.Order;
                    composite.AddAttribute(attr);
                }

                composite.AddRange(subdrawables);
                resultingMember = composite;

                resultingMember = DrawableWrapperFactory.TryWrapDrawable(resultingMember, attributes);
            }
            catch (Exception /*e*/) // TODO What are we trying to silence here? Because it swallowing everything is annoying
            {
                resultingMember = null;
            }

            return resultingMember;
        }

        private static bool TryCreate(GenericMemberEntry entry, out IOrderedDrawable drawableMember)
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
    }
}