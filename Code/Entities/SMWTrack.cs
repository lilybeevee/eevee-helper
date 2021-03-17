using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.EeveeHelper.Entities {
    [Tracked]
    [CustomEntity("EeveeHelper/SMWTrack")]
    public class SMWTrack : Entity {
        public Color Color;
        public bool StartOpen;
        public bool EndOpen;

        public List<Section> Sections;
        public float Length;
        public bool Enabled = true;

        protected List<Vector2> nodes;
        private string startOpenFlag;
        private string endOpenFlag;
        private string flag;
        private bool notFlag;
        private bool hidden;

        private MTexture pointTexture;
        private MTexture endTexture;
        private bool notStartOpenFlag;
        private bool notEndOpenFlag;
        private Collider startCollider;
        private Collider endCollider;

        public SMWTrack(EntityData data, Vector2 offset) : base(data.Position + offset) {
            Depth = Depths.BGDecals + 10;

            pointTexture = GFX.Game.GetOrDefault("objects/EeveeHelper/smwTrack/point", GFX.Game.GetFallback());
            endTexture = GFX.Game.GetOrDefault("objects/EeveeHelper/smwTrack/end", GFX.Game.GetFallback());

            Color = data.HexColor("color", Color.White);
            StartOpen = data.Bool("startOpen");
            EndOpen = data.Bool("endOpen");
            flag = data.Attr("flag");
            notFlag = data.Bool("notFlag");
            startOpenFlag = data.Attr("startOpenFlag");
            endOpenFlag = data.Attr("endOpenFlag");
            notStartOpenFlag = !StartOpen;
            notEndOpenFlag = !EndOpen;
            hidden = data.Bool("hidden");

            nodes = new List<Vector2>();
            nodes.Add(Position);
            nodes.AddRange(data.NodesOffset(offset));

            startCollider = new Circle(4f, nodes.First().X - X, nodes.First().Y - Y);
            endCollider = new Circle(4f, nodes.Last().X - X, nodes.Last().Y - Y);

            if (hidden)
                Visible = false;

            GenerateSections();
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            FlagCheck();
        }

        public override void Render() {
            base.Render();
            foreach (var section in Sections) {
                DrawSection(section);
            }
            for (var i = 0; i < nodes.Count; i++) {
                var node = nodes[i];
                if ((i == 0 && !StartOpen) || (i == nodes.Count - 1 && !EndOpen)) {
                    endTexture.DrawCentered(node.Floor(), Color);
                } else {
                    pointTexture.DrawCentered(node.Floor(), Color);
                }
            }
        }

        public override void Update() {
            base.Update();
            FlagCheck();
        }

        protected virtual void FlagCheck() {
            var level = SceneAs<Level>();
            if (!string.IsNullOrEmpty(flag)) {
                if (level.Session.GetFlag(flag) != notFlag) {
                    Visible = !hidden;
                    Enabled = Collidable = true;
                } else {
                    Enabled = Collidable = Visible = false;
                }
            }
            if (!string.IsNullOrEmpty(startOpenFlag))
                StartOpen = level.Session.GetFlag(startOpenFlag) != notStartOpenFlag;
            if (!string.IsNullOrEmpty(endOpenFlag))
                EndOpen = level.Session.GetFlag(endOpenFlag) != notEndOpenFlag;
        }

        protected virtual void GenerateSections() {
            Sections = new List<Section>();
            Length = 0f;
            for (int i = 0; i < nodes.Count - 1; i++) {
                var section = new Section(nodes[i], nodes[i + 1], Length);
                Sections.Add(section);
                Length += section.Length;
            }
        }

        protected virtual void DrawSection(Section section) {
            Draw.Line(section.Start, section.End, Color.Black, 4f);
            Draw.Line(section.Start, section.End, Color, 2f);
        }

        public Section TryQuickAttach(Vector2 point, out Vector2 pos, Section ignore = null) {
            pos = Vector2.Zero;
            if (!Collidable)
                return null;
            if (Sections.First() != ignore) {
                Collider = startCollider;
                if (CollidePoint(point)) {
                    Collider = null;
                    pos = Sections.First().Start;
                    return Sections.First();
                }
            }
            if (Sections.Last() != ignore) {
                Collider = endCollider;
                if (CollidePoint(point)) {
                    Collider = null;
                    pos = Sections.Last().End;
                    return Sections.Last();
                }
            }
            return null;
        }

        public virtual Section GetIntersect(Vector2 lineFrom, Vector2 lineTo, out Vector2 intersect) {
            intersect = Vector2.Zero;
            if (!Enabled)
                return null;
            foreach (var section in Sections)
                if (Collide.LineCheck(section.Start, section.End, lineFrom, lineTo, out intersect))
                    return section;
            return null;
        }

        public virtual Section GetSection(float progress)
            => Sections.FirstOrDefault(s => progress >= s.Offset && progress < s.Offset + s.Length);

        public class Section {
            public Vector2 Start;
            public Vector2 End;
            public float Offset;
            public float Length;

            public Section(Vector2 start, Vector2 end, float offset) {
                Start = start;
                End = end;
                Offset = offset;
                Length = Vector2.Distance(start, end);
            }

            public virtual Vector2 GetPos(float progress) {
                return Vector2.Lerp(Start, End, progress / Length);
            }

            public virtual float GetProgress(Vector2 pos) {
                return Vector2.Distance(pos, Start);
            }

            public virtual Vector2 GetAngle(float progress) {
                return Calc.SafeNormalize(End - Start);
            }
        }
    }
}
