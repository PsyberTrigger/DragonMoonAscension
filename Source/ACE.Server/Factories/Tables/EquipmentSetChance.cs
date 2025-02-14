using System.Collections.Generic;

using ACE.Common;
using ACE.Entity.Enum;
using ACE.Database.Models.World;
using ACE.Server.Factories.Entity;
using ACE.Server.WorldObjects;

namespace ACE.Server.Factories.Tables
{
    public static class EquipmentSetChance
    {
        // t7 and t8 armor has a ~1/3 chance of having an equipment set
        private static readonly ChanceTable<bool> armorSetChance = new ChanceTable<bool>()
        {
            ( false, 0.6f ),
            ( true,  0.4f ),
        };

        private static readonly List<EquipmentSet> armorSets = new List<EquipmentSet>()
        {
            EquipmentSet.Soldiers,
            EquipmentSet.Adepts,
            EquipmentSet.Archers,
            EquipmentSet.Defenders,
            EquipmentSet.Tinkers,
            EquipmentSet.Crafters,
            EquipmentSet.Hearty,
            EquipmentSet.Dexterous,
            EquipmentSet.Wise,
            EquipmentSet.Swift,
            EquipmentSet.Hardened,
            EquipmentSet.Reinforced,
            EquipmentSet.Interlocking,
            EquipmentSet.Flameproof,
            EquipmentSet.Acidproof,
            EquipmentSet.Coldproof,
            EquipmentSet.Lightningproof
        };

        private static readonly ChanceTable<EquipmentSet> armorSetsRoller = new ChanceTable<EquipmentSet>()
        {
            (EquipmentSet.Soldiers,         0.080f),
            (EquipmentSet.Adepts,           0.080f),
            (EquipmentSet.Archers,          0.080f),
            (EquipmentSet.Defenders,        0.086f),
            (EquipmentSet.Tinkers,          0.035f),
            (EquipmentSet.Crafters,         0.035f),
            (EquipmentSet.Hearty,           0.070f),
            (EquipmentSet.Dexterous,        0.080f),
            (EquipmentSet.Wise,             0.080f),
            (EquipmentSet.Swift,            0.080f), // 0.706
            (EquipmentSet.Hardened,         0.042f),
            (EquipmentSet.Reinforced,       0.042f),
            (EquipmentSet.Interlocking,     0.042f),
            (EquipmentSet.Flameproof,       0.042f),
            (EquipmentSet.Acidproof,        0.042f),
            (EquipmentSet.Coldproof,        0.042f),
            (EquipmentSet.Lightningproof,   0.042f) // 0.294
        };

        public static EquipmentSet? Roll(WorldObject wo, TreasureDeath profile, TreasureRoll roll)
        {
            //if (profile.Tier < 6 || !roll.HasArmorLevel(wo))
            if (profile.Tier < 6)
                return null;

            if (wo.ClothingPriority == null || ((wo.ClothingPriority & (CoverageMask)CoverageMaskHelper.Outerwear) == 0 && (wo.ClothingPriority & (CoverageMask)CoverageMaskHelper.Underwear) == 0))
                return null;

            // loot quality mod?

            if (profile.Tier < 10)
            {
                if (!armorSetChance.Roll(profile.LootQualityMod) && profile.Tier < 10)
                        return null;
            }

            return armorSetsRoller.Roll();

            // each armor set has an even chance of being selected
            //var rng = ThreadSafeRandom.Next(0, armorSets.Count - 1);

            //return armorSets[rng];
        }
    }
}
