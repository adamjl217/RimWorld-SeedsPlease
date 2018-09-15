using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;

namespace SeedsPlease
{
    public class Seed : ThingWithComps
    {
        public override IEnumerable<StatDrawEntry> SpecialDisplayStats() {

            foreach(StatDrawEntry s in base.SpecialDisplayStats())
            {
                yield return s;
            }

            var seedDef = def as SeedDef;
            if (seedDef != null)
            {
                StatDrawEntry[] extraStats = {
                        new StatDrawEntry (StatCategoryDefOf.PawnWork, ResourceBank.StringPlantMinFertlity, seedDef.plant.plant.fertilityMin.ToString ("P0"), 5),
                        new StatDrawEntry (StatCategoryDefOf.PawnMisc, ResourceBank.StringHarvestMultiplier , seedDef.seed.harvestFactor.ToString ("P0"), 4),
                        new StatDrawEntry (StatCategoryDefOf.PawnMisc, ResourceBank.StringSeedMultiplier, seedDef.seed.seedFactor.ToString ("P0"), 3),
                        new StatDrawEntry (StatCategoryDefOf.PawnMisc, ResourceBank.StringSeedBaseChance, seedDef.seed.baseChance.ToString ("P0"), 2),
                        new StatDrawEntry (StatCategoryDefOf.PawnMisc, ResourceBank.StringSeedExtraChance, seedDef.seed.extraChance.ToString ("P0"), 1),
                    };
                foreach (StatDrawEntry statDrawEntry in extraStats){
                    yield return statDrawEntry;
                }
            }
        }

        public override string GetInspectString ()
        {
            var inspectString = base.GetInspectString ();

            var seedDef = def as SeedDef;
            if (seedDef != null && seedDef.plant.plant.fertilityMin > 1.0f) {
                inspectString += ResourceBank.StringPlantMinFertlity + " : " + seedDef.plant.plant.fertilityMin.ToString ("P0");
            }
            return inspectString;
        }
    }
}

