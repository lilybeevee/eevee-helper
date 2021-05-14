using Celeste.Mod.EeveeHelper.Components;
using Celeste.Mod.EeveeHelper.Entities;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Reflection;

namespace Celeste.Mod.EeveeHelper {
    public class EeveeHelperModule : EverestModule {
        public static EeveeHelperModule Instance { get; set; }

        public EeveeHelperModule() {
            Instance = this;
        }

        public override void Load() {
            MiscHooks.Load();
            RoomChest.Load();
            HoldableTiles.Load();
            PatientBooster.Load();
        }

        public override void Unload() {
            MiscHooks.Unload();
            RoomChest.Unload();
            HoldableTiles.Unload();
            PatientBooster.Unload();
        }
    }
}
