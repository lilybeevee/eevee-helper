using Celeste.Mod.EeveeHelper.Components;
using Celeste.Mod.EeveeHelper.Entities;
using Celeste.Mod.EeveeHelper.Entities.Modifiers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.EeveeHelper {
    public static class MiscHooks {
        private static ILHook levelLoadHook;

        public static void Load() {
            On.Monocle.Collide.Check_Entity_Entity += Collide_Check_Entity_Entity;
            On.Celeste.Actor.MoveHExact += Actor_MoveHExact;
            On.Celeste.Actor.MoveVExact += Actor_MoveVExact;
            On.Celeste.Actor.OnGround_int += Actor_OnGround_int;
            IL.Celeste.Holdable.Release += Holdable_Release;
            IL.Celeste.MapData.ParseBackdrop += MapData_ParseBackdrop;
            IL.Monocle.EntityList.UpdateLists += EntityList_UpdateLists;

            levelLoadHook = new ILHook(typeof(Level).GetMethod("orig_LoadLevel", BindingFlags.Public | BindingFlags.Instance), Level_orig_LoadLevel);
        }

        public static void Unload() {
            On.Monocle.Collide.Check_Entity_Entity -= Collide_Check_Entity_Entity;
            On.Celeste.Actor.MoveHExact -= Actor_MoveHExact;
            On.Celeste.Actor.MoveVExact -= Actor_MoveVExact;
            On.Celeste.Actor.OnGround_int -= Actor_OnGround_int;
            IL.Celeste.Holdable.Release -= Holdable_Release;
            IL.Celeste.MapData.ParseBackdrop -= MapData_ParseBackdrop;
            IL.Monocle.EntityList.UpdateLists -= EntityList_UpdateLists;

            levelLoadHook?.Dispose();
        }

        private static HashSet<EntityData> GlobalModifiedData = new HashSet<EntityData>();
        private static int LastLoadedGlobalModified;
        private static bool LoadingGlobalModified;
        private static int LoadingGlobalTags;

        private static void EntityList_UpdateLists(ILContext il) {
            var cursor = new ILCursor(il);

            var addedEntityLoc = -1;
            if (cursor.TryGotoNext(MoveType.Before,
                instr => instr.MatchLdloc(out addedEntityLoc),
                instr => true,
                instr => true,
                instr => instr.MatchCallvirt<Entity>("Added"))) {

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldloc, addedEntityLoc);
                cursor.EmitDelegate<Action<EntityList, Entity>>((entityList, entity) => {
                    var component = entity.Get<TagAdderComponent>();
                    if (component != null) {
                        LoadingGlobalModified = true;
                        LoadingGlobalTags = component.Tags;
                        LastLoadedGlobalModified = entityList.ToAdd.Count;
                    }
                });

                cursor.Index += 4;

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Action<EntityList>>(entityList => {
                    if (LoadingGlobalModified) {
                        foreach (var entity in entityList.ToAdd.Skip(LastLoadedGlobalModified))
                            entity.Add(new TagAdderComponent(LoadingGlobalTags));
                        LastLoadedGlobalModified = 0;
                        LoadingGlobalModified = false;
                    }
                });

                Logger.Log("EeveeHelper", "Added IL Hook for EntityList.UpdateLists (Added)");
            }

            var awakeEntityLoc = -1;
            if (cursor.TryGotoNext(MoveType.Before,
                instr => instr.MatchLdloc(out awakeEntityLoc),
                instr => true,
                instr => true,
                instr => instr.MatchCallvirt<Entity>("Awake"))) {

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldloc, addedEntityLoc);
                cursor.EmitDelegate<Action<EntityList, Entity>>((entityList, entity) => {
                    var component = entity.Get<TagAdderComponent>();
                    if (component != null) {
                        LoadingGlobalModified = true;
                        LoadingGlobalTags = component.Tags;
                        LastLoadedGlobalModified = entityList.ToAdd.Count;
                    }
                });

                cursor.Index += 4;

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Action<EntityList>>(entityList => {
                    if (LoadingGlobalModified) {
                        foreach (var entity in entityList.ToAdd.Skip(LastLoadedGlobalModified))
                            entity.Add(new TagAdderComponent(LoadingGlobalTags));
                        LastLoadedGlobalModified = 0;
                        LoadingGlobalModified = false;
                    }
                });

                Logger.Log("EeveeHelper", "Added IL Hook for EntityList.UpdateLists (Awake)");
            }
        }

        private static void Level_orig_LoadLevel(ILContext il) {
            var cursor = new ILCursor(il);

            HookLevelEntityLoading(cursor, "Entities");
            HookLevelEntityLoading(cursor, "Triggers");
        }

        private static void HookLevelEntityLoading(ILCursor cursor, string type) {
            if (!cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<LevelData>(type)))
                return;

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldarg_2);
            cursor.EmitDelegate<Func<List<EntityData>, Level, bool, List<EntityData>>>((list, self, isFromLoader) => {
                if (isFromLoader) {
                    if (type == "Entities")
                        GlobalModifiedData.Clear();
                    var newList = new List<EntityData>(list);
                    foreach (var level in self.Session.MapData.Levels) {
                        foreach (var modifier in level.Entities.Where(data => data.Name == GlobalModifier.ID)) {
                            var whitelist = modifier.Attr("whitelist").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            var contain = new Rectangle((int)modifier.Position.X, (int)modifier.Position.Y, modifier.Width, modifier.Height);
                            int tags = Tags.Global;
                            if (modifier.Bool("frozenUpdate")) tags |= Tags.FrozenUpdate;
                            if (modifier.Bool("pauseUpdate")) tags |= Tags.PauseUpdate;
                            if (modifier.Bool("transitionUpdate")) tags |= Tags.TransitionUpdate;

                            foreach (var entity in (type == "Entities" ? level.Entities : level.Triggers)) {
                                if (entity != modifier && (whitelist.Length == 0 || whitelist.Any(str => entity.Name == str || entity.ID.ToString() == str))) {
                                    var matched = false;
                                    if (entity.Width != 0 && entity.Height != 0) {
                                        var rect = new Rectangle((int)entity.Position.X, (int)entity.Position.Y, entity.Width, entity.Height);
                                        matched = contain.Intersects(rect);
                                    } else {
                                        matched = contain.Contains((int)entity.Position.X, (int)entity.Position.Y);
                                    }

                                    if (matched) {
                                        if (newList.Contains(entity))
                                            newList.Remove(entity);
                                        GlobalModifiedData.Add(entity);

                                        var data = EeveeUtils.CloneEntityData(entity, self.Session.LevelData);
                                        data.Values["globalModifierSpawned"] = tags;

                                        newList.Add(data);
                                    }
                                }
                            }
                        }
                    }
                    return newList;
                } else if (GlobalModifiedData.Count > 0) {
                    return list.Except(GlobalModifiedData).ToList();
                } else {
                    return list;
                }
            });
            Logger.Log("EeveeHelper", $"Added IL Hook for Level.orig_LoadLevel ({type} - 1)");

            if (!cursor.TryGotoNext(MoveType.Before, instr => instr.MatchLdfld<EntityData>("ID")))
                return;

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<EntityData, Level, EntityData>>((entityData, self) => {
                var tags = 0;
                if ((tags = entityData.Int("globalModifierSpawned", -1)) != -1) {
                    LoadingGlobalModified = true;
                    LoadingGlobalTags = tags;
                    LastLoadedGlobalModified = self.Entities.ToAdd.Count;
                }
                return entityData;
            });
            Logger.Log("EeveeHelper", $"Added IL Hook for Level.orig_LoadLevel ({type} - 2)");

            if (!cursor.TryGotoNext(MoveType.Before, instr => instr.MatchBrtrue(out _), instr => instr.MatchLeaveS(out _)))
                return;

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Action<Level>>(self => {
                if (LoadingGlobalModified) {
                    foreach (var entity in self.Entities.ToAdd.Skip(LastLoadedGlobalModified))
                        entity.Add(new TagAdderComponent(LoadingGlobalTags));
                    LastLoadedGlobalModified = 0;
                    LoadingGlobalModified = false;
                }
            });
            Logger.Log("EeveeHelper", $"Added IL Hook for Level.orig_LoadLevel ({type} - 3)");
        }

        private static bool Collide_Check_Entity_Entity(On.Monocle.Collide.orig_Check_Entity_Entity orig, Entity a, Entity b) {
            if ((a is CollidableModifier.Solidifier aSolid && aSolid.Entity == b) ||
                (b is CollidableModifier.Solidifier bSolid && bSolid.Entity == a)) {

                return false;
            }
            var aContainer = a.Get<EntityContainer>();
            var bContainer = b.Get<EntityContainer>();
            if ((aContainer != null && !aContainer.CollideWithContained && aContainer.Contained.Contains(b)) ||
                (bContainer != null && !bContainer.CollideWithContained && bContainer.Contained.Contains(a))) {

                return false;
            }
            return orig(a, b);
        }

        private static bool Actor_MoveHExact(On.Celeste.Actor.orig_MoveHExact orig, Actor self, int moveH, Collision onCollide, Solid pusher) {
            var selfData = new DynData<Entity>(self);
            var result = true;

            var solidified = selfData.Get<CollidableModifier.Solidifier>("solidModifierSolidifier");
            var solidifierCollidable = solidified?.Collidable ?? true;
            if (solidified != null)
                solidified.Collidable = false;

            if (self is HoldableContainer container) {
                var collidable = self.Collidable;
                container.Container.DoMoveAction(() => {
                    result = orig(self, moveH, onCollide, pusher);
                }, (entity, offset) => {
                    if (entity is Platform platform) {
                        platform.MoveToX(self.Position.X + offset.X, platform.LiftSpeed.X + container.Speed.X);
                    } else {
                        entity.Position = self.Position + offset;
                    }
                });
            } else {
                result = orig(self, moveH, onCollide, pusher);
            }

            if (solidified != null) {
                solidified.Collidable = solidifierCollidable;
                solidified.MoveTo(self.ExactPosition);
            }

            return result;
        }

        private static bool Actor_MoveVExact(On.Celeste.Actor.orig_MoveVExact orig, Actor self, int moveV, Collision onCollide, Solid pusher) {
            var selfData = new DynData<Entity>(self);
            var result = true;

            var solidified = selfData.Get<CollidableModifier.Solidifier>("solidModifierSolidifier");
            var solidifierCollidable = solidified?.Collidable ?? true;
            if (solidified != null)
                solidified.Collidable = false;

            if (self is HoldableContainer container) {
                var collidable = self.Collidable;
                container.Container.DoMoveAction(() => {
                    result = orig(self, moveV, onCollide, pusher);
                }, (entity, offset) => {
                    if (entity is Platform platform) {
                        platform.MoveToY(self.Position.Y + offset.Y, platform.LiftSpeed.Y + container.Speed.Y);
                    } else {
                        entity.Position.Y = self.Position.Y + offset.Y;
                    }
                });
            } else {
                result = orig(self, moveV, onCollide, pusher);
            }

            if (solidified != null) {
                solidified.Collidable = solidifierCollidable;
                var collidable = self.Collidable;
                var pushable = self.AllowPushing;
                self.Collidable = false;
                self.AllowPushing = false;
                solidified.MoveTo(self.ExactPosition);
                self.Collidable = collidable;
                self.AllowPushing = pushable;
            }

            return result;
        }

        private static bool Actor_OnGround_int(On.Celeste.Actor.orig_OnGround_int orig, Actor self, int downCheck) {
            var selfData = new DynData<Actor>(self);
            bool result = true;

            var solidified = selfData.Get<CollidableModifier.Solidifier>("solidModifierSolidifier");
            var solidifierCollidable = solidified?.Collidable ?? true;
            if (solidified != null)
                solidified.Collidable = false;

            if (self is HoldableContainer container) {
                container.Container.DoIgnoreCollision(() => {
                    result = orig(self, downCheck);
                });
            } else {
                result = orig(self, downCheck);
            }

            if (solidified != null)
                solidified.Collidable = solidifierCollidable;

            return result;
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
