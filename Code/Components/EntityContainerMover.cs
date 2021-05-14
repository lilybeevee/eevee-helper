using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.EeveeHelper.Components {
    public class EntityContainerMover : EntityContainer {
        private static HashSet<string> IgnoredAnchors = new HashSet<string>() {
            "Position", "ExactPosition", "TopLeft", "TopCenter", "TopRight", "Center", "CenterLeft", "CenterRight", "BottomLeft", "BottomCenter", "BottomRight"
        };
        private static HashSet<string> CommonAnchors = new HashSet<string>() {
            "anchor", "anchorPosition", "start", "startPosition"
        };

        public Vector4 Padding;

        public bool FitContained;
        public Action<Vector2, float, float> OnFit;
        public Action OnPreMove;
        public Action OnPostMove;

        public EntityContainerMover() : base() { }

        public EntityContainerMover(EntityData data, bool fitContained = true) : base(data) {
            if (fitContained)
                FitContained = data.Bool("fitContained");
        }

        public override void Added(Entity entity) {
            base.Added(entity);
            entity.Add(new TransitionListener {
                OnOutBegin = () => {
                    if (!Entity.TagCheck(Tags.Persistent))
                        Contained.RemoveAll(e => e.TagCheck(Tags.Persistent));
                }
            });
        }

        public override void EntityAwake() {
            base.EntityAwake();

            if (!Attached)
                Padding = new Vector4(Entity.Width / 2f, Entity.Height / 2f, Entity.Width / 2f, Entity.Height / 2f);
        }

        public override void EntityRemoved(Scene scene) {
            base.EntityRemoved(scene);
            RemoveContained();
        }

        public override void Update() {
            if (Attached && FitContained) {
                var bounds = GetContainedBounds();
                var targetPos = new Vector2(bounds.X + Padding.X, bounds.Y + Padding.Y);
                var targetCorner = new Vector2(bounds.X + bounds.Width + Padding.Z, bounds.Y + bounds.Height + Padding.W);
                var targetWidth = targetCorner.X - targetPos.X;
                var targetHeight = targetCorner.Y - targetPos.Y;
                if (Entity.TopLeft != targetPos || Entity.BottomRight != targetCorner) {
                    if (OnFit != null) {
                        OnFit(targetPos, targetWidth, targetHeight);
                    } else {
                        Entity.Position = targetPos;
                        Entity.Collider.Width = targetWidth;
                        Entity.Collider.Height = targetHeight;
                    }
                }
            }

            base.Update();
        }

        protected override void AddContained(Entity entity) {
            base.AddContained(entity);

            var data = new DynamicData(entity);
            if (!data.TryGet<List<string>>("entityContainerAnchors", out _)) {
                var anchors = new List<string>();
                foreach (var pair in data)
                    if (pair.Value is Vector2 vector && !IgnoredAnchors.Contains(pair.Key) && (vector == EeveeUtils.GetPosition(entity) || CommonAnchors.Contains(pair.Key))) {
                        Console.WriteLine($"Adding anchor: {pair.Key}");
                        anchors.Add(pair.Key);
                    }

                if (anchors.Count > 0)
                    data.Set("entityContainerAnchors", anchors);
            }
        }

        protected override void AttachInside(bool first = false) {
            base.AttachInside(first);

            var bounds = GetContainedBounds();
            Padding = new Vector4(Entity.Left - bounds.X, Entity.Top - bounds.Y, Entity.Right - (bounds.X + bounds.Width), Entity.Bottom - (bounds.Y + bounds.Height));
        }

        protected override void DetachOutside() {
            base.DetachOutside();

            var bounds = GetContainedBounds();
            Padding = new Vector4(Entity.Left - bounds.X, Entity.Top - bounds.Y, Entity.Right - (bounds.X + bounds.Width), Entity.Bottom - (bounds.Y + bounds.Height));
        }

        protected override void DetachAll() {
            base.DetachAll();

            Padding = new Vector4(Entity.Width / 2f, Entity.Height / 2f, Entity.Width / 2f, Entity.Height / 2f);
        }

        public void DoMoveAction(Action moveAction, Action<Entity, Vector2> moveFinalizer = null) {
            Cleanup();
            var anchorOffsets = new Dictionary<Entity, Dictionary<string, Vector2>>();
            var offsets = new Dictionary<Entity, Vector2>();
            var collidable = new Dictionary<Entity, bool>();
            var startPosition = EeveeUtils.GetPosition(Entity);
            foreach (var entity in Contained) {
                var data = new DynamicData(entity);
                var anchorNames = data.Get<List<string>>("entityContainerAnchors");
                if (anchorNames != null) {
                    var currentAnchors = new Dictionary<string, Vector2>();
                    foreach (var name in anchorNames)
                        currentAnchors.Add(name, data.Get<Vector2>(name) - startPosition);
                    anchorOffsets.Add(entity, currentAnchors);
                }
                offsets.Add(entity, EeveeUtils.GetPosition(entity) - startPosition);
                collidable.Add(entity, entity.Collidable);
                entity.Collidable = false;
            }
            moveAction();
            var selfCollidable = Entity.Collidable;
            Entity.Collidable = false;
            OnPreMove?.Invoke();
            foreach (var entity in Contained) {
                entity.Collidable = collidable[entity];
                if (anchorOffsets.ContainsKey(entity)) {
                    var data = new DynamicData(entity);
                    var currentAnchors = anchorOffsets[entity];
                    foreach (var pair in currentAnchors)
                        data.Set(pair.Key, Entity.Position + pair.Value);
                }
                if (moveFinalizer != null) {
                    moveFinalizer(entity, offsets[entity]);
                } else {
                    if (entity is Platform platform) {
                        platform.MoveTo(Entity.Position + offsets[entity]);
                    } else {
                        entity.Position = Entity.Position + offsets[entity];
                    }
                }
            }
            Entity.Collidable = selfCollidable;
            OnPostMove?.Invoke();
        }

        public void DoIgnoreCollision(Action action) {
            var lastCollidable = new Dictionary<Entity, bool>();
            foreach (var entity in Contained) {
                lastCollidable.Add(entity, entity.Collidable);
                entity.Collidable = false;
            }
            action();
            foreach (var entity in Contained)
                entity.Collidable = lastCollidable[entity];
        }
    }
}
