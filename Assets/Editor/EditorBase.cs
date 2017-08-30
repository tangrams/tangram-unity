using System;

/// <summary>
/// Base class for editor, each editor has a unique guid used
/// for saving custom preferences in the Unity editor prefs.
/// </summary>
public class EditorBase
{
    protected Guid guid;

    public EditorBase()
    {
        guid = Guid.NewGuid();
    }

    public Guid GUID
    {
        get { return guid; }
    }
}
