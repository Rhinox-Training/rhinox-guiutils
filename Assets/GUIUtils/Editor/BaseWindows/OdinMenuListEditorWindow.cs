using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

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

	public abstract class MenuListEditorWindow : CustomMenuEditorWindow
	{
		protected override IEnumerable<object> GetTargets()
		{
			if (MenuTree != null)
			{
				if (MenuTree.SelectionCount > 1)
				{
					var list = MenuTree.Selection
						.Select(x => x?.GetInstanceValue())
						.Where(x => x != null)
						.Select(x => TransformTarget(x))
						.Where(x => x != null)
						.ToList();
					yield return new ListDrawer(list);
					yield break;
				}

				for (int i = 0; i < MenuTree.SelectionCount; ++i)
				{
					var menuItem = MenuTree.Selection[i];
					if (menuItem == null)
						continue;

					var o = menuItem.GetInstanceValue();
					if (o == null)
						continue;
					o = TransformTarget(o);
					if (o != null)
						yield return o;
				}
			}
			else
			{
				Debug.LogError("MenuTree is null?");
			}
		}

		protected virtual object TransformTarget(object item)
		{
			return item;
		}
	}
}
