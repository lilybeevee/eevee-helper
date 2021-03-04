using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.EeveeHelper {
    public static class EeveeUtils {
        public static Vector2 GetPosition(Entity entity) => entity is Platform platform ? platform.ExactPosition : entity.Position;
    }
}
