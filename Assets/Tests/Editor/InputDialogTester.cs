using System.Collections.Generic;
using Sirenix.OdinInspector;
using Rhinox.GUIUtils.Editor;
using UnityEditor;
using UnityEngine;

namespace Tests
{
    public static class InputDialogTester
    {
#if UNITY_EDITOR
        [MenuItem("Rhinox/GUIUtils/Test Dialog")]
        public static void TryDialog()
        {
            EditorInputDialog.Create("Question?", "Real Question?")
                .BooleanField("BoolTest", out var boolValue)
                .FloatField("FloatText", out var floatValue)
                .TextField("TextField", out var textValue)
                .TransformField("TransformField", out var transformValue)
                .GameObjectField("GameobjectField", out var gameObjectValue)
                .MaterialField("MaterialField", out var materialValue)
                .TextureField("TextureField", out var textureValue)
                .Dropdown("Options", new List<ValueDropdownItem>() { new ValueDropdownItem("foo", "foo"), new ValueDropdownItem("bar", "bar")}, out var option)
                .OnAccept(() =>
                {
                    Debug.Log(boolValue.Value);
                    Debug.Log(floatValue.Value);
                    Debug.Log(textValue.Value);
                    Debug.Log(transformValue.Value);
                    Debug.Log(gameObjectValue.Value);
                    Debug.Log(materialValue.Value);
                    Debug.Log(textureValue.Value);
                    Debug.Log(option.Value);
                })
                .Show();
        }
#endif
    }
}