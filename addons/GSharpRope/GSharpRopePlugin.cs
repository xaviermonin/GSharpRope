using Godot;

namespace Xydion.Plugins
{
    [Tool]
    public class GSharpRopePlugin : EditorPlugin
    {
        public override string GetPluginName()
        {
            return "GSharpRope";
        }

        public override void _EnterTree()
        {
            GD.Print($"{nameof(GSharpRopePlugin)} loaded");
            
            ResourceManager.Load();
            AddCustomType("GSharpRope", nameof(Path2D), ResourceManager.RopePathScript, null);
        }

        public override void _ExitTree()
        {
            RemoveCustomType(nameof(RopePath));
        }
    }
}