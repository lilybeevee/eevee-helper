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
    [CustomEntity("EeveeHelper/FlagToggleModifier")]
    public class FlagToggleModifier : Entity {
        public bool Toggled => string.IsNullOrEmpty(flag) || SceneAs<Level>().Session.GetFlag(flag) != notFlag;

        private string flag;
        private bool notFlag;

        private EntityContainer container;
        private Dictionary<Entity, EntityState> entityStates = new Dictionary<Entity, EntityState>();

        public FlagToggleModifier(EntityData data, Vector2 offset) : base(data.Position + offset) {
            Collider = new Hitbox(data.Width, data.Height);
            Depth = Depths.Top - 9;

            var parsedFlag = EeveeUtils.ParseFlagAttr(data.Attr("flag"));
            flag = parsedFlag.Item1;
            notFlag = parsedFlag.Item2;

            Add(container = new EntityContainer(data) {
                DefaultIgnored = e => e.Get<EntityContainer>() != null,
                OnDetach = EnableEntity
            });

            Add(new TransitionListener {
                OnIn = (f) => CheckToggled()
            });
        }

        public override void Update() {
            base.Update();
            CheckToggled();
        }

        private void CheckToggled() {
            if (Toggled)
                EnableEntities();
            else
                DisableEntities();
        }

        private void DisableEntities() {
            foreach (var entity in container.Contained) {
                if (!entityStates.ContainsKey(entity))
                    entityStates.Add(entity, new EntityState(entity));
                EntityState.Disable(entity);
            }
        }

        private void EnableEntities() {
            foreach (var entity in container.Contained)
                EnableEntity(entity);

            entityStates.Clear();
        }

        private void EnableEntity(Entity entity) {
            if (entityStates.ContainsKey(entity))
                entityStates[entity].Apply(entity);
        }

        private struct EntityState {
            bool Active;
            bool Visible;
            bool Collidable;

            bool TalkComponentEnabled;

            public EntityState(Entity entity) {
                Active = entity.Active;
                Visible = entity.Visible;
                Collidable = entity.Collidable;

                var talkComponent = entity.Get<TalkComponent>();
                if (talkComponent != null)
                    TalkComponentEnabled = talkComponent.Enabled;
                else
                    TalkComponentEnabled = false;
            }

            public void Apply(Entity entity) {
                entity.Active = Active;
                entity.Visible = Visible;
                entity.Collidable = Collidable;

                var talkComponent = entity.Get<TalkComponent>();
                if (talkComponent != null)
                    talkComponent.Enabled = TalkComponentEnabled;
            }

            public static void Disable(Entity entity) {
                entity.Active = false;
                entity.Visible = false;
                entity.Collidable = false;

                var talkComponent = entity.Get<TalkComponent>();
                if (talkComponent != null)
                    talkComponent.Enabled = false;
            }
        }
    }
}
