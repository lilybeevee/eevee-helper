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

namespace Celeste.Mod.EeveeHelper.Handlers.Impl.Compat {
    class MultipleNodeTrackSpinnerHandler : EntityHandler, IMoveable {
        private DynamicData entityData;

        public MultipleNodeTrackSpinnerHandler(Entity entity) : base(entity) {
            entityData = new DynamicData(AdventureHelperCompat.multiNodeTrackSpinnerType, entity);
        }

        public bool Move(Vector2 move, Vector2? liftSpeed) {
            if (!(Container as EntityContainerMover).IgnoreAnchors) {
                var path = entityData.Get<Vector2[]>("Path");
                for (int i = 0; i < path.Length; i++) {
                    path[i] += move;
                }
                entityData.Set("Path", path);
            }
            return false;
        }

        public void PreMove() {
        }
    }
}
