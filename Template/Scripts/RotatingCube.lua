local transform = obj.parent.findType("Transform")

function loop(dt)
    
    transform.pos = f3(0, math.sin(time.passed * 2.5) * 0.35, 0);
    transform.rotateY(dt * 50)
end