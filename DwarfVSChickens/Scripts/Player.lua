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

mouse.setVisible(false)

function loop(dt)

    input();

    mouse.moveToCenter()

    movement(dt)
    rotation(dt)

    camera()

    anim.track = moveInput == f2.zero and 6 or 7
end

function input()

    moveInput = f2.zero

    if kb.down("W") then moveInput.y = moveInput.y + 1 end
    if kb.down("S") then moveInput.y = moveInput.y - 1 end
    if kb.down("D") then moveInput.x = moveInput.x + 1 end
    if kb.down("A") then moveInput.x = moveInput.x - 1 end

    camRot.x = camRot.x - mouse.delta.y * sensitivity;
    camRot.y = camRot.y - mouse.delta.x * sensitivity;
end

function movement(dt)

    local fwd = f3.normalize(cam.obj.fwd)
    fwd.y = 0

    local right = f3.normalize(cam.obj.right)
    right.y = 0

    movePos = movePos - moveInput.x * dt * moveSpeed * right
                      + moveInput.y * dt * moveSpeed * fwd

    tr.pos = f3.lerp(tr.pos, movePos, dt * 15)
end

function rotation(dt)

    if moveInput ~= f2.zero then

        local angle = mt.dirAngle(moveInput * -1)

        moveRot = quat.fromEuler(0, angle, 0) *
                  quat.fromEuler(-90, 0, 90)
    end

    tr.rot = quat.lerp(tr.rot, moveRot, dt * 15)
end

function camera()
    
    camTr.rot = quat.fromEuler(camRot.x, camRot.y, 0)
    camTr.pos = tr.pos - f3.fromQuaternion(camTr.rot) * camDistance
end