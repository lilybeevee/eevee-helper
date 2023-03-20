﻿using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.EeveeHelper.Entities {
    [CustomEntity("EeveeHelper/SMWPlatform")]
    public class SMWPlatform : JumpThru {
        private string texturePath;
        private string flag;
        private bool notFlag;
        private bool startOnTouch;
        private bool stopAtEnd;
        private bool once;
        private bool disableBoost;
        private float startDelay;

        public SMWTrackMover Mover;

        private List<MTexture> gearFrames;
        private bool started;
        private bool movedOnce;
        private bool waitingForRestart;
        private float moved;
        private float startTimer = 0f;

        public SMWPlatform(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, false) {
            Collider.Position -= new Vector2(Width / 2f, 8);
            Depth = -60; // jumpthru depth
            
            // incorrectly checked "sprite" instead of "texturePath" before, prioritize sprite option incase anyone set it manually
            texturePath = data.Attr("sprite", data.Attr("texturePath", "objects/EeveeHelper/smwPlatform"));

            flag = data.Attr("flag");
            notFlag = data.Bool("notFlag");
            startOnTouch = data.Bool("startOnTouch");
            stopAtEnd = data.Bool("stopAtEnd");
            once = data.Bool("moveOnce");
            disableBoost = data.Bool("disableBoost");
            startDelay = data.Float("startDelay");

            Add(Mover = new SMWTrackMover(data) {
                StopAtEnd = stopAtEnd,

                GetPosition = () => ExactPosition,
                SetPosition = (pos, move) => {
                    moved += Vector2.Distance(pos, ExactPosition) / 12f;
                    MoveTo(pos, EeveeUtils.GetTrackBoost(move, disableBoost));
                },

                OnEnd = (mover, direction) => {
                    if (stopAtEnd) {
                        if (startOnTouch) {
                            started = false;
                        }
                        movedOnce = true;
                        waitingForRestart = true;
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

        public override void Added(Scene scene) {
            base.Added(scene);
            var tiles = (int)Math.Floor(Width / 8f);
            var texture = GFX.Game[$"{texturePath}/platform"];
            for (var i = 0; i < tiles; i++) {
                var sx = 1;
                if (i == 0)
                    sx = 0;
                if (i == tiles - 1)
                    sx = 2;
                Add(new Image(texture.GetSubtexture(sx * 8, 0, 8, 8)) {
                    Position = new Vector2(-Width / 2f + (i * 8f), -8f)
                });
            }
            gearFrames = GFX.Game.GetAtlasSubtextures($"{texturePath}/gear");
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            // If the platform is active on spawn, skip the start delay
            if (CheckStarted())
                startTimer = startDelay;
        }

        // sorry the vanilla code doesnt set liftspeed here
        public override void MoveHExact(int move) {
            base.MoveHExact(move);
            foreach (Actor actor in Scene.Tracker.GetEntities<Actor>()) {
                if (actor.IsRiding(this)) {
                    actor.LiftSpeed = LiftSpeed; // this line doesnt exist in vanilla code (why)
                }
            }
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

        public override void Render() {
            base.Render();
            var texture = gearFrames[(int)Math.Floor(moved) % gearFrames.Count];
            texture.DrawCentered(Position);
        }

        private bool CheckStarted() {
            var flagActive = (string.IsNullOrEmpty(flag) || SceneAs<Level>().Session.GetFlag(flag) != notFlag);

            if (waitingForRestart && (!flagActive || (startOnTouch && !HasPlayerRider())))
                waitingForRestart = false;

            if (!started && startOnTouch && !waitingForRestart)
                started = HasPlayerRider();

            return started && flagActive;
        }
    }
}
