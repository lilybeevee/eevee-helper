﻿using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.EeveeHelper.Entities {
    [CustomEntity("EeveeHelper/RoomChest")]
    public class RoomChest : Actor {
        public static Stack<List<Entity>> LastEntities = new Stack<List<Entity>>();
        public static Stack<RoomChest> LastChests = new Stack<RoomChest>();
        public static Stack<string> LastRooms = new Stack<string>();

        public static bool NoEntityCallbacks;

        public string Room;

        public Vector2 Speed;
        public Holdable Hold;
        public ChestExtension ChestLid;
        public bool Exiting;

        private Image bodyImage;
        private Image closedImage;
        private Vector2 prevLiftSpeed;
        private float noGravityTimer;
        private bool playerWasAbove;
        private Player playerInside;
        private float squishTimer;
        private bool closed;

        public RoomChest(EntityData data, Vector2 offset) : base(data.Position + offset) {
            Room = data.Attr("room");

            Depth = 5;
            Collider = new Hitbox(16f, 8f, -8f, -8f);

            Add(bodyImage = new Image(GFX.Game["objects/EeveeHelper/roomChest/body"]));
            Add(closedImage = new Image(GFX.Game["objects/EeveeHelper/roomChest/closed"]) { Visible = false });
            bodyImage.JustifyOrigin(0.5f, 1f);
            closedImage.JustifyOrigin(0.5f, 1f);
            bodyImage.Position = Vector2.UnitY;
            closedImage.Position = Vector2.UnitY;

            Add(Hold = new Holdable {
                PickupCollider = new Hitbox(20f, 12f, -10f, -10f),
                OnCarry = OnCarry,
                OnPickup = OnPickup,
                OnRelease = OnRelease,
                SpeedGetter = () => Speed,
                SlowRun = false,
                SlowFall = false
            });
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            scene.Add(ChestLid = new ChestExtension(this, Position));
        }

        public override void Update() {
            base.Update();

            var level = SceneAs<Level>();

            if (squishTimer > 0f) {
                squishTimer -= Engine.DeltaTime;
            } else {
                bodyImage.Scale = Vector2.One;
                closedImage.Scale = Vector2.One;
                ChestLid.Scale = Vector2.One;
            }

            var player = level.Tracker.GetEntity<Player>();
            var wasInside = playerInside != null;
            var wasClosed = closed;
            if (player != null) {
                var inside = !Hold.IsHeld && player.Left >= Left - 2f && player.Right <= Right + 2f && player.Bottom >= Top && player.Bottom <= Bottom;
                if (playerInside == null && inside && playerWasAbove) {
                    player.Left = Math.Max(player.Left, Left);
                    player.Right = Math.Min(player.Right, Right);
                    playerInside = player;
                } else if (!inside) {
                    playerInside = null;
                }
                playerWasAbove = player.Bottom <= Top;
            } else {
                playerWasAbove = false;
            }

            if (playerInside != null) {
                Depth = -5;
                ChestLid.Collidable = true;
                closed = playerInside.Ducking || (playerInside.Holding != null && !Exiting);
            } else {
                Depth = 5;
                ChestLid.Collidable = false;
                closed = false;
                Exiting = false;
            }

            if (closed && !wasClosed) {
                bodyImage.Visible = false;
                ChestLid.Visible = false;
                closedImage.Visible = true;
                closedImage.Scale = new Vector2(1.2f, 0.8f);
                player.Visible = false;
                if (player.Holding != null)
                    player.Holding.Entity.Visible = false;
                squishTimer = 0.05f;
                if (!string.IsNullOrEmpty(Room)) {
                    if (playerInside.Ducking)
                        ChestLid.TopSolid = true;
                    ChangeRoom();
                }
            } else if (!closed && wasClosed) {
                bodyImage.Visible = true;
                ChestLid.Visible = true;
                closedImage.Visible = false;
                bodyImage.Scale = new Vector2(0.8f, 1.2f);
                ChestLid.Scale = new Vector2(0.8f, 1.2f);
                player.Visible = true;
                if (player.Holding != null)
                    player.Holding.Entity.Visible = true;
                squishTimer = 0.05f;
            }

            if (Hold.IsHeld) {
                prevLiftSpeed = Vector2.Zero;

                ChestLid.Visible = Visible;
            } else {
                if (OnGround()) {
                    var target = ((!OnGround(Position + Vector2.UnitX * 3f)) ? 20f : (OnGround(Position - Vector2.UnitX * 3f) ? 0f : (-20f)));
                    Speed.X = Calc.Approach(Speed.X, target, 800f * Engine.DeltaTime);
                    var liftSpeed = LiftSpeed;
                    if (liftSpeed == Vector2.Zero && prevLiftSpeed != Vector2.Zero) {
                        Speed = prevLiftSpeed;
                        prevLiftSpeed = Vector2.Zero;
                        Speed.Y = Math.Min(Speed.Y * 0.6f, 0f);
                        if (Speed.X != 0f && Speed.Y == 0f) {
                            Speed.Y = -60f;
                        }
                        if (Speed.Y < 0f) {
                            noGravityTimer = 0.15f;
                        }
                    } else {
                        prevLiftSpeed = liftSpeed;
                        if (liftSpeed.Y < 0f && Speed.Y < 0f) {
                            Speed.Y = 0f;
                        }
                    }
                } else if (Hold.ShouldHaveGravity) {
                    float num = 600f;
                    if (Math.Abs(Speed.Y) <= 30f) {
                        num *= 0.5f;
                    }
                    float num2 = 400f;
                    if (Speed.Y < 0f) {
                        num2 *= 0.5f;
                    }
                    Speed.X = Calc.Approach(Speed.X, 0f, num2 * Engine.DeltaTime);
                    if (noGravityTimer > 0f) {
                        noGravityTimer -= Engine.DeltaTime;
                    } else {
                        Speed.Y = Calc.Approach(Speed.Y, 200f, num * Engine.DeltaTime);
                    }
                }
                MoveH(Speed.X * Engine.DeltaTime, OnCollideH);
                MoveV(Speed.Y * Engine.DeltaTime, OnCollideV);
                if (Left - 8f > level.Bounds.Right || Right + 8f < level.Bounds.Left || Top - 12f > level.Bounds.Bottom) {
                    ChestLid.RemoveSelf();
                    RemoveSelf();
                    return;
                }
                if (Top < level.Bounds.Top - 4) {
                    Top = level.Bounds.Top + 4;
                    Speed.Y = 0f;
                }
                if (X < level.Bounds.Left + 10) {
                    MoveH(32f * Engine.DeltaTime);
                }
            }
            Hold.CheckAgainstColliders();

            ChestLid.MoveTo(Position);
        }

        private void ChangeRoom() {
            var level = SceneAs<Level>();
            Add(Alarm.Create(Alarm.AlarmMode.Oneshot, () => {
                level.DoScreenWipe(false, () => {
                    var player = level.Tracker.GetEntity<Player>();
                    var dashes = player.Dashes;

                    LastChests.Push(this);
                    LastRooms.Push(level.Session.Level);

                    player.CleanUpTriggers();

                    Holdable held = null;
                    if (player.Holding != null) {
                        held = player.Holding;
                        level.Remove(player.Holding.Entity);
                        player.Holding = null;
                    }

                    var lastFollowers = new List<Follower>();
                    foreach (var follower in player.Leader.Followers) {
                        lastFollowers.Add(follower);
                        if (follower.Entity != null) {
                            level.Remove(follower.Entity);
                        }
                    }
                    player.Leader.Followers.Clear();
                    UpdateListsNoCallbacks(level);

                    level.Remove(player);
                    level.Entities.UpdateLists();

                    DeactivateEntities(level);
                    level.Session.Level = Room;
                    level.Session.FirstLevel = false;
                    level.Session.DeathsInCurrentLevel = 0;
                    level.Session.RespawnPoint = level.DefaultSpawnPoint;
                    level.LoadLevel(Player.IntroTypes.Transition);

                    level.Add(player);
                    if (held != null) {
                        level.Add(held.Entity);
                        player.Holding = held;
                        held.Entity.Visible = true;
                    }
                    level.Entities.UpdateLists();

                    player.Visible = true;
                    player.Position = level.DefaultSpawnPoint;
                    player.Dashes = dashes;

                    player.Leader.Followers = lastFollowers;
                    player.Leader.PastPoints.Clear();
                    player.Leader.PastPoints.Add(player.Position);
                    foreach (var follower in lastFollowers) {
                        for (int i = 0; i < 5; i++) {
                            player.Leader.PastPoints.Add(player.Position);
                        }
                        if (follower.Entity != null) {
                            level.Add(follower.Entity);
                            follower.Entity.Position = player.Position;
                        }
                    }
                    UpdateListsNoCallbacks(level);

                    var camPosition = player.Position - new Vector2(160f, 90f);
                    camPosition.X = MathHelper.Clamp(camPosition.X, level.Bounds.Left, level.Bounds.Right - 320);
                    camPosition.Y = MathHelper.Clamp(camPosition.Y, level.Bounds.Top, level.Bounds.Bottom - 180);
                    level.Camera.Position = camPosition;

                    level.DoScreenWipe(true);
                });
            }, 0.2f, true));
        }

        private void OnCollideH(CollisionData data) {
            (data.Hit as DashSwitch)?.OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
            Speed.X *= -0.4f;
        }

        private void OnCollideV(CollisionData data) {
            (data.Hit as DashSwitch)?.OnDashCollide(null, Vector2.UnitY * Math.Sign(Speed.Y));
        }

        private void OnCarry(Vector2 target) {
            Position = target;
            ChestLid.MoveTo(ExactPosition);
        }

        private void OnPickup() {
            Speed = Vector2.Zero;
            AddTag(Tags.Persistent);
            ChestLid.AddTag(Tags.Persistent);
        }

        private void OnRelease(Vector2 force) {
            RemoveTag(Tags.Persistent);
            ChestLid.RemoveTag(Tags.Persistent);
            if (force.X != 0f && force.Y == 0f) {
                force.Y = -0.4f;
            }
            Speed = force * 200f;
            if (Speed != Vector2.Zero) {
                noGravityTimer = 0.1f;
            }
        }

        public static void DeactivateEntities(Level level) {
            var deactivated = new List<Entity>();
            foreach (var entity in level.GetEntitiesExcludingTagMask(Tags.Global)) {
                level.Remove(entity);
                if (IsEntitySaved(entity))
                    deactivated.Add(entity);
            }
            LastEntities.Push(deactivated);

            UpdateListsNoCallbacks(level);
        }

        public static void ActivateEntities(Level level, List<Entity> exclude = null) {
            foreach (var entity in level.GetEntitiesExcludingTagMask(Tags.Global)) {
                if (IsEntitySaved(entity) && (exclude == null || !exclude.Contains(entity)))
                    level.Remove(entity);
            }
            level.Entities.UpdateLists();

            if (LastEntities.Count == 0)
                return;
            var entities = LastEntities.Pop();
            foreach (var entity in entities)
                level.Add(entity);

            UpdateListsNoCallbacks(level);
        }

        public static void UpdateListsNoCallbacks(Scene scene) {
            NoEntityCallbacks = true;
            scene.Entities.UpdateLists();
            NoEntityCallbacks = false;
        }

        public static bool IsEntitySaved(Entity entity) => !(entity is Trigger);


        private static MethodInfo m_Scene_SetActualDepth = typeof(Scene).GetMethod("SetActualDepth", BindingFlags.NonPublic | BindingFlags.Instance);
        private static MethodInfo m_Tracker_ComponentAdded = typeof(Tracker).GetMethod("ComponentAdded", BindingFlags.NonPublic | BindingFlags.Instance);
        private static MethodInfo m_Tracker_ComponentRemoved = typeof(Tracker).GetMethod("ComponentRemoved", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Load() {
            On.Celeste.Level.LoadLevel += Level_LoadLevel;
            IL.Monocle.EntityList.UpdateLists += EntityList_UpdateLists;
        }

        public static void Unload() {
            On.Celeste.Level.LoadLevel -= Level_LoadLevel;
            IL.Monocle.EntityList.UpdateLists -= EntityList_UpdateLists;
        }

        private static void Level_LoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
            if (isFromLoader) {
                LastEntities.Clear();
                LastChests.Clear();
                LastRooms.Clear();
            }
            orig(self, playerIntro, isFromLoader);
        }

        private static void EntityList_UpdateLists(MonoMod.Cil.ILContext il) {
            var cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<Entity>("Added"))) {
                var postLabel = cursor.MarkLabel();
                cursor.Index--;
                var thisLabel = cursor.DefineLabel();

                cursor.EmitDelegate<Func<bool>>(() => NoEntityCallbacks);
                cursor.Emit(OpCodes.Brfalse, thisLabel);
                cursor.EmitDelegate<Action<Entity, Scene>>((entity, scene) => {
                    DynamicData.For(entity).Set("Scene", scene);
                    foreach (var component in entity.Components)
                        m_Tracker_ComponentAdded.Invoke(scene.Tracker, new object[] { component });
                    m_Scene_SetActualDepth.Invoke(scene, new object[] { entity });
                });
                cursor.Emit(OpCodes.Br, postLabel);
                cursor.MarkLabel(thisLabel);

                cursor.Index++;
            }

            cursor.Index = 0;

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<Entity>("Removed"))) {
                var postLabel = cursor.MarkLabel();
                cursor.Index--;
                var thisLabel = cursor.DefineLabel();

                cursor.EmitDelegate<Func<bool>>(() => NoEntityCallbacks);
                cursor.Emit(OpCodes.Brfalse, thisLabel);
                cursor.EmitDelegate<Action<Entity, Scene>>((entity, scene) => {
                    DynamicData.For(entity).Set("Scene", null);
                    foreach (var component in entity.Components)
                        m_Tracker_ComponentRemoved.Invoke(scene.Tracker, new object[] { component });
                });
                cursor.Emit(OpCodes.Br, postLabel);
                cursor.MarkLabel(thisLabel);

                cursor.Index++;
            }

            cursor.Index = 0;

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<Entity>("Awake"))) {
                var postLabel = cursor.MarkLabel();
                cursor.Index--;
                var thisLabel = cursor.DefineLabel();

                cursor.EmitDelegate<Func<bool>>(() => NoEntityCallbacks);
                cursor.Emit(OpCodes.Brfalse, thisLabel);
                cursor.Emit(OpCodes.Pop);
                cursor.Emit(OpCodes.Pop);
                cursor.Emit(OpCodes.Br, postLabel);
                cursor.MarkLabel(thisLabel);

                cursor.Index++;
            }
        }


        public class ChestExtension : Solid {
            public bool TopSolid {
                get => Collider == closedColliders;
                set => Collider = value ? closedColliders : openColliders;
            }

            public Vector2 Scale;
            public RoomChest Chest;

            private ColliderList openColliders;
            private ColliderList closedColliders;
            private Image lidImage;

            public ChestExtension(RoomChest chest, Vector2 position) : base(position, 16f, 16f, false) {
                Chest = chest;
                Scale = Vector2.One;

                Depth = 6;
                Collider = openColliders = new ColliderList(
                    new OnlyPlayerHitbox(4f, 8f, -12f, -8f),
                    new OnlyPlayerHitbox(4f, 8f, 8f, -8f),
                    new OnlyPlayerHitbox(24f, 4f, -12f, -1f)
                );
                closedColliders = new ColliderList(
                    new OnlyPlayerHitbox(4f, 8f, -12f, -8f),
                    new OnlyPlayerHitbox(4f, 8f, 8f, -8f),
                    new OnlyPlayerHitbox(24f, 4f, -12f, -1f),
                    new OnlyPlayerHitbox(24f, 4f, -12f, -12f)
                );
                Collidable = false;

                Add(lidImage = new Image(GFX.Game["objects/EeveeHelper/roomChest/lid"]));
                lidImage.JustifyOrigin(0.5f, 1f);
                lidImage.Position = new Vector2(0f, -8f);
            }

            public override void Render() {
                lidImage.Scale = Scale;
                base.Render();
            }
        }

        public class OnlyPlayerHitbox : Hitbox {
            public OnlyPlayerHitbox(float width, float height, float x = 0, float y = 0) : base(width, height, x, y) { }

            public override bool Collide(Hitbox hitbox) {
                if (hitbox.Entity != null && hitbox.Entity is Player)
                    return base.Collide(hitbox);
                return false;
            }
        }
    }
}
