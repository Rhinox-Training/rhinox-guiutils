using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;

namespace Rhinox.GUIUtils.Editor
{
    public class CompositeDrawableMember : IOrderedDrawable
    {
        public int Order { get; set; }
        
        private readonly ICollection<IOrderedDrawable> _drawableMemberChildren;

        public CompositeDrawableMember(ICollection<IOrderedDrawable> subdrawables)
        {
            _drawableMemberChildren = subdrawables;
        }

        public void Draw()
        {
            if (_drawableMemberChildren == null)
                return;

            foreach (var childDrawable in _drawableMemberChildren)
            {
                if (childDrawable == null)
                    continue;
                childDrawable.Draw();
            }
        }

        public void Draw(Rect rect)
        {
            if (_drawableMemberChildren == null)
                return;

            foreach (var childDrawable in _drawableMemberChildren)
            {
                if (childDrawable == null)
                    continue;
                childDrawable.Draw(rect);
            }
        }
    }
}