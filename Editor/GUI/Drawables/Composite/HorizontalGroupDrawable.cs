using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

namespace Rhinox.GUIUtils.Editor
{
    public class HorizontalGroupDrawable : BaseHorizontalGroupDrawable<HorizontalGroupAttribute>
    {
        public HorizontalGroupDrawable(GroupedDrawable parent, string groupID, int order)
            : base(parent, groupID, order)
        {
        }
        
        protected override void ParseAttributeSmart(IOrderedDrawable child, HorizontalGroupAttribute attr)
        {
            var info = new SizeInfo
            {
                PreferredSize = attr.Width,
                MaxSize = attr.MaxWidth,
                MinSize = attr.MinWidth
            };

            EnsureSizeFits(info);

            _sizeInfoByDrawable.Add(child, info);
        }
        
        protected override void ParseAttributeSmart(HorizontalGroupAttribute attr)
        {
            SetOrder(attr.Order);

            _size.MinSize = _groupAttributes.Sum(x => x.Width > 0 ? x.Width : x.MinWidth);
            if (_groupAttributes.All(x => x.Width > 0))
                _size.PreferredSize = _groupAttributes.Sum(x => x.Width);
            else
                _size.PreferredSize = 0;
            if (_groupAttributes.All(x => x.Width > 0 || x.MaxWidth > 0))
                _size.MaxSize = _groupAttributes.Sum(x => x.Width > 0 ? x.Width : x.MaxWidth);
            else
                _size.MaxSize = 0;

            _parent?.EnsureSizeFits(_size);
        }
    }
}