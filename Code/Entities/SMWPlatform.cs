using Celeste.Mod.Entities;
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
        private bool disableBoost;

        public SMWTrackMover Mover;

        private List<MTexture> gearFrames;
        private bool started;
        private float moved;

        public SMWPlatform(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, false) {
            Collider.Position -= new Vector2(Width / 2f, 8);
            Depth = -60; // jumpthru depth
            
            // incorrectly checked "sprite" instead of "texturePath" before, prioritize sprite option incase anyone set it manually
            texturePath = data.Attr("sprite", data.Attr("texturePath", "objects/EeveeHelper/smwPlatform"));

            flag = data.Attr("flag");
            notFlag = data.Bool("notFlag");
            startOnTouch = data.Bool("startOnTouch");
            disableBoost = data.Bool("disableBoost");

            Add(Mover = new SMWTrackMover {
                Direction = data.Enum<Facings>("direction"),
                MoveSpeed = data.Float("moveSpeed"),
                Gravity = data.Float("gravity"),
                FallSpeed = data.Float("fallSpeed"),
                GetPosition = () => ExactPosition,
                SetPosition = (pos, move) => {
                    moved += Vector2.Distance(pos, ExactPosition) / 12f;
                    MoveTo(pos, EeveeUtils.GetTrackBoost(move, disableBoost));
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
            if (!started && startOnTouch)
                started = HasPlayerRider();
            Mover.Activated = started && (string.IsNullOrEmpty(flag) || SceneAs<Level>().Session.GetFlag(flag) != notFlag);
            base.Update();
            if (Top > SceneAs<Level>().Bounds.Bottom)
                RemoveSelf();
        }

        public override void Render() {
            base.Render();
            var texture = gearFrames[(int)Math.Floor(moved) % gearFrames.Count];
            texture.DrawCentered(Position);
        }
    }
}
