using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.EeveeHelper.Entities {
    public class SMWTrackMover : Component {
        // Right = move forward, Left = move back
        public Facings Direction;
        public float MoveSpeed = 100f;
        public float Gravity = 200f;
        public float FallSpeed = 200f;
        public Func<Vector2> GetPosition;
        public Action<Vector2, Vector2> SetPosition;

        public SMWTrack Track;
        public float Progress;
        public Vector2 Speed;
        public bool Activated = true;

        private Vector2 lastMove;
        private SMWTrack.Section lastSection;
        private float lastSectionCooldown;

        public SMWTrackMover()
            : base(true, true) { }

        public override void EntityAwake() {
            base.EntityAwake();
            foreach (SMWTrack track in Scene.Tracker.GetEntities<SMWTrack>()) {
                var section = track.GetIntersect(GetAttachPos() - Vector2.UnitY * 2f, GetAttachPos() + Vector2.UnitY * 2f, out var intersect);
                if (section != null) {
                    AttachTo(track, section, intersect);
                    break;
                }
                section = track.GetIntersect(GetAttachPos() - Vector2.UnitX * 2f, GetAttachPos() + Vector2.UnitX * 2f, out intersect);
                if (section != null) {
                    AttachTo(track, section, intersect);
                    break;
                }
            }
        }

        public override void Update() {
            base.Update();
            if (lastSectionCooldown > 0f)
                lastSectionCooldown -= Engine.DeltaTime;
            if (!Activated)
                return;
            if (Track != null) {
                var move = MoveSpeed * Engine.DeltaTime * (Direction == Facings.Left ? -1f : 1f);
                SetProgress(Progress + move);
            } else {
                if ((FallSpeed <= 0f && Speed.Y < FallSpeed) || (FallSpeed >= 0f && Speed.Y > FallSpeed))
                    Speed.Y = FallSpeed;
                Speed.Y = Calc.Approach(Speed.Y, FallSpeed, Gravity * Engine.DeltaTime);

                foreach (SMWTrack track in Scene.Tracker.GetEntities<SMWTrack>()) {
                    var section = track.GetIntersect(GetAttachPos(), GetAttachPos() + Speed * Engine.DeltaTime, out var intersect);
                    if (section != null && (section != lastSection || lastSectionCooldown <= 0f)) {
                        AttachTo(track, section, intersect);
                        return;
                    }
                }

                MoveTo(GetAttachPos() + Speed * Engine.DeltaTime, Speed);
            }
        }

        private bool TryFall(float progress) {
            if (Track.Enabled) {
                if (progress < 0f && Track.StartOpen) {
                    lastSection = Track.Sections.First();
                    lastMove = lastSection.GetAngle(0f) * -1f;
                    MoveTo(lastSection.Start);
                } else if (progress >= Track.Length && Track.EndOpen) {
                    lastSection = Track.Sections.Last();
                    lastMove = lastSection.GetAngle(lastSection.Length);
                    MoveTo(lastSection.End);
                    progress -= Track.Length;
                } else {
                    return false;
                }
            } else {
                progress = 0;
            }
            lastSectionCooldown = 0.3f;
            Speed = lastMove * MoveSpeed;
            if (Speed.X < 0)
                Direction = Facings.Left;
            if (Speed.X > 0)
                Direction = Facings.Right;
            Progress = progress;
            Track = null;

            return true;
        }

        private void SetProgress(float progress) {
            var lastTrack = Track;
            if (TryFall(progress)) {
                foreach (SMWTrack track in Scene.Tracker.GetEntities<SMWTrack>()) {
                    var section = track.TryQuickAttach(GetAttachPos(), out var hit, ignore: lastSectionCooldown <= 0f ? null : lastSection);
                    if (section != null) {
                        var progressAdd = Math.Abs(Progress);
                        AttachTo(track, section, hit);
                        Direction = Progress == 0f ? Facings.Right : Facings.Left;
                        SetProgress(Progress + progressAdd * (Direction == Facings.Left ? -1f : 1f));
                        break;
                    }
                }
                return;
            }

            if (Track.Length > 0f) {
                if (progress < 0f || progress >= Track.Length) {
                    if (progress < 0f) {
                        progress *= -1f;
                        Flip();
                    } else if (progress >= Track.Length) {
                        progress = Track.Length - (progress - Track.Length);
                        Flip();
                    }
                    // hmmm
                    SetProgress(progress);
                    return;
                }

                var section = Track.GetSection(progress);
                lastSection = section;
                lastMove = section.GetAngle(progress) * (int)Direction;
                MoveTo(section.GetPos(progress - section.Offset));
                Progress = progress;
            } else {
                lastSection = null;
                lastMove = Vector2.Zero;
                MoveTo(Track.Position);
                Progress = 0f;
            }
        }

        private void AttachTo(SMWTrack track, SMWTrack.Section section, Vector2 hit) {
            var progress = section.GetProgress(hit);
            MoveTo(section.GetPos(progress));

            Track = track;
            Progress = progress + section.Offset;

            if (section.End.X < section.Start.X) {
                // Flip direction if track direction is left to maintain movement direction (Right = move forward, Left = move back)
                Flip();
            } else if (section.Start.X == section.End.X && Speed != Vector2.Zero) {
                // If track is vertical and we're moving, always move in the direction we fall
                Direction = section.End.Y < section.Start.Y ? Facings.Left : Facings.Right;
                if (Gravity < 0f)
                    Flip();
            }

            Speed = Vector2.Zero;
        }

        private void Flip() {
            Direction = (Direction == Facings.Left) ? Facings.Right : Facings.Left;
        }

        private Vector2 GetAttachPos() {
            return GetPosition?.Invoke() ?? Entity.Center;
        }

        private void MoveTo(Vector2 pos, Vector2? liftSpeed = null) {
            if (SetPosition != null)
                SetPosition(pos, liftSpeed ?? lastMove * MoveSpeed);
            else
                Entity.Center = pos;
        }
    }
}
