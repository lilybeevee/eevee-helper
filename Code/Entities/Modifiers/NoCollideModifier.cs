using Celeste.Mod.EeveeHelper.Components;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.EeveeHelper.Entities.Modifiers {
    [CustomEntity("EeveeHelper/NoCollideModifier")]
    public class NoCollideModifier : Entity {
        private EntityContainer container;
        private Dictionary<Entity, bool> wasCollidable = new Dictionary<Entity, bool>();

        public NoCollideModifier(EntityData data, Vector2 offset) : base(data.Position + offset) {
            Collider = new Hitbox(data.Width, data.Height);
            Depth = 1;

            Add(container = new EntityContainer(data) {
                DefaultIgnored = e => e.Get<EntityContainer>() != null,
                OnAttach = OnAttach,
                OnDetach = OnDetach
            });
        }

        private void OnAttach(Entity entity) {
            if (!wasCollidable.ContainsKey(entity))
                wasCollidable.Add(entity, entity.Collidable);
            entity.Collidable = false;
        }

        private void OnDetach(Entity entity) {
            if (wasCollidable.ContainsKey(entity))
                entity.Collidable = wasCollidable[entity];
            else
                entity.Collidable = true;
        }

        public override void Update() {
            base.Update();

            foreach (var entity in container.Contained)
                entity.Collidable = false;
        }
    }
}
