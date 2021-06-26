module EeveeHelperSMWTrack

using ..Ahorn, Maple

@mapdef Entity "EeveeHelper/SMWTrack" SMWTrack(x::Integer, y::Integer, color::String="FFFFFF", inactiveColor::String="",
    flag::String="", startOpenFlag::String="", endOpenFlag::String="",
    startOpen::Bool=false, endOpen::Bool=false, notFlag::Bool=false, hidden::Bool=false, hideInactive::Bool=false)

const placements = Ahorn.PlacementDict(
    "SMW Track (Eevee Helper)" => Ahorn.EntityPlacement(
        SMWTrack,
        "line"
    )
)

Ahorn.nodeLimits(entity::SMWTrack) = 1, -1

function Ahorn.selection(entity::SMWTrack)
    x, y = Ahorn.position(entity)

    res = Ahorn.Rectangle[Ahorn.Rectangle(x - 4, y - 4, 8, 8)]

    for node in get(entity.data, "nodes", ())
        nx, ny = Int.(node)

        push!(res, Ahorn.Rectangle(nx - 4, ny - 4, 8, 8))
    end

    return res
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::SMWTrack, room::Maple.Room)
    x, y = Ahorn.position(entity)

    nodes = get(entity.data, "nodes", [])
    fullNodes = [(x, y); nodes]

    rawColor = Ahorn.argb32ToRGBATuple(parse(Int, get(entity.data, "color", "ffffff"), base=16))[1:3] ./ 255
    color = (rawColor..., 1.0)

    for i in 1:length(fullNodes)-1
        Ahorn.drawLines(ctx, [fullNodes[i], fullNodes[i+1]], (0.0, 0.0, 0.0, 1.0), thickness=4)
        Ahorn.drawLines(ctx, [fullNodes[i], fullNodes[i+1]], color, thickness=2)
    end

    startOpen = get(entity.data, "startOpen", false)
    endOpen = get(entity.data, "endOpen", false)

    for i in 1:length(fullNodes)
        nx, ny = Int.(fullNodes[i])
        if (i == 1 && !startOpen) || (i == length(fullNodes) && !endOpen)
            Ahorn.drawRectangle(ctx, nx - 4, ny - 4, 8, 8, (0.0, 0.0, 0.0, 1.0))
            Ahorn.drawRectangle(ctx, nx - 3, ny - 3, 6, 6, color)
        else
            Ahorn.drawCircle(ctx, nx, ny, 3, (0.0, 0.0, 0.0, 1.0))
            Ahorn.drawCircle(ctx, nx, ny, 2, color)
        end
    end
end

end