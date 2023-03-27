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
        private bool stopAtNode;
        private bool stopAtEnd;
        private bool once;
        private bool disableBoost;
        private float startDelay;

        public EntityContainerMover _Container;
        public SMWTrackMover Mover;

        private bool started;
        private bool waitingForRestart;
        private bool movedOnce;
        private float startTimer = 0f;

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
            stopAtNode = data.Bool("stopAtNode");
            stopAtEnd = data.Bool("stopAtEnd");
            once = data.Bool("moveOnce");
            startDelay = data.Float("startDelay");

            Add(_Container = new EntityContainerMover(data) {
                DefaultIgnored = e => e.Get<SMWTrackMover>() != null || e is SMWTrack
            });

            Add(Mover = new SMWTrackMover(data) {
                StopAtNode = stopAtNode,
                StopAtEnd = stopAtEnd,

                GetPosition = () => Center,
                SetPosition = (pos, move) => _Container.DoMoveAction(() => Center = pos, (h, delta) => EeveeUtils.GetTrackBoost(move, disableBoost)),

                OnStop = (mover, end) => {
                    if (startOnTouch) {
                        started = false;
                    }
                    waitingForRestart = true;
                    if (end) {
                        movedOnce = true;
                    }
                }
            });

            if (startOnTouch) {
                started = false;
                Mover.Activated = false;
            } else {
                started = true;
            }
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            // If the platform is active on spawn, skip the start delay
            if (!startOnTouch && CheckStarted())
                startTimer = startDelay;
        }

        public override void Update() {
            if (!movedOnce || !once) {
                var active = CheckStarted();

                if (active)
                    startTimer = Calc.Approach(startTimer, startDelay, Engine.DeltaTime);
                else
                    startTimer = 0f;

                Mover.Activated = active && startTimer >= startDelay;
            }

            base.Update();

            if (Top > SceneAs<Level>().Bounds.Bottom)
                RemoveSelf();
        }

        private bool CheckStarted() {
            var flagActive = (string.IsNullOrEmpty(moveFlag) || SceneAs<Level>().Session.GetFlag(moveFlag) != notFlag);

            if (waitingForRestart && (!flagActive || (startOnTouch && !PlayerCheck())))
                waitingForRestart = false;

            if (!started && startOnTouch && !waitingForRestart)
                started = PlayerCheck();

            return started && flagActive;
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
