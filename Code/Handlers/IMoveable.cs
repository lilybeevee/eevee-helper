using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.EeveeHelper.Handlers {
    public interface IMoveable {
        void PreMove();

        bool Move(Vector2 move, Vector2? liftSpeed);
    }
}
