using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Xydion.Plugins
{
    [Tool] // Doit être ajouté pour être transtypable dans l'éditeur.
    public class Rope : Line2D
    {
        public Curve2D Curve { get; set; }

        /// <summary>
        /// <see cref="PhysicsBody2D"/> connecté au début de cette corde.
        /// </summary>
        [Export(hintString: "PhysicsBody2D connected to the start of this rope")]
        public NodePath StartNode { get; set; }

        /// <summary>
        /// <see cref="PhysicsBody2D"/> connecté à la fin de cete corde.
        /// </summary>
        [Export(hintString: "PhysicsBody2D connected to the end of this rope")]
        public NodePath EndNode { get; set; }

        /// <summary>
        /// Espace en pixel généré entre chaque morceau de corde.
        /// </summary>
        public float BakedPoint
        {
            get => Curve?.BakeInterval ?? 20;
            set => Curve.BakeInterval = value;
        }

        /// <summary>
        /// Premier <see cref="RopePart"/>.
        /// </summary>
        public RopePart StartRopePart { get => _ropeParts.FirstOrDefault(); }

        /// <summary>
        /// Dernier <see cref="RopePart"/>.
        /// </summary>
        public RopePart EndRopePart { get => _ropeParts.LastOrDefault(); }

        /// <summary>
        /// <see cref="PinJoint2D"/> utilisé pour connecter la fin de la corde à un <see cref="PhysicsBody2D"/> externe.
        /// </summary>
        private PinJoint2D _endRopeJoint;

        /// <summary>
        /// Liste des <see cref="RopePart"/> utilisé pour composer la corde.
        /// </summary>
        List<RopePart> _ropeParts;

        /// <summary>
        /// Scène utilisée pour générer les morceaux de corde.
        /// </summary>
        PackedScene pieceScene;

        RopePath RopePath { get; set; }

        public override void _Ready()
        {
            RopePath = GetParentOrNull<RopePath>();

            Curve = RopePath.Curve;

            if (!Engine.EditorHint)
                OnGameReady();
        }

        public override void _PhysicsProcess(float delta)
        {
            if (!Engine.EditorHint)
                Points = _ropeParts.Select(c => c.Position).ToArray();
        }

        public override void _Process(float delta)
        {
            if (Engine.EditorHint)
                Points = Curve.GetBakedPoints();
        }

        /// <summary>
        /// Appelé lorsque <see cref="Rope"/> est prêt et en jeu.
        /// </summary>
        private void OnGameReady()
        {
            if (Curve.GetPointCount() < 2)
            {
                GD.PrintErr("Minimum points: 2");
                return;
            }

            pieceScene = ResourceLoader.Load<PackedScene>("res://addons/GSharpRope/RopePart.tscn");
            _endRopeJoint = new PinJoint2D() { Name = "EndJoint" };

            BuildRope();
            ConnectTerminalNodes();
        }

        public void AddRopePartAfter(RopePart previousRopePart)
        {
            RopePart firstRopePart = (RopePart)pieceScene.Instance();
            RopePart secondRopePart = (RopePart)pieceScene.Instance();

            firstRopePart.Position = previousRopePart.Position.DirectionTo(previousRopePart.Next.Position).Normalized() * BakedPoint + previousRopePart.Position;
            secondRopePart.Position = previousRopePart.Position;

            firstRopePart.Next = secondRopePart;
            secondRopePart.Next = previousRopePart.Next;
            firstRopePart.Previous = previousRopePart;

            _ropeParts.AddAfterEach(c => c == previousRopePart, firstRopePart);
            _ropeParts.AddAfterEach(c => c == firstRopePart, secondRopePart);

            AddChild(firstRopePart);
            AddChild(secondRopePart);

            firstRopePart.UpdateCollisionBox(Width);
            secondRopePart.UpdateCollisionBox(Width);

            _ropeParts.ForEach(c => c.ConnectNodes());

            /*firstRopePart.ConnectNodes();
            secondRopePart.ConnectNodes();

            if (secondRopePart.Next is RopePart nextRopePart)
                nextRopePart.ConnectNodes();*/
        }

        public void AddRopePartBefore(RopePart previousRopePart)
        {
            if (previousRopePart.Previous is RopePart ropePart)
                AddRopePartAfter(ropePart);
        }

        public void RemoveRopePartAfter(RopePart ropePart)
        {

        }

        public void RemoveRopePartBefore(RopePart ropePart)
        {

        }

        /// <summary>
        /// Connecte le début de la corde à <see cref="PhysicsBody2D"/> externe : <see cref="StartNode"/>.
        /// </summary>
        private void ConnectStartNode()
        {
            StartRopePart.Previous = StartNode == null ? null : GetNode<PhysicsBody2D>(StartNode);
            StartRopePart.ConnectNodes();
        }

        /// <summary>
        /// Connecte la fin de la corde à un <see cref="PhysicsBody2D"/> externe : <see cref="EndNode"/>v
        /// </summary>
        private void ConnectEndNode()
        {
            var lastPiece = EndRopePart;

            var parentOfEndRopeJoint = _endRopeJoint.GetParentOrNull<RopePart>();
            if (parentOfEndRopeJoint != lastPiece)
            {
                parentOfEndRopeJoint?.RemoveChild(_endRopeJoint);
                lastPiece.AddChild(_endRopeJoint);
            }

            _endRopeJoint.NodeA = lastPiece.GetPath();
            _endRopeJoint.NodeB = EndNode == null ? new NodePath() : GetNode(EndNode).GetPath();
        }

        public void ConnectTerminalNodes()
        {
            ConnectStartNode();
            ConnectEndNode();
        }

        /// <summary>
        /// Construit la corde à partir des points de <see cref="Rope"/> issu de <see cref="Line2D"/>.
        /// </summary>
        private void BuildRope()
        {
            /// L'ordre est EXTREMEMENT important !
            /// Il faut créer le <see cref="RopePart"/> puis lui assigner la position et la collision box
            /// Ajouter le noeud puis connecter le noeud.

            _ropeParts = new List<RopePart>();

            foreach (var point in Curve.GetBakedPoints())
            {
                RopePart ropePart = (RopePart)pieceScene.Instance();

                ropePart.Position = point;
                ropePart.Previous = _ropeParts.LastOrDefault();

                _ropeParts.Add(ropePart);

                AddChild(ropePart);
            }

            foreach (var ropePart in _ropeParts)
            {
                ropePart.UpdateCollisionBox(Width);
                ropePart.ConnectNodes();
            }
        }
    }
}