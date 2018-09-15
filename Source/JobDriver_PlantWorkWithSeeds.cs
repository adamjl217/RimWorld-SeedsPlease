﻿using System.Collections.Generic;

using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace SeedsPlease
{
    public abstract class JobDriver_PlantWorkWithSeeds : JobDriver_PlantHarvest
    {
        float workDone;

        protected override IEnumerable<Toil> MakeNewToils ()
        {
            yield return Toils_JobTransforms.MoveCurrentTargetIntoQueue (TargetIndex.A);
            yield return Toils_Reserve.ReserveQueue (TargetIndex.A);

            var init = Toils_JobTransforms.ClearDespawnedNullOrForbiddenQueuedTargets (TargetIndex.A);

            yield return init;
            yield return Toils_JobTransforms.SucceedOnNoTargetInQueue (TargetIndex.A);
            yield return Toils_JobTransforms.ExtractNextTargetFromQueue (TargetIndex.A);

            var clear = Toils_JobTransforms.ClearDespawnedNullOrForbiddenQueuedTargets (TargetIndex.A);
            yield return Toils_Goto.GotoThing (TargetIndex.A, PathEndMode.Touch).JumpIfDespawnedOrNullOrForbidden (TargetIndex.A, clear);

            yield return HarvestSeedsToil ();
            yield return PlantWorkDoneToil ();
            yield return Toils_Jump.JumpIfHaveTargetInQueue (TargetIndex.A, init);
        }

        Toil HarvestSeedsToil ()
        {
            var toil = new Toil ();
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            toil.tickAction = delegate {
                var actor = toil.actor;
                var plant = Plant;

                if (actor.skills != null) {
                    actor.skills.Learn (SkillDefOf.Plants, xpPerTick, true);
                }

                workDone += actor.GetStatValue (StatDefOf.PlantWorkSpeed, true);
                if (workDone >= plant.def.plant.harvestWork) {
                    if (plant.def.plant.harvestedThingDef != null) {
                        if (actor.RaceProps.Humanlike && plant.def.plant.harvestFailable && Rand.Value > actor.GetStatValue (StatDefOf.PlantHarvestYield, true)) {
                            MoteMaker.ThrowText ((actor.DrawPos + plant.DrawPos) / 2f, actor.Map, "TextMote_HarvestFailed".Translate (), 3.65f);
                        } else {
                            int plantYield = plant.YieldNow ();

                            ThingDef harvestedThingDef;

                            var seedDef = plant.def.blueprintDef as SeedDef;
                            if (seedDef != null) {
                                var minGrowth = plant.def.plant.harvestMinGrowth;

                                float parameter;
                                if (minGrowth < 0.9f) {
                                    parameter = Mathf.InverseLerp (minGrowth, 0.9f, plant.Growth);
                                } else if (minGrowth < plant.Growth) {
                                    parameter = 1f;
                                } else {
                                    parameter = 0f;
                                }
                                parameter = Mathf.Min (parameter, 1f);

                                if (seedDef.seed.seedFactor > 0 && Rand.Value < seedDef.seed.baseChance * parameter) {
                                    int count;
                                    if (Rand.Value < seedDef.seed.extraChance) {
                                        count = 2;
                                    } else {
                                        count = 1;
                                    }

                                    Thing seeds = ThingMaker.MakeThing (seedDef, null);
                                    seeds.stackCount = Mathf.RoundToInt (seedDef.seed.seedFactor * count);

                                    GenPlace.TryPlaceThing (seeds, actor.Position, actor.Map, ThingPlaceMode.Near);
                                }

                                plantYield = Mathf.RoundToInt (plantYield * seedDef.seed.harvestFactor);

                                harvestedThingDef = seedDef.harvest;
                            } else {
                                harvestedThingDef = plant.def.plant.harvestedThingDef;
                            }

                            if (plantYield > 0) {
                                var thing = ThingMaker.MakeThing (harvestedThingDef, null);
                                thing.stackCount = plantYield;
                                if (actor.Faction != Faction.OfPlayer) {
                                    thing.SetForbidden (true, true);
                                }
                                GenPlace.TryPlaceThing (thing, actor.Position, actor.Map, ThingPlaceMode.Near, null);
                                actor.records.Increment (RecordDefOf.PlantsHarvested);
                            }
                        }
                    }
                    plant.def.plant.soundHarvestFinish.PlayOneShot (actor);
                    plant.PlantCollected ();
                    workDone = 0;
                    ReadyForNextToil ();
                    return;
                }
            };

            toil.FailOnDespawnedNullOrForbidden (TargetIndex.A);
            toil.WithEffect (EffecterDefOf.Harvest, TargetIndex.A);
            toil.WithProgressBar (TargetIndex.A, () => workDone / Plant.def.plant.harvestWork, true, -0.5f);
            toil.PlaySustainerOrSound (() => Plant.def.plant.soundHarvesting);

            return toil;
        }
    }
}
