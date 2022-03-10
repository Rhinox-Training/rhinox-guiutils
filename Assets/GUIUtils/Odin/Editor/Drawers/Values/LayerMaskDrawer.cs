// ===============================
// AUTHOR     :	      John Leonard
// CREATE DATE     :    25.04.2020
// NOTE     :  Always include this 
//	  Header when copying the file
// ===============================
// Licence: MIT (https://opensource.org/licenses/MIT)
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
// BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ===============================

using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Odin.Editor
{
	public class LayerMaskDrawer : OdinValueDrawer<LayerMask>
	{
		Rect buttonRect;
		List<string> layerNames = new List<string>();
		List<int> layerIndexes = new List<int>();
		List<bool> selectedLayers = new List<bool>();
		string buttonText = null;

		protected override void DrawPropertyLayout(GUIContent label)
		{
			GetAllLayerNames(layerNames, layerIndexes);

			EditorGUILayout.BeginHorizontal();

			if (label != null)
				EditorGUILayout.PrefixLabel(label);

			// get a list of selected layers with bitwise operations
			selectedLayers.Clear();
			int layerBitVal = ValueEntry.SmartValue;
			for (int i = 0; i < layerNames.Count; i++)
			{
				int bitVal = (int) Mathf.Pow(2, layerIndexes[i]);
				bool isSelected = (layerBitVal & bitVal) == bitVal;
				selectedLayers.Add(isSelected);
			}


			if (string.IsNullOrEmpty(buttonText))
			{
				bool all = true;
				bool none = true;
				buttonText = "";
				for (int i = 0; i < layerNames.Count; i++)
				{
					if (selectedLayers[i])
					{
						none = false;
						buttonText += layerNames[i] + ", ";
					}
					else
					{
						all = false;
					}
				}

				if (none) buttonText = "Nothing";
				else if (all) buttonText = "Everything";
				else if (buttonText.Length > 1) buttonText = buttonText.Remove(buttonText.Length - 2);
			}

			if (GUILayout.Button(buttonText, SirenixGUIStyles.DropDownMiniButton))
			{
				buttonText = null;
				PopupWindow.Show(buttonRect, new LayerMaskPopupSelector()
				{
					layerNames = layerNames,
					layerIndexes = layerIndexes,
					selectedLayers = selectedLayers,
					bitmask = ValueEntry.SmartValue,
					OnSet = (bitmask) =>
					{
						ValueEntry.SmartValue = bitmask;
						buttonText = null;
					}
				});
			}

			// get the rectangle for the popup window
			if (Event.current.type == EventType.Repaint)
				buttonRect = GUILayoutUtility.GetLastRect();

			EditorGUILayout.EndHorizontal();
		}


		public class LayerMaskPopupSelector : PopupWindowContent
		{
			public List<string> layerNames = new List<string>();
			public List<int> layerIndexes = new List<int>();
			public List<bool> selectedLayers = new List<bool>();
			public Action<int> OnSet;
			public int bitmask;

			public override Vector2 GetWindowSize()
			{
				// every button is 30 but the label is only 22 so we substract 8
				float heightOfOne = 30;
				float height = heightOfOne * (2 + layerNames.Count) - 8;

				return new Vector2(200, height);
			}

			public override void OnGUI(Rect rect)
			{
				GUILayout.Label("Layers", EditorStyles.boldLabel);

				// check if all or none are selected
				bool all = true;
				bool none = true;
				foreach (bool b in selectedLayers)
				{
					if (b)
						none = false;
					else
						all = false;
					if (!all && !none) break;
				}

				// none button
				Texture2D noneIcon = none ? EditorIcons.TestPassed : EditorIcons.TestNormal;
				if (SirenixEditorGUI.MenuButton(0, " None", false, noneIcon))
				{
					SetBitmask(-1, true);
					OnSet(bitmask);
					for (int i = 0; i < selectedLayers.Count; i++)
					{
						selectedLayers[i] = false;
					}
				}

				// layer buttons
				for (int i = 0; i < layerNames.Count; i++)
				{
					Texture2D currentIcon =
						selectedLayers[i] ? EditorIcons.TestPassed : EditorIcons.TestNormal;
					if (SirenixEditorGUI.MenuButton(0, " " + layerNames[i],
						false, currentIcon))
					{
						selectedLayers[i] = !selectedLayers[i];
						SetBitmask(layerIndexes[i], selectedLayers[i]);
						OnSet(bitmask);
					}
				}

				// all button
				Texture2D allIcon = all ? EditorIcons.TestPassed : EditorIcons.TestNormal;
				if (SirenixEditorGUI.MenuButton(0, " All", false, allIcon))
				{
					SetBitmask(-2, true);
					OnSet(bitmask);
					for (int i = 0; i < selectedLayers.Count; i++)
					{
						selectedLayers[i] = true;
					}
				}

				OnSet(bitmask);

				// faster update of this window, otherwise the highlight of buttons lags
				editorWindow.Repaint();
			}

			void SetBitmask(int index, bool set)
			{
				// -1 = none
				if (index == -1)
				{
					bitmask = 0;
					return;
				}

				// -2 = all
				if (index == -2)
				{
					bitmask = ~0;
					return;
				}

				int bitVal = (int) Mathf.Pow(2, index);
				if (set)
					// or "|" will add the value, the 1 at the right position
					bitmask |= bitVal;
				else
					// and "&" will multiply the value, but we take the inverse
					// so the bit position is 0 while all others are 1
					// everything stays as it is, except for the one value
					// which will be set to 0
					bitmask &= ~bitVal;
			}

			public override void OnOpen()
			{
			}

			public override void OnClose()
			{
				OnSet(bitmask);
			}
		}

		// simple function to return the names of all layers and their corresponding index
		List<string> GetAllLayerNames(List<string> layerNames, List<int> layerIndexes)
		{
			layerNames.Clear();
			layerIndexes.Clear();
			//user defined layers start with layer 8 and unity
			//supports 31 layers
			for (int i = 0; i <= 31; i++)
			{
				//get the name of the layer
				var layerN = LayerMask.LayerToName(i);

				//only add the layer if it has been named
				if (layerN.Length > 0)
				{
					layerIndexes.Add(i);
					layerNames.Add(layerN);
				}
			}

			return layerNames;
		}

	}
}