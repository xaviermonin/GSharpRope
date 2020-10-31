using Godot;

namespace Xydion.Plugins
{
    [Tool]
    public class RopePath : Path2D
    {
        Rope Rope { get; set; }

        /// <summary>
        /// <see cref="PhysicsBody2D"/> connecté au début de cette corde.
        /// </summary>
        [Export(hintString: "PhysicsBody2D connected to the start of this rope")]
        public NodePath StartNode { get; set; }

        /// <summary>
        /// <see cref="PhysicsBody2D"/> connecté à la fin de cette corde.
        /// </summary>
        [Export(hintString: "PhysicsBody2D connected to the end of this rope")]
        public NodePath EndNode { get; set; }

        [Export(hintString: "The texture used for the line's texture. Uses texture_mode for drawing style.")]
        public Texture Texture { get; set; }

        [Export(hintString: "The style to render the texture on the line. Use Godot.Line2D.LineTextureMode constants.")]
        public Line2D.LineTextureMode TextureMode { get; set; }

        [Export]
        public float Width { get; set; } = 10;

        /// <summary>
        /// Appelé lorsque <see cref="Rope"/> est prêt et en jeu.
        /// </summary>
        private void OnGameReady()
        {
            SelfModulate = Colors.Transparent;

            if (StartNode != null && !StartNode.IsEmpty())
                Rope.StartNode = GetNode(StartNode)?.GetPath();

            if (EndNode != null && !EndNode.IsEmpty())
                Rope.EndNode = GetNode(EndNode)?.GetPath();
        }

        /// <summary>
        /// Appelé lorsque <see cref="Rope"/> est prêt et dans l'éditeur.
        /// </summary>
        private void OnEditorReady()
        {

        }

        public override void _Ready()
        {
            Rope = new Rope()
            {
                TextureMode = TextureMode,
                Texture = Texture,
                Width = Width,
            };

            if (Engine.EditorHint)
                OnEditorReady();
            else
                OnGameReady();

            AddChild(Rope);
        }
    }
}
