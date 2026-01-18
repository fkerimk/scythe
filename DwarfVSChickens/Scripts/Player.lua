local tr = obj:findParentType("Transform")
local camTr = cam:findParentType("Transform")
local anim = obj:findParentType("Model"):findType("Animation")

local moveInput = f2.zero
local movePos = tr.pos
local moveRot = quat.fromEuler(-90, 0, 0)
local moveSpeed = 3

local sensitivity = 0.3
local camRot = f2.zero
local camDistance = 5;
local smoothCamDistance = 5;

--newObj = level:cloneObject(obj)
--newObj.setParent(level.root)
--newObj:findParentType("Script").path = ""
--newObjTr = newObj.findParentType("Transform")
--newObjTr.pos = f3.new(0, 3, 0)

mouse.setVisible(false)

function loop(dt)

    input();

    mouse.moveToCenter()

    movement(dt)
    rotation(dt)
    camera(dt)

    anim.track = moveInput == f2.zero and 6 or 7
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

    local fwd = cam.obj.fwd
    fwd.y = 0
    fwd = f3.normalize(fwd)

    local right = cam.obj.right
    right.y = 0
    right = f3.normalize(right)

    movePos = movePos - moveInput.x * dt * moveSpeed * right
                      + moveInput.y * dt * moveSpeed * fwd

    tr.pos = f3.lerp(tr.pos, movePos, dt * 15)
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

    tr.rot = quat.lerp(tr.rot, moveRot, dt * 15)
end

function camera(dt)

    if mouse.scroll > 0 then camDistance = camDistance - 1 end
    if mouse.scroll < 0 then camDistance = camDistance + 1 end
    camDistance = mt.clamp(camDistance, 1, 10)

    smoothCamDistance = mt.lerp(smoothCamDistance, camDistance, dt * 15)

    camTr.rot = quat.fromEuler(camRot.x, camRot.y, 0)
    camTr.pos = tr.pos + f3.up * 0.5 - cam.parent.fwd * smoothCamDistance
end