using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;
using Object = System.Object;

namespace Rhinox.GUIUtils.Editor
{
	[Serializable]
	public class ListDrawer
	{
		public ListDrawer(List<object> l)
		{
			List = l;
		}

		[ShowInInspector, HideLabel, HideReferenceObjectPicker, ListDrawerSettings(Expanded = true, IsReadOnly = true)]
		public List<object> List;
	}

	public abstract class OdinMenuListEditorWindow : OdinMenuEditorWindow
	{
		protected override IEnumerable<object> GetTargets()
		{
			if (MenuTree != null)
			{
				if (MenuTree.Selection.Count > 1)
				{
					yield return new ListDrawer(MenuTree.Selection.Select(GetObject).Where(x => x != null).ToList());
					yield break;
				}

				for (int i = 0; i < MenuTree.Selection.Count; ++i)
				{
					OdinMenuItem odinMenuItem = MenuTree.Selection[i];
					var o = GetObject(odinMenuItem);
					if (o != null)
						yield return o;
				}
			}
			else
			{
				throw new Exception("MenuTree is null?");
			}
		}

		protected virtual object GetObject(OdinMenuItem item)
		{
			if (item == null)
				return null;
			object obj = item.Value;
			Func<object> func = obj as Func<object>;
			if (func != null)
				obj = func();

			return obj;
		}
	}
}
