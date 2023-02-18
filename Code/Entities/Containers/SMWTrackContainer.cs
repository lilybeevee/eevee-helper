﻿using Celeste.Mod.EeveeHelper.Components;
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
    public class SMWTrackContainer : Entity, IContainer {
        public EntityContainer Container => _Container;

        private string moveFlag;
        private bool notFlag;
        private bool startOnTouch;
        private bool disableBoost;
        private bool liftSpeedFix;

        public EntityContainerMover _Container;
        public SMWTrackMover Mover;

        private bool started;

        public SMWTrackContainer(EntityData data, Vector2 offset) : base(data.Position + offset) {
            Collider = new Hitbox(data.Width, data.Height);
            Depth = Depths.Top - 10;

            if (data.Has("moveFlag")) {
                var flag = EeveeUtils.ParseFlagAttr(data.Attr("moveFlag"));
                moveFlag = flag.Item1;
                notFlag = flag.Item2;
            } else {
                moveFlag = data.Attr("flag");
                notFlag = data.Bool("notFlag");
            }
            startOnTouch = data.Bool("startOnTouch");
            disableBoost = data.Bool("disableBoost");

            Add(_Container = new EntityContainerMover(data) {
                DefaultIgnored = e => e.Get<SMWTrackMover>() != null || e is SMWTrack
            });


            Add(Mover = new SMWTrackMover {
                Direction = data.Enum<Facings>("direction"),
                MoveSpeed = data.Float("moveSpeed"),
                Gravity = data.Float("gravity"),
                FallSpeed = data.Float("fallSpeed"),
                GetPosition = () => Center,
                SetPosition = (pos, move) => _Container.DoMoveAction(() => Center = pos, (h, delta) => EeveeUtils.GetTrackBoost(move, disableBoost))
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
            Mover.Activated = started && (string.IsNullOrEmpty(moveFlag) || SceneAs<Level>().Session.GetFlag(moveFlag) != notFlag);
            base.Update();
            if (Top > SceneAs<Level>().Bounds.Bottom)
                RemoveSelf();
        }

        private bool PlayerCheck() {
            foreach (var entity in _Container.GetEntities()) {
                if (entity is Solid solid) {
                    if (solid.HasPlayerRider())
                        return true;
                } else if (entity is JumpThru jumpThru) {
                    if (jumpThru.HasPlayerRider())
                        return true;
                } else {
                    if (Scene.Tracker.GetEntities<Player>().Any(p => p.CollideCheck(entity)))
                        return true;
                }
            }
            return false;
        }
    }
}
