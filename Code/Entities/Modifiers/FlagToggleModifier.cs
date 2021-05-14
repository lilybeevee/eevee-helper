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
        private Dictionary<Entity, Tuple<bool, bool>> entityState = new Dictionary<Entity, Tuple<bool, bool>>();

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
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            if (Toggled)
                EnableEntities();
            else
                DisableEntities();
        }

        public override void Update() {
            base.Update();

            if (Toggled)
                EnableEntities();
            else
                DisableEntities();
        }

        private void DisableEntities() {
            foreach (var entity in container.Contained) {
                if (!entityState.ContainsKey(entity)) {
                    entityState.Add(entity, Tuple.Create(entity.Visible, entity.Collidable));

                    entity.Active = entity.Visible = entity.Collidable = false;
                }
            }
        }

        private void EnableEntities() {
            foreach (var entity in container.Contained) {
                EnableEntity(entity);
            }

            entityState.Clear();
        }

        private void EnableEntity(Entity entity) {
            if (entityState.ContainsKey(entity)) {
                var state = entityState[entity];

                entity.Active = true;
                entity.Visible = state.Item1;
                entity.Collidable = state.Item2;
            }
        }
    }
}
