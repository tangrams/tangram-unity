using System;

namespace Mapzen
{
    public class RegionScaleUnits
    {
        [Flags]
        public enum Units
        {
            None = 0,
            Meters = 1 << 0,
            KiloMeters = 1 << 1,
            Miles = 1 << 2,
            Feet = 1 << 3,
        }

        // A name identifer
        public string name;
        public Units unit;

        /// <summary>
        /// Tests whether this unit is enabled
        /// </summary>
        /// <param name="unit">The unit to check.</param>
        /// <param name="regionUnit">Unit defined for the region.</param>
        public static bool Test(Units unit, Units regionUnit)
        {
            return ((int)unit & (int)regionUnit) == (int)unit;
        }

        public RegionScaleUnits(Units u, string n)
        {
            this.unit = u;
            this.name = n;
        }
    }
}

