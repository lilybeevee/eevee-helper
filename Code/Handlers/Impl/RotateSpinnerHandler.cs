using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.EeveeHelper.Handlers.Impl {
    public class RotateSpinnerHandler : EntityHandler, IAnchorProvider {
        public RotateSpinnerHandler(Entity entity) : base(entity) {
        }

        public List<string> GetAnchors() => new List<string>{ "center" };
    }
}
