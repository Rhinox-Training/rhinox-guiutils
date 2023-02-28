using System.Linq;
using Rhinox.GUIUtils.Editor;
using UnityEditor;

namespace GUIUtils.Editor.Windows.PagerWindow
{
    [CustomEditor(typeof(PagerPage), true)]
    public class PagerPageInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (targets == null)
                return;
            foreach (var t in targets.OfType<PagerPage>())
            {
                if (t == null)
                    continue;
                t.Draw();
            }
        }
    }
}