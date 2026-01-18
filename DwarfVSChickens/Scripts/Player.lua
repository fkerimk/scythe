local moveSpeed = 3
local sensitivity = 0.3

local posTarget = self.pos
local rotTarget = quat.fromEuler(-90, 0, 0)

local camRotTarget = f2.zero
local camDistance = 5;
local smoothCamDistance = 5;

local anim = self:findComponent({"Animation"}) --[[@as Animation]]

mouse.isLocked = true;

function loop(dt)

    movement(dt)
    camera(dt)

    if kb.pressed("Escape") then game.quit() end
end

function movement(dt)

    local moveInput = f2.new(

        (kb.down("D") and 1 or 0) - (kb.down("A") and 1 or 0),
        (kb.down("W") and 1 or 0) - (kb.down("S") and 1 or 0)
    )

    posTarget = posTarget - moveInput.x * dt * moveSpeed * cam.rightFlat
                          + moveInput.y * dt * moveSpeed * cam.fwdFlat

    if moveInput ~= f2.zero then

        rotTarget = quat.fromDir(cam.fwdFlat) *
                    quat.fromEuler(0, mt.dirAngle(moveInput * -1), 0) *
                    quat.fromEuler(-90, 0, 90)
    end

    self.pos = f3.lerp(self.pos, posTarget, dt * 15)
    self.rot = quat.lerp(self.rot, rotTarget, dt * 15)

    anim.track = moveInput == f2.zero and 6 or 7
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