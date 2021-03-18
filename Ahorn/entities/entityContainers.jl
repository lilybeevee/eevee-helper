module EeveeHelperEntityContainers

using ..Ahorn, Maple

@mapdef Entity "EeveeHelper/HoldableContainer" HoldableContainer(x::Integer, y::Integer, width::Integer=8, height::Integer=8, whitelist::String="", fitContained::Bool=true,
    gravity::Bool=true, holdable::Bool=true, noDuplicate::Bool=false, slowFall::Bool=false, slowRun::Bool=true, destroyable::Bool=true)
@mapdef Entity "EeveeHelper/AttachedContainer" AttachedContainer(x::Integer, y::Integer, width::Integer=8, height::Integer=8, whitelist::String="", attachTo::String="")
@mapdef Entity "EeveeHelper/FloatyContainer" FloatyContainer(x::Integer, y::Integer, width::Integer=8, height::Integer=8, whitelist::String="",
    floatSpeed::Number=1.0, floatMove::Number=4.0, pushSpeed::Number=1.0, pushMove::Number=8.0, sinkSpeed::Number=1.0, sinkMove::Number=12.0,
    disableSpawnOffset::Bool=false, disablePush::Bool=false)
@mapdef Entity "EeveeHelper/SMWTrackContainer" SMWTrackContainer(x::Integer, y::Integer, width::Integer=8, height::Integer=8, whitelist::String="", fitContained::Bool=true,
    moveSpeed::Number=100.0, fallSpeed::Number=200.0, gravity::Number=200.0, direction::String="Right", flag::String="", notFlag::Bool=false, startOnTouch::Bool=false,
    disableBoost::Bool=false)

const containerUnion = Union{HoldableContainer, AttachedContainer, FloatyContainer, SMWTrackContainer}

const placements = Ahorn.PlacementDict(
    "Entity Container (Holdable)\n(Eevee Helper)" => Ahorn.EntityPlacement(
        HoldableContainer,
        "rectangle",
        Dict{String, Any}(
            "holdable" => true,
            "gravity" => true
        )
    ),
    "Entity Container (Falling)\n(Eevee Helper)" => Ahorn.EntityPlacement(
        HoldableContainer,
        "rectangle",
        Dict{String, Any}(
            "holdable" => false,
            "gravity" => true
        )
    ),
    "Entity Container (Attached)\n(Eevee Helper)" => Ahorn.EntityPlacement(
        AttachedContainer,
        "rectangle"
    ),
    "Entity Container (Floaty)\n(Eevee Helper)" => Ahorn.EntityPlacement(
        FloatyContainer,
        "rectangle"
    ),
    "Entity Container (SMW Track)\n(Eevee Helper)" => Ahorn.EntityPlacement(
        SMWTrackContainer,
        "rectangle"
    )
)

Ahorn.minimumSize(entity::containerUnion) = 8, 8
Ahorn.resizable(entity::containerUnion) = true, true

Ahorn.editingOrder(entity::FloatyContainer) = ["x", "y", "width", "height", "floatMove", "floatSpeed", "pushMove", "pushSpeed", "sinkMove", "sinkSpeed", "whitelist"]
Ahorn.editingOptions(entity::SMWTrackContainer) = Dict{String, Any}(
    "direction" => ["Left", "Right"]
)

Ahorn.nodeLimits(entity::AttachedContainer) = 0, 1

function Ahorn.selection(entity::containerUnion)
    res = Ahorn.Rectangle[Ahorn.getEntityRectangle(entity)]

    for node in get(entity.data, "nodes", ())
        nx, ny = Int.(node)

        push!(res, Ahorn.Rectangle(nx, ny, 8, 8))
    end

    return res
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::containerUnion, room::Maple.Room)
    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    Ahorn.drawRectangle(ctx, 0, 0, width, height, (1.0, 0.6, 0.6, 0.4), (1.0, 0.6, 0.6, 1.0))
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::containerUnion)
    x, y = Ahorn.position(entity)
    width = get(entity.data, "width", 0)
    height = get(entity.data, "height", 0)

    sx = x + (width / 2)
    sy = y + (height / 2)

    for node in get(entity.data, "nodes", ())
        nx, ny = Int.(node)

        theta = atan(sy - ny, sx - nx)
        Ahorn.drawArrow(ctx, sx, sy, nx + cos(theta) + 4, ny + sin(theta) + 4, (1.0, 0.5, 0.5, 0.6), headLength=6)
        Ahorn.drawRectangle(ctx, nx, ny, 8, 8, (1.0, 0.4, 0.4, 0.4), (1.0, 0.4, 0.4, 1.0))
    end
end

end