local anim = self:findComponent({"Animation"}) --[[@as Animation]]

local track = 0

function loop()

    local targetTrack = 0

    if kb:down("D") or kb:down("A") or kb:down("W") or kb:down("S") then
        targetTrack = 2
    end

    if targetTrack ~= track then
        track = targetTrack
        anim:play(track, 0.2)
    end
end