using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.EeveeHelper.Components {
    public class EntityContainer : Component {
        private static HashSet<string> IgnoredAnchors = new HashSet<string>() {
            "Position", "ExactPosition", "TopLeft", "TopCenter", "TopRight", "Center", "CenterLeft", "CenterRight", "BottomLeft", "BottomCenter", "BottomRight"
        };
        private static HashSet<string> CommonAnchors = new HashSet<string>() {
            "anchor", "anchorPosition", "start", "startPosition"
        };

        public List<Entity> Contained = new List<Entity>();
        public Vector4 Padding;

        public bool FitContained;
        public Func<Entity, bool> IsValid;
        public Action<Vector2, float, float> OnFit;
        public Action OnPreMove;
        public Action OnPostMove;

        public EntityContainer() : base(true, true) { }

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
            foreach (var entity in Scene.Entities) {
                if (ContainsEntity(entity)) {
                    var valid = IsValid?.Invoke(entity) ?? !(entity == Entity || entity is Player || entity is SolidTiles || entity is BackgroundTiles || entity is Decal || entity is Trigger);

                    if (valid) {
                        Contained.Add(entity);

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
                }
            }

            var bounds = GetContainedBounds();
            Padding = new Vector4(Entity.Left - bounds.X, Entity.Top - bounds.Y, Entity.Right - (bounds.X + bounds.Width), Entity.Bottom - (bounds.Y + bounds.Height));
        }

        public override void EntityRemoved(Scene scene) {
            base.EntityRemoved(scene);
            RemoveContained();
        }

        public override void Update() {
            base.Update();
            Cleanup();
            if (FitContained) {
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
        }

        private void Cleanup() {
            Contained.RemoveAll(e => e.Scene == null);
        }

        public bool ContainsEntity(Entity entity) {
            if (entity.Collider != null) {
                var collidable = entity.Collidable;
                entity.Collidable = true;
                var result = Entity.CollideCheck(entity);
                entity.Collidable = collidable;
                return result;
            } else {
                return entity.X >= Entity.Left && entity.Y >= Entity.Top && entity.X <= Entity.Right && entity.Y <= Entity.Bottom;
            }
        }

        public Rectangle GetContainedBounds() {
            var topLeft = Vector2.Zero;
            var bottomRight = Vector2.Zero;

            var first = true;
            foreach (var entity in Contained) {
                if (entity.Collider != null) {
                    topLeft = first ? entity.TopLeft : Vector2.Min(topLeft, entity.TopLeft);
                    bottomRight = first ? entity.BottomRight : Vector2.Max(bottomRight, entity.BottomRight);
                } else {
                    topLeft = first ? entity.Position : Vector2.Min(topLeft, entity.Position);
                    bottomRight = first ? entity.Position : Vector2.Max(bottomRight, entity.Position);
                }
                first = false;
            }

            return new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)(bottomRight.X - topLeft.X), (int)(bottomRight.Y - topLeft.Y));
        }

        public void DoMoveAction(Action moveAction, Action<Entity, Vector2> moveFinalizer = null) {
            Cleanup();
            var anchorOffsets = new Dictionary<Entity, Dictionary<string, Vector2>>();
            var offsets = new Dictionary<Entity, Vector2>();
            var collidable = new Dictionary<Entity, bool>();
            var startPosition = EeveeUtils.GetPosition(Entity);
            foreach (var entity in Contained) {
                var data = new DynamicData(entity);
                var anchorNames = data.Get<List<string>>("holdableContainerAnchors");
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

        public void RemoveContained() {
            foreach (var entity in Contained) {
                if (entity is Platform platform)
                    platform.DestroyStaticMovers();
                entity.RemoveSelf();
            }
            Contained.Clear();
        }


        public static void Load() {
            On.Monocle.Collide.Check_Entity_Entity += Collide_Check_Entity_Entity;
        }

        public static void Unload() {
            On.Monocle.Collide.Check_Entity_Entity -= Collide_Check_Entity_Entity;
        }

        private static bool Collide_Check_Entity_Entity(On.Monocle.Collide.orig_Check_Entity_Entity orig, Entity a, Entity b) {
            var aContainer = a.Get<EntityContainer>();
            var bContainer = b.Get<EntityContainer>();
            if ((aContainer != null && aContainer.Contained.Contains(b)) ||
                (bContainer != null && bContainer.Contained.Contains(a))) {

                return false;
            }
            return orig(a, b);
        }
    }
}
