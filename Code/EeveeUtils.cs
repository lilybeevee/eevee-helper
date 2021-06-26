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

        public static Vector2 GetPosition(Entity entity) =>
            entity is Platform platform ? platform.ExactPosition : entity.Position;

        public static Vector2 GetTrackBoost(Vector2 move, bool disableBoost) {
            return move * new Vector2(disableBoost ? 0f : 1f, 1f) + (move.X != 0f && move.Y == 0f && disableBoost ? Vector2.UnitY * 0.01f : Vector2.Zero);
        }

        public static Tuple<string, bool> ParseFlagAttr(string flag)
            => flag.StartsWith("!") ? Tuple.Create(flag.Substring(1), true) : Tuple.Create(flag, false);
    }
}
