using Celeste.Mod.EeveeHelper.Handlers;
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

        public List<IEntityHandler> Contained = new List<IEntityHandler>();
        public Dictionary<Entity, List<IEntityHandler>> HandlersFor = new Dictionary<Entity, List<IEntityHandler>>();
        public List<Tuple<string, int>> Blacklist;
        public List<Tuple<string, int>> Whitelist;
        public ContainMode Mode;
        public string ContainFlag;
        public bool NotFlag;
        public bool ForceStandardBehavior;

        public Func<Entity, bool> IsValid;
        public Func<Entity, bool> DefaultIgnored;
        public Action<IEntityHandler> OnAttach;
        public Action<IEntityHandler> OnDetach;

        public bool Attached;
        public bool CollideWithContained;

        private List<IEntityHandler> containedSaved = new List<IEntityHandler>();

        public EntityContainer() : base(true, true) { }

        public EntityContainer(EntityData data) : this() {
            Whitelist = ParseList(data.Attr("whitelist"));
            Blacklist = ParseList(data.Attr("blacklist"));
            Mode = data.Enum("containMode", ContainMode.FlagChanged);
            var flag = EeveeUtils.ParseFlagAttr(data.Attr("containFlag"));
            ContainFlag = flag.Item1;
            NotFlag = flag.Item2;
            ForceStandardBehavior = data.Bool("forceStandardBehavior", true);
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

        public virtual List<IEntityHandler> GetHandlersFor(Entity entity) {
            if (entity == null || !HandlersFor.ContainsKey(entity))
                return new List<IEntityHandler>();
            else
                return HandlersFor[entity];
        }

        public virtual bool HasHandlerFor<T>(Entity entity) {
            if (entity == null || !HandlersFor.ContainsKey(entity))
                return false;
            return HandlersFor[entity].Any(h => h is T);
        }

        public virtual bool IsFirstHandler(IEntityHandler handler) {
            if (!HandlersFor.ContainsKey(handler.Entity))
                return true;
            return HandlersFor[handler.Entity][0] == handler;
        }

        public virtual List<Entity> GetEntities() {
            var list = new List<Entity>();
            foreach (var handler in Contained) {
                if (!list.Contains(handler.Entity))
                    list.Add(handler.Entity);
            }
            return list;
        }

        protected virtual void AddContained(IEntityHandler handler) {
            handler.OnAttach(this);
            Contained.Add(handler);

            List<IEntityHandler> handlers;
            if (!HandlersFor.TryGetValue(handler.Entity, out handlers)) {
                handlers = new List<IEntityHandler>();
                HandlersFor.Add(handler.Entity, handlers);
            }
            handlers.Add(handler);
        }

        protected virtual void RemoveContained(IEntityHandler handler) {
            Contained.Remove(handler);
            var handlers = HandlersFor[handler.Entity];
            handlers.Remove(handler);
            if (handlers.Count == 0) {
                HandlersFor.Remove(handler.Entity);
            }
            handler.OnDetach(this);
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

        protected virtual bool WhitelistCheck(Entity entity) {
            if (Blacklist.Any(pair => pair.Item1 == entity.GetType().Name))
                return false;
            if (Whitelist.Count == 0)
                return !((DefaultIgnored?.Invoke(entity) ?? false) || entity is Player || entity is SolidTiles || entity is BackgroundTiles || entity is Decal || entity is Trigger);
            else
                return Whitelist.Any(pair => pair.Item1 == entity.GetType().Name);
        }

        protected virtual bool WhitelistCheckCount(Entity entity, int count) {
            if (Blacklist.Any(pair => pair.Item1 == entity.GetType().Name && (pair.Item2 == -1 || count == pair.Item2)))
                return false;
            if (Whitelist.Count == 0)
                return true;
            else
                return Whitelist.Any(pair => pair.Item1 == entity.GetType().Name && (pair.Item2 == -1 || count == pair.Item2));
        }

        protected virtual void AttachInside(bool first = false) {
            if (first || Mode != ContainMode.RoomStart) {
                var counts = new Dictionary<Type, int>();
                foreach (var entity in Scene.Entities) {
                    if (entity != Entity && WhitelistCheck(entity) && (IsValid?.Invoke(entity) ?? true)) {
                        if (!counts.ContainsKey(entity.GetType()))
                            counts.Add(entity.GetType(), 0);

                        var anyInside = false;
                        var handlers = EntityHandler.CreateAll(entity, this, ForceStandardBehavior);
                        foreach (var handler in handlers) {
                            if (handler.IsInside(this)) {
                                anyInside = true;

                                if ((Mode != ContainMode.Always || !Contained.Contains(handler)) && WhitelistCheckCount(entity, counts[entity.GetType()] + 1)) {
                                    AddContained(handler);
                                    OnAttach?.Invoke(handler);
                                }
                            }
                        }

                        if (anyInside)
                            counts[entity.GetType()]++;
                    }
                }
            } else {
                Cleanup();
                foreach (var handler in containedSaved) {
                    AddContained(handler);
                    OnAttach?.Invoke(handler);
                }
                containedSaved.Clear();
            }
        }

        protected virtual void DetachAll() {
            var lastContained = new List<IEntityHandler>(Contained);
            if (Mode == ContainMode.RoomStart)
                containedSaved = lastContained;

            foreach (var handler in lastContained) {
                RemoveContained(handler);
                OnDetach?.Invoke(handler);
            }
        }

        protected virtual void DetachOutside() {
            var toRemove = new List<IEntityHandler>();
            foreach (var handler in Contained) {
                if (!handler.IsInside(this)) {
                    toRemove.Add(handler);
                }
            }
            foreach (var handler in toRemove) {
                RemoveContained(handler);
                OnDetach?.Invoke(handler);
            }

        }

        protected void Cleanup() {
            Contained.RemoveAll(e => e.Entity?.Scene == null);
        }

        public bool CheckCollision(Entity entity) {
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
            var bounds = new Rectangle();

            var first = true;
            foreach (var handler in Contained) {
                var rect = handler.GetBounds();
                bounds = first ? rect : Rectangle.Union(bounds, rect);
                first = false;
            }

            return bounds;
        }

        public void DestroyContained() {
            foreach (var handler in Contained) {
                handler.Destroy();
            }
            Contained.Clear();
        }
    }
}
