function loop(dt)
    self.pos = f3.new(0, math.sin(time.passed * 2.5) * 0.35 + 0.5, 0);
    self.transform:rotateY(dt * 50)
end