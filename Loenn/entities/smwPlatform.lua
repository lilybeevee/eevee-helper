local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local smwPlatform = {
    name = "EeveeHelper/SMWPlatform",
    depth = -60,
    minimumSize = {16, 0},
    canResize = {true, false},

    placements = {
        name = "default",
        data = {
            width = 40,
            height = 8,
            texturePath = "objects/EeveeHelper/smwPlatform",
            moveSpeed = 100.0,
            fallSpeed = 200.0,
            gravity = 200.0,
            direction = "Right",
            flag = "",
            notFlag = false,
            startOnTouch = true,
            disableBoost = false,
        }
    },

    fieldInformation = {
        direction = {
            options = { "Left", "Right" },
            editable = false,
        }
    },

    selection = function (room, entity)
        return utils.rectangle(entity.x - entity.width / 2, entity.y - 8, entity.width, 8)
    end,

    sprite = function (room, entity)
        local sprites = {}

        local texturePath = entity.texturePath or "objects/EeveeHelper/smwPlatform"
        local platformPath = string.format("%s/platform", texturePath)
        local gearPath = string.format("%s/gear00", texturePath)

        local width = entity.width or 16
        local tiles = width / 8
        for i = 1, tiles, 1 do
            local tx = 1
            if i == 1 then
                tx = 0
            elseif i == tiles then
                tx = 2
            end

            local sprite = drawableSprite.fromTexture(platformPath, entity)
            sprite:useRelativeQuad(tx * 8, 0, 8, 8)
            sprite:addPosition((i - 1) * 8 - width / 2, -8)
            table.insert(sprites, sprite)
        end

        table.insert(sprites, drawableSprite.fromTexture(gearPath, entity))

        return sprites
    end
}

return smwPlatform