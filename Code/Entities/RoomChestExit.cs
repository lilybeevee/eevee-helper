using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.EeveeHelper.Entities {
    [CustomEntity("EeveeHelper/RoomChestExit")]
    public class RoomChestExit : Entity {
        public RoomChestExit(EntityData data, Vector2 offset) : base(data.Position + offset) {
            Depth = 1000;
            Collider = new Hitbox(data.Width, data.Height);

            Add(new TalkComponent(new Rectangle(0, 0, data.Width, data.Height), new Vector2(data.Width/2, data.Height/4), (player) => {
                if (RoomChest.LastRooms.Count == 0)
                    return;

                var level = SceneAs<Level>();
                Add(Alarm.Create(Alarm.AlarmMode.Oneshot, () => {
                    level.DoScreenWipe(false, () => {
                        level.OnEndOfFrame += () => {
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
                            RoomChest.UpdateListsNoCallbacks(level);

                            level.Remove(player);
                            level.UnloadLevel();
                            level.Session.Level = RoomChest.LastRooms.Pop();
                            level.Session.FirstLevel = false;
                            level.Session.DeathsInCurrentLevel = 0;
                            level.Session.RespawnPoint = level.DefaultSpawnPoint;
                            level.LoadLevel(Player.IntroTypes.Transition);
                            RoomChest.ActivateEntities(level);

                            level.Add(player);
                            if (held != null) {
                                level.Add(held.Entity);
                                player.Holding = held;
                            }
                            level.Entities.UpdateLists();

                            var lastChest = RoomChest.LastChests.Pop();
                            player.Position = lastChest.BottomCenter - Vector2.UnitY * 2f;

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
                            RoomChest.UpdateListsNoCallbacks(level);

                            if (player.Holding == null) {
                                player.Ducking = true;
                                player.Visible = false;
                                lastChest.ChestLid.TopSolid = true;
                                lastChest.Add(Alarm.Create(Alarm.AlarmMode.Oneshot, () => {
                                    lastChest.ChestLid.TopSolid = false;
                                    player.Ducking = false;
                                }, 0.5f, true));
                            } else {
                                player.Ducking = false;
                                player.Visible = true;
                                player.Holding.Entity.Visible = true;
                                lastChest.ChestLid.TopSolid = false;
                                lastChest.Exiting = true;
                            }

                            var camPosition = player.Position - new Vector2(160f, 90f);
                            camPosition.X = MathHelper.Clamp(camPosition.X, level.Bounds.Left, level.Bounds.Right - 320);
                            camPosition.Y = MathHelper.Clamp(camPosition.Y, level.Bounds.Top, level.Bounds.Bottom - 180);
                            level.Camera.Position = camPosition;

                            level.DoScreenWipe(true);
                        };
                    });
                }, 0.2f, true));
            }));
        }

        public override void Render() {
            base.Render();
            Draw.Rect(Collider, Color.LightPink * 0.2f);
        }
    }
}
