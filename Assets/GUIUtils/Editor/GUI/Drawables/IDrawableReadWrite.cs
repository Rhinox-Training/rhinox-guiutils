namespace Rhinox.GUIUtils.Editor
{
    public interface IDrawableRead : IOrderedDrawable
    {
        object GetValue();
    }
    
    public interface IDrawableReadWrite : IDrawableRead
    {
        bool TrySetValue(object value);
    }
}