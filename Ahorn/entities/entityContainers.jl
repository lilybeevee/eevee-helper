module EeveeHelperEntityContainers

using ..Ahorn, Maple, Ahorn.Selection

@mapdef Entity "EeveeHelper/HoldableContainer" HoldableContainer(x::Integer, y::Integer, width::Integer=8, height::Integer=8,
    whitelist::String="", blacklist::String="", containMode::String="RoomStart", containFlag::String="",
    fitContained::Bool=true, ignoreAnchors::Bool=false, forceStandardBehavior::Bool=false,
    gravity::Bool=true, holdable::Bool=true, noDuplicate::Bool=false, slowFall::Bool=false, slowRun::Bool=true, destroyable::Bool=true, tutorial::Bool=false,
    respawn::Bool=false, waitForGrab::Bool=false)
@mapdef Entity "EeveeHelper/AttachedContainer" AttachedContainer(x::Integer, y::Integer, width::Integer=8, height::Integer=8,
    whitelist::String="", blacklist::String="", containMode::String="RoomStart", containFlag::String="",
    fitContained::Bool=true, ignoreAnchors::Bool=false, forceStandardBehavior::Bool=false,
    attachMode::String="RoomStart", attachFlag::String="", attachTo::String="", restrictToNode::Bool=true, onlyX::Bool=false, onlyY::Bool=false,
    matchCollidable::Bool=false, matchVisible::Bool=false, destroyable::Bool=true)
@mapdef Entity "EeveeHelper/FloatyContainer" FloatyContainer(x::Integer, y::Integer, width::Integer=8, height::Integer=8,
    whitelist::String="", blacklist::String="", containMode::String="RoomStart", containFlag::String="",
    ignoreAnchors::Bool=false, forceStandardBehavior::Bool=false,
    floatSpeed::Number=1.0, floatMove::Number=4.0, pushSpeed::Number=1.0, pushMove::Number=8.0, sinkSpeed::Number=1.0, sinkMove::Number=12.0,
    disableSpawnOffset::Bool=false, disablePush::Bool=false)
@mapdef Entity "EeveeHelper/SMWTrackContainer" SMWTrackContainer(x::Integer, y::Integer, width::Integer=8, height::Integer=8,
    whitelist::String="", blacklist::String="", containMode::String="RoomStart", containFlag::String="",
    fitContained::Bool=true, ignoreAnchors::Bool=false, forceStandardBehavior::Bool=false,
    moveSpeed::Number=100.0, fallSpeed::Number=200.0, gravity::Number=200.0, direction::String="Right", moveFlag::String="", startOnTouch::Bool=false,
    disableBoost::Bool=false)
@mapdef Entity "EeveeHelper/FlagGateContainer" FlagGateContainer(x::Integer, y::Integer, width::Integer=8, height::Integer=8,
    whitelist::String="", blacklist::String="", containMode::String="RoomStart", containFlag::String="",
    fitContained::Bool=true, ignoreAnchors::Bool=false, forceStandardBehavior::Bool=false,
    moveFlag::String="", shakeTime::Number=0.5, moveTime::Number=2.0, easing::String="CubeOut", icon::String="objects/switchgate/icon",
    inactiveColor::String="5FCDE4", activeColor::String="FFFFFF", finishColor::String="F141DF",
    staticFit::Bool=false, canReturn::Bool=false, iconVisible::Bool=true, playSounds::Bool=true)

@mapdef Entity "EeveeHelper/FlagToggleModifier" FlagToggleModifier(x::Integer, y::Integer, width::Integer=8, height::Integer=8,
    whitelist::String="", blacklist::String="", containMode::String="RoomStart", containFlag::String="", forceStandardBehavior::Bool=false,
    flag::String="", notFlag::Bool=false, toggleActive::Bool=true, toggleVisible::Bool=true, toggleCollidable::Bool=true)
@mapdef Entity "EeveeHelper/CollidableModifier" CollidableModifier(x::Integer, y::Integer, width::Integer=8, height::Integer=8,
    whitelist::String="", blacklist::String="", containMode::String="RoomStart", containFlag::String="", forceStandardBehavior::Bool=false,
    noCollide::Bool=false, solidify::Bool=false)
@mapdef Entity "EeveeHelper/GlobalModifier" GlobalModifier(x::Integer, y::Integer, width::Integer=8, height::Integer=8,
    whitelist::String="", frozenUpdate::Bool=false, pauseUpdate::Bool=false, transitionUpdate::Bool=false)

const containerUnion = Union{HoldableContainer, AttachedContainer, FloatyContainer, SMWTrackContainer, FlagGateContainer, FlagToggleModifier, CollidableModifier}
const allContainerUnion = Union{containerUnion, GlobalModifier}

function gateFinalizer(entity)
    x, y = Ahorn.position(entity)

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    entity.data["nodes"] = [(x + width / 2, y + height / 2),(x + width, y)]
end

const placements = Ahorn.PlacementDict(
    "Entity Container (Holdable)\n(Eevee Helper)" => Ahorn.EntityPlacement(
        HoldableContainer,
        "rectangle",
        Dict{String, Any}(
            "holdable" => true,
            "gravity" => true,
            "liftSpeedFix" => true
        )
    ),
    "Entity Container (Falling)\n(Eevee Helper)" => Ahorn.EntityPlacement(
        HoldableContainer,
        "rectangle",
        Dict{String, Any}(
            "holdable" => false,
            "gravity" => true,
            "liftSpeedFix" => true
        )
    ),
    "Entity Container (Attached)\n(Eevee Helper)" => Ahorn.EntityPlacement(
        AttachedContainer,
        "rectangle"
    ),
    "Entity Container (Floaty)\n(Eevee Helper)" => Ahorn.EntityPlacement(
        FloatyContainer,
        "rectangle",
        Dict{String, Any}(
            "liftSpeedFix" => true
        )
    ),
    "Entity Container (SMW Track)\n(Eevee Helper)" => Ahorn.EntityPlacement(
        SMWTrackContainer,
        "rectangle",
        Dict{String, Any}(
            "liftSpeedFix" => true
        )
    ),
    "Entity Container (Switch Gate)\n(Eevee Helper)" => Ahorn.EntityPlacement(
        FlagGateContainer,
        "rectangle",
        Dict{String, Any}(),
        gateFinalizer
    ),
    "Entity Container (Flag Mover)\n(Eevee Helper)" => Ahorn.EntityPlacement(
        FlagGateContainer,
        "rectangle",
        Dict{String, Any}(
            "shakeTime" => 0.0,
            "moveFlag" => "flag",
            "canReturn" => true,
            "iconVisible" => false,
            "playSounds" => false
        ),
        gateFinalizer
    ),

    "Entity Modifier (Flag Toggle)\n(Eevee Helper)" => Ahorn.EntityPlacement(
        FlagToggleModifier,
        "rectangle"
    ),
    "Entity Modifier (No Collide)\n(Eevee Helper)" => Ahorn.EntityPlacement(
        CollidableModifier,
        "rectangle",
        Dict{String, Any}(
            "noCollide" => true,
            "solidify" => false
        )
    ),
    "Entity Modifier (Solidify)\n(Eevee Helper)" => Ahorn.EntityPlacement(
        CollidableModifier,
        "rectangle",
        Dict{String, Any}(
            "noCollide" => false,
            "solidify" => true
        )
    ),
    "Entity Modifier (Global)\n(Eevee Helper)" => Ahorn.EntityPlacement(
        GlobalModifier,
        "rectangle"
    )
)

const containerTypes = Dict{Type, Any}(
    HoldableContainer        => "container",
    AttachedContainer        => "container",
    FloatyContainer          => "container",
    SMWTrackContainer        => "container",
    FlagGateContainer        => "container",

    FlagToggleModifier       => "modifier",
    CollidableModifier       => "modifier",
    GlobalModifier           => "modifier"
)

Ahorn.minimumSize(entity::allContainerUnion) = 8, 8
Ahorn.resizable(entity::allContainerUnion) = true, true

Ahorn.editingOrder(entity::containerUnion) = ["x", "y", "width", "height", "containMode", "containFlag", "whitelist", "blacklist"]
Ahorn.editingOrder(entity::AttachedContainer) = ["x", "y", "width", "height", "containMode", "containFlag", "whitelist", "blacklist", "attachMode", "attachFlag", "attachTo"]
Ahorn.editingOrder(entity::FloatyContainer) = ["x", "y", "width", "height", "containMode", "containFlag", "whitelist", "blacklist", "floatMove", "floatSpeed", "pushMove", "pushSpeed", "sinkMove", "sinkSpeed"]

const containModeOptions = ["RoomStart", "FlagChanged", "Always"]

const easeTypes = ["Linear", "SineIn", "SineOut", "SineInOut", "QuadIn", "QuadOut", "QuadInOut", "CubeIn", "CubeOut", "CubeInOut", "QuintIn", "QuintOut", "QuintInOut", "BackIn", "BackOut", "BackInOut", "ExpoIn", "ExpoOut", "ExpoInOut", "BigBackIn", "BigBackOut", "BigBackInOut", "ElasticIn", "ElasticOut", "ElasticInOut", "BounceIn", "BounceOut", "BounceInOut"]

Ahorn.editingOptions(entity::containerUnion) = Dict{String, Any}(
    "containMode" => containModeOptions
)
Ahorn.editingOptions(entity::AttachedContainer) = Dict{String, Any}(
    "containMode" => containModeOptions,
    "attachMode" => containModeOptions
)
Ahorn.editingOptions(entity::SMWTrackContainer) = Dict{String, Any}(
    "containMode" => containModeOptions,
    "direction" => ["Left", "Right"]
)
Ahorn.editingOptions(entity::FlagGateContainer) = Dict{String, Any}(
    "containMode" => containModeOptions,
    "icon" => findSwitchGateIcons(),
    "easing" => easeTypes
)

Ahorn.nodeLimits(entity::AttachedContainer) = 0, 1
Ahorn.nodeLimits(entity::FlagGateContainer) = 2, 2

function Ahorn.selection(entity::allContainerUnion)
    res = Ahorn.Rectangle[Ahorn.getEntityRectangle(entity)]

    for node in get(entity.data, "nodes", ())
        nx, ny = Int.(node)

        push!(res, Ahorn.Rectangle(nx, ny, 8, 8))
    end

    return res
end

function Ahorn.selection(entity::FlagGateContainer)
    width = get(entity.data, "width", 8)
    height = get(entity.data, "height", 8)
    icon = get(entity.data, "icon", "objects/switchgate/icon")
    iconVisible = get(entity.data, "iconVisible", false)

    res = Ahorn.Rectangle[Ahorn.getEntityRectangle(entity)]

    for (i, node) in enumerate(get(entity.data, "nodes", []))
        nx, ny = Int.(node)

        if i == 1
            if iconVisible
                push!(res, Ahorn.getSpriteRectangle("$(icon)00", nx, ny))
            else
                push!(res, Ahorn.Rectangle(nx, ny, 0, 0))
            end
        else
            push!(res, Ahorn.Rectangle(nx, ny, width, height))
        end
    end

    return res
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::allContainerUnion, room::Maple.Room)
    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))
    ctype = get(containerTypes, typeof(entity), "container")

    color = ctype == "container" ? (1.0, 0.6, 0.6) : (0.6, 1.0, 0.6)

    Ahorn.drawRectangle(ctx, 0, 0, width, height, (color..., 0.4), (color..., 1.0))

    if isa(entity, FlagGateContainer)
        icon = get(entity.data, "icon", "objects/switchgate/icon")
        iconVisible = get(entity.data, "iconVisible", false)

        if iconVisible
            ix, iy = getFlagGateIconPos(entity)
            Ahorn.drawSprite(ctx, "$(icon)00", ix, iy)
        end
    end
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::allContainerUnion)
    x, y = Ahorn.position(entity)
    width = get(entity.data, "width", 0)
    height = get(entity.data, "height", 0)
    ctype = get(containerTypes, typeof(entity), "container")

    arrowColor = ctype == "container" ? (1.0, 0.5, 0.5) : (0.5, 1.0, 0.5)
    rectColor = ctype == "container" ? (1.0, 0.4, 0.4) : (0.4, 1.0, 0.4)

    sx = x + (width / 2)
    sy = y + (height / 2)

    for node in get(entity.data, "nodes", ())
        nx, ny = Int.(node)

        theta = atan(sy - ny, sx - nx)
        Ahorn.drawArrow(ctx, sx, sy, nx + cos(theta) + 4, ny + sin(theta) + 4, (arrowColor..., 0.6), headLength=6)
        Ahorn.drawRectangle(ctx, nx, ny, 8, 8, (rectColor..., 0.4), (rectColor..., 1.0))
    end
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::FlagGateContainer)
    x, y = Ahorn.position(entity)
    width = get(entity.data, "width", 0)
    height = get(entity.data, "height", 0)
    icon = get(entity.data, "icon", "objects/switchgate/icon")
    iconVisible = get(entity.data, "iconVisible", false)

    sx = x + (width / 2)
    sy = y + (height / 2)

    for (i, node) in enumerate(get(entity.data, "nodes", []))
        if i == 1
            continue
        end

        nx, ny = Int.(node)

        ex = nx + (width / 2)
        ey = ny + (height / 2)

        theta = atan(sy - ey, sx - ex)
        Ahorn.drawArrow(ctx, sx, sy, ex + cos(theta), ey + sin(theta), (1.0, 0.4, 0.4, 0.6), headLength=6)
        Ahorn.drawRectangle(ctx, nx, ny, width, height, (1.0, 0.6, 0.6, 0.4), (1.0, 0.6, 0.6, 1.0))

        if iconVisible
            ix, iy = getFlagGateIconPos(entity)
            Ahorn.drawSprite(ctx, "$(icon)00", nx + ix, ny + iy)
        end

        sx = ex
        sy = ey
    end
end

function Selection.applyMovement!(target::FlagGateContainer, ox, oy, node=0)
    nodes = get(target.data, "nodes", ())

    if node == 0
        target.data["x"] += ox
        target.data["y"] += oy

        nodes[1] = nodes[1] .+ (ox, oy)
    else
        nodes = get(target.data, "nodes", ())

        if length(nodes) >= node
            nodes[node] = nodes[node] .+ (ox, oy)
        end
    end

    return true
end

function getFlagGateIconPos(entity::FlagGateContainer)
    x, y = Ahorn.position(entity)
    nodes = get(entity.data, "nodes", [(0.0, 0.0)])

    nx, ny = Int.(nodes[1])

    return (nx - x, ny - y)
end

const switchGateIconPaths = ["objects/EeveeHelper/flagGateIcons/", "objects/MaxHelpingHand/flagSwitchGate/"]
function findSwitchGateIcons()
    icons = Dict{String, String}("vanilla" => "objects/switchgate/icon")

    Ahorn.loadChangedExternalSprites!()
    for (path, spriteHolder) in Ahorn.getAtlas("Gameplay")
        for prefix in switchGateIconPaths
            if startswith(path, prefix) && endswith(path, "/icon00")
                name = path[length(prefix)+1:end-7]
                if !haskey(icons, name)
                    icons[name] = path[1:end-2]
                end
            end
        end
    end

    return icons
end

end