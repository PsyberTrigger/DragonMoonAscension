using System.Collections.Generic;

using ACE.Database.Models.World;
using ACE.Server.Factories.Entity;

namespace ACE.Server.Factories.Tables.Wcids
{
    public static class AetheriaChance
    {
        private static ChanceTable<int> T5_ItemMaxLevel = new ChanceTable<int>()
        {
            ( 1, 0.60f ),
            ( 2, 0.40f ),
        };

        private static ChanceTable<int> T6_ItemMaxLevel = new ChanceTable<int>()
        {
            ( 1, 0.350f ),
            ( 2, 0.405f ),
            ( 3, 0.240f ),
            ( 4, 0.005f ),
        };

        // lack of data samples here for 4+
        private static ChanceTable<int> T7_ItemMaxLevel = new ChanceTable<int>()
        {
            ( 3, 0.60f ),
            ( 4, 0.35f ),
            ( 5, 0.05f ),
        };

        // also lack of data samples for level 5,
        // there was possibly no indication it was more common than t7
        private static ChanceTable<int> T8_ItemMaxLevel = new ChanceTable<int>()
        {
            ( 3, 0.40f ),
            ( 4, 0.45f ),
            ( 5, 0.15f ),
        };

        private static ChanceTable<int> T9_ItemMaxLevel = new ChanceTable<int>()
        {
            ( 4, 0.747f ),
            ( 5, 0.250f ),
            ( 7, 0.003f ),
        };

        private static ChanceTable<int> T10_ItemMaxLevel = new ChanceTable<int>()
        {
            ( 4, 0.600f ),
            ( 5, 0.250f ),
            ( 7, 0.125f ),
            ( 9, 0.025f ),
        };

        private static ChanceTable<int> T11_ItemMaxLevel = new ChanceTable<int>()
        {
            (  7, 0.465f ),
            (  9, 0.350f ),
            ( 11, 0.125f ),
            ( 14, 0.040f ),
            ( 15, 0.020f ),
        };

        private static ChanceTable<int> T12_ItemMaxLevel = new ChanceTable<int>()
        {
            (  7, 0.500f ),
            (  9, 0.430f ),
            ( 11, 0.035f ),
            ( 15, 0.035f ),
        };

        private static readonly List<ChanceTable<int>> itemMaxLevels = new List<ChanceTable<int>>()
        {
            T5_ItemMaxLevel,
            T6_ItemMaxLevel,
            T7_ItemMaxLevel,
            T8_ItemMaxLevel,
            T9_ItemMaxLevel,
            T10_ItemMaxLevel,
            T11_ItemMaxLevel,
            T12_ItemMaxLevel,
        };

        public static int Roll_ItemMaxLevel(TreasureDeath profile)
        {
            if (profile.Tier < 5)
                return 0;

            var table = itemMaxLevels[profile.Tier - 5];

            return table.Roll(profile.LootQualityMod);
        }
    }
}
