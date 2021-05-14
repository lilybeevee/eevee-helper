using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.EeveeHelper.Components {
    [Tracked(true)]
    public class EntityContainer : Component {
        public enum ContainMode {
            FlagChanged,
            RoomStart,
            Always
        }

        public List<Entity> Contained = new List<Entity>();
        public List<Tuple<string, int>> Blacklist;
        public List<Tuple<string, int>> Whitelist;
        public ContainMode Mode;
        public string ContainFlag;
        public bool NotFlag;

        public Func<Entity, bool> IsValid;
        public Func<Entity, bool> DefaultIgnored;
        public Action<Entity> OnAttach;
        public Action<Entity> OnDetach;

        public bool Attached;
        public bool CollideWithContained;

        private List<Entity> containedSaved = new List<Entity>();

        public EntityContainer() : base(true, true) { }

        public EntityContainer(EntityData data) : this() {
            Whitelist = ParseList(data.Attr("whitelist"));
            Blacklist = ParseList(data.Attr("blacklist"));
            Mode = data.Enum("containMode", ContainMode.FlagChanged);
            var flag = EeveeUtils.ParseFlagAttr(data.Attr("containFlag"));
            ContainFlag = flag.Item1;
            NotFlag = flag.Item2;
        }

        public override void EntityAwake() {
            base.EntityAwake();

            Attached = string.IsNullOrEmpty(ContainFlag) || SceneAs<Level>().Session.GetFlag(ContainFlag) != NotFlag;

            if (Attached)
                AttachInside(true);
        }

        public override void Update() {
            base.Update();
            Cleanup();

            var newAttached = string.IsNullOrEmpty(ContainFlag) || SceneAs<Level>().Session.GetFlag(ContainFlag) != NotFlag;

            if (Mode != ContainMode.Always) {
                if (newAttached != Attached) {
                    Attached = newAttached;

                    if (Attached)
                        AttachInside();
                    else
                        DetachAll();
                }
            } else {
                var attachChanged = newAttached != Attached;

                Attached = newAttached;

                if (Attached) {
                    DetachOutside();
                    AttachInside();
                } else if (attachChanged) {
                    DetachAll();
                }
            }
        }

        protected virtual void AddContained(Entity entity) {
            Contained.Add(entity);
        }

        protected List<Tuple<string, int>> ParseList(string list) {
            return list.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(entry => {
                var split = entry.Split(':');
                if (split.Length >= 2 && int.TryParse(split[1], out var count))
                    return Tuple.Create(split[0], count);
                else
                    return Tuple.Create(entry, -1);
            }).ToList();
        }

        protected virtual bool WhitelistCheck(Entity entity, int count) {
            if (Blacklist.Any(pair => pair.Item1 == entity.GetType().Name && (pair.Item2 == -1 || count == pair.Item2)))
                return false;
            if (Whitelist.Count == 0)
                return !((DefaultIgnored?.Invoke(entity) ?? false) || entity is Player || entity is SolidTiles || entity is BackgroundTiles || entity is Decal || entity is Trigger);
            else
                return Whitelist.Any(pair => pair.Item1 == entity.GetType().Name && (pair.Item2 == -1 || count == pair.Item2));
        }

        protected virtual void AttachInside(bool first = false) {
            if (first || Mode != ContainMode.RoomStart) {
                var counts = new Dictionary<Type, int>();
                foreach (var entity in Scene.Entities) {
                    if (entity != Entity && (Mode != ContainMode.Always || !Contained.Contains(entity)) && ContainsEntity(entity) && (IsValid?.Invoke(entity) ?? true)) {
                        if (!counts.ContainsKey(entity.GetType()))
                            counts.Add(entity.GetType(), 0);

                        if (WhitelistCheck(entity, ++counts[entity.GetType()])) {
                            AddContained(entity);
                            OnAttach?.Invoke(entity);
                        }
                    }
                }
            } else {
                Contained = new List<Entity>(containedSaved);
                Cleanup();
                foreach (var entity in Contained)
                    OnAttach?.Invoke(entity);
                containedSaved.Clear();
            }
        }

        protected virtual void DetachAll() {
            var lastContained = new List<Entity>(Contained);
            if (Mode == ContainMode.RoomStart)
                containedSaved = lastContained;

            Contained.Clear();
            
            foreach (var entity in lastContained)
                OnDetach?.Invoke(entity);
        }

        protected virtual void DetachOutside() {
            var toRemove = new List<Entity>();
            foreach (var entity in Contained) {
                if (!ContainsEntity(entity)) {
                    toRemove.Add(entity);
                }
            }
            foreach (var entity in toRemove) {
                Console.WriteLine($"Detaching {entity.GetType().Name}");
                Contained.Remove(entity);
                OnDetach?.Invoke(entity);
            }

        }

        protected void Cleanup() {
            Contained.RemoveAll(e => e.Scene == null);
        }

        public bool ContainsEntity(Entity entity) {
            if (entity.Collider != null) {
                var collidable = entity.Collidable;
                var parentCollidable = Entity.Collidable;
                entity.Collidable = true;
                Entity.Collidable = true;
                CollideWithContained = true;
                var result = Entity.CollideCheck(entity);
                CollideWithContained = false;
                entity.Collidable = collidable;
                Entity.Collidable = parentCollidable;
                return result;
            } else {
                return entity.X >= Entity.Left && entity.Y >= Entity.Top && entity.X <= Entity.Right && entity.Y <= Entity.Bottom;
            }
        }

        public Rectangle GetContainedBounds() {
            var topLeft = Vector2.Zero;
            var bottomRight = Vector2.Zero;

            var first = true;
            foreach (var entity in Contained) {
                if (entity.Collider != null) {
                    topLeft = first ? entity.TopLeft : Vector2.Min(topLeft, entity.TopLeft);
                    bottomRight = first ? entity.BottomRight : Vector2.Max(bottomRight, entity.BottomRight);
                } else {
                    topLeft = first ? entity.Position : Vector2.Min(topLeft, entity.Position);
                    bottomRight = first ? entity.Position : Vector2.Max(bottomRight, entity.Position);
                }
                first = false;
            }

            return new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)(bottomRight.X - topLeft.X), (int)(bottomRight.Y - topLeft.Y));
        }

        public void RemoveContained() {
            foreach (var entity in Contained) {
                if (entity is Platform platform)
                    platform.DestroyStaticMovers();
                entity.RemoveSelf();
            }
            Contained.Clear();
        }
    }
}
