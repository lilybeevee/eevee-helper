using Celeste.Mod.EeveeHelper.Components;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.EeveeHelper.Entities.Modifiers {
    [CustomEntity("EeveeHelper/CollidableModifier")]
    public class CollidableModifier : Entity {
        private bool noCollide;
        private bool solidify;

        private EntityContainer container;
        private Dictionary<Entity, Solid> solids = new Dictionary<Entity, Solid>();
        private Dictionary<Entity, bool> wasCollidable = new Dictionary<Entity, bool>();

        public CollidableModifier(EntityData data, Vector2 offset) : base(data.Position + offset) {
            Collider = new Hitbox(data.Width, data.Height);

            noCollide = data.Bool("noCollide");
            solidify = data.Bool("solidify");

            Add(container = new EntityContainer(data) {
                IsValid = e => !(e is Solidifier solidifier && container.Contained.Contains(solidifier.Entity)),
                DefaultIgnored = e => e.Get<EntityContainer>() != null,
                OnAttach = OnAttach,
                OnDetach = OnDetach
            });
        }

        private void OnAttach(Entity entity) {
            if (solidify && !solids.ContainsKey(entity) && entity.Collider != null && !(entity is Solid)) {
                var solid = new Solidifier(entity);
                new DynData<Entity>(entity).Set("solidModifierSolidifier", solid);
                solids.Add(entity, solid);
                Scene.Add(solid);
            }
            if (!wasCollidable.ContainsKey(entity))
                wasCollidable.Add(entity, entity.Collidable);
            entity.Collidable = false;
        }

        private void OnDetach(Entity entity) {
            if (solidify && solids.ContainsKey(entity)) {
                var solid = solids[entity];
                new DynData<Entity>(entity).Set<Solidifier>("solidModifierSolidifier", null);
                if (solid.Scene != null)
                    solid.RemoveSelf();
            }
            if (noCollide) {
                if (wasCollidable.ContainsKey(entity))
                    entity.Collidable = wasCollidable[entity];
                else
                    entity.Collidable = true;
            }
        }

        public override void Update() {
            base.Update();

            if (noCollide) {
                foreach (var entity in container.Contained)
                    entity.Collidable = false;
            }
        }


        public class Solidifier : Solid {
            public Entity Entity;

            public Solidifier(Entity entity) : base(EeveeUtils.GetPosition(entity), 1f, 1f, false) {
                Collider = Collider.Clone();
                Depth = entity.Depth + 1;
                Entity = entity;
            }

            public override void Update() {
                base.Update();

                if (Entity == null || Entity.Scene == null) {
                    RemoveSelf();
                    return;
                }

                Depth = Entity.Depth + 1;

                if (Entity.Collider.Size != Collider.Size)
                    Collider = Entity.Collider.Clone();

                if (ExactPosition != EeveeUtils.GetPosition(Entity))
                    MoveTo(EeveeUtils.GetPosition(Entity));
            }

            public override void MoveHExact(int move) {
                var collidable = Entity.Collidable;
                Entity.Collidable = false;
                var pushable = true;
                var actor = Entity as Actor;
                if (actor != null) {
                    pushable = actor.AllowPushing;
                    actor.AllowPushing = false;
                }
                base.MoveHExact(move);
                Entity.Collidable = collidable;
                if (actor != null)
                    actor.AllowPushing = pushable;
            }

            public override void MoveVExact(int move) {
                var collidable = Entity.Collidable;
                Entity.Collidable = false;
                var pushable = true;
                var actor = Entity as Actor;
                if (actor != null) {
                    pushable = actor.AllowPushing;
                    actor.AllowPushing = false;
                }
                base.MoveVExact(move);
                Entity.Collidable = collidable;
                if (actor != null)
                    actor.AllowPushing = pushable;
            }
        }
    }
}
