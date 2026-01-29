local speed = 7.5
local power = 0.1

function loop(dt)

    local targetPos = f3.zero

    if kb:down("D") or kb:down("A") or kb:down("W") or kb:down("S") then
        targetPos = f3.new(math.sin(time.passed * speed * 0.5) * power, math.sin(time.passed * speed) * power, 0)
    end

    self.transform.pos = f3.lerp(self.transform.pos, targetPos, dt * 5)
end