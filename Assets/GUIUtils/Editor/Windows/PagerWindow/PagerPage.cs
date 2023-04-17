using Rhinox.GUIUtils.Editor;
using Rhinox.GUIUtils.Editor.Helpers;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public abstract class PagerTreePage : PagerPage
    {
        protected Rhinox.GUIUtils.Editor.EditorWrapper _targetWrapper;

        protected PagerTreePage(SlidePagedWindowNavigationHelper<object> pager) : base(pager)
        {
        }

        protected PagerTreePage(SlidePagedWindowNavigationHelper<object> pager, object target) : base(pager)
        {
            SetTarget(target);
        }

        protected override void OnDraw()
        {
            base.OnDraw();

            _targetWrapper.Draw();
        }

        protected bool TryGetTypedTarget<T>(out T target)
        {
            if (_targetWrapper.Target is T typedTarget)
            {
                target = typedTarget;
                return true;
            }

            target = default;
            return false;
        }

        public void SetTarget(object target)
        {
            if (target == this)
            {
                Debug.LogError("Cannot use SetTarget on itself as it would cause recurring drawing...");
                return;
            }

            _targetWrapper = new EditorWrapper(target);
        }
    }

    public abstract class PagerPage
    {
        protected SlidePagedWindowNavigationHelper<object> _pager;

        protected int _topWidth;
        protected int _topHeight = 18;

        private Vector2 _scrollPos;
        private Rect _rect;

        protected bool _changed;

        protected PagerPage(SlidePagedWindowNavigationHelper<object> pager)
        {
            _pager = pager;
        }


        public virtual void Update()
        {

        }

        // for after deserialization, etc
        public void SetPager(SlidePagedWindowNavigationHelper<object> pager)
        {
            _pager = pager;
        }

        protected virtual int CalculateTopWidth()
        {
            return 0;
        }

        [Sirenix.OdinInspector.OnInspectorGUI]
        public void Draw()
        {
            GUILayout.BeginVertical();

            OnDrawTop();

            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUIStyle.none);

            OnDraw();

            GUILayout.EndScrollView();

            OnDrawBottom();
            GUILayout.EndVertical();
        }

        protected virtual void OnDrawTop()
        {
        }

        protected virtual void OnDrawBottom()
        {
        }

        protected virtual void OnDraw()
        {
        }

        public virtual void DrawTopOverlay()
        {
            _topWidth = CalculateTopWidth();
            if (_topWidth <= 0)
                return;
            
            var rect = CustomEditorGUI.GetTopLevelLayoutRect();
            if (rect.IsValid())
                _rect = rect;
            rect = _rect.AlignTop(_topHeight).AlignRight(_topWidth);
            rect.x -= 10;
            rect.y += 5;

            GUILayout.BeginArea(rect);
            GUILayout.BeginHorizontal();

            OnDrawTopOverlay();

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        protected virtual void OnDrawTopOverlay()
        {
        }

        public virtual void MarkAsChanged()
        {
            _changed = true;
        }

        protected virtual void ResolveChange()
        {
            _changed = false;
        }

        protected virtual void NavigateBack()
        {
            EditorApplication.delayCall += _pager.NavigateBack;
        }
    }
}