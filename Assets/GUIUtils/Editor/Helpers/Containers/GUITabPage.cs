using Rhinox.GUIUtils;
using Rhinox.GUIUtils.Editor;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
  /// <summary>
  /// A tab page created by <see cref="T:Sirenix.Utilities.Editor.GUITabGroup" />.
  /// </summary>
  /// <seealso cref="T:Sirenix.Utilities.Editor.GUITabGroup" />
  public class GUITabPage
  {
    private static GUIStyle innerContainerStyle;
    internal int Order;
    private GUITabGroup tabGroup;
    private Color prevColor;
    private static int pageIndexIncrementer;
    private bool isSeen;
    private bool isMessured;

    private static GUIStyle InnerContainerStyle
    {
      get
      {
        if (GUITabPage.innerContainerStyle == null)
          GUITabPage.innerContainerStyle = new GUIStyle()
          {
            padding = new RectOffset(3, 3, 3, 3)
          };
        return GUITabPage.innerContainerStyle;
      }
    }

    /// <summary>Gets the title of the tab.</summary>
    internal string Name { get; private set; }

    /// <summary>Gets the title of the tab.</summary>
    public string Title { get; set; }

    /// <summary>Gets the rect of the page.</summary>
    public Rect Rect { get; private set; }

    internal bool IsActive { get; set; }

    internal bool IsVisible { get; set; }

    internal GUITabPage(GUITabGroup tabGroup, string title)
    {
      this.Name = title;
      this.Title = title;
      this.tabGroup = tabGroup;
      this.IsActive = true;
    }

    internal void OnBeginGroup()
    {
      GUITabPage.pageIndexIncrementer = 0;
      this.isSeen = false;
    }

    internal void OnEndGroup()
    {
      if (Event.current.type != UnityEngine.EventType.Repaint)
        return;
      this.IsActive = this.isSeen;
    }

    /// <summary>Begins the page.</summary>
    public bool BeginPage()
    {
      if (this.tabGroup.FixedHeight && !this.isMessured)
        this.IsVisible = true;
      this.isSeen = true;
      if (this.IsVisible)
      {
        Rect rect = EditorGUILayout.BeginVertical(GUITabPage.InnerContainerStyle, GUILayout.Width(this.tabGroup.InnerContainerWidth), 
          GUILayout.ExpandHeight(this.tabGroup.ExpandHeight));
        GUIContentHelper.PushHierarchyMode(false);
        GUIContentHelper.PushLabelWidth(this.tabGroup.LabelWidth - 4f);
        if (Event.current.type == UnityEngine.EventType.Repaint)
          this.Rect = rect;
        if (this.tabGroup.IsAnimating)
        {
          this.prevColor = GUI.color;
          Color prevColor = this.prevColor;
          prevColor.a *= this.tabGroup.CurrentPage == this ? this.tabGroup.T : 1f - this.tabGroup.T;
          GUI.color = prevColor;
        }
      }
      return this.IsVisible;
    }

    /// <summary>Ends the page.</summary>
    public void EndPage()
    {
      if (this.IsVisible)
      {
        GUIContentHelper.PopLabelWidth();
        GUIContentHelper.PopHierarchyMode();
        if (this.tabGroup.IsAnimating)
          GUI.color = this.prevColor;
        EditorGUILayout.EndVertical();
      }
      if (Event.current.type != UnityEngine.EventType.Repaint)
        return;
      this.isMessured = true;
      this.Order = GUITabPage.pageIndexIncrementer++;
    }
  }
}
