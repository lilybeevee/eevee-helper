using Celeste.Mod.EeveeHelper.Compat;
using Celeste.Mod.EeveeHelper.Components;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.EeveeHelper.Handlers.Impl {
    class WaterHandler : EntityHandler, IMoveable {
        private DynData<Water> entityData;

        public WaterHandler(Entity entity) : base(entity) {
            entityData = new DynData<Water>(entity as Water);
        }

        public bool Move(Vector2 move, Vector2? liftSpeed) {
            if (!(Container as EntityContainerMover).IgnoreAnchors) {
                var surfaces = entityData.Get<List<Water.Surface>>("Surfaces");
                for (int i = 0; i < (surfaces.Count); i++) {
                    surfaces[i].Position += move;
                }
                entityData.Set("Surfaces", surfaces);
            }
            return false;
        }

        public void PreMove() {
        }
    }
}
