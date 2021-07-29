using Celeste.Mod.AdventureHelper.Entities;
using Celeste.Mod.EeveeHelper.Components;
using Celeste.Mod.EeveeHelper.Handlers;
using Celeste.Mod.EeveeHelper.Handlers.Impl.Compat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.EeveeHelper.Compat {
    public class AdventureHelperCompat {
        public static Type multiNodeTrackSpinnerType;

        public static void Initialize() {
            multiNodeTrackSpinnerType = typeof(LinkedZipMover).Assembly.GetType("Celeste.Mod.AdventureHelper.Entities.MultipleNodeTrackSpinner");

            EntityHandler.RegisterInherited(multiNodeTrackSpinnerType, (entity, container) => new MultipleNodeTrackSpinnerHandler(entity));
        }
    }
}
