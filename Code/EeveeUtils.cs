using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.EeveeHelper {
    public static class EeveeUtils {
        internal static MethodInfo m_SpringBounceAnimate = typeof(Spring).GetMethod("BounceAnimate", BindingFlags.NonPublic | BindingFlags.Instance);

        public static Vector2 GetPosition(Entity entity) => entity is Platform platform ? platform.ExactPosition : entity.Position;
    }
}
