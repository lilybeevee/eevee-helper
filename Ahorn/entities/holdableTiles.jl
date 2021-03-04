module EeveeHelperHoldableTiles

using ..Ahorn, Maple

@mapdef Entity "EeveeHelper/HoldableTiles" HoldableTiles(x::Integer, y::Integer, width::Integer=8, height::Integer=8, tiletype::String="3", noDuplicate::Bool=false)

const placements = Ahorn.PlacementDict(
    "Holdable Tiles (Eevee Helper)" => Ahorn.EntityPlacement(
        HoldableTiles,
        "rectangle",
        Dict{String, Any}(),
        Ahorn.tileEntityFinalizer
    )
)

Ahorn.editingOptions(entity::HoldableTiles) = Dict{String, Any}(
    "tiletype" => Ahorn.tiletypeEditingOptions()
)
Ahorn.minimumSize(entity::HoldableTiles) = 8, 8
Ahorn.resizable(entity::HoldableTiles) = true, true
Ahorn.selection(entity::HoldableTiles) = Ahorn.getEntityRectangle(entity)
Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::HoldableTiles, room::Maple.Room) = Ahorn.drawTileEntity(ctx, room, entity)
end
