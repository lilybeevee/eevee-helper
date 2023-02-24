local holdableContainer = {
    name = "EeveeHelper/HoldableContainer",
    fillColor = { 1.0, 0.6, 0.6, 0.4 },
    borderColor = { 1.0, 0.6, 0.6, 1 },

    placements = {
        default = {
            data = {
                width = 8,
                height = 8,
                whitelist = "",
                blacklist = "",
                containMode = "RoomStart",
                containFlag = "",
                fitContained = true,
                ignoreAnchors = false,
                forceStandardBehavior = false,
                gravity = true,
                holdable = true,
                noDuplicate = false,
                slowFall = false,
                slowRun = true,
                destroyable = true,
                tutorial = false,
                respawn = false,
                waitForGrab = false,
            }
        },
        {
            name = "holdable",
            data = {
                width = 8,
                height = 8,
            }
        },
        {
            name = "falling",
            data = {
                width = 8,
                height = 8,
                holdable = false,
            }
        }
    },
}

local attachedContainer = {
    name = "EeveeHelper/AttachedContainer",
    fillColor = { 1.0, 0.6, 0.6, 0.4 },
    borderColor = { 1.0, 0.6, 0.6, 1 },
    nodeLimits = { 0, 1 },
    nodeLineRenderType = "line",

    placements = {
        name = "default",
        data = {
            width = 8,
            height = 8,
            whitelist = "",
            blacklist = "",
            containMode = "RoomStart",
            containFlag = "",
            fitContained = true,
            ignoreAnchors = false,
            forceStandardBehavior = false,
            attachMode = "RoomStart",
            attachFlag = "",
            attachTo = "",
            restrictToNode = true,
            onlyX = false,
            onlyY = false,
            matchCollidable = false,
            matchVisible = false,
            destroyable = true,
        }
    },
    fieldOrder = { "x", "y", "width", "height", "containMode", "containFlag", "whitelist", "blacklist", "attachMode", "attachFlag", "attachTo" },
}

local floatyContainer = {
    name = "EeveeHelper/FloatyContainer",
    fillColor = { 1.0, 0.6, 0.6, 0.4 },
    borderColor = { 1.0, 0.6, 0.6, 1 },

    placements = {
        name = "default",
        data = {
            width = 8,
            height = 8,
            whitelist = "",
            blacklist = "",
            containMode = "RoomStart",
            containFlag = "",
            ignoreAnchors = false,
            forceStandardBehavior = false,
            floatSpeed = 1.0,
            floatMove = 4.0,
            pushSpeed = 1.0,
            pushMove = 8.0,
            sinkSpeed = 1.0,
            sinkMove = 12.0,
            disableSpawnOffset = false,
            disablePush = false,
        }
    },
    fieldOrder = { "x", "y", "width", "height", "containMode", "containFlag", "whitelist", "blacklist", "floatMove", "floatSpeed", "pushMove", "pushSpeed", "sinkMove", "sinkSpeed" },
}

local SMWTrackContainer = {
    name = "EeveeHelper/SMWTrackContainer",
    fillColor = { 1.0, 0.6, 0.6, 0.4 },
    borderColor = { 1.0, 0.6, 0.6, 1 },

    placements = {
        name = "default",
        data = {
            width = 8,
            height = 8,
            whitelist = "",
            blacklist = "",
            containMode = "RoomStart",
            containFlag = "",
            fitContained = true,
            ignoreAnchors = false,
            forceStandardBehavior = false,
            moveSpeed = 100.0,
            fallSpeed = 200.0,
            gravity = 200.0,
            direction = "Right",
            moveFlag = "",
            startOnTouch = false,
            disableBoost = false,
        }
    }
}

local flagGateContainer = {
    name = "EeveeHelper/FlagGateContainer",
    fillColor = { 1.0, 0.6, 0.6, 0.4 },
    borderColor = { 1.0, 0.6, 0.6, 1 },
    nodeLimits = {2, 2},
    nodeLineRenderType = "line",

    placements = {
        default = {
            data = {
                width = 8,
                height = 8,
                whitelist = "",
                blacklist = "",
                containMode = "RoomStart",
                containFlag = "",
                fitContained = true,
                ignoreAnchors = false,
                forceStandardBehavior = false,
                moveFlag = "",
                shakeTime = 0.5,
                moveTime = 2.0,
                easing = "CubeOut",
                icon = "objects/switchgate/icon",
                inactiveColor = "5FCDE4",
                activeColor = "FFFFFF",
                finishColor = "F141DF",
                staticFit = false,
                canReturn = false,
                iconVisible = true,
                playSounds = true,
            }
        },
        {
            name = "switchGate",
            data = {
                width = 8,
                height = 8,
            }
        },
        {
            name = "flagMover",
            data = {
                width = 8,
                height = 8,
                shakeTime = 0.0,
                moveFlag = "flag",
                canReturn = true,
                iconVisible = false,
                playSounds = false
            }
        }
    }
}

local flagToggleModifier = {
    name = "EeveeHelper/FlagToggleModifier",
    fillColor = { 0.6, 1.0, 0.6, 0.4 },
    borderColor = { 0.6, 1.0, 0.6, 1 },

    placements = {
        name = "default",
        data = {
            width = 8,
            height = 8,
            whitelist = "",
            blacklist = "",
            containMode = "RoomStart",
            containFlag = "",
            forceStandardBehavior = false,
            flag = "",
            notFlag = false,
            toggleActive = true,
            toggleVisible = true,
            toggleCollidable = true,
        }
    }
}

local collidableModifier = {
    name = "EeveeHelper/CollidableModifier",
    fillColor = { 0.6, 1.0, 0.6, 0.4 },
    borderColor = { 0.6, 1.0, 0.6, 1 },

    placements = {
        default = {
            data = {
                width = 8,
                height = 8,
                whitelist = "",
                blacklist = "",
                containMode = "RoomStart",
                containFlag = "",
                forceStandardBehavior = false,
                noCollide = false,
                solidify = false,
            }
        },
        {
            name = "noCollide",
            data = {
                width = 8,
                height = 8,
                noCollide = true,
                solidify = false
            }
        },
        {
            name = "solidify",
            data = {
                width = 8,
                height = 8,
                noCollide = false,
                solidify = true
            }
        }
    }
}

local globalModifier = {
    name = "EeveeHelper/GlobalModifier",
    fillColor = { 0.6, 1.0, 0.6, 0.4 },
    borderColor = { 0.6, 1.0, 0.6, 1 },

    placements = {
        name = "default",
        data = {
            width = 8,
            height = 8,
            whitelist = "",
            frozenUpdate = false,
            pauseUpdate = false,
            transitionUpdate = false,
        }
    }
}

local containers = {
    holdableContainer,
    attachedContainer,
    floatyContainer,
    SMWTrackContainer,
    flagGateContainer,
    flagToggleModifier,
    collidableModifier,
    globalModifier,
}

local containModes = { "RoomStart", "FlagChanged", "Always" }
local directions = { "Left", "Right" }
local easeTypes = { "Linear", "SineIn", "SineOut", "SineInOut", "QuadIn", "QuadOut", "QuadInOut", "CubeIn", "CubeOut", "CubeInOut", "QuintIn", "QuintOut", "QuintInOut", "BackIn", "BackOut", "BackInOut", "ExpoIn", "ExpoOut", "ExpoInOut", "BigBackIn", "BigBackOut", "BigBackInOut", "ElasticIn", "ElasticOut", "ElasticInOut", "BounceIn", "BounceOut", "BounceInOut" }

local sharedFieldInformation = {
    containMode = {
        options = containModes,
        editable = false
    },
    attachMode = {
        options = containModes,
        editable = false
    },
    easing = {
        options = easeTypes,
        editable = false
    },
    direction = {
        options = directions,
        editable = false
    },
    inactiveColor = {
        fieldType = "color"
    },
    activeColor = {
        fieldType = "color"
    },
    finishColor = {
        fieldType = "color"
    },
}

for _, container in ipairs(containers) do
    container.fieldInformation = sharedFieldInformation
    container.depth = math.huge -- make containers render below everything
end

return containers