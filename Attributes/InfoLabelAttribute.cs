using System;

public class InfoLabelAttribute : Attribute
{
    public string Text;
    // public string Icon;
    public bool AlwaysShowAsTooltip;

    public InfoLabelAttribute(string text)
    {
        Text = text;
    }
}
