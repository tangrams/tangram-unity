using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

namespace Nextzen.Unity.Editor
{
    public class FeatureLayerTreeView : TreeView
    {
        public IList<FeatureLayer> Layers;

        public FeatureLayerTreeView(TreeViewState state)
            : base(state)
        {
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            // The root item is required to have a depth of -1, and the rest of the items increment from that.
            var root = new TreeViewItem { id = -1, depth = -1, displayName = "Root" };

            var items = new List<TreeViewItem>();
            var index = 0;

            if (Layers != null)
            {
                foreach (var layer in Layers)
                {
                    items.Add(new TreeViewItem { id = index, depth = 0, displayName = layer.Name });
                    index++;
                }
            }

            // Utility method that initializes the TreeViewItem.children and .parent for all items.
            SetupParentsAndChildrenFromDepths(root, items);

            // Return root of the tree
            return root;
        }

        protected override bool CanRename(TreeViewItem item)
        {
            return true;
        }

        protected override void RenameEnded(RenameEndedArgs args)
        {
            if (Layers != null)
            {
                var layer = Layers[args.itemID];
                layer.Name = args.newName;
            }
        }
    }
}