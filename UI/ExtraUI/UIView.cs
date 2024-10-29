using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace LargerInventory.UI.ExtraUI;

public class UIView : UIElement, IEnumerable<UIElement>, IEnumerable
{
    public delegate bool ElementSearchMethod(UIElement element);
    private class UIInnerList : UIElement
    {
        public override bool ContainsPoint(Vector2 point) => true;

        protected override void DrawChildren(SpriteBatch spriteBatch)
        {
            lock (Elements)
            {
                Vector2 position = Parent.GetDimensions().Position();
                Vector2 dimensions = new Vector2(Parent.GetDimensions().Width, Parent.GetDimensions().Height);
                foreach (UIElement element in Elements)
                {
                    Vector2 position2 = element.GetDimensions().Position();
                    Vector2 dimensions2 = new Vector2(element.GetDimensions().Width, element.GetDimensions().Height);
                    if (Collision.CheckAABBvAABBCollision(position, dimensions, position2, dimensions2))
                        element.Draw(spriteBatch);
                }
            }
        }

        public override Rectangle GetViewCullingArea() => Parent.GetDimensions().ToRectangle();
    }

    //TML: Made public instead of protected.
    public List<UIElement> _items = new List<UIElement>();
    protected UIScrollbar _scrollbar;
    //TML: Made internal instead of private.
    internal UIElement _innerList = new UIInnerList();
    private float _innerListHeight;
    public float ListPaddingY = 5f, ListPaddingX = 5f;

    /// <summary>
    /// innerUIE, paddingX, paddingY, return innerHeight
    /// </summary>
    public Func<List<UIElement>, float, float, float> ManualRePosMethod;

    public int Count => _items.Count;

    public UIView()
    {
        _innerList.OverflowHidden = false;
        _innerList.Width.Set(0f, 1f);
        _innerList.Height.Set(0f, 1f);
        OverflowHidden = true;
        Append(_innerList);
    }

    public float GetTotalHeight() => _innerListHeight;

    public void Goto(ElementSearchMethod searchMethod)
    {
        for (int i = 0; i < _items.Count; i++)
        {
            if (searchMethod(_items[i]))
            {
                _scrollbar.ViewPosition = _items[i].Top.Pixels;
                break;
            }
        }
    }

    public virtual void Add(UIElement item)
    {
        _items.Add(item);
        _innerList.Append(item);
    }

    public virtual bool Remove(UIElement item)
    {
        _innerList.RemoveChild(item);
        return _items.Remove(item);
    }

    public virtual void Clear()
    {
        _innerList.RemoveAllChildren();
        _items.Clear();
    }
    public override void Recalculate()
    {
        base.Recalculate();
        UpdateScrollbar();
    }

    public override void ScrollWheel(UIScrollWheelEvent evt)
    {
        base.ScrollWheel(evt);
        if (_scrollbar != null)
            _scrollbar.ViewPosition -= evt.ScrollWheelValue;
    }

    public override void RecalculateChildren()
    {
        lock (Elements)
        {
            if (ManualRePosMethod != null)
            {
                _innerListHeight = ManualRePosMethod.Invoke(_items, ListPaddingX, ListPaddingY);
            }
            else
            {
                float num = 0f;
                for (int i = 0; i < _items.Count; i++)
                {
                    float num2 = _items.Count == 1 ? 0f : ListPaddingY;
                    _items[i].Top.Set(num, 0f);
                    _items[i].Recalculate();
                    num += _items[i].GetOuterDimensions().Height + num2;
                }
                _innerListHeight = num;
            }
            base.RecalculateChildren();
        }
    }

    private void UpdateScrollbar()
    {
        if (_scrollbar != null)
        {
            float height = GetInnerDimensions().Height;
            _scrollbar.SetView(height, _innerListHeight);
        }
    }

    public void SetScrollbar(UIScrollbar scrollbar)
    {
        _scrollbar = scrollbar;
        UpdateScrollbar();
    }

    public override List<SnapPoint> GetSnapPoints()
    {
        List<SnapPoint> list = new List<SnapPoint>();
        if (GetSnapPoint(out var point))
            list.Add(point);

        foreach (UIElement item in _items)
        {
            list.AddRange(item.GetSnapPoints());
        }

        return list;
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        Main.DebugDrawer.Begin();
        DrawDebugHitbox(Main.DebugDrawer);
        Main.DebugDrawer.End();
        if (_scrollbar != null)
            _innerList.Top.Set(0f - _scrollbar.GetValue(), 0f);
        Recalculate();
    }
    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
    }
    protected override void DrawChildren(SpriteBatch spriteBatch)
    {
        base.DrawChildren(spriteBatch);
    }

    public IEnumerator<UIElement> GetEnumerator() => ((IEnumerable<UIElement>)_items).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<UIElement>)_items).GetEnumerator();
}
