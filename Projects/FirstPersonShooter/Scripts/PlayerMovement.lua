local rb = self:findComponent({"Rigidbody"}) --[[@as Rigidbody]]

local speed = 3

function loop()

    local moveInput = f2.new(

        (kb:down(key.D) and 1 or 0) - (kb:down(key.A) and 1 or 0),
        (kb:down(key.W) and 1 or 0) - (kb:down(key.S) and 1 or 0)
    )

    local vel = - moveInput.x * speed * cam.rightFlat
                + moveInput.y * speed * cam.fwdFlat
                + f3.up * rb.velocity.y

    rb.velocity = vel
end