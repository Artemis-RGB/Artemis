using System.ComponentModel;

namespace Artemis.Modules.Games.ProjectCars.Data
{
    public enum eTerrain
    {
        [Description("TERRAIN_ROAD")] TERRAIN_ROAD = 0,
        [Description("TERRAIN_LOW_GRIP_ROAD")] TERRAIN_LOW_GRIP_ROAD,
        [Description("TERRAIN_BUMPY_ROAD1")] TERRAIN_BUMPY_ROAD1,
        [Description("TERRAIN_BUMPY_ROAD2")] TERRAIN_BUMPY_ROAD2,
        [Description("TERRAIN_BUMPY_ROAD3")] TERRAIN_BUMPY_ROAD3,
        [Description("TERRAIN_MARBLES")] TERRAIN_MARBLES,
        [Description("TERRAIN_GRASSY_BERMS")] TERRAIN_GRASSY_BERMS,
        [Description("TERRAIN_GRASS")] TERRAIN_GRASS,
        [Description("TERRAIN_GRAVEL")] TERRAIN_GRAVEL,
        [Description("TERRAIN_BUMPY_GRAVEL")] TERRAIN_BUMPY_GRAVEL,
        [Description("TERRAIN_RUMBLE_STRIPS")] TERRAIN_RUMBLE_STRIPS,
        [Description("TERRAIN_DRAINS")] TERRAIN_DRAINS,
        [Description("TERRAIN_TYREWALLS")] TERRAIN_TYREWALLS,
        [Description("TERRAIN_CEMENTWALLS")] TERRAIN_CEMENTWALLS,
        [Description("TERRAIN_GUARDRAILS")] TERRAIN_GUARDRAILS,
        [Description("TERRAIN_SAND")] TERRAIN_SAND,
        [Description("TERRAIN_BUMPY_SAND")] TERRAIN_BUMPY_SAND,
        [Description("TERRAIN_DIRT")] TERRAIN_DIRT,
        [Description("TERRAIN_BUMPY_DIRT")] TERRAIN_BUMPY_DIRT,
        [Description("TERRAIN_DIRT_ROAD")] TERRAIN_DIRT_ROAD,
        [Description("TERRAIN_BUMPY_DIRT_ROAD")] TERRAIN_BUMPY_DIRT_ROAD,
        [Description("TERRAIN_PAVEMENT")] TERRAIN_PAVEMENT,
        [Description("TERRAIN_DIRT_BANK")] TERRAIN_DIRT_BANK,
        [Description("TERRAIN_WOOD")] TERRAIN_WOOD,
        [Description("TERRAIN_DRY_VERGE")] TERRAIN_DRY_VERGE,
        [Description("TERRAIN_EXIT_RUMBLE_STRIPS")] TERRAIN_EXIT_RUMBLE_STRIPS,
        [Description("TERRAIN_GRASSCRETE")] TERRAIN_GRASSCRETE,
        [Description("TERRAIN_LONG_GRASS")] TERRAIN_LONG_GRASS,
        [Description("TERRAIN_SLOPE_GRASS")] TERRAIN_SLOPE_GRASS,
        [Description("TERRAIN_COBBLES")] TERRAIN_COBBLES,
        [Description("TERRAIN_SAND_ROAD")] TERRAIN_SAND_ROAD,
        [Description("TERRAIN_BAKED_CLAY")] TERRAIN_BAKED_CLAY,
        [Description("TERRAIN_ASTROTURF")] TERRAIN_ASTROTURF,
        [Description("TERRAIN_SNOWHALF")] TERRAIN_SNOWHALF,
        [Description("TERRAIN_SNOWFULL")] TERRAIN_SNOWFULL,
        //-------------
        [Description("TERRAIN_MAX")] TERRAIN_MAX
    }
}