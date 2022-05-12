using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Rhinox.GUIUtils.UnitTesting
{
   


    public static class StyleTests
    {
        
        
        private static PropertyInfo[] _styleFields
        {
            get
            {
                return typeof(CustomGUIStyles)
                    .GetProperties(BindingFlags.Public | BindingFlags.Static)
                    .Where(x => x.PropertyType == typeof(GUIStyle)).ToArray();
            }
        }

        [UnityTest]
        public static IEnumerator CustomStyle_GetStyle([ValueSource(nameof(_styleFields))] PropertyInfo value)
        {
            
            // var result = value.GetMethod.Invoke(null, null);
            // Assert.IsNotNull(result);
            // yield return null;
            yield return new MonoBehaviourTest<OnGUITestWrapper>(false)
            {
                component = {Property = value}
            };
        }
    }
}
