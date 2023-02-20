local fakeTilesHelper = require("helpers.fake_tiles")

local holdableTiles = {
    name = "EeveeHelper/HoldableTiles",
    depth = -10000 + 2,
    sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", "blendin"),
    fieldInformation = fakeTilesHelper.getFieldInformation("tiletype"),
    placements = {
        name = "default",
        data = {
            width = 8,
            height = 8,
            tiletype = "3",
            holdable = true,
            noDuplicate = false,
            destroyable = true,
        }
    }
}

return holdableTiles