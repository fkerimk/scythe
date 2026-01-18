local anim = obj:findComponent({"Animation"}) --[[@as Animation]]

local moveInput = f2.zero
local movePos = obj.transform.pos
local moveRot = quat.fromEuler(-90, 0, 0)
local moveSpeed = 3

local sensitivity = 0.3
local camRot = f2.zero
local camDistance = 5;
local smoothCamDistance = 5;

mouse.isLocked = true;

function loop(dt)

    input();

    movement(dt)
    rotation(dt)
    camera(dt)

    anim.track = moveInput == f2.zero and 6 or 7

    if kb.pressed("Escape") then game.quit() end
end

function input()

    moveInput = f2.zero

    if kb.down("W") then moveInput.y = moveInput.y + 1 end
    if kb.down("S") then moveInput.y = moveInput.y - 1 end
    if kb.down("D") then moveInput.x = moveInput.x + 1 end
    if kb.down("A") then moveInput.x = moveInput.x - 1 end

    camRot.x = camRot.x + mouse.delta.y * sensitivity;
    camRot.y = camRot.y - mouse.delta.x * sensitivity;

    camRot.x = mt.clamp(camRot.x, 5, 60)
end

function movement(dt)

    movePos = movePos - moveInput.x * dt * moveSpeed * cam.rightFlat
                      + moveInput.y * dt * moveSpeed * cam.fwdFlat

    obj.transform.pos = f3.lerp(obj.transform.pos, movePos, dt * 15)
end

function rotation(dt)

    if moveInput ~= f2.zero then

        local fwd = cam.obj.fwd
        fwd.y = 0
        fwd = f3.normalize(fwd)

        local angle = mt.dirAngle(moveInput * -1)

        moveRot =
            quat.fromDir(fwd) * 
            quat.fromEuler(0, angle, 0) *
            quat.fromEuler(-90, 0, 90)
    end

    obj.transform.rot = quat.lerp(obj.transform.rot, moveRot, dt * 15)
end

function camera(dt)

    if mouse.scroll > 0 then camDistance = camDistance - 1 end
    if mouse.scroll < 0 then camDistance = camDistance + 1 end
    camDistance = mt.clamp(camDistance, 1, 10)

    smoothCamDistance = mt.lerp(smoothCamDistance, camDistance, dt * 15)

    cam.obj.transform.rot = quat.fromEuler(camRot.x, camRot.y, 0)
    cam.obj.transform.pos = obj.transform.pos + f3.up * 0.5 - cam.obj.fwd * smoothCamDistance
end