using Celeste.Mod.EeveeHelper.Components;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.EeveeHelper.Handlers.Impl {
    public class TrackSpinnerHandler : EntityHandler, IAnchorProvider {
        public TrackSpinnerHandler(Entity entity) : base(entity) {
        }

        public List<string> GetAnchors() => new List<string>{ "Start", "End" };
    }
}
