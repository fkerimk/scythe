local tr = obj:findParentType("Transform")
local camTr = cam:findParentType("Transform")
local anim = obj:findParentType("Model"):findType("Animation")

local moveInput = f2.zero
local movePos = tr.pos
local moveRot = quat.identity
local moveSpeed = 3

local sensitivity = 0.3
local camRot = f2.zero
local camPos = camTr.pos
local camOffset = f3.new(0, 2, -3)

function loop(dt)

    input();

    movement(dt)
    rotation(dt)

    cameraRotation(dt)
    cameraFollow(dt)

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

    movePos.x = movePos.x + moveInput.x * dt * moveSpeed
    movePos.z = movePos.z + moveInput.y * dt * moveSpeed

    tr.pos = f3.lerp(tr.pos, movePos, dt * 15)
end

moveRot = quat.fromEuler(-90, 0, 0)

function rotation(dt)

    if moveInput ~= f2.zero then

        local angle = mt.dirAngle(moveInput * -1)
        
        moveRot = quat.fromEuler(0, angle, 0) *
                  quat.fromEuler(-90, 0, 90)
    end

    tr.rot = quat.lerp(tr.rot, moveRot, dt * 15)
end

function cameraRotation()
    
    camTr.rot = quat.fromEuler(camRot.x, camRot.y, 0)
    camTr.pos = tr.pos - f3.fromQuaternion(camTr.rot) * 5
end

function cameraFollow(dt)
    

    --camPos = tr.pos + camOffset
    --camTr.pos = f3.lerp(camTr.pos, camPos, dt * 15)
end