using ACE.Common;

namespace ACE.Server.Factories.Entity
{
    public static class MissileMagicDefense
    {
        // WeaponMissileDefense / WeaponMagicDefense

        private static readonly ChanceTable<float> T1_T6_Defense = new ChanceTable<float>()
        {
            ( 1.005f, 0.10f ),
            ( 1.010f, 0.20f ),
            ( 1.015f, 0.40f ),
            ( 1.020f, 0.20f ),
            ( 1.025f, 0.10f ),
        };

        private static readonly ChanceTable<float> T7_T8_Defense = new ChanceTable<float>()
        {
            ( 1.005f, 0.05f ),
            ( 1.010f, 0.10f ),
            ( 1.015f, 0.15f ),
            ( 1.020f, 0.20f ),
            ( 1.025f, 0.20f ),
            ( 1.030f, 0.15f ),
            ( 1.035f, 0.10f ),
            ( 1.040f, 0.05f ),
        };

        private static readonly ChanceTable<float> T9_Defense = new ChanceTable<float>()
        {
            ( 1.005f, 0.05f ),
            ( 1.010f, 0.05f ),
            ( 1.015f, 0.15f ),
            ( 1.020f, 0.15f ),
            ( 1.025f, 0.20f ),
            ( 1.030f, 0.20f ),
            ( 1.035f, 0.10f ),
            ( 1.040f, 0.10f ),
        };

        private static readonly ChanceTable<float> T10_Defense = new ChanceTable<float>()
        {
            ( 1.015f, 0.05f ),
            ( 1.020f, 0.05f ),
            ( 1.025f, 0.15f ),
            ( 1.030f, 0.15f ),
            ( 1.035f, 0.20f ),
            ( 1.040f, 0.20f ),
            ( 1.045f, 0.10f ),
            ( 1.050f, 0.10f ),
        };

        private static readonly ChanceTable<float> T11_Defense = new ChanceTable<float>()
        {
            ( 1.045f, 0.05f ),
            ( 1.050f, 0.10f ),
            ( 1.055f, 0.15f ),
            ( 1.060f, 0.20f ),
            ( 1.065f, 0.20f ),
            ( 1.070f, 0.15f ),
            ( 1.075f, 0.10f ),
            ( 1.080f, 0.05f ),
        };

        private static readonly ChanceTable<float> T12_Defense = new ChanceTable<float>()
        {
            ( 1.085f, 0.05f ),
            ( 1.090f, 0.10f ),
            ( 1.095f, 0.15f ),
            ( 1.100f, 0.20f ),
            ( 1.105f, 0.20f ),
            ( 1.110f, 0.15f ),
            ( 1.115f, 0.10f ),
            ( 1.120f, 0.05f ),
        };

        public static float? Roll(int tier)
        {
            // preliminary roll: 10% chance
            var rng = ThreadSafeRandom.Next(0.0f, 1.0f);
            if (rng >= 0.07f * tier) return null;

            switch (tier)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                    return T1_T6_Defense.Roll();
                case 7:
                case 8:
                    return T7_T8_Defense.Roll();
                case 9:
                    return T9_Defense.Roll();
                case 10:
                    return T10_Defense.Roll();
                case 11:
                    return T11_Defense.Roll();
                case 12:
                case 13:
                case 14:
                case 15:
                    return T12_Defense.Roll();
                default:
                    return null;
            }
        }
    }
}
