using Celeste.Mod.EeveeHelper.Components;
using Celeste.Mod.EeveeHelper.Handlers;
using Celeste.Mod.EeveeHelper.Handlers.Impl.Compat;
using Celeste.Mod.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.EeveeHelper.Compat {
    public class AdventureHelperCompat {
        public static Type multiNodeTrackSpinnerType;

        public static void Initialize() {
            multiNodeTrackSpinnerType = FakeAssembly.GetFakeEntryAssembly().GetType("Celeste.Mod.AdventureHelper.Entities.MultipleNodeTrackSpinner");

            EntityHandler.RegisterInherited(multiNodeTrackSpinnerType, (entity, container) => new MultipleNodeTrackSpinnerHandler(entity));
        }
    }
}
