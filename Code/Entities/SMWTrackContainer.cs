using Celeste.Mod.EeveeHelper.Components;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.EeveeHelper.Entities {
    [CustomEntity("EeveeHelper/SMWTrackContainer")]
    public class SMWTrackContainer : Entity {
        private string[] whitelist;
        private string flag;
        private bool notFlag;
        private bool startOnTouch;

        public EntityContainer Container;
        public SMWTrackMover Mover;

        private bool started;

        public SMWTrackContainer(EntityData data, Vector2 offset) : base(data.Position + offset) {
            Collider = new Hitbox(data.Width, data.Height);
            Depth = Depths.Top - 10;

            whitelist = data.Attr("whitelist").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            flag = data.Attr("flag");
            notFlag = data.Bool("notFlag");
            startOnTouch = data.Bool("startOnTouch");

            Add(Container = new EntityContainer {
                FitContained = data.Bool("fitContained"),
                IsValid = IsValid
            });

            Add(Mover = new SMWTrackMover {
                Direction = data.Enum<Facings>("direction"),
                MoveSpeed = data.Float("moveSpeed"),
                Gravity = data.Float("gravity"),
                FallSpeed = data.Float("fallSpeed"),
                GetPosition = () => Center,
                SetPosition = (pos, move) => Container.DoMoveAction(() => Center = pos, (entity, delta) => {
                    if (entity is Platform platform) {
                        platform.MoveTo(Position + delta, move);
                    } else {
                        entity.Position = Position + delta;
                    }
                })
            });

            if (startOnTouch) {
                started = false;
                Mover.Activated = false;
            } else {
                started = true;
            }
        }

        public override void Update() {
            if (!started && startOnTouch)
                started = PlayerCheck();
            Mover.Activated = started && (string.IsNullOrEmpty(flag) || SceneAs<Level>().Session.GetFlag(flag) != notFlag);
            base.Update();
            if (Top > SceneAs<Level>().Bounds.Bottom)
                RemoveSelf();
        }

        private bool IsValid(Entity entity) {
            if (whitelist.Length == 0)
                return !(entity.Get<SMWTrackMover>() != null || entity is SMWTrack || entity is Player || entity is SolidTiles || entity is BackgroundTiles || entity is Decal || entity is Trigger);
            else
                return whitelist.Contains(entity.GetType().Name);
        }

        private bool PlayerCheck() {
            foreach (var entity in Container.Contained) {
                if (entity is Solid solid) {
                    if (solid.HasPlayerRider())
                        return true;
                } else if (entity is JumpThru jumpThru) {
                    if (jumpThru.HasPlayerRider())
                        return true;
                } else {
                    if (Scene.Tracker.GetEntities<Player>().Any(p => {
                        return p.CollideCheck(entity);
                    }))
                        return true;
                }
            }
            return false;
        }
    }
}
