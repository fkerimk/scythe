local rb = self:findComponent({"Rigidbody"}) --[[@as Rigidbody]]

local forceDir = f3.new(math.random(), math.random(), math.random())

function loop()
    if time.passed < 3.5 then return end
    rb.velocity = rb.velocity + forceDir * 0.25
end