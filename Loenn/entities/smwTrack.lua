local utils = require("utils")
local drawableLine = require("structs.drawable_line")
local drawableSprite = require("structs.drawable_sprite")

local black = { 0.0, 0.0, 0.0, 1.0 }
local pointTexture = "objects/EeveeHelper/smwTrack/point"
local endTexture = "objects/EeveeHelper/smwTrack/end"

local function addAll(to, from)
    for _, value in ipairs(from) do
        table.insert(to, value)
    end

    return to
end

local function flattenNodes(nodes)
    local flattened = {}

    for _, node in ipairs(nodes) do
        table.insert(flattened, node.x)
        table.insert(flattened, node.y)
    end

    return flattened
end

local smwTrack = {
    name = "EeveeHelper/SMWTrack",
    depth = 9000 - 10,
    nodeLimits = { 1, -1 },
    nodeLineRenderType = "line",

    placements = {
        name = "default",
        placementType = "line",
        data = {
            color = "FFFFFF",
            inactiveColor = "",
            flag = "",
            startOpenFlag = "",
            endOpenFlag = "",
            startOpen = false,
            endOpen = false,
            notFlag = false,
            hidden = false,
            hideInactive = false,
        }
    },

    fieldInformation = {
        color = {
            fieldType = "color"
        },
        -- once lonn supports empty values being valid for color fields, uncomment this
        --[[
        inactiveColor = {
            fieldType = "color"
        }
        ]]
    },

    selection = function (room, entity)
        local main = utils.rectangle(entity.x - 4, entity.y - 4, 8, 8)
        local nodeSelections = {}
        for _, node in ipairs(entity.nodes) do
            table.insert(nodeSelections, utils.rectangle(node.x - 4, node.y - 4, 8, 8))
        end

        return main, nodeSelections
    end,

    sprite = function (room, entity)
        local sprites = {}

        local startOpen = entity.startOpen
        local endOpen = entity.endOpen
        local fullNodes = addAll({entity}, entity.nodes)
        local color = utils.getColor(entity.color or "ffffff")
        local flattenedNodes = flattenNodes(fullNodes)

        table.insert(sprites, drawableLine.fromPoints(flattenedNodes, black, 3, 1, 1))
        table.insert(sprites, drawableLine.fromPoints(flattenedNodes, color, 1))

        for i, node in ipairs(fullNodes) do
            local nx, ny = node.x, node.y

            table.insert(sprites, drawableSprite.fromTexture(
                ((i == 1 and not startOpen) or (i == #fullNodes and not endOpen)) and endTexture or pointTexture,
                { x = nx, y = ny, color = color }
            ))
        end

        return sprites
    end,

    nodeSprite = function () end,
}

return smwTrack
