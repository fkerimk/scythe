local t = obj.findParentType("Transform")

function loop(dt)
    t.pos = f3.new(0, math.sin(time.passed * 2.5) * 0.35 + 0.5, 0);
    t.rotateY(dt * 50)
end