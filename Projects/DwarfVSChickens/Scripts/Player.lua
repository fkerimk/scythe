local moveSpeed = 3
local sensitivity = 0.3

local rotTarget = quat.identity

local camRotTarget = f2.zero
local camDistance = 5;
local smoothCamDistance = 5;

local anim = self:findComponent({"Animation"}) --[[@as Animation]]
local rb = self:findComponent({"Rigidbody"}) --[[@as Rigidbody]]

mouse.isLocked = true;

function loop(dt)

    camera(dt)
    movement(dt)

    if kb:pressed(key.Escape) then game.quit() end
end

function movement(dt)

    local moveInput = f2.new(

        (kb:down(key.D) and 1 or 0) - (kb:down(key.A) and 1 or 0),
        (kb:down(key.W) and 1 or 0) - (kb:down(key.S) and 1 or 0)
    )

    if moveInput ~= f2.zero then

        rotTarget = quat.fromDir(cam.fwdFlat) *
                    quat.fromEuler(0, mt.dirAngle(moveInput * 1) - 90, 0)
    end

    local vel = - moveInput.x * moveSpeed * cam.rightFlat
                + moveInput.y * moveSpeed * cam.fwdFlat
                + f3.up * rb.velocity.y

    rb.velocity = vel

    self.rot = quat.lerp(self.rot, rotTarget, dt * 15)

    -- Animation
    local track = 5

    if moveInput ~= f2.zero then track = 6 end
    if vel.y < -0.5 then track = 3 end

    anim.track = track
end

function camera(dt)

    camRotTarget = f2.new (

        mt.clamp(camRotTarget.x + mouse.delta.y * sensitivity, 5, 60),
        camRotTarget.y - mouse.delta.x * sensitivity
    )

    camDistance = mt.clamp(camDistance - mt.sign(mouse.scroll), 1, 10)
    smoothCamDistance = mt.lerp(smoothCamDistance, camDistance, dt * 15)

    cam.rot = quat.fromEuler(camRotTarget.x, camRotTarget.y, 0)
    cam.pos = self.pos + f3.up * 0.5 - cam.fwd * smoothCamDistance
end