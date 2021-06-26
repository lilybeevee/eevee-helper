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
        private EntityContainer.ContainMode attachMode;
        private string attachFlag;
        private bool notAttachFlag;
        private string attachTo;
        private bool restrictToNode;
        private bool onlyX;
        private bool onlyY;
        private Vector2? node;

        private EntityContainerMover container;
        private StaticMover mover;
        private bool attached;
        private Entity customAttached;
        private Vector2 lastAttachedPos;
        private Entity firstAttached = null;
        private Dictionary<Entity, Tuple<bool, bool, bool>> lastStates = new Dictionary<Entity, Tuple<bool, bool, bool>>();

        public AttachedContainer(EntityData data, Vector2 offset) : base(data.Position + offset) {
            Collider = new Hitbox(data.Width, data.Height);
            Depth = Depths.Top - 11;

            attachMode = data.Enum("attachMode", EntityContainer.ContainMode.RoomStart);
            var parsedFlag = EeveeUtils.ParseFlagAttr(data.Attr("attachFlag"));
            attachFlag = parsedFlag.Item1;
            notAttachFlag = parsedFlag.Item2;
            attachTo = data.Attr("attachTo");
            restrictToNode = data.Bool("restrictToNode");
            onlyX = data.Bool("onlyX");
            onlyY = data.Bool("onlyY");

            var nodes = data.NodesOffset(offset);
            if (nodes.Length > 0)
                node = nodes[0] - Center;

            Add(container = new EntityContainerMover(data) {
                OnFit = OnFit,
                IsValid = e => !IsValid(e),
                DefaultIgnored = e => e is AttachedContainer
            });

            if (string.IsNullOrEmpty(attachTo) && attachMode == EntityContainer.ContainMode.RoomStart) {
                Add(mover = new StaticMover {
                    JumpThruChecker = (entity) => IsRiding(entity, true),
                    SolidChecker = (entity) => IsRiding(entity, true),
                    OnMove = (amount) => { if (attached) OnMove(amount); },
                    OnShake = (amount) => { if (attached) OnMove(amount); },
                    OnEnable = () => { if (attached) OnEnable(); },
                    OnDisable = () => { if (attached) OnDisable(); }
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
            attached = string.IsNullOrEmpty(attachFlag) || SceneAs<Level>().Session.GetFlag(attachFlag) != notAttachFlag;

            if (attachMode == EntityContainer.ContainMode.RoomStart) {
                TryAttach(true);
                if (!attached)
                    customAttached = null;
            } else {
                if (attached)
                    TryAttach();
            }

            base.Awake(scene);
        }

        public override void Update() {
            base.Update();

            var newAttached = string.IsNullOrEmpty(attachFlag) || SceneAs<Level>().Session.GetFlag(attachFlag) != notAttachFlag;

            if (attachMode != EntityContainer.ContainMode.Always) {
                if (newAttached != attached) {
                    attached = newAttached;

                    if (attached)
                        TryAttach();
                    else
                        customAttached = null;
                }
            } else {
                var attachChanged = newAttached != attached;

                attached = newAttached;

                if (attached && customAttached == null)
                    TryAttach();
                else if (!attached)
                    customAttached = null;
            }

            if (customAttached != null) {
                var newPos = EeveeUtils.GetPosition(customAttached);
                if (newPos != lastAttachedPos) {
                    var delta = newPos - lastAttachedPos;
                    if (onlyX) delta.Y = 0f;
                    if (onlyY) delta.X = 0f;
                    container.DoMoveAction(() => Position += delta);
                    lastAttachedPos = newPos;
                }
                if (customAttached.Scene == null)
                    RemoveSelf();
            }
        }

        private bool TryAttach(bool first = false) {
            if (mover == null) {
                if (attachMode != EntityContainer.ContainMode.RoomStart || first) {
                    var closestDist = 0f;
                    Entity closest = null;
                    foreach (var entity in Scene.Entities) {
                        if (string.IsNullOrEmpty(attachTo) ? (entity is JumpThru || entity is Solid) : entity.GetType().Name == attachTo) {
                            if (node != null && entity.CollidePoint(Center + node.Value)) {
                                closest = entity;
                                break;
                            }
                            if (node == null || !restrictToNode) {
                                if (!string.IsNullOrEmpty(attachTo)) {
                                    var dist = Vector2.Distance(node != null ? (Center + node.Value) : Center, entity.Center);
                                    if (closest == null || dist < closestDist) {
                                        closestDist = dist;
                                        closest = entity;
                                    }
                                } else if (IsRiding(entity, false)) {
                                    closest = entity;
                                }
                            }
                        }
                    }
                    if (closest != null) {
                        customAttached = closest;
                        lastAttachedPos = EeveeUtils.GetPosition(customAttached);
                        return true;
                    }
                } else {
                    customAttached = firstAttached;
                    if (customAttached != null)
                        lastAttachedPos = EeveeUtils.GetPosition(customAttached);
                    return true;
                }
            }
            return false;
        }

        private bool IsValid(Entity entity) {
            if (mover != null)
                return entity == mover.Platform;
            else
                return entity == customAttached;
        }

        private bool IsRiding(Entity entity, bool checkNode) {
            if (checkNode && node != null)
                return entity.CollidePoint(Center + node.Value);
            else if (entity is JumpThru)
                return CollideCheckOutside(entity, Position + Vector2.UnitY);
            else
                return CollideCheckOutside(entity, Position + Vector2.UnitY)
                    || CollideCheckOutside(entity, Position - Vector2.UnitY)
                    || CollideCheckOutside(entity, Position + Vector2.UnitX)
                    || CollideCheckOutside(entity, Position - Vector2.UnitX);
        }
        
        private void OnMove(Vector2 amount) {
            if (onlyX) amount.Y = 0f;
            if (onlyY) amount.X = 0f;
            container.DoMoveAction(() => Position += amount, KeepLiftSpeed);
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
