module EeveeHelperBooster

using ..Ahorn, Maple

@mapdef Entity "EeveeHelper/PatientBooster" PatientBooster(x::Integer, y::Integer, red::Bool=false)

const placements = Ahorn.PlacementDict(
    "Patient Booster (Green) (Eevee Helper)" => Ahorn.EntityPlacement(
        PatientBooster
    ),
    "Patient Booster (Red) (Eevee Helper)" => Ahorn.EntityPlacement(
        PatientBooster,
        "point",
        Dict{String, Any}(
            "red" => true
        )
    )
)

function boosterSprite(entity::PatientBooster)
    red = get(entity.data, "red", false)
    
    if red
        return "objects/EeveeHelper/patientBooster/boosterRed00"

    else
        return "objects/EeveeHelper/patientBooster/booster00"
    end
end

function Ahorn.selection(entity::PatientBooster)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 11, y - 9, 22, 18)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::PatientBooster, room::Maple.Room)
    sprite = boosterSprite(entity)

    Ahorn.drawSprite(ctx, sprite, 0, 0)
end

end