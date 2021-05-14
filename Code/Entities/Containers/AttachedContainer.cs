using Celeste.Mod.EeveeHelper.Components;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.EeveeHelper.Entities {
    [CustomEntity("EeveeHelper/AttachedContainer")]
    public class AttachedContainer : Entity {
        private string attachTo;
        private Vector2? node;

        private EntityContainerMover container;
        private StaticMover mover;
        private Entity customAttached;
        private Vector2 lastAttachedPos;
        private Dictionary<Entity, Tuple<bool, bool, bool>> lastStates = new Dictionary<Entity, Tuple<bool, bool, bool>>();

        public AttachedContainer(EntityData data, Vector2 offset) : base(data.Position + offset) {
            Collider = new Hitbox(data.Width, data.Height);
            Depth = Depths.Top - 11;

            attachTo = data.Attr("attachTo");

            var nodes = data.NodesOffset(offset);
            if (nodes.Length > 0)
                node = nodes[0];

            Add(container = new EntityContainerMover(data) {
                OnFit = OnFit,
                IsValid = e => !IsRiding(e),
                DefaultIgnored = e => e is AttachedContainer
            });

            if (string.IsNullOrEmpty(attachTo)) {
                Add(mover = new StaticMover {
                    JumpThruChecker = (entity) => IsRiding(entity),
                    SolidChecker = (entity) => IsRiding(entity),
                    OnMove = (amount) => container.DoMoveAction(() => Position += amount, KeepLiftSpeed),
                    OnShake = (amount) => container.DoMoveAction(() => Position += amount, KeepLiftSpeed),
                    OnEnable = OnEnable,
                    OnDisable = OnDisable
                });
            }
        }

        private void OnFit(Vector2 pos, float width, float height) {
            var lastCenter = Center;
            Position = pos;
            Collider.Width = width;
            Collider.Height = height;
            container.DoMoveAction(() => Center = lastCenter);
        }

        public override void Awake(Scene scene) {
            if (!string.IsNullOrEmpty(attachTo)) {
                var closestDist = 0f;
                Entity closest = null;
                foreach (var entity in scene.Entities) {
                    if (entity.GetType().Name == attachTo) {
                        if (node != null && entity.CollidePoint(node.Value)) {
                            closest = entity;
                            break;
                        } else {
                            var dist = Vector2.Distance(node ?? Center, entity.Center);
                            if (closest == null || dist < closestDist) {
                                closestDist = dist;
                                closest = entity;
                            }
                        }
                    }
                }
                if (closest != null) {
                    customAttached = closest;
                    lastAttachedPos = EeveeUtils.GetPosition(customAttached);
                }
            }
            base.Awake(scene);
        }

        public override void Update() {
            base.Update();

            if (customAttached != null) {
                var newPos = EeveeUtils.GetPosition(customAttached);
                if (newPos != lastAttachedPos) {
                    var delta = newPos - lastAttachedPos;
                    container.DoMoveAction(() => Position += delta);
                    lastAttachedPos = newPos;
                }
                if (customAttached.Scene == null)
                    RemoveSelf();
            }
        }

        private bool IsRiding(Entity entity) {
            if (mover != null && mover.Platform != null)
                return entity == mover.Platform;

            if (!string.IsNullOrEmpty(attachTo))
                return entity == customAttached;

            if (node != null)
                return entity.CollidePoint(node.Value);
            else if (entity is JumpThru)
                return CollideCheckOutside(entity, Position + Vector2.UnitY);
            else
                return CollideCheckOutside(entity, Position + Vector2.UnitY)
                    || CollideCheckOutside(entity, Position - Vector2.UnitY)
                    || CollideCheckOutside(entity, Position + Vector2.UnitX)
                    || CollideCheckOutside(entity, Position - Vector2.UnitX);
        }

        private void OnEnable() {
            Active = Visible = Collidable = true;
            foreach (var entity in container.Contained) {
                if (lastStates.ContainsKey(entity)) {
                    var state = lastStates[entity];
                    entity.Active = state.Item1;
                    entity.Visible = state.Item2;
                    entity.Collidable = state.Item3;
                }
            }
            lastStates.Clear();
        }

        private void OnDisable() {
            Active = Visible = Collidable = false;
            foreach (var entity in container.Contained) {
                if (!lastStates.ContainsKey(entity)) {
                    lastStates.Add(entity, new Tuple<bool, bool, bool>(entity.Active, entity.Visible, entity.Collidable));
                    entity.Active = entity.Visible = entity.Collidable = false;
                }
            }
        }

        private void KeepLiftSpeed(Entity entity, Vector2 offset) {
            if (entity is Platform platform) {
                platform.MoveTo(Position + offset, mover.Platform.LiftSpeed);
            } else {
                entity.Position = Position + offset;
            }
        }
    }
}
