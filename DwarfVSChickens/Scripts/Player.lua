local transform = obj.findParentType("Transform")
local cameraTransform = cam.findParentType("Transform")

local movePos = transform.pos
local moveDir = f3.fwd
local camPos = cameraTransform.pos

local speed = 3
local camOffset = f3.new(0, 0, -3)

function loop(dt)

    movement(dt)
    cameraFollow(dt)
end

function movement(dt)

    local moveInput = f2.zero
    if kb.down("W") then moveInput.y = moveInput.y + 1 end
    if kb.down("S") then moveInput.y = moveInput.y - 1 end
    if kb.down("D") then moveInput.x = moveInput.x + 1 end
    if kb.down("A") then moveInput.x = moveInput.x - 1 end

    movePos.x = movePos.x + moveInput.x * dt * speed
    movePos.z = movePos.z + moveInput.y * dt * speed

    transform.pos = f3.lerp(transform.pos, movePos, dt * 15)

    print(moveInput)
    
    local rot = mt.rotDir(f3.new(moveInput.x, 0, moveInput.y))
    rot = mt.multiply(rot, mt.rotFromAxisAngle(f3.left, 90))

    transform.rot = rot
end

function cameraFollow(dt)
    
    camPos.z = transform.pos.z + camOffset.z
    cameraTransform.pos = f3.lerp(cameraTransform.pos, camPos, dt * 15)
end