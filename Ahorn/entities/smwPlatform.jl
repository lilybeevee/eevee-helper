module EeveeHelperSMWPlatform

using ..Ahorn, Maple

@mapdef Entity "EeveeHelper/SMWPlatform" SMWPlatform(x::Integer, y::Integer, width::Integer=40, height::Integer=8, texturePath::String="objects/EeveeHelper/smwPlatform",
    moveSpeed::Number=100.0, fallSpeed::Number=200.0, gravity::Number=200.0, startDelay::Number=0.0, direction::String="Right", flag::String="",
    moveBehaviour::String="Linear", easing::String="SineInOut", easeDuration::Number=2.0, easeTrackDirection::Bool=false, notFlag::Bool=false, startOnTouch::Bool=true,
    stopAtNode::Bool=false, stopAtEnd::Bool=false, moveOnce::Bool=false, disableBoost::Bool=false)

const placements = Ahorn.PlacementDict(
    "SMW Platform (Eevee Helper)" => Ahorn.EntityPlacement(
        SMWPlatform,
        "rectangle"
    )
)

Ahorn.minimumSize(entity::SMWPlatform) = 16, 0
Ahorn.resizable(entity::SMWPlatform) = true, false

Ahorn.editingOptions(entity::SMWPlatform) = Dict{String, Any}(
    "direction" => ["Left", "Right"],
    "moveBehaviour" => ["Linear", "Easing"],
    "easing" => ["Linear", "SineIn", "SineOut", "SineInOut", "QuadIn", "QuadOut", "QuadInOut", "CubeIn", "CubeOut", "CubeInOut", "QuintIn", "QuintOut", "QuintInOut", "ExpoIn", "ExpoOut", "ExpoInOut"]
)

function Ahorn.selection(entity::SMWPlatform)
    x, y = Ahorn.position(entity)
    width = get(entity.data, "width", 16)

    return Ahorn.Rectangle(x - width/2, y - 8, width, 8)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SMWPlatform, room::Maple.Room)
    texturePath = get(entity.data, "texturePath", "objects/EeveeHelper/smwPlatform")

    width = get(entity.data, "width", 16)

    tiles = div(width, 8)
    for i in 1:tiles
        tx = 1
        if i == 1
            tx = 0
        elseif i == tiles
            tx = 2
        end

        Ahorn.drawImage(ctx, "$(texturePath)/platform", (i - 1) * 8 - width / 2, -8, tx * 8, 0, 8, 8)
    end

    Ahorn.drawImage(ctx, "$(texturePath)/gear00", -4, -4)
end

end