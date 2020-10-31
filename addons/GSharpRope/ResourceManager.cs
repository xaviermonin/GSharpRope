#if TOOLS

using Godot;

namespace Xydion.Plugins
{
    static class ResourceManager
    {
        public static CSharpScript RopeScript { get; private set; }
        public static CSharpScript RopePathScript { get; private set; }

        public static void Load()
        {
            RopeScript ??= ResourceLoader.Load<CSharpScript>("res://addons/GSharpRope/Rope.cs");
            RopePathScript ??= ResourceLoader.Load<CSharpScript>("res://addons/GSharpRope/RopePath.cs");
        }
    }
}

#endif