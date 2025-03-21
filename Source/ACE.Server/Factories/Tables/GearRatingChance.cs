using log4net;

using ACE.Database.Models.World;
using ACE.Server.Factories.Entity;
using ACE.Server.WorldObjects;
using ACE.Common;
using ACE.Entity.Enum.Properties;
using Microsoft.Extensions.Options;

namespace ACE.Server.Factories.Tables
{
    public static class GearRatingChance
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        private static ChanceTable<bool> RatingChance = new ChanceTable<bool>()
        {
            ( false, 0.6f ),
            ( true,  0.4f )
        };

        private static ChanceTable<int> RatingScale = new ChanceTable<int>()
        {
            ( 1, 0.20f ),
            ( 2, 0.20f ),
            ( 3, 0.15f ),
            ( 4, 0.15f ),
            ( 5, 0.075f ),
            ( 6, 0.075f ),
            ( 7, 0.0575f ),
            ( 8, 0.055f ),
            ( 9, 0.025f ),
            ( 10, 0.0125f )
        };

        private static ChanceTable<int> TierRatingMod9 = new ChanceTable<int>()
        {
            ( 0, 0.025f ),
            ( 1, 0.075f ),
            ( 2, 0.425f ),
            ( 3, 0.425f ),
            ( 4, 0.025f ),
            ( 5, 0.025f )
        };

        private static ChanceTable<int> TierRatingMod10 = new ChanceTable<int>()
        {
            ( 9, 0.8f ),
            ( 10, 0.065f ),
            ( 11, 0.045f ),
            ( 12, 0.040f ),
            ( 13, 0.0375f ),
            ( 14, 0.0125f )
        };

        private static ChanceTable<int> TierRatingMod11 = new ChanceTable<int>()
        {
            ( 15, 0.8f ),
            ( 16, 0.065f ),
            ( 17, 0.045f ),
            ( 18, 0.040f ),
            ( 19, 0.0375f ),
            ( 20, 0.0125f )
        };

        private static ChanceTable<int> TierRatingMod12 = new ChanceTable<int>()
        {
            ( 20, 0.60f ),
            ( 22, 0.15f ),
            ( 24, 0.125f ),
            ( 26, 0.075f ),
            ( 28, 0.0375f ),
            ( 30, 0.0125f )
        };

        private static ChanceTable<int> TierRatingMod13 = new ChanceTable<int>()
        {
            ( 32, 0.60f ),
            ( 34, 0.15f ),
            ( 36, 0.125f ),
            ( 38, 0.075f ),
            ( 40, 0.0375f ),
            ( 45, 0.0125f )
        };

        private static ChanceTable<int> TierRatingMod14 = new ChanceTable<int>()
        {
            ( 50, 0.60f ),
            ( 55, 0.15f ),
            ( 60, 0.125f ),
            ( 65, 0.075f ),
            ( 70, 0.0375f ),
            ( 75, 0.0125f )
        };

        private static ChanceTable<int> TierRatingMod15 = new ChanceTable<int>()
        {
            ( 80, 0.60f ),
            ( 85, 0.15f ),
            ( 90, 0.125f ),
            ( 95, 0.075f ),
            ( 100, 0.0375f ),
            ( 105, 0.0125f )
        };

        public static int Roll(WorldObject wo, TreasureDeath profile, TreasureRoll roll)
        {
            // initial roll for rating chance
            if (!RatingChance.Roll(profile.LootQualityMod) && profile.Tier < 9)
                return 0;

            // roll for the actual rating
            ChanceTable<int> rating = null;

            ChanceTable<int> ratingTierMod = null;


            if (roll.HasArmorLevel(wo)|| roll.IsClothing || roll.IsJewelry || roll.IsCloak)
            {
                rating = RatingScale;

                switch (profile.Tier)
                {
                    case 9:
                        ratingTierMod = TierRatingMod9;
                        break;
                    case 10:
                        ratingTierMod = TierRatingMod10;
                        break;
                    case 11:
                        ratingTierMod = TierRatingMod11;
                        break;
                    case 12:
                        ratingTierMod = TierRatingMod12;
                        break;
                    case 13:
                        ratingTierMod = TierRatingMod13;
                        break;
                    case 14:
                        ratingTierMod = TierRatingMod14;
                        break;
                    case 15:
                        ratingTierMod = TierRatingMod15;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                log.Error($"GearRatingChance.Roll({wo.Name}, {profile.TreasureType}, {roll.ItemType}): unknown item type");
                return 0;
            }


            if (ratingTierMod != null)
            {
                var tratings = (rating.Roll(profile.LootQualityMod) + ratingTierMod.Roll(profile.LootQualityMod));
                return tratings;
            }
            else
            {
                return rating.Roll(profile.LootQualityMod);
            }
        }
    }
}
