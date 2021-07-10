module EeveeHelperSeededStarfield

using ..Ahorn, Maple

@mapdef Effect "EeveeHelper/SeededStarfield" SeededStarfield(only::String="*", exclude::String="", color::String="", scrollx::Number=1.0, scrolly::Number=1.0, speed::Number=1.0, seed::Integer=0)

const placements = SeededStarfield

Ahorn.canFgBg(effect::SeededStarfield) = true, true

end