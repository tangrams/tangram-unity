using System;

namespace PluginEditor
{
    /// <summary>
    /// Base class for editor, each editor has a unique guid used
    /// for saving custom preferences in the Unity editor prefs.
    /// </summary>
    public abstract class EditorBase : IEditor
    {
        protected Guid guid;
        public string Name;

        public EditorBase()
        {
            this.guid = Guid.NewGuid();
        }

        public EditorBase(string name)
        {
            this.Name = name;
            this.guid = Guid.NewGuid();
        }

        public Guid GUID
        {
            get { return guid; }
        }

        public abstract void OnInspectorGUI();
    }
}
