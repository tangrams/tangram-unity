using System;

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
