using Godot;

namespace Xydion.Plugins
{

    public class RopePart : RigidBody2D
    {
        private PhysicsBody2D _previous;
        private PhysicsBody2D _next;

        // TODO: Ajouter la force contraingnant la corde à rester droit. Rigidité comme un tuyau ?

        /// <summary>
        /// Morceau de corde précédent.
        /// </summary>
        public PhysicsBody2D Previous
        {
            get => _previous;
            set
            {
                _previous = value;

                if (_previous is RopePart p && p.Next != this)
                    p.Next = this;
            }
        }

        /// <summary>
        /// Morceau de corde suivant.
        /// </summary>
        public PhysicsBody2D Next
        {
            get => _next;
            set
            {
                _next = value;

                if (_next is RopePart p && p.Previous != this)
                    p.Previous = this;
            }
        }

        /// <summary>
        /// Corde parente.
        /// </summary>
        public Rope Rope { get => GetParentOrNull<Rope>(); }

        /// <summary>
        /// Indique si ce noeud est un noeud de début ou de fin.
        /// </summary>
        public bool TerminalNode { get => !(Next is RopePart) || !(Previous is RopePart); }

        /// <summary>
        /// Indique si ce noeud est connecté à un élément extérieur.
        /// </summary>
        public bool Connected { get => TerminalNode && (Next != null || Previous != null); }

        public PinJoint2D Joint { get => GetNodeOrNull<PinJoint2D>("Joint"); }

        public CollisionShape2D Collision { get => GetNodeOrNull<CollisionShape2D>("Collision"); }

        public bool DebugVisibility
        {
            get => GetNode<ColorRect>("ColorRect").Visible;
            set => GetNode<ColorRect>("ColorRect").Visible = value;
        }

        public override void _IntegrateForces(Physics2DDirectBodyState state)
        {
            base._IntegrateForces(state);

            if (!TerminalNode || Connected)
                return;

            var rope = GetParentOrNull<Rope>();

            if (rope.EndRopePart == this && rope.EndNode != null)
            {
                var targetBody = rope.GetNodeOrNull<PhysicsBody2D>(rope.EndNode);
                TryAttachTo(targetBody);
            }
            
            if (rope.StartRopePart == this && rope.StartNode != null)
            {
                var targetBody = rope.GetNodeOrNull<PhysicsBody2D>(rope.StartNode);
                TryAttachTo(targetBody);
            }
        }

        /// <summary>
        /// Essai de connecter ce morceau de corde à la cible.
        /// </summary>
        /// <param name="body"></param>
        private void TryAttachTo(PhysicsBody2D body)
        {
            if (GlobalPosition.DistanceTo(body.GlobalPosition) < 10)
            {
                GD.Print("Connected !");
                ConnectNodes();
            }
            else
                ApplyCentralImpulse(GlobalPosition.DirectionTo(body.GlobalPosition) * 50);
        }

        /// <summary>
        /// Met à jour la collision box du morceau de corde.
        /// </summary>
        /// <param name="width"></param>
        internal void UpdateCollisionBox(float width)
        {
            if (Next is RopePart)
            {
                Collision.Position = GlobalPosition.DirectionTo(Next.GlobalPosition) * Rope.BakedPoint / 2f;
                Collision.Rotation = GlobalPosition.AngleToPoint(Next.GlobalPosition) - Mathf.Pi / 2f;
                /*Collision.Shape = new CapsuleShape2D()
                {
                    Height = Rope.BakedPoint / 2f,
                    Radius = width / 2f
                };*/
                Collision.Shape = new RectangleShape2D()
                {
                    Extents = new Vector2(width / 2f, Rope.BakedPoint / 2f)
                };
            }
            else
            {
                //Collision.Disabled = true;
            }
        }

        /// <summary>
        /// Connecte le joint de ce morceau au précédent.
        /// </summary>
        public void ConnectNodes()
        {
            Joint.NodeA = GetPath();
            Joint.NodeB = Previous == null ? new NodePath() : Previous.GetPath();
        }
    }
}