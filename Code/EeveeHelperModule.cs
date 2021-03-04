using Celeste.Mod.EeveeHelper.Components;
using Celeste.Mod.EeveeHelper.Entities;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;

namespace Celeste.Mod.EeveeHelper {
    public class EeveeHelperModule : EverestModule {
        public static EeveeHelperModule Instance { get; set; }

        public EeveeHelperModule() {
            Instance = this;
        }

        public override void Load() {
            RoomChest.Load();
            EntityContainer.Load();
            HoldableTiles.Load();
            HoldableContainer.Load();

            IL.Celeste.Holdable.Release += this.Holdable_Release;
        }

        public override void Unload() {
            RoomChest.Unload();
            EntityContainer.Unload();
            HoldableTiles.Unload();
            HoldableContainer.Unload();

            IL.Celeste.Holdable.Release -= this.Holdable_Release;
        }

        private void Holdable_Release(ILContext il) {
            var cursor = new ILCursor(il);

            ILLabel endLabel = null;
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchBrfalse(out endLabel))) {
                Logger.Log("EeveeHelper", "Added IL hook at Holdable.Release");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<Holdable, bool>>(self => self.Entity is HoldableContainer || self.Entity is HoldableTiles);
                cursor.Emit(OpCodes.Brtrue_S, endLabel);
            }
        }
    }
}
