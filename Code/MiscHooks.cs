using Celeste.Mod.EeveeHelper.Entities;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.EeveeHelper {
    public static class MiscHooks {
        public static void Load() {
            IL.Celeste.Holdable.Release += Holdable_Release;
            IL.Celeste.MapData.ParseBackdrop += MapData_ParseBackdrop;
        }

        public static void Unload() {
            IL.Celeste.Holdable.Release -= Holdable_Release;
            IL.Celeste.MapData.ParseBackdrop -= MapData_ParseBackdrop;
        }

        private static void Holdable_Release(ILContext il) {
            var cursor = new ILCursor(il);

            ILLabel endLabel = null;
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchBrfalse(out endLabel))) {
                Logger.Log("EeveeHelper", "Added IL hook at Holdable.Release");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<Holdable, bool>>(self => self.Entity is HoldableContainer || self.Entity is HoldableTiles);
                cursor.Emit(OpCodes.Brtrue_S, endLabel);
            }
        }


        private static BlendState MultiplyBlend = new BlendState {
            ColorBlendFunction = BlendFunction.Add,
            ColorSourceBlend = Blend.DestinationColor,
            ColorDestinationBlend = Blend.Zero
        };

        private static BlendState ReverseSubtractBlend = new BlendState {
            ColorSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.One,
            ColorBlendFunction = BlendFunction.Subtract,
            AlphaSourceBlend = Blend.One,
            AlphaDestinationBlend = Blend.One,
            AlphaBlendFunction = BlendFunction.Add
        };

        private static ConstructorInfo m_ParallaxCtor 
            = typeof(Parallax).GetConstructor(new Type[] { typeof(MTexture) });

        private static void MapData_ParseBackdrop(ILContext il) {
            var cursor = new ILCursor(il);

            int parallaxLoc = -1;
            if (cursor.TryGotoNext(instr => instr.MatchNewobj(m_ParallaxCtor))) {
                if (cursor.TryGotoNext(instr => instr.MatchStloc(out parallaxLoc))) {
                    Logger.Log("EeveeHelper", "Passed first for MapData.ParseBackdrop");

                    int textLoc = 0;
                    if (cursor.TryGotoNext(MoveType.AfterLabel, instr => instr.MatchLdloc(out textLoc), instr => instr.MatchLdstr("additive"))) {
                        Logger.Log("EeveeHelper", "Added IL hook at MapData.ParseBackdrop");

                        cursor.Emit(OpCodes.Ldloc, parallaxLoc);
                        cursor.Emit(OpCodes.Ldloc, textLoc);
                        cursor.EmitDelegate<Action<Parallax, string>>((parallax, text) => {
                            if (text == "multiply") {
                                parallax.BlendState = MultiplyBlend;
                            } else if (text == "subtract") {
                                parallax.BlendState = GFX.Subtract;
                            } else if (text == "reversesubtract") {
                                parallax.BlendState = ReverseSubtractBlend;
                            }
                        });
                    }
                }
            }
        }
    }
}
