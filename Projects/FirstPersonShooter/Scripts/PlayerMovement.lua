local rb = self:findComponent({"Rigidbody"}) --[[@as Rigidbody]]

local speed = 3

function loop()

    local moveInput = f2.new(

        (kb:down("D") and 1 or 0) - (kb:down("A") and 1 or 0),
        (kb:down("W") and 1 or 0) - (kb:down("S") and 1 or 0)
    )

    local vel = - moveInput.x * speed * cam.rightFlat
                + moveInput.y * speed * cam.fwdFlat
                + f3.up * rb.velocity.y

    rb.velocity = vel
end