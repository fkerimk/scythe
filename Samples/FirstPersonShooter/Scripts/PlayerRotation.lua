local rot = f2.zero
local sensitivity = 0.3

mouse.isLocked = true

local parent = self.parent;
local player = parent.parent;
self:setParent(level.root, true)

function loop(dt)

    rot.x = mt.clamp(rot.x + mouse.delta.y * sensitivity, -89, 89)
    rot.y = rot.y - mouse.delta.x * sensitivity

    self.transform.rot = quat.fromEuler(rot.x, rot.y, 0)
    player.transform.rot = quat.fromEuler(0, rot.y, 0)

    self.transform.pos = f3.lerp(self.transform.pos, parent.transform.worldPos, dt * 15)
end