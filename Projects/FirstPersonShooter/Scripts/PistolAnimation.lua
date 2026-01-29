local anim = self:findComponent({"Animation"}) --[[@as Animation]]

local track = 0

function loop()

    local targetTrack = 0

    if kb:down(key.D) or kb:down(key.A) or kb:down(key.W) or kb:down(key.S) then
        targetTrack = 2
    end

    if targetTrack ~= track then
        track = targetTrack
        anim:play(track, 0.2)
    end
end