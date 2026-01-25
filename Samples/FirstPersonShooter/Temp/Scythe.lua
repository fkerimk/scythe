---@meta

---@type Obj
self = nil

---@type Level
level = nil

---@type Camera
cam = nil

---@type RenderSettings
renderSettings = nil

---@type LuaF2
f2 = nil

---@type LuaF3
f3 = nil

---@type LuaMt
mt = nil

---@type LuaTime
time = nil

---@type LuaKb
kb = nil

---@type LuaMouse
mouse = nil

---@type LuaQuat
quat = nil

---@type LuaGame
game = nil

---@type LuaColor
color = nil

---@class Obj
---@field icon string
---@field color Color
---@field name string
---@field components Dictionary
---@field up Vector3
---@field fwd Vector3
---@field right Vector3
---@field fwdFlat Vector3
---@field rightFlat Vector3
---@field pos Vector3
---@field rot Quaternion
---@field parent Obj
---@field children Dictionary
---@field transform Transform
---@field matrix Matrix4x4
---@field rotMatrix Matrix4x4
---@field worldMatrix Matrix4x4
---@field worldRotMatrix Matrix4x4
---@field visualWorldMatrix Matrix4x4
---@field isSelected boolean
local Obj = {}
---@return void
function Obj:delete() end

---@return void
function Obj:dispose() end

---@return void
function Obj:recordedDelete() end

---@param obj Obj
---@param keepWorld boolean
---@return void
function Obj:setParent(obj, keepWorld) end

---@param obj Obj
---@return void
function Obj:recordedSetParent(obj) end

---@param pos Vector3
---@param rot Quaternion
---@param scale Vector3
---@return void
function Obj:decomposeMatrix(pos, rot, scale) end

---@param worldPos Vector3
---@param worldRot Quaternion
---@param worldScale Vector3
---@return void
function Obj:decomposeWorldMatrix(worldPos, worldRot, worldScale) end

---@return String
function Obj:getPathFromRoot() end

---@param t Table
---@return Obj
function Obj:find(t) end

---@param t Table
---@return Component
function Obj:findComponent(t) end

---@param name string
---@return Component
function Obj:makeComponent(name) end

---@class Level
---@field name string
---@field jsonPath string
---@field isDirty boolean
---@field root Obj
---@field editorCamera CameraData
local Level = {}
---@return void
function Level:save() end

---@param pretty boolean
---@return string
function Level:toSnapshot(pretty) end

---@param name string
---@param parent Obj
---@return Obj
function Level.makeObject(name, parent) end

---@param name string
---@param parent Obj
---@return Obj
function Level.recordedMakeObject(name, parent) end

---@param t Table
---@return Obj
function Level:find(t) end

---@param t Table
---@return Component
function Level:findComponent(t) end

---@class Camera : Component
---@field labelIcon string
---@field labelColor Color
---@field fov number
---@field nearClip number
---@field farClip number
---@field isSelected boolean
---@field up Vector3
---@field fwd Vector3
---@field right Vector3
---@field fwdFlat Vector3
---@field rightFlat Vector3
---@field pos Vector3
---@field rot Quaternion
---@field cam Camera3D
---@field obj Obj
---@field isLoaded boolean
local Camera = {}
---@return void
function Camera:logic() end

---@param cam Camera3D
---@param near number
---@param far number
---@return void
function Camera.applySettings(cam, near, far) end

---@return void
function Camera:render3D() end

---@class RenderSettings
---@field ambientIntensity number
---@field ambientColor Color
---@field shadowFovScale number
---@field shadowBias number
local RenderSettings = {}
---@class LuaF2
---@field new fun(arg0: number, arg1: number): Vector2
---@field zero Vector2
---@field up Vector2
---@field down Vector2
---@field right Vector2
---@field left Vector2
local LuaF2 = {}
---@param a Vector2
---@param b Vector2
---@param t number
---@return Vector2
function LuaF2.lerp(a, b, t) end

---@class LuaF3
---@field zero Vector3
---@field up Vector3
---@field down Vector3
---@field fwd Vector3
---@field back Vector3
---@field right Vector3
---@field left Vector3
---@field new fun(arg0: number, arg1: number, arg2: number): Vector3
local LuaF3 = {}
---@param a Vector3
---@param b Vector3
---@param t number
---@return Vector3
function LuaF3.lerp(a, b, t) end

---@param q Quaternion
---@return Vector3
function LuaF3.fromQuaternion(q) end

---@param value Vector3
---@return Vector3
function LuaF3.normalize(value) end

---@class LuaMt
local LuaMt = {}
---@param a number
---@param b number
---@param t number
---@return number
function LuaMt.lerp(a, b, t) end

---@param value number
---@param min number
---@param max number
---@return number
function LuaMt.clamp(value, min, max) end

---@param dir Vector2
---@return number
function LuaMt.dirAngle(dir) end

---@param value number
---@return number
function LuaMt.sign(value) end

---@class LuaTime
---@field delta number
---@field passed number
local LuaTime = {}
---@class LuaKb
local LuaKb = {}
---@param keyName string
---@return boolean
function LuaKb:down(keyName) end

---@param keyName string
---@return boolean
function LuaKb:pressed(keyName) end

---@param keyName string
---@return boolean
function LuaKb:released(keyName) end

---@param keyName string
---@return boolean
function LuaKb:up(keyName) end

---@class LuaMouse
---@field scroll number
---@field isLocked boolean
---@field delta Vector2
local LuaMouse = {}
---@return void
function LuaMouse.loop() end

---@param visible boolean
---@return void
function LuaMouse.setVisible(visible) end

---@return void
function LuaMouse.moveToCenter() end

---@class LuaQuat
---@field identity Quaternion
local LuaQuat = {}
---@param x number
---@param y number
---@param z number
---@return Quaternion
function LuaQuat.fromEuler(x, y, z) end

---@param a Quaternion
---@param b Quaternion
---@return Quaternion
function LuaQuat.multiply(a, b) end

---@param a Quaternion
---@param b Quaternion
---@param t number
---@return Quaternion
function LuaQuat.lerp(a, b, t) end

---@param dir Vector3
---@return Quaternion
function LuaQuat.fromDir(dir) end

---@class LuaGame
local LuaGame = {}
---@return void
function LuaGame.quit() end

---@class LuaColor
local LuaColor = {}
---@param r number
---@param g number
---@param b number
---@param a number
---@return Color
function LuaColor.new(r, g, b, a) end

---@class Component
---@field labelIcon string
---@field labelColor Color
---@field isSelected boolean
---@field up Vector3
---@field fwd Vector3
---@field right Vector3
---@field fwdFlat Vector3
---@field rightFlat Vector3
---@field pos Vector3
---@field rot Quaternion
---@field obj Obj
---@field isLoaded boolean
local Component = {}
---@return boolean
function Component:load() end

---@param is2D boolean
---@param isLogic boolean
---@param isRender boolean
---@return void
function Component:loop(is2D, isLogic, isRender) end

---@return void
function Component:logic() end

---@return void
function Component:render3D() end

---@return void
function Component:render2D() end

---@return void
function Component:unload() end

---@return void
function Component:quit() end

---@return void
function Component:unloadAndQuit() end

---@class Animation : Component
---@field labelIcon string
---@field labelColor Color
---@field path string
---@field track number
---@field isPlaying boolean
---@field looping boolean
---@field isSelected boolean
---@field up Vector3
---@field fwd Vector3
---@field right Vector3
---@field fwdFlat Vector3
---@field rightFlat Vector3
---@field pos Vector3
---@field rot Quaternion
---@field obj Obj
---@field isLoaded boolean
local Animation = {}
---@return boolean
function Animation:load() end

---@param trackIndex number
---@param blendTime number
---@return void
function Animation:play(trackIndex, blendTime) end

---@return void
function Animation:logic() end

---@class Light : Component
---@field labelIcon string
---@field labelColor Color
---@field enabled boolean
---@field type number
---@field scytheColor Color
---@field intensity number
---@field range number
---@field shadows boolean
---@field shadowStrength number
---@field isSelected boolean
---@field up Vector3
---@field fwd Vector3
---@field right Vector3
---@field fwdFlat Vector3
---@field rightFlat Vector3
---@field pos Vector3
---@field rot Quaternion
---@field obj Obj
---@field isLoaded boolean
local Light = {}
---@param index number
---@return void
function Light:update(index) end

---@return void
function Light:logic() end

---@return void
function Light:render3D() end

---@class Model : Component
---@field labelIcon string
---@field labelColor Color
---@field path string
---@field color Color
---@field isTransparent boolean
---@field alphaCutoff number
---@field castShadows boolean
---@field receiveShadows boolean
---@field isSelected boolean
---@field up Vector3
---@field fwd Vector3
---@field right Vector3
---@field fwdFlat Vector3
---@field rightFlat Vector3
---@field pos Vector3
---@field rot Quaternion
---@field meshes List
---@field bones List
---@field boneMap Dictionary
---@field assetRef ModelAsset
---@field obj Obj
---@field isLoaded boolean
local Model = {}
---@return boolean
function Model:load() end

---@return void
function Model:logic() end

---@return void
function Model:render3D() end

---@return void
function Model:drawTransparent() end

---@return void
function Model:drawShadow() end

---@param overrideAlphaCutoff Nullable
---@return void
function Model:draw(overrideAlphaCutoff) end

---@return void
function Model:unload() end

---@class Script : Component
---@field labelIcon string
---@field labelColor Color
---@field path string
---@field isSelected boolean
---@field up Vector3
---@field fwd Vector3
---@field right Vector3
---@field fwdFlat Vector3
---@field rightFlat Vector3
---@field pos Vector3
---@field rot Quaternion
---@field luaScript Script
---@field luaLoop DynValue
---@field luaMt LuaMt
---@field luaTime LuaTime
---@field luaKb LuaKb
---@field luaMouse LuaMouse
---@field luaF2 LuaF2
---@field luaF3 LuaF3
---@field luaQuat LuaQuat
---@field luaGame LuaGame
---@field luaColor LuaColor
---@field obj Obj
---@field isLoaded boolean
local Script = {}
---@return void
function Script.register() end

---@return boolean
function Script:load() end

---@return void
function Script:logic() end

---@class Transform : Component
---@field labelIcon string
---@field labelColor Color
---@field pos Vector3
---@field euler Vector3
---@field scale Vector3
---@field rot Quaternion
---@field worldPos Vector3
---@field worldRot Quaternion
---@field worldEuler Vector3
---@field worldScale Vector3
---@field isHovered boolean
---@field isDragging boolean
---@field isSelected boolean
---@field up Vector3
---@field fwd Vector3
---@field right Vector3
---@field fwdFlat Vector3
---@field rightFlat Vector3
---@field obj Obj
---@field isLoaded boolean
local Transform = {}
---@return void
function Transform:updateTransform() end

---@return void
function Transform:logic() end

---@return void
function Transform:render3D() end

---@return void
function Transform:render2D() end

---@param deg number
---@return void
function Transform:rotateX(deg) end

---@param deg number
---@return void
function Transform:rotateY(deg) end

---@param deg number
---@return void
function Transform:rotateZ(deg) end

---@param x number
---@param y number
---@param z number
---@return void
function Transform:addEuler(x, y, z) end

---@class Rigidbody : Component
---@field labelColor Color
---@field labelIcon string
---@field isStatic boolean
---@field gravity boolean
---@field friction number
---@field bounciness number
---@field freezePosX boolean
---@field freezePosY boolean
---@field freezePosZ boolean
---@field freezeRotX boolean
---@field freezeRotY boolean
---@field freezeRotZ boolean
---@field velocity Vector3
---@field angularVelocity Vector3
---@field isSelected boolean
---@field up Vector3
---@field fwd Vector3
---@field right Vector3
---@field fwdFlat Vector3
---@field rightFlat Vector3
---@field pos Vector3
---@field rot Quaternion
---@field body RigidBody
---@field obj Obj
---@field isLoaded boolean
local Rigidbody = {}
---@param force Vector3
---@return void
function Rigidbody:addForce(force) end

---@return boolean
function Rigidbody:load() end

---@return void
function Rigidbody:logic() end

---@return void
function Rigidbody:unload() end

---@class BoxCollider : Component
---@field labelColor Color
---@field labelIcon string
---@field size Vector3
---@field center Vector3
---@field isSelected boolean
---@field up Vector3
---@field fwd Vector3
---@field right Vector3
---@field fwdFlat Vector3
---@field rightFlat Vector3
---@field pos Vector3
---@field rot Quaternion
---@field shape BoxShape
---@field obj Obj
---@field isLoaded boolean
local BoxCollider = {}
---@return boolean
function BoxCollider:load() end

---@return void
function BoxCollider:render3D() end

---@class SphereCollider : Component
---@field labelColor Color
---@field labelIcon string
---@field radius number
---@field center Vector3
---@field isSelected boolean
---@field up Vector3
---@field fwd Vector3
---@field right Vector3
---@field fwdFlat Vector3
---@field rightFlat Vector3
---@field pos Vector3
---@field rot Quaternion
---@field shape SphereShape
---@field obj Obj
---@field isLoaded boolean
local SphereCollider = {}
---@return boolean
function SphereCollider:load() end

---@return void
function SphereCollider:render3D() end

---@class RelativePathConverter : JsonConverter
---@field canRead boolean
---@field canWrite boolean
local RelativePathConverter = {}
---@param objectType Type
---@return boolean
function RelativePathConverter:canConvert(objectType) end

---@param reader JsonReader
---@param objectType Type
---@param existingValue Object
---@param serializer JsonSerializer
---@return Object
function RelativePathConverter:readJson(reader, objectType, existingValue, serializer) end

---@param writer JsonWriter
---@param value Object
---@param serializer JsonSerializer
---@return void
function RelativePathConverter:writeJson(writer, value, serializer) end

---@class CameraData
---@field position Vector3
---@field rotation Vector2
local CameraData = {}
---@class Vector2
---@field allBitsSet Vector2
---@field e Vector2
---@field epsilon Vector2
---@field naN Vector2
---@field negativeInfinity Vector2
---@field negativeZero Vector2
---@field one Vector2
---@field pi Vector2
---@field positiveInfinity Vector2
---@field tau Vector2
---@field unitX Vector2
---@field unitY Vector2
---@field zero Vector2
---@field item number
---@field x number
---@field y number
local Vector2 = {}
---@param value Vector2
---@return Vector2
function Vector2.abs(value) end

---@param left Vector2
---@param right Vector2
---@return Vector2
function Vector2.add(left, right) end

---@param vector Vector2
---@param value number
---@return boolean
function Vector2.all(vector, value) end

---@param vector Vector2
---@return boolean
function Vector2.allWhereAllBitsSet(vector) end

---@param left Vector2
---@param right Vector2
---@return Vector2
function Vector2.andNot(left, right) end

---@param vector Vector2
---@param value number
---@return boolean
function Vector2.any(vector, value) end

---@param vector Vector2
---@return boolean
function Vector2.anyWhereAllBitsSet(vector) end

---@param left Vector2
---@param right Vector2
---@return Vector2
function Vector2.bitwiseAnd(left, right) end

---@param left Vector2
---@param right Vector2
---@return Vector2
function Vector2.bitwiseOr(left, right) end

---@param value1 Vector2
---@param min Vector2
---@param max Vector2
---@return Vector2
function Vector2.clamp(value1, min, max) end

---@param value1 Vector2
---@param min Vector2
---@param max Vector2
---@return Vector2
function Vector2.clampNative(value1, min, max) end

---@param condition Vector2
---@param left Vector2
---@param right Vector2
---@return Vector2
function Vector2.conditionalSelect(condition, left, right) end

---@param value Vector2
---@param sign Vector2
---@return Vector2
function Vector2.copySign(value, sign) end

---@param vector Vector2
---@return Vector2
function Vector2.cos(vector) end

---@param vector Vector2
---@param value number
---@return number
function Vector2.count(vector, value) end

---@param vector Vector2
---@return number
function Vector2.countWhereAllBitsSet(vector) end

---@param value number
---@return Vector2
function Vector2.create(value) end

---@param x number
---@return Vector2
function Vector2.createScalar(x) end

---@param x number
---@return Vector2
function Vector2.createScalarUnsafe(x) end

---@param value1 Vector2
---@param value2 Vector2
---@return number
function Vector2.cross(value1, value2) end

---@param degrees Vector2
---@return Vector2
function Vector2.degreesToRadians(degrees) end

---@param value1 Vector2
---@param value2 Vector2
---@return number
function Vector2.distance(value1, value2) end

---@param value1 Vector2
---@param value2 Vector2
---@return number
function Vector2.distanceSquared(value1, value2) end

---@param left Vector2
---@param right Vector2
---@return Vector2
function Vector2.divide(left, right) end

---@param value1 Vector2
---@param value2 Vector2
---@return number
function Vector2.dot(value1, value2) end

---@param vector Vector2
---@return Vector2
function Vector2.exp(vector) end

---@param left Vector2
---@param right Vector2
---@return Vector2
function Vector2.equals(left, right) end

---@param left Vector2
---@param right Vector2
---@return boolean
function Vector2.equalsAll(left, right) end

---@param left Vector2
---@param right Vector2
---@return boolean
function Vector2.equalsAny(left, right) end

---@param left Vector2
---@param right Vector2
---@param addend Vector2
---@return Vector2
function Vector2.fusedMultiplyAdd(left, right, addend) end

---@param left Vector2
---@param right Vector2
---@return Vector2
function Vector2.greaterThan(left, right) end

---@param left Vector2
---@param right Vector2
---@return boolean
function Vector2.greaterThanAll(left, right) end

---@param left Vector2
---@param right Vector2
---@return boolean
function Vector2.greaterThanAny(left, right) end

---@param left Vector2
---@param right Vector2
---@return Vector2
function Vector2.greaterThanOrEqual(left, right) end

---@param left Vector2
---@param right Vector2
---@return boolean
function Vector2.greaterThanOrEqualAll(left, right) end

---@param left Vector2
---@param right Vector2
---@return boolean
function Vector2.greaterThanOrEqualAny(left, right) end

---@param x Vector2
---@param y Vector2
---@return Vector2
function Vector2.hypot(x, y) end

---@param vector Vector2
---@param value number
---@return number
function Vector2.indexOf(vector, value) end

---@param vector Vector2
---@return number
function Vector2.indexOfWhereAllBitsSet(vector) end

---@param vector Vector2
---@return Vector2
function Vector2.isEvenInteger(vector) end

---@param vector Vector2
---@return Vector2
function Vector2.isFinite(vector) end

---@param vector Vector2
---@return Vector2
function Vector2.isInfinity(vector) end

---@param vector Vector2
---@return Vector2
function Vector2.isInteger(vector) end

---@param vector Vector2
---@return Vector2
function Vector2.isNaN(vector) end

---@param vector Vector2
---@return Vector2
function Vector2.isNegative(vector) end

---@param vector Vector2
---@return Vector2
function Vector2.isNegativeInfinity(vector) end

---@param vector Vector2
---@return Vector2
function Vector2.isNormal(vector) end

---@param vector Vector2
---@return Vector2
function Vector2.isOddInteger(vector) end

---@param vector Vector2
---@return Vector2
function Vector2.isPositive(vector) end

---@param vector Vector2
---@return Vector2
function Vector2.isPositiveInfinity(vector) end

---@param vector Vector2
---@return Vector2
function Vector2.isSubnormal(vector) end

---@param vector Vector2
---@return Vector2
function Vector2.isZero(vector) end

---@param vector Vector2
---@param value number
---@return number
function Vector2.lastIndexOf(vector, value) end

---@param vector Vector2
---@return number
function Vector2.lastIndexOfWhereAllBitsSet(vector) end

---@param value1 Vector2
---@param value2 Vector2
---@param amount number
---@return Vector2
function Vector2.lerp(value1, value2, amount) end

---@param left Vector2
---@param right Vector2
---@return Vector2
function Vector2.lessThan(left, right) end

---@param left Vector2
---@param right Vector2
---@return boolean
function Vector2.lessThanAll(left, right) end

---@param left Vector2
---@param right Vector2
---@return boolean
function Vector2.lessThanAny(left, right) end

---@param left Vector2
---@param right Vector2
---@return Vector2
function Vector2.lessThanOrEqual(left, right) end

---@param left Vector2
---@param right Vector2
---@return boolean
function Vector2.lessThanOrEqualAll(left, right) end

---@param left Vector2
---@param right Vector2
---@return boolean
function Vector2.lessThanOrEqualAny(left, right) end

---@param source Single
---@return Vector2
function Vector2.load(source) end

---@param source Single
---@return Vector2
function Vector2.loadAligned(source) end

---@param source Single
---@return Vector2
function Vector2.loadAlignedNonTemporal(source) end

---@param source number
---@return Vector2
function Vector2.loadUnsafe(source) end

---@param vector Vector2
---@return Vector2
function Vector2.log(vector) end

---@param vector Vector2
---@return Vector2
function Vector2.log2(vector) end

---@param value1 Vector2
---@param value2 Vector2
---@return Vector2
function Vector2.max(value1, value2) end

---@param value1 Vector2
---@param value2 Vector2
---@return Vector2
function Vector2.maxMagnitude(value1, value2) end

---@param value1 Vector2
---@param value2 Vector2
---@return Vector2
function Vector2.maxMagnitudeNumber(value1, value2) end

---@param value1 Vector2
---@param value2 Vector2
---@return Vector2
function Vector2.maxNative(value1, value2) end

---@param value1 Vector2
---@param value2 Vector2
---@return Vector2
function Vector2.maxNumber(value1, value2) end

---@param value1 Vector2
---@param value2 Vector2
---@return Vector2
function Vector2.min(value1, value2) end

---@param value1 Vector2
---@param value2 Vector2
---@return Vector2
function Vector2.minMagnitude(value1, value2) end

---@param value1 Vector2
---@param value2 Vector2
---@return Vector2
function Vector2.minMagnitudeNumber(value1, value2) end

---@param value1 Vector2
---@param value2 Vector2
---@return Vector2
function Vector2.minNative(value1, value2) end

---@param value1 Vector2
---@param value2 Vector2
---@return Vector2
function Vector2.minNumber(value1, value2) end

---@param left Vector2
---@param right Vector2
---@return Vector2
function Vector2.multiply(left, right) end

---@param left Vector2
---@param right Vector2
---@param addend Vector2
---@return Vector2
function Vector2.multiplyAddEstimate(left, right, addend) end

---@param value Vector2
---@return Vector2
function Vector2.negate(value) end

---@param vector Vector2
---@param value number
---@return boolean
function Vector2.none(vector, value) end

---@param vector Vector2
---@return boolean
function Vector2.noneWhereAllBitsSet(vector) end

---@param value Vector2
---@return Vector2
function Vector2.normalize(value) end

---@param value Vector2
---@return Vector2
function Vector2.onesComplement(value) end

---@param radians Vector2
---@return Vector2
function Vector2.radiansToDegrees(radians) end

---@param vector Vector2
---@param normal Vector2
---@return Vector2
function Vector2.reflect(vector, normal) end

---@param vector Vector2
---@return Vector2
function Vector2.round(vector) end

---@param vector Vector2
---@param xIndex number
---@param yIndex number
---@return Vector2
function Vector2.shuffle(vector, xIndex, yIndex) end

---@param vector Vector2
---@return Vector2
function Vector2.sin(vector) end

---@param vector Vector2
---@return ValueTuple
function Vector2.sinCos(vector) end

---@param value Vector2
---@return Vector2
function Vector2.squareRoot(value) end

---@param left Vector2
---@param right Vector2
---@return Vector2
function Vector2.subtract(left, right) end

---@param value Vector2
---@return number
function Vector2.sum(value) end

---@param position Vector2
---@param matrix Matrix3x2
---@return Vector2
function Vector2.transform(position, matrix) end

---@param normal Vector2
---@param matrix Matrix3x2
---@return Vector2
function Vector2.transformNormal(normal, matrix) end

---@param vector Vector2
---@return Vector2
function Vector2.truncate(vector) end

---@param left Vector2
---@param right Vector2
---@return Vector2
function Vector2.xor(left, right) end

---@param array Single
---@return void
function Vector2:copyTo(array) end

---@param destination Span
---@return boolean
function Vector2:tryCopyTo(destination) end

---@return number
function Vector2:getHashCode() end

---@return number
function Vector2:length() end

---@return number
function Vector2:lengthSquared() end

---@return string
function Vector2:toString() end

---@class Vector3
---@field allBitsSet Vector3
---@field e Vector3
---@field epsilon Vector3
---@field naN Vector3
---@field negativeInfinity Vector3
---@field negativeZero Vector3
---@field one Vector3
---@field pi Vector3
---@field positiveInfinity Vector3
---@field tau Vector3
---@field unitX Vector3
---@field unitY Vector3
---@field unitZ Vector3
---@field zero Vector3
---@field item number
---@field x number
---@field y number
---@field z number
local Vector3 = {}
---@param value Vector3
---@return Vector3
function Vector3.abs(value) end

---@param left Vector3
---@param right Vector3
---@return Vector3
function Vector3.add(left, right) end

---@param vector Vector3
---@param value number
---@return boolean
function Vector3.all(vector, value) end

---@param vector Vector3
---@return boolean
function Vector3.allWhereAllBitsSet(vector) end

---@param left Vector3
---@param right Vector3
---@return Vector3
function Vector3.andNot(left, right) end

---@param vector Vector3
---@param value number
---@return boolean
function Vector3.any(vector, value) end

---@param vector Vector3
---@return boolean
function Vector3.anyWhereAllBitsSet(vector) end

---@param left Vector3
---@param right Vector3
---@return Vector3
function Vector3.bitwiseAnd(left, right) end

---@param left Vector3
---@param right Vector3
---@return Vector3
function Vector3.bitwiseOr(left, right) end

---@param value1 Vector3
---@param min Vector3
---@param max Vector3
---@return Vector3
function Vector3.clamp(value1, min, max) end

---@param value1 Vector3
---@param min Vector3
---@param max Vector3
---@return Vector3
function Vector3.clampNative(value1, min, max) end

---@param condition Vector3
---@param left Vector3
---@param right Vector3
---@return Vector3
function Vector3.conditionalSelect(condition, left, right) end

---@param value Vector3
---@param sign Vector3
---@return Vector3
function Vector3.copySign(value, sign) end

---@param vector Vector3
---@return Vector3
function Vector3.cos(vector) end

---@param vector Vector3
---@param value number
---@return number
function Vector3.count(vector, value) end

---@param vector Vector3
---@return number
function Vector3.countWhereAllBitsSet(vector) end

---@param value number
---@return Vector3
function Vector3.create(value) end

---@param x number
---@return Vector3
function Vector3.createScalar(x) end

---@param x number
---@return Vector3
function Vector3.createScalarUnsafe(x) end

---@param vector1 Vector3
---@param vector2 Vector3
---@return Vector3
function Vector3.cross(vector1, vector2) end

---@param degrees Vector3
---@return Vector3
function Vector3.degreesToRadians(degrees) end

---@param value1 Vector3
---@param value2 Vector3
---@return number
function Vector3.distance(value1, value2) end

---@param value1 Vector3
---@param value2 Vector3
---@return number
function Vector3.distanceSquared(value1, value2) end

---@param left Vector3
---@param right Vector3
---@return Vector3
function Vector3.divide(left, right) end

---@param vector1 Vector3
---@param vector2 Vector3
---@return number
function Vector3.dot(vector1, vector2) end

---@param vector Vector3
---@return Vector3
function Vector3.exp(vector) end

---@param left Vector3
---@param right Vector3
---@return Vector3
function Vector3.equals(left, right) end

---@param left Vector3
---@param right Vector3
---@return boolean
function Vector3.equalsAll(left, right) end

---@param left Vector3
---@param right Vector3
---@return boolean
function Vector3.equalsAny(left, right) end

---@param left Vector3
---@param right Vector3
---@param addend Vector3
---@return Vector3
function Vector3.fusedMultiplyAdd(left, right, addend) end

---@param left Vector3
---@param right Vector3
---@return Vector3
function Vector3.greaterThan(left, right) end

---@param left Vector3
---@param right Vector3
---@return boolean
function Vector3.greaterThanAll(left, right) end

---@param left Vector3
---@param right Vector3
---@return boolean
function Vector3.greaterThanAny(left, right) end

---@param left Vector3
---@param right Vector3
---@return Vector3
function Vector3.greaterThanOrEqual(left, right) end

---@param left Vector3
---@param right Vector3
---@return boolean
function Vector3.greaterThanOrEqualAll(left, right) end

---@param left Vector3
---@param right Vector3
---@return boolean
function Vector3.greaterThanOrEqualAny(left, right) end

---@param x Vector3
---@param y Vector3
---@return Vector3
function Vector3.hypot(x, y) end

---@param vector Vector3
---@param value number
---@return number
function Vector3.indexOf(vector, value) end

---@param vector Vector3
---@return number
function Vector3.indexOfWhereAllBitsSet(vector) end

---@param vector Vector3
---@return Vector3
function Vector3.isEvenInteger(vector) end

---@param vector Vector3
---@return Vector3
function Vector3.isFinite(vector) end

---@param vector Vector3
---@return Vector3
function Vector3.isInfinity(vector) end

---@param vector Vector3
---@return Vector3
function Vector3.isInteger(vector) end

---@param vector Vector3
---@return Vector3
function Vector3.isNaN(vector) end

---@param vector Vector3
---@return Vector3
function Vector3.isNegative(vector) end

---@param vector Vector3
---@return Vector3
function Vector3.isNegativeInfinity(vector) end

---@param vector Vector3
---@return Vector3
function Vector3.isNormal(vector) end

---@param vector Vector3
---@return Vector3
function Vector3.isOddInteger(vector) end

---@param vector Vector3
---@return Vector3
function Vector3.isPositive(vector) end

---@param vector Vector3
---@return Vector3
function Vector3.isPositiveInfinity(vector) end

---@param vector Vector3
---@return Vector3
function Vector3.isSubnormal(vector) end

---@param vector Vector3
---@return Vector3
function Vector3.isZero(vector) end

---@param vector Vector3
---@param value number
---@return number
function Vector3.lastIndexOf(vector, value) end

---@param vector Vector3
---@return number
function Vector3.lastIndexOfWhereAllBitsSet(vector) end

---@param value1 Vector3
---@param value2 Vector3
---@param amount number
---@return Vector3
function Vector3.lerp(value1, value2, amount) end

---@param left Vector3
---@param right Vector3
---@return Vector3
function Vector3.lessThan(left, right) end

---@param left Vector3
---@param right Vector3
---@return boolean
function Vector3.lessThanAll(left, right) end

---@param left Vector3
---@param right Vector3
---@return boolean
function Vector3.lessThanAny(left, right) end

---@param left Vector3
---@param right Vector3
---@return Vector3
function Vector3.lessThanOrEqual(left, right) end

---@param left Vector3
---@param right Vector3
---@return boolean
function Vector3.lessThanOrEqualAll(left, right) end

---@param left Vector3
---@param right Vector3
---@return boolean
function Vector3.lessThanOrEqualAny(left, right) end

---@param source Single
---@return Vector3
function Vector3.load(source) end

---@param source Single
---@return Vector3
function Vector3.loadAligned(source) end

---@param source Single
---@return Vector3
function Vector3.loadAlignedNonTemporal(source) end

---@param source number
---@return Vector3
function Vector3.loadUnsafe(source) end

---@param vector Vector3
---@return Vector3
function Vector3.log(vector) end

---@param vector Vector3
---@return Vector3
function Vector3.log2(vector) end

---@param value1 Vector3
---@param value2 Vector3
---@return Vector3
function Vector3.max(value1, value2) end

---@param value1 Vector3
---@param value2 Vector3
---@return Vector3
function Vector3.maxMagnitude(value1, value2) end

---@param value1 Vector3
---@param value2 Vector3
---@return Vector3
function Vector3.maxMagnitudeNumber(value1, value2) end

---@param value1 Vector3
---@param value2 Vector3
---@return Vector3
function Vector3.maxNative(value1, value2) end

---@param value1 Vector3
---@param value2 Vector3
---@return Vector3
function Vector3.maxNumber(value1, value2) end

---@param value1 Vector3
---@param value2 Vector3
---@return Vector3
function Vector3.min(value1, value2) end

---@param value1 Vector3
---@param value2 Vector3
---@return Vector3
function Vector3.minMagnitude(value1, value2) end

---@param value1 Vector3
---@param value2 Vector3
---@return Vector3
function Vector3.minMagnitudeNumber(value1, value2) end

---@param value1 Vector3
---@param value2 Vector3
---@return Vector3
function Vector3.minNative(value1, value2) end

---@param value1 Vector3
---@param value2 Vector3
---@return Vector3
function Vector3.minNumber(value1, value2) end

---@param left Vector3
---@param right Vector3
---@return Vector3
function Vector3.multiply(left, right) end

---@param left Vector3
---@param right Vector3
---@param addend Vector3
---@return Vector3
function Vector3.multiplyAddEstimate(left, right, addend) end

---@param value Vector3
---@return Vector3
function Vector3.negate(value) end

---@param vector Vector3
---@param value number
---@return boolean
function Vector3.none(vector, value) end

---@param vector Vector3
---@return boolean
function Vector3.noneWhereAllBitsSet(vector) end

---@param value Vector3
---@return Vector3
function Vector3.normalize(value) end

---@param value Vector3
---@return Vector3
function Vector3.onesComplement(value) end

---@param radians Vector3
---@return Vector3
function Vector3.radiansToDegrees(radians) end

---@param vector Vector3
---@param normal Vector3
---@return Vector3
function Vector3.reflect(vector, normal) end

---@param vector Vector3
---@return Vector3
function Vector3.round(vector) end

---@param vector Vector3
---@param xIndex number
---@param yIndex number
---@param zIndex number
---@return Vector3
function Vector3.shuffle(vector, xIndex, yIndex, zIndex) end

---@param vector Vector3
---@return Vector3
function Vector3.sin(vector) end

---@param vector Vector3
---@return ValueTuple
function Vector3.sinCos(vector) end

---@param value Vector3
---@return Vector3
function Vector3.squareRoot(value) end

---@param left Vector3
---@param right Vector3
---@return Vector3
function Vector3.subtract(left, right) end

---@param value Vector3
---@return number
function Vector3.sum(value) end

---@param position Vector3
---@param matrix Matrix4x4
---@return Vector3
function Vector3.transform(position, matrix) end

---@param normal Vector3
---@param matrix Matrix4x4
---@return Vector3
function Vector3.transformNormal(normal, matrix) end

---@param vector Vector3
---@return Vector3
function Vector3.truncate(vector) end

---@param left Vector3
---@param right Vector3
---@return Vector3
function Vector3.xor(left, right) end

---@param array Single
---@return void
function Vector3:copyTo(array) end

---@param destination Span
---@return boolean
function Vector3:tryCopyTo(destination) end

---@return number
function Vector3:getHashCode() end

---@return number
function Vector3:length() end

---@return number
function Vector3:lengthSquared() end

---@return string
function Vector3:toString() end

---@class Quaternion
---@field zero Quaternion
---@field identity Quaternion
---@field item number
---@field isIdentity boolean
---@field x number
---@field y number
---@field z number
---@field w number
local Quaternion = {}
---@param value1 Quaternion
---@param value2 Quaternion
---@return Quaternion
function Quaternion.add(value1, value2) end

---@param value1 Quaternion
---@param value2 Quaternion
---@return Quaternion
function Quaternion.concatenate(value1, value2) end

---@param value Quaternion
---@return Quaternion
function Quaternion.conjugate(value) end

---@param x number
---@param y number
---@param z number
---@param w number
---@return Quaternion
function Quaternion.create(x, y, z, w) end

---@param axis Vector3
---@param angle number
---@return Quaternion
function Quaternion.createFromAxisAngle(axis, angle) end

---@param matrix Matrix4x4
---@return Quaternion
function Quaternion.createFromRotationMatrix(matrix) end

---@param yaw number
---@param pitch number
---@param roll number
---@return Quaternion
function Quaternion.createFromYawPitchRoll(yaw, pitch, roll) end

---@param value1 Quaternion
---@param value2 Quaternion
---@return Quaternion
function Quaternion.divide(value1, value2) end

---@param quaternion1 Quaternion
---@param quaternion2 Quaternion
---@return number
function Quaternion.dot(quaternion1, quaternion2) end

---@param value Quaternion
---@return Quaternion
function Quaternion.inverse(value) end

---@param quaternion1 Quaternion
---@param quaternion2 Quaternion
---@param amount number
---@return Quaternion
function Quaternion.lerp(quaternion1, quaternion2, amount) end

---@param value1 Quaternion
---@param value2 Quaternion
---@return Quaternion
function Quaternion.multiply(value1, value2) end

---@param value Quaternion
---@return Quaternion
function Quaternion.negate(value) end

---@param value Quaternion
---@return Quaternion
function Quaternion.normalize(value) end

---@param quaternion1 Quaternion
---@param quaternion2 Quaternion
---@param amount number
---@return Quaternion
function Quaternion.slerp(quaternion1, quaternion2, amount) end

---@param value1 Quaternion
---@param value2 Quaternion
---@return Quaternion
function Quaternion.subtract(value1, value2) end

---@param obj Object
---@return boolean
function Quaternion:equals(obj) end

---@return number
function Quaternion:getHashCode() end

---@return number
function Quaternion:length() end

---@return number
function Quaternion:lengthSquared() end

---@return string
function Quaternion:toString() end

---@class Color
---@field r number
---@field g number
---@field b number
---@field a number
---@field lightGray Color
---@field gray Color
---@field darkGray Color
---@field yellow Color
---@field gold Color
---@field orange Color
---@field pink Color
---@field red Color
---@field maroon Color
---@field green Color
---@field lime Color
---@field darkGreen Color
---@field skyBlue Color
---@field blue Color
---@field darkBlue Color
---@field purple Color
---@field violet Color
---@field darkPurple Color
---@field beige Color
---@field brown Color
---@field darkBrown Color
---@field white Color
---@field black Color
---@field blank Color
---@field magenta Color
---@field rayWhite Color
local Color = {}
---@param h number
---@param s number
---@param v number
---@return void
function Color:getHSV(h, s, v) end

---@param h number
---@param s number
---@param v number
---@return Color
function Color.fromHSV(h, s, v) end

---@param origin Color
---@param target Color
---@param t number
---@return Color
function Color.lerp(origin, target, t) end

---@return string
function Color:toString() end

---@class Matrix4x4
---@field identity Matrix4x4
---@field isIdentity boolean
---@field translation Vector3
---@field x Vector4
---@field y Vector4
---@field z Vector4
---@field w Vector4
---@field item Vector4
---@field m11 number
---@field m12 number
---@field m13 number
---@field m14 number
---@field m21 number
---@field m22 number
---@field m23 number
---@field m24 number
---@field m31 number
---@field m32 number
---@field m33 number
---@field m34 number
---@field m41 number
---@field m42 number
---@field m43 number
---@field m44 number
local Matrix4x4 = {}
---@param value1 Matrix4x4
---@param value2 Matrix4x4
---@return Matrix4x4
function Matrix4x4.add(value1, value2) end

---@param value number
---@return Matrix4x4
function Matrix4x4.create(value) end

---@param objectPosition Vector3
---@param cameraPosition Vector3
---@param cameraUpVector Vector3
---@param cameraForwardVector Vector3
---@return Matrix4x4
function Matrix4x4.createBillboard(objectPosition, cameraPosition, cameraUpVector, cameraForwardVector) end

---@param objectPosition Vector3
---@param cameraPosition Vector3
---@param cameraUpVector Vector3
---@param cameraForwardVector Vector3
---@return Matrix4x4
function Matrix4x4.createBillboardLeftHanded(objectPosition, cameraPosition, cameraUpVector, cameraForwardVector) end

---@param objectPosition Vector3
---@param cameraPosition Vector3
---@param rotateAxis Vector3
---@param cameraForwardVector Vector3
---@param objectForwardVector Vector3
---@return Matrix4x4
function Matrix4x4.createConstrainedBillboard(objectPosition, cameraPosition, rotateAxis, cameraForwardVector, objectForwardVector) end

---@param objectPosition Vector3
---@param cameraPosition Vector3
---@param rotateAxis Vector3
---@param cameraForwardVector Vector3
---@param objectForwardVector Vector3
---@return Matrix4x4
function Matrix4x4.createConstrainedBillboardLeftHanded(objectPosition, cameraPosition, rotateAxis, cameraForwardVector, objectForwardVector) end

---@param axis Vector3
---@param angle number
---@return Matrix4x4
function Matrix4x4.createFromAxisAngle(axis, angle) end

---@param quaternion Quaternion
---@return Matrix4x4
function Matrix4x4.createFromQuaternion(quaternion) end

---@param yaw number
---@param pitch number
---@param roll number
---@return Matrix4x4
function Matrix4x4.createFromYawPitchRoll(yaw, pitch, roll) end

---@param cameraPosition Vector3
---@param cameraTarget Vector3
---@param cameraUpVector Vector3
---@return Matrix4x4
function Matrix4x4.createLookAt(cameraPosition, cameraTarget, cameraUpVector) end

---@param cameraPosition Vector3
---@param cameraTarget Vector3
---@param cameraUpVector Vector3
---@return Matrix4x4
function Matrix4x4.createLookAtLeftHanded(cameraPosition, cameraTarget, cameraUpVector) end

---@param cameraPosition Vector3
---@param cameraDirection Vector3
---@param cameraUpVector Vector3
---@return Matrix4x4
function Matrix4x4.createLookTo(cameraPosition, cameraDirection, cameraUpVector) end

---@param cameraPosition Vector3
---@param cameraDirection Vector3
---@param cameraUpVector Vector3
---@return Matrix4x4
function Matrix4x4.createLookToLeftHanded(cameraPosition, cameraDirection, cameraUpVector) end

---@param width number
---@param height number
---@param zNearPlane number
---@param zFarPlane number
---@return Matrix4x4
function Matrix4x4.createOrthographic(width, height, zNearPlane, zFarPlane) end

---@param width number
---@param height number
---@param zNearPlane number
---@param zFarPlane number
---@return Matrix4x4
function Matrix4x4.createOrthographicLeftHanded(width, height, zNearPlane, zFarPlane) end

---@param left number
---@param right number
---@param bottom number
---@param top number
---@param zNearPlane number
---@param zFarPlane number
---@return Matrix4x4
function Matrix4x4.createOrthographicOffCenter(left, right, bottom, top, zNearPlane, zFarPlane) end

---@param left number
---@param right number
---@param bottom number
---@param top number
---@param zNearPlane number
---@param zFarPlane number
---@return Matrix4x4
function Matrix4x4.createOrthographicOffCenterLeftHanded(left, right, bottom, top, zNearPlane, zFarPlane) end

---@param width number
---@param height number
---@param nearPlaneDistance number
---@param farPlaneDistance number
---@return Matrix4x4
function Matrix4x4.createPerspective(width, height, nearPlaneDistance, farPlaneDistance) end

---@param width number
---@param height number
---@param nearPlaneDistance number
---@param farPlaneDistance number
---@return Matrix4x4
function Matrix4x4.createPerspectiveLeftHanded(width, height, nearPlaneDistance, farPlaneDistance) end

---@param fieldOfView number
---@param aspectRatio number
---@param nearPlaneDistance number
---@param farPlaneDistance number
---@return Matrix4x4
function Matrix4x4.createPerspectiveFieldOfView(fieldOfView, aspectRatio, nearPlaneDistance, farPlaneDistance) end

---@param fieldOfView number
---@param aspectRatio number
---@param nearPlaneDistance number
---@param farPlaneDistance number
---@return Matrix4x4
function Matrix4x4.createPerspectiveFieldOfViewLeftHanded(fieldOfView, aspectRatio, nearPlaneDistance, farPlaneDistance) end

---@param left number
---@param right number
---@param bottom number
---@param top number
---@param nearPlaneDistance number
---@param farPlaneDistance number
---@return Matrix4x4
function Matrix4x4.createPerspectiveOffCenter(left, right, bottom, top, nearPlaneDistance, farPlaneDistance) end

---@param left number
---@param right number
---@param bottom number
---@param top number
---@param nearPlaneDistance number
---@param farPlaneDistance number
---@return Matrix4x4
function Matrix4x4.createPerspectiveOffCenterLeftHanded(left, right, bottom, top, nearPlaneDistance, farPlaneDistance) end

---@param value Plane
---@return Matrix4x4
function Matrix4x4.createReflection(value) end

---@param radians number
---@return Matrix4x4
function Matrix4x4.createRotationX(radians) end

---@param radians number
---@return Matrix4x4
function Matrix4x4.createRotationY(radians) end

---@param radians number
---@return Matrix4x4
function Matrix4x4.createRotationZ(radians) end

---@param xScale number
---@param yScale number
---@param zScale number
---@return Matrix4x4
function Matrix4x4.createScale(xScale, yScale, zScale) end

---@param lightDirection Vector3
---@param plane Plane
---@return Matrix4x4
function Matrix4x4.createShadow(lightDirection, plane) end

---@param position Vector3
---@return Matrix4x4
function Matrix4x4.createTranslation(position) end

---@param x number
---@param y number
---@param width number
---@param height number
---@param minDepth number
---@param maxDepth number
---@return Matrix4x4
function Matrix4x4.createViewport(x, y, width, height, minDepth, maxDepth) end

---@param x number
---@param y number
---@param width number
---@param height number
---@param minDepth number
---@param maxDepth number
---@return Matrix4x4
function Matrix4x4.createViewportLeftHanded(x, y, width, height, minDepth, maxDepth) end

---@param position Vector3
---@param forward Vector3
---@param up Vector3
---@return Matrix4x4
function Matrix4x4.createWorld(position, forward, up) end

---@param matrix Matrix4x4
---@param scale Vector3
---@param rotation Quaternion
---@param translation Vector3
---@return boolean
function Matrix4x4.decompose(matrix, scale, rotation, translation) end

---@param matrix Matrix4x4
---@param result Matrix4x4
---@return boolean
function Matrix4x4.invert(matrix, result) end

---@param matrix1 Matrix4x4
---@param matrix2 Matrix4x4
---@param amount number
---@return Matrix4x4
function Matrix4x4.lerp(matrix1, matrix2, amount) end

---@param value1 Matrix4x4
---@param value2 Matrix4x4
---@return Matrix4x4
function Matrix4x4.multiply(value1, value2) end

---@param value Matrix4x4
---@return Matrix4x4
function Matrix4x4.negate(value) end

---@param value1 Matrix4x4
---@param value2 Matrix4x4
---@return Matrix4x4
function Matrix4x4.subtract(value1, value2) end

---@param value Matrix4x4
---@param rotation Quaternion
---@return Matrix4x4
function Matrix4x4.transform(value, rotation) end

---@param matrix Matrix4x4
---@return Matrix4x4
function Matrix4x4.transpose(matrix) end

---@param obj Object
---@return boolean
function Matrix4x4:equals(obj) end

---@return number
function Matrix4x4:getDeterminant() end

---@param row number
---@param column number
---@return number
function Matrix4x4:getElement(row, column) end

---@param index number
---@return Vector4
function Matrix4x4:getRow(index) end

---@return number
function Matrix4x4:getHashCode() end

---@return string
function Matrix4x4:toString() end

---@param row number
---@param column number
---@param value number
---@return Matrix4x4
function Matrix4x4:withElement(row, column, value) end

---@param index number
---@param value Vector4
---@return Matrix4x4
function Matrix4x4:withRow(index, value) end

---@class Camera3D
---@field projection CameraProjection
---@field fovY number
---@field position Vector3
---@field target Vector3
---@field up Vector3
---@field fwd Vector3
---@field right Vector3
---@field raylib Camera3D
local Camera3D = {}
---@class ModelAsset : Asset
---@field isLoaded boolean
---@field file string
---@field thumbnail Nullable
---@field meshes List
---@field bones List
---@field boneMap Dictionary
---@field rootNode ModelNode
---@field globalInverse Matrix4x4
---@field animations List
---@field materials Material
---@field materialPaths String
---@field cachedMaterialAssets List
---@field settings ModelSettings
local ModelAsset = {}
---@return void
function ModelAsset:applySettings() end

---@return boolean
function ModelAsset:load() end

---@return void
function ModelAsset:saveSettings() end

---@param index number
---@param path string
---@return void
function ModelAsset:applyMaterial(index, path) end

---@return void
function ModelAsset:updateMaterialsIfDirty() end

---@param index number
---@param force boolean
---@return void
function ModelAsset:applyMaterialState(index, force) end

---@return void
function ModelAsset:unload() end

---@class RigidBody
---@field data RigidBodyData
---@field handle JHandle
---@field island Island
---@field connections ReadOnlyList
---@field contacts ReadOnlyHashSet
---@field constraints ReadOnlyHashSet
---@field shapes ReadOnlyList
---@field friction number
---@field restitution number
---@field world World
---@field deactivationTime TimeSpan
---@field deactivationThreshold ValueTuple
---@field damping ValueTuple
---@field inverseInertia JMatrix
---@field position JVector
---@field orientation JQuaternion
---@field velocity JVector
---@field angularVelocity JVector
---@field affectedByGravity boolean
---@field tag Object
---@field enableSpeculativeContacts boolean
---@field motionType MotionType
---@field isStatic boolean
---@field isActive boolean
---@field enableGyroscopicForces boolean
---@field force JVector
---@field torque JVector
---@field mass number
---@field rigidBodyId number
local RigidBody = {}
---@param active boolean
---@return void
function RigidBody:setActivationState(active) end

---@param shapes IEnumerable
---@param setMassInertia boolean
---@return void
function RigidBody:addShape(shapes, setMassInertia) end

---@param force JVector
---@param wakeup boolean
---@return void
function RigidBody:addForce(force, wakeup) end

---@param dt number
---@return JVector
function RigidBody:predictPosition(dt) end

---@param dt number
---@return JQuaternion
function RigidBody:predictOrientation(dt) end

---@param dt number
---@param position JVector
---@param orientation JQuaternion
---@return void
function RigidBody:predictPose(dt, position, orientation) end

---@param shape RigidBodyShape
---@param setMassInertia boolean
---@return void
function RigidBody:removeShape(shape, setMassInertia) end

---@param setMassInertia boolean
---@return void
function RigidBody:clearShapes(setMassInertia) end

---@return void
function RigidBody:setMassInertia() end

---@param drawer IDebugDrawer
---@return void
function RigidBody:debugDraw(drawer) end

---@class BoxShape : RigidBodyShape
---@field size JVector
---@field rigidBody RigidBody
---@field velocity JVector
---@field worldBoundingBox JBoundingBox
---@field shapeId number
local BoxShape = {}
---@param direction JVector
---@param result JVector
---@return void
function BoxShape:supportMap(direction, result) end

---@param origin JVector
---@param direction JVector
---@param normal JVector
---@param lambda number
---@return boolean
function BoxShape:localRayCast(origin, direction, normal, lambda) end

---@param point JVector
---@return void
function BoxShape:getCenter(point) end

---@param orientation JQuaternion
---@param position JVector
---@param box JBoundingBox
---@return void
function BoxShape:calculateBoundingBox(orientation, position, box) end

---@param inertia JMatrix
---@param com JVector
---@param mass number
---@return void
function BoxShape:calculateMassInertia(inertia, com, mass) end

---@class SphereShape : RigidBodyShape
---@field radius number
---@field rigidBody RigidBody
---@field velocity JVector
---@field worldBoundingBox JBoundingBox
---@field shapeId number
local SphereShape = {}
---@param direction JVector
---@param result JVector
---@return void
function SphereShape:supportMap(direction, result) end

---@param point JVector
---@return void
function SphereShape:getCenter(point) end

---@param orientation JQuaternion
---@param position JVector
---@param box JBoundingBox
---@return void
function SphereShape:calculateBoundingBox(orientation, position, box) end

---@param origin JVector
---@param direction JVector
---@param normal JVector
---@param lambda number
---@return boolean
function SphereShape:localRayCast(origin, direction, normal, lambda) end

---@param inertia JMatrix
---@param com JVector
---@param mass number
---@return void
function SphereShape:calculateMassInertia(inertia, com, mass) end

---@class JsonConverter
---@field canRead boolean
---@field canWrite boolean
local JsonConverter = {}
---@param writer JsonWriter
---@param value Object
---@param serializer JsonSerializer
---@return void
function JsonConverter:writeJson(writer, value, serializer) end

---@param reader JsonReader
---@param objectType Type
---@param existingValue Object
---@param serializer JsonSerializer
---@return Object
function JsonConverter:readJson(reader, objectType, existingValue, serializer) end

---@param objectType Type
---@return boolean
function JsonConverter:canConvert(objectType) end

---@class JsonReader
---@field closeInput boolean
---@field supportMultipleContent boolean
---@field quoteChar number
---@field dateTimeZoneHandling DateTimeZoneHandling
---@field dateParseHandling DateParseHandling
---@field floatParseHandling FloatParseHandling
---@field dateFormatString string
---@field maxDepth Nullable
---@field tokenType JsonToken
---@field value Object
---@field valueType Type
---@field depth number
---@field path string
---@field culture CultureInfo
local JsonReader = {}
---@param cancellationToken CancellationToken
---@return Task
function JsonReader:readAsync(cancellationToken) end

---@param cancellationToken CancellationToken
---@return Task
function JsonReader:skipAsync(cancellationToken) end

---@param cancellationToken CancellationToken
---@return Task
function JsonReader:readAsBooleanAsync(cancellationToken) end

---@param cancellationToken CancellationToken
---@return Task
function JsonReader:readAsBytesAsync(cancellationToken) end

---@param cancellationToken CancellationToken
---@return Task
function JsonReader:readAsDateTimeAsync(cancellationToken) end

---@param cancellationToken CancellationToken
---@return Task
function JsonReader:readAsDateTimeOffsetAsync(cancellationToken) end

---@param cancellationToken CancellationToken
---@return Task
function JsonReader:readAsDecimalAsync(cancellationToken) end

---@param cancellationToken CancellationToken
---@return Task
function JsonReader:readAsDoubleAsync(cancellationToken) end

---@param cancellationToken CancellationToken
---@return Task
function JsonReader:readAsInt32Async(cancellationToken) end

---@param cancellationToken CancellationToken
---@return Task
function JsonReader:readAsStringAsync(cancellationToken) end

---@return boolean
function JsonReader:read() end

---@return Nullable
function JsonReader:readAsInt32() end

---@return string
function JsonReader:readAsString() end

---@return Byte
function JsonReader:readAsBytes() end

---@return Nullable
function JsonReader:readAsDouble() end

---@return Nullable
function JsonReader:readAsBoolean() end

---@return Nullable
function JsonReader:readAsDecimal() end

---@return Nullable
function JsonReader:readAsDateTime() end

---@return Nullable
function JsonReader:readAsDateTimeOffset() end

---@return void
function JsonReader:skip() end

---@return void
function JsonReader:close() end

---@class JsonSerializer
---@field referenceResolver IReferenceResolver
---@field binder SerializationBinder
---@field serializationBinder ISerializationBinder
---@field traceWriter ITraceWriter
---@field equalityComparer IEqualityComparer
---@field typeNameHandling TypeNameHandling
---@field typeNameAssemblyFormat FormatterAssemblyStyle
---@field typeNameAssemblyFormatHandling TypeNameAssemblyFormatHandling
---@field preserveReferencesHandling PreserveReferencesHandling
---@field referenceLoopHandling ReferenceLoopHandling
---@field missingMemberHandling MissingMemberHandling
---@field nullValueHandling NullValueHandling
---@field defaultValueHandling DefaultValueHandling
---@field objectCreationHandling ObjectCreationHandling
---@field constructorHandling ConstructorHandling
---@field metadataPropertyHandling MetadataPropertyHandling
---@field converters JsonConverterCollection
---@field contractResolver IContractResolver
---@field context StreamingContext
---@field formatting Formatting
---@field dateFormatHandling DateFormatHandling
---@field dateTimeZoneHandling DateTimeZoneHandling
---@field dateParseHandling DateParseHandling
---@field floatParseHandling FloatParseHandling
---@field floatFormatHandling FloatFormatHandling
---@field stringEscapeHandling StringEscapeHandling
---@field dateFormatString string
---@field culture CultureInfo
---@field maxDepth Nullable
---@field checkAdditionalContent boolean
local JsonSerializer = {}
---@return JsonSerializer
function JsonSerializer.create() end

---@return JsonSerializer
function JsonSerializer.createDefault() end

---@param reader TextReader
---@param target Object
---@return void
function JsonSerializer:populate(reader, target) end

---@param reader JsonReader
---@return Object
function JsonSerializer:deserialize(reader) end

---@param textWriter TextWriter
---@param value Object
---@return void
function JsonSerializer:serialize(textWriter, value) end

---@class JsonWriter
---@field closeOutput boolean
---@field autoCompleteOnClose boolean
---@field writeState WriteState
---@field path string
---@field formatting Formatting
---@field dateFormatHandling DateFormatHandling
---@field dateTimeZoneHandling DateTimeZoneHandling
---@field stringEscapeHandling StringEscapeHandling
---@field floatFormatHandling FloatFormatHandling
---@field dateFormatString string
---@field culture CultureInfo
local JsonWriter = {}
---@param cancellationToken CancellationToken
---@return Task
function JsonWriter:closeAsync(cancellationToken) end

---@param cancellationToken CancellationToken
---@return Task
function JsonWriter:flushAsync(cancellationToken) end

---@param json string
---@param cancellationToken CancellationToken
---@return Task
function JsonWriter:writeRawAsync(json, cancellationToken) end

---@param cancellationToken CancellationToken
---@return Task
function JsonWriter:writeEndAsync(cancellationToken) end

---@param cancellationToken CancellationToken
---@return Task
function JsonWriter:writeEndArrayAsync(cancellationToken) end

---@param cancellationToken CancellationToken
---@return Task
function JsonWriter:writeEndConstructorAsync(cancellationToken) end

---@param cancellationToken CancellationToken
---@return Task
function JsonWriter:writeEndObjectAsync(cancellationToken) end

---@param cancellationToken CancellationToken
---@return Task
function JsonWriter:writeNullAsync(cancellationToken) end

---@param name string
---@param cancellationToken CancellationToken
---@return Task
function JsonWriter:writePropertyNameAsync(name, cancellationToken) end

---@param cancellationToken CancellationToken
---@return Task
function JsonWriter:writeStartArrayAsync(cancellationToken) end

---@param text string
---@param cancellationToken CancellationToken
---@return Task
function JsonWriter:writeCommentAsync(text, cancellationToken) end

---@param json string
---@param cancellationToken CancellationToken
---@return Task
function JsonWriter:writeRawValueAsync(json, cancellationToken) end

---@param name string
---@param cancellationToken CancellationToken
---@return Task
function JsonWriter:writeStartConstructorAsync(name, cancellationToken) end

---@param cancellationToken CancellationToken
---@return Task
function JsonWriter:writeStartObjectAsync(cancellationToken) end

---@param reader JsonReader
---@param cancellationToken CancellationToken
---@return Task
function JsonWriter:writeTokenAsync(reader, cancellationToken) end

---@param value boolean
---@param cancellationToken CancellationToken
---@return Task
function JsonWriter:writeValueAsync(value, cancellationToken) end

---@param cancellationToken CancellationToken
---@return Task
function JsonWriter:writeUndefinedAsync(cancellationToken) end

---@param ws string
---@param cancellationToken CancellationToken
---@return Task
function JsonWriter:writeWhitespaceAsync(ws, cancellationToken) end

---@return void
function JsonWriter:flush() end

---@return void
function JsonWriter:close() end

---@return void
function JsonWriter:writeStartObject() end

---@return void
function JsonWriter:writeEndObject() end

---@return void
function JsonWriter:writeStartArray() end

---@return void
function JsonWriter:writeEndArray() end

---@param name string
---@return void
function JsonWriter:writeStartConstructor(name) end

---@return void
function JsonWriter:writeEndConstructor() end

---@param name string
---@return void
function JsonWriter:writePropertyName(name) end

---@return void
function JsonWriter:writeEnd() end

---@param reader JsonReader
---@return void
function JsonWriter:writeToken(reader) end

---@return void
function JsonWriter:writeNull() end

---@return void
function JsonWriter:writeUndefined() end

---@param json string
---@return void
function JsonWriter:writeRaw(json) end

---@param json string
---@return void
function JsonWriter:writeRawValue(json) end

---@param value string
---@return void
function JsonWriter:writeValue(value) end

---@param text string
---@return void
function JsonWriter:writeComment(text) end

---@param ws string
---@return void
function JsonWriter:writeWhitespace(ws) end

---@class Matrix3x2
---@field identity Matrix3x2
---@field isIdentity boolean
---@field translation Vector2
---@field x Vector2
---@field y Vector2
---@field z Vector2
---@field item Vector2
---@field m11 number
---@field m12 number
---@field m21 number
---@field m22 number
---@field m31 number
---@field m32 number
local Matrix3x2 = {}
---@param value1 Matrix3x2
---@param value2 Matrix3x2
---@return Matrix3x2
function Matrix3x2.add(value1, value2) end

---@param value number
---@return Matrix3x2
function Matrix3x2.create(value) end

---@param radians number
---@return Matrix3x2
function Matrix3x2.createRotation(radians) end

---@param scales Vector2
---@return Matrix3x2
function Matrix3x2.createScale(scales) end

---@param radiansX number
---@param radiansY number
---@return Matrix3x2
function Matrix3x2.createSkew(radiansX, radiansY) end

---@param position Vector2
---@return Matrix3x2
function Matrix3x2.createTranslation(position) end

---@param matrix Matrix3x2
---@param result Matrix3x2
---@return boolean
function Matrix3x2.invert(matrix, result) end

---@param matrix1 Matrix3x2
---@param matrix2 Matrix3x2
---@param amount number
---@return Matrix3x2
function Matrix3x2.lerp(matrix1, matrix2, amount) end

---@param value1 Matrix3x2
---@param value2 Matrix3x2
---@return Matrix3x2
function Matrix3x2.multiply(value1, value2) end

---@param value Matrix3x2
---@return Matrix3x2
function Matrix3x2.negate(value) end

---@param value1 Matrix3x2
---@param value2 Matrix3x2
---@return Matrix3x2
function Matrix3x2.subtract(value1, value2) end

---@param obj Object
---@return boolean
function Matrix3x2:equals(obj) end

---@return number
function Matrix3x2:getDeterminant() end

---@param row number
---@param column number
---@return number
function Matrix3x2:getElement(row, column) end

---@param index number
---@return Vector2
function Matrix3x2:getRow(index) end

---@return number
function Matrix3x2:getHashCode() end

---@return string
function Matrix3x2:toString() end

---@param row number
---@param column number
---@param value number
---@return Matrix3x2
function Matrix3x2:withElement(row, column, value) end

---@param index number
---@param value Vector2
---@return Matrix3x2
function Matrix3x2:withRow(index, value) end

---@class Vector4
---@field allBitsSet Vector4
---@field e Vector4
---@field epsilon Vector4
---@field naN Vector4
---@field negativeInfinity Vector4
---@field negativeZero Vector4
---@field one Vector4
---@field pi Vector4
---@field positiveInfinity Vector4
---@field tau Vector4
---@field unitX Vector4
---@field unitY Vector4
---@field unitZ Vector4
---@field unitW Vector4
---@field zero Vector4
---@field item number
---@field x number
---@field y number
---@field z number
---@field w number
local Vector4 = {}
---@param value Vector4
---@return Vector4
function Vector4.abs(value) end

---@param left Vector4
---@param right Vector4
---@return Vector4
function Vector4.add(left, right) end

---@param vector Vector4
---@param value number
---@return boolean
function Vector4.all(vector, value) end

---@param vector Vector4
---@return boolean
function Vector4.allWhereAllBitsSet(vector) end

---@param left Vector4
---@param right Vector4
---@return Vector4
function Vector4.andNot(left, right) end

---@param vector Vector4
---@param value number
---@return boolean
function Vector4.any(vector, value) end

---@param vector Vector4
---@return boolean
function Vector4.anyWhereAllBitsSet(vector) end

---@param left Vector4
---@param right Vector4
---@return Vector4
function Vector4.bitwiseAnd(left, right) end

---@param left Vector4
---@param right Vector4
---@return Vector4
function Vector4.bitwiseOr(left, right) end

---@param value1 Vector4
---@param min Vector4
---@param max Vector4
---@return Vector4
function Vector4.clamp(value1, min, max) end

---@param value1 Vector4
---@param min Vector4
---@param max Vector4
---@return Vector4
function Vector4.clampNative(value1, min, max) end

---@param condition Vector4
---@param left Vector4
---@param right Vector4
---@return Vector4
function Vector4.conditionalSelect(condition, left, right) end

---@param value Vector4
---@param sign Vector4
---@return Vector4
function Vector4.copySign(value, sign) end

---@param vector Vector4
---@return Vector4
function Vector4.cos(vector) end

---@param vector Vector4
---@param value number
---@return number
function Vector4.count(vector, value) end

---@param vector Vector4
---@return number
function Vector4.countWhereAllBitsSet(vector) end

---@param value number
---@return Vector4
function Vector4.create(value) end

---@param x number
---@return Vector4
function Vector4.createScalar(x) end

---@param x number
---@return Vector4
function Vector4.createScalarUnsafe(x) end

---@param vector1 Vector4
---@param vector2 Vector4
---@return Vector4
function Vector4.cross(vector1, vector2) end

---@param degrees Vector4
---@return Vector4
function Vector4.degreesToRadians(degrees) end

---@param value1 Vector4
---@param value2 Vector4
---@return number
function Vector4.distance(value1, value2) end

---@param value1 Vector4
---@param value2 Vector4
---@return number
function Vector4.distanceSquared(value1, value2) end

---@param left Vector4
---@param right Vector4
---@return Vector4
function Vector4.divide(left, right) end

---@param vector1 Vector4
---@param vector2 Vector4
---@return number
function Vector4.dot(vector1, vector2) end

---@param vector Vector4
---@return Vector4
function Vector4.exp(vector) end

---@param left Vector4
---@param right Vector4
---@return Vector4
function Vector4.equals(left, right) end

---@param left Vector4
---@param right Vector4
---@return boolean
function Vector4.equalsAll(left, right) end

---@param left Vector4
---@param right Vector4
---@return boolean
function Vector4.equalsAny(left, right) end

---@param left Vector4
---@param right Vector4
---@param addend Vector4
---@return Vector4
function Vector4.fusedMultiplyAdd(left, right, addend) end

---@param left Vector4
---@param right Vector4
---@return Vector4
function Vector4.greaterThan(left, right) end

---@param left Vector4
---@param right Vector4
---@return boolean
function Vector4.greaterThanAll(left, right) end

---@param left Vector4
---@param right Vector4
---@return boolean
function Vector4.greaterThanAny(left, right) end

---@param left Vector4
---@param right Vector4
---@return Vector4
function Vector4.greaterThanOrEqual(left, right) end

---@param left Vector4
---@param right Vector4
---@return boolean
function Vector4.greaterThanOrEqualAll(left, right) end

---@param left Vector4
---@param right Vector4
---@return boolean
function Vector4.greaterThanOrEqualAny(left, right) end

---@param x Vector4
---@param y Vector4
---@return Vector4
function Vector4.hypot(x, y) end

---@param vector Vector4
---@param value number
---@return number
function Vector4.indexOf(vector, value) end

---@param vector Vector4
---@return number
function Vector4.indexOfWhereAllBitsSet(vector) end

---@param vector Vector4
---@return Vector4
function Vector4.isEvenInteger(vector) end

---@param vector Vector4
---@return Vector4
function Vector4.isFinite(vector) end

---@param vector Vector4
---@return Vector4
function Vector4.isInfinity(vector) end

---@param vector Vector4
---@return Vector4
function Vector4.isInteger(vector) end

---@param vector Vector4
---@return Vector4
function Vector4.isNaN(vector) end

---@param vector Vector4
---@return Vector4
function Vector4.isNegative(vector) end

---@param vector Vector4
---@return Vector4
function Vector4.isNegativeInfinity(vector) end

---@param vector Vector4
---@return Vector4
function Vector4.isNormal(vector) end

---@param vector Vector4
---@return Vector4
function Vector4.isOddInteger(vector) end

---@param vector Vector4
---@return Vector4
function Vector4.isPositive(vector) end

---@param vector Vector4
---@return Vector4
function Vector4.isPositiveInfinity(vector) end

---@param vector Vector4
---@return Vector4
function Vector4.isSubnormal(vector) end

---@param vector Vector4
---@return Vector4
function Vector4.isZero(vector) end

---@param vector Vector4
---@param value number
---@return number
function Vector4.lastIndexOf(vector, value) end

---@param vector Vector4
---@return number
function Vector4.lastIndexOfWhereAllBitsSet(vector) end

---@param value1 Vector4
---@param value2 Vector4
---@param amount number
---@return Vector4
function Vector4.lerp(value1, value2, amount) end

---@param left Vector4
---@param right Vector4
---@return Vector4
function Vector4.lessThan(left, right) end

---@param left Vector4
---@param right Vector4
---@return boolean
function Vector4.lessThanAll(left, right) end

---@param left Vector4
---@param right Vector4
---@return boolean
function Vector4.lessThanAny(left, right) end

---@param left Vector4
---@param right Vector4
---@return Vector4
function Vector4.lessThanOrEqual(left, right) end

---@param left Vector4
---@param right Vector4
---@return boolean
function Vector4.lessThanOrEqualAll(left, right) end

---@param left Vector4
---@param right Vector4
---@return boolean
function Vector4.lessThanOrEqualAny(left, right) end

---@param source Single
---@return Vector4
function Vector4.load(source) end

---@param source Single
---@return Vector4
function Vector4.loadAligned(source) end

---@param source Single
---@return Vector4
function Vector4.loadAlignedNonTemporal(source) end

---@param source number
---@return Vector4
function Vector4.loadUnsafe(source) end

---@param vector Vector4
---@return Vector4
function Vector4.log(vector) end

---@param vector Vector4
---@return Vector4
function Vector4.log2(vector) end

---@param value1 Vector4
---@param value2 Vector4
---@return Vector4
function Vector4.max(value1, value2) end

---@param value1 Vector4
---@param value2 Vector4
---@return Vector4
function Vector4.maxMagnitude(value1, value2) end

---@param value1 Vector4
---@param value2 Vector4
---@return Vector4
function Vector4.maxMagnitudeNumber(value1, value2) end

---@param value1 Vector4
---@param value2 Vector4
---@return Vector4
function Vector4.maxNative(value1, value2) end

---@param value1 Vector4
---@param value2 Vector4
---@return Vector4
function Vector4.maxNumber(value1, value2) end

---@param value1 Vector4
---@param value2 Vector4
---@return Vector4
function Vector4.min(value1, value2) end

---@param value1 Vector4
---@param value2 Vector4
---@return Vector4
function Vector4.minMagnitude(value1, value2) end

---@param value1 Vector4
---@param value2 Vector4
---@return Vector4
function Vector4.minMagnitudeNumber(value1, value2) end

---@param value1 Vector4
---@param value2 Vector4
---@return Vector4
function Vector4.minNative(value1, value2) end

---@param value1 Vector4
---@param value2 Vector4
---@return Vector4
function Vector4.minNumber(value1, value2) end

---@param left Vector4
---@param right Vector4
---@return Vector4
function Vector4.multiply(left, right) end

---@param left Vector4
---@param right Vector4
---@param addend Vector4
---@return Vector4
function Vector4.multiplyAddEstimate(left, right, addend) end

---@param value Vector4
---@return Vector4
function Vector4.negate(value) end

---@param vector Vector4
---@param value number
---@return boolean
function Vector4.none(vector, value) end

---@param vector Vector4
---@return boolean
function Vector4.noneWhereAllBitsSet(vector) end

---@param vector Vector4
---@return Vector4
function Vector4.normalize(vector) end

---@param value Vector4
---@return Vector4
function Vector4.onesComplement(value) end

---@param radians Vector4
---@return Vector4
function Vector4.radiansToDegrees(radians) end

---@param vector Vector4
---@return Vector4
function Vector4.round(vector) end

---@param vector Vector4
---@param xIndex number
---@param yIndex number
---@param zIndex number
---@param wIndex number
---@return Vector4
function Vector4.shuffle(vector, xIndex, yIndex, zIndex, wIndex) end

---@param vector Vector4
---@return Vector4
function Vector4.sin(vector) end

---@param vector Vector4
---@return ValueTuple
function Vector4.sinCos(vector) end

---@param value Vector4
---@return Vector4
function Vector4.squareRoot(value) end

---@param left Vector4
---@param right Vector4
---@return Vector4
function Vector4.subtract(left, right) end

---@param value Vector4
---@return number
function Vector4.sum(value) end

---@param position Vector2
---@param matrix Matrix4x4
---@return Vector4
function Vector4.transform(position, matrix) end

---@param vector Vector4
---@return Vector4
function Vector4.truncate(vector) end

---@param left Vector4
---@param right Vector4
---@return Vector4
function Vector4.xor(left, right) end

---@param array Single
---@return void
function Vector4:copyTo(array) end

---@param destination Span
---@return boolean
function Vector4:tryCopyTo(destination) end

---@return number
function Vector4:getHashCode() end

---@return number
function Vector4:length() end

---@return number
function Vector4:lengthSquared() end

---@return string
function Vector4:toString() end

---@class Plane
---@field normal Vector3
---@field d number
local Plane = {}
---@param value Vector4
---@return Plane
function Plane.create(value) end

---@param point1 Vector3
---@param point2 Vector3
---@param point3 Vector3
---@return Plane
function Plane.createFromVertices(point1, point2, point3) end

---@param plane Plane
---@param value Vector4
---@return number
function Plane.dot(plane, value) end

---@param plane Plane
---@param value Vector3
---@return number
function Plane.dotCoordinate(plane, value) end

---@param plane Plane
---@param value Vector3
---@return number
function Plane.dotNormal(plane, value) end

---@param value Plane
---@return Plane
function Plane.normalize(value) end

---@param plane Plane
---@param matrix Matrix4x4
---@return Plane
function Plane.transform(plane, matrix) end

---@param obj Object
---@return boolean
function Plane:equals(obj) end

---@return number
function Plane:getHashCode() end

---@return string
function Plane:toString() end

---@class CameraProjection
---@field value__ number
---@field perspective CameraProjection
---@field orthographic CameraProjection
local CameraProjection = {}
---@class Asset
---@field isLoaded boolean
---@field file string
---@field thumbnail Nullable
local Asset = {}
---@return boolean
function Asset:load() end

---@return void
function Asset:unload() end

---@class ModelNode
---@field name string
---@field transformation Matrix4x4
---@field children List
local ModelNode = {}
---@class Material
---@field length number
---@field longLength number
---@field rank number
---@field syncRoot Object
---@field isReadOnly boolean
---@field isFixedSize boolean
---@field isSynchronized boolean
local Material = {}
---@param arg0 number
---@return Material
function Material:get(arg0) end

---@param arg0 number
---@param arg1 Material
---@return void
function Material:set(arg0, arg1) end

---@param arg0 number
---@return Material
function Material:address(arg0) end

---@class ModelSettings
---@field meshMaterials Dictionary
---@field importScale number
local ModelSettings = {}
---@return Object
function ModelSettings:clone() end

---@class RigidBodyData
---@field isActive boolean
---@field enableGyroscopicForces boolean
---@field isStaticOrInactive boolean
---@field motionType MotionType
---@field position JVector
---@field velocity JVector
---@field angularVelocity JVector
---@field deltaVelocity JVector
---@field deltaAngularVelocity JVector
---@field orientation JQuaternion
---@field inverseInertiaWorld JMatrix
---@field inverseMass number
---@field flags number
local RigidBodyData = {}
---@class JHandle
---@field data RigidBodyData
---@field isZero boolean
---@field zero JHandle
local JHandle = {}
---@param handle JHandle
---@return JHandle
function JHandle.asHandle(handle) end

---@param other JHandle
---@return boolean
function JHandle:equals(other) end

---@return number
function JHandle:getHashCode() end

---@class Island
---@field bodies ReadOnlyHashSet
local Island = {}
---@class ReadOnlyList
---@field item RigidBody
---@field count number
local ReadOnlyList = {}
---@return Enumerator
function ReadOnlyList:getEnumerator() end

---@class ReadOnlyHashSet
---@field count number
local ReadOnlyHashSet = {}
---@return Enumerator
function ReadOnlyHashSet:getEnumerator() end

---@param item Arbiter
---@return boolean
function ReadOnlyHashSet:contains(item) end

---@param array Arbiter
---@return void
function ReadOnlyHashSet:copyTo(array) end

---@class World
---@field rawData SpanData
---@field threadModel ThreadModelType
---@field islands ReadOnlyPartitionedSet
---@field rigidBodies ReadOnlyPartitionedSet
---@field dynamicTree DynamicTree
---@field nullBody RigidBody
---@field allowDeactivation boolean
---@field solverIterations ValueTuple
---@field substepCount number
---@field gravity JVector
---@field narrowPhaseFilter INarrowPhaseFilter
---@field broadPhaseFilter IBroadPhaseFilter
---@field enableAuxiliaryContactPoints boolean
---@field speculativeRelaxationFactor number
---@field speculativeVelocityThreshold number
---@field debugTimings Double
local World = {}
---@return number
function World.requestId() end

---@param proxyA IDynamicTreeProxy
---@param proxyB IDynamicTreeProxy
---@return boolean
function World.defaultDynamicTreeFilter(proxyA, proxyB) end

---@return void
function World:clear() end

---@param body RigidBody
---@return void
function World:remove(body) end

---@param body1 RigidBody
---@param body2 RigidBody
---@return any
function World:createConstraint(body1, body2) end

---@return RigidBody
function World:createRigidBody() end

---@return void
function World:dispose() end

---@param arbiter Arbiter
---@param point1 JVector
---@param point2 JVector
---@param normal JVector
---@param removeFlags SolveMode
---@return void
function World:registerContact(arbiter, point1, point2, normal, removeFlags) end

---@param id0 number
---@param id1 number
---@param arbiter Arbiter
---@return boolean
function World:getArbiter(id0, id1, arbiter) end

---@param id0 number
---@param id1 number
---@param body1 RigidBody
---@param body2 RigidBody
---@param arbiter Arbiter
---@return void
function World:getOrCreateArbiter(id0, id1, body1, body2, arbiter) end

---@param dt number
---@param multiThread boolean
---@return void
function World:step(dt, multiThread) end

---@param b1 RigidBodyData
---@param b2 RigidBodyData
---@return boolean
function World.tryLockTwoBody(b1, b2) end

---@param b1 RigidBodyData
---@param b2 RigidBodyData
---@return void
function World.lockTwoBody(b1, b2) end

---@param b1 RigidBodyData
---@param b2 RigidBodyData
---@return void
function World.unlockTwoBody(b1, b2) end

---@class JMatrix
---@field m11 number
---@field m21 number
---@field m31 number
---@field m12 number
---@field m22 number
---@field m32 number
---@field m13 number
---@field m23 number
---@field m33 number
---@field identity JMatrix
---@field zero JMatrix
local JMatrix = {}
---@param col1 JVector
---@param col2 JVector
---@param col3 JVector
---@return JMatrix
function JMatrix.fromColumns(col1, col2, col3) end

---@param index number
---@return JVector
function JMatrix:unsafeGet(index) end

---@param index number
---@return JVector
function JMatrix:getColumn(index) end

---@param matrix1 JMatrix
---@param matrix2 JMatrix
---@return JMatrix
function JMatrix.multiply(matrix1, matrix2) end

---@param matrix1 JMatrix
---@param matrix2 JMatrix
---@return JMatrix
function JMatrix.multiplyTransposed(matrix1, matrix2) end

---@param matrix1 JMatrix
---@param matrix2 JMatrix
---@return JMatrix
function JMatrix.transposedMultiply(matrix1, matrix2) end

---@param axis JVector
---@param angle number
---@return JMatrix
function JMatrix.createRotationMatrix(axis, angle) end

---@param matrix1 JMatrix
---@param matrix2 JMatrix
---@return JMatrix
function JMatrix.add(matrix1, matrix2) end

---@param radians number
---@return JMatrix
function JMatrix.createRotationX(radians) end

---@param radians number
---@return JMatrix
function JMatrix.createRotationY(radians) end

---@param radians number
---@return JMatrix
function JMatrix.createRotationZ(radians) end

---@param scale JVector
---@return JMatrix
function JMatrix.createScale(scale) end

---@param matrix1 JMatrix
---@param matrix2 JMatrix
---@param result JMatrix
---@return void
function JMatrix.subtract(matrix1, matrix2, result) end

---@return number
function JMatrix:determinant() end

---@param matrix JMatrix
---@param result JMatrix
---@return boolean
function JMatrix.inverse(matrix, result) end

---@param quaternion JQuaternion
---@return JMatrix
function JMatrix.createFromQuaternion(quaternion) end

---@param matrix JMatrix
---@param result JMatrix
---@return void
function JMatrix.absolute(matrix, result) end

---@param matrix JMatrix
---@return JMatrix
function JMatrix.transpose(matrix) end

---@param vec JVector
---@return JMatrix
function JMatrix.createCrossProduct(vec) end

---@return number
function JMatrix:trace() end

---@param other JMatrix
---@return boolean
function JMatrix:equals(other) end

---@return string
function JMatrix:toString() end

---@return number
function JMatrix:getHashCode() end

---@class JVector
---@field item number
---@field x number
---@field y number
---@field z number
---@field zero JVector
---@field unitX JVector
---@field unitY JVector
---@field unitZ JVector
---@field one JVector
---@field minValue JVector
---@field maxValue JVector
local JVector = {}
---@return any
function JVector:unsafeAs() end

---@param value any
---@return JVector
function JVector.unsafeFrom(value) end

---@param x number
---@param y number
---@param z number
---@return void
function JVector:set(x, y, z) end

---@param index number
---@return number
function JVector:unsafeGet(index) end

---@return string
function JVector:toString() end

---@param obj Object
---@return boolean
function JVector:equals(obj) end

---@param value1 JVector
---@param value2 JVector
---@return JVector
function JVector.min(value1, value2) end

---@param value1 JVector
---@param value2 JVector
---@return JVector
function JVector.max(value1, value2) end

---@param value1 JVector
---@return JVector
function JVector.abs(value1) end

---@param value1 JVector
---@return number
function JVector.maxAbs(value1) end

---@param vector JVector
---@param matrix JMatrix
---@return JVector
function JVector.transform(vector, matrix) end

---@param vector JVector
---@param matrix JMatrix
---@return JVector
function JVector.transposedTransform(vector, matrix) end

---@param vector JVector
---@param quat JQuaternion
---@return JVector
function JVector.conjugatedTransform(vector, quat) end

---@param u JVector
---@param v JVector
---@return JMatrix
function JVector.outer(u, v) end

---@param vector1 JVector
---@param vector2 JVector
---@return number
function JVector.dot(vector1, vector2) end

---@param value1 JVector
---@param value2 JVector
---@return JVector
function JVector.add(value1, value2) end

---@param value1 JVector
---@param value2 JVector
---@return JVector
function JVector.subtract(value1, value2) end

---@param vector1 JVector
---@param vector2 JVector
---@return JVector
function JVector.cross(vector1, vector2) end

---@return number
function JVector:getHashCode() end

---@return void
function JVector:negate() end

---@param vector JVector
---@return void
function JVector.negateInPlace(vector) end

---@param value JVector
---@return JVector
function JVector.normalize(value) end

---@param value JVector
---@param epsilonSquared number
---@return JVector
function JVector.normalizeSafe(value, epsilonSquared) end

---@param toNormalize JVector
---@return void
function JVector.normalizeInPlace(toNormalize) end

---@return number
function JVector:lengthSquared() end

---@return number
function JVector:length() end

---@param vector1 JVector
---@param vector2 JVector
---@return void
function JVector.swap(vector1, vector2) end

---@param value1 JVector
---@param scaleFactor number
---@return JVector
function JVector.multiply(value1, scaleFactor) end

---@class JQuaternion
---@field vector JVector
---@field scalar number
---@field identity JQuaternion
---@field x number
---@field y number
---@field z number
---@field w number
local JQuaternion = {}
---@return any
function JQuaternion:unsafeAs() end

---@param value any
---@return JQuaternion
function JQuaternion.unsafeFrom(value) end

---@param quaternion1 JQuaternion
---@param quaternion2 JQuaternion
---@return JQuaternion
function JQuaternion.add(quaternion1, quaternion2) end

---@param from JVector
---@param to JVector
---@return JQuaternion
function JQuaternion.createFromToRotation(from, to) end

---@param value JQuaternion
---@return JQuaternion
function JQuaternion.conjugate(value) end

---@return string
function JQuaternion:toString() end

---@param quaternion1 JQuaternion
---@param quaternion2 JQuaternion
---@return JQuaternion
function JQuaternion.subtract(quaternion1, quaternion2) end

---@param quaternion1 JQuaternion
---@param quaternion2 JQuaternion
---@return JQuaternion
function JQuaternion.multiply(quaternion1, quaternion2) end

---@return JVector
function JQuaternion:getBasisX() end

---@return JVector
function JQuaternion:getBasisY() end

---@return JVector
function JQuaternion:getBasisZ() end

---@param radians number
---@return JQuaternion
function JQuaternion.createRotationX(radians) end

---@param radians number
---@return JQuaternion
function JQuaternion.createRotationY(radians) end

---@param radians number
---@return JQuaternion
function JQuaternion.createRotationZ(radians) end

---@param axis JVector
---@param angle number
---@return JQuaternion
function JQuaternion.createFromAxisAngle(axis, angle) end

---@param quaternion JQuaternion
---@param axis JVector
---@param angle number
---@return void
function JQuaternion.toAxisAngle(quaternion, axis, angle) end

---@param quaternion1 JQuaternion
---@param quaternion2 JQuaternion
---@param result JQuaternion
---@return void
function JQuaternion.conjugateMultiply(quaternion1, quaternion2, result) end

---@param quaternion1 JQuaternion
---@param quaternion2 JQuaternion
---@param result JQuaternion
---@return void
function JQuaternion.multiplyConjugate(quaternion1, quaternion2, result) end

---@return number
function JQuaternion:length() end

---@return number
function JQuaternion:lengthSquared() end

---@return void
function JQuaternion:normalize() end

---@param quaternion JQuaternion
---@return void
function JQuaternion.normalizeInPlace(quaternion) end

---@param matrix JMatrix
---@return JQuaternion
function JQuaternion.createFromMatrix(matrix) end

---@param quaternion1 JQuaternion
---@param quaternion2 JQuaternion
---@return number
function JQuaternion.dot(quaternion1, quaternion2) end

---@param value JQuaternion
---@return JQuaternion
function JQuaternion.inverse(value) end

---@param quaternion1 JQuaternion
---@param quaternion2 JQuaternion
---@param amount number
---@return JQuaternion
function JQuaternion.lerp(quaternion1, quaternion2, amount) end

---@param quaternion1 JQuaternion
---@param quaternion2 JQuaternion
---@param amount number
---@return JQuaternion
function JQuaternion.slerp(quaternion1, quaternion2, amount) end

---@param other JQuaternion
---@return boolean
function JQuaternion:equals(other) end

---@return number
function JQuaternion:getHashCode() end

---@class MotionType
---@field value__ number
---@field dynamic MotionType
---@field kinematic MotionType
---@field static MotionType
local MotionType = {}
---@class RigidBodyShape : Shape
---@field rigidBody RigidBody
---@field velocity JVector
---@field worldBoundingBox JBoundingBox
---@field shapeId number
local RigidBodyShape = {}
---@param dt number
---@return void
function RigidBodyShape:updateWorldBoundingBox(dt) end

---@param orientation JQuaternion
---@param position JVector
---@param box JBoundingBox
---@return void
function RigidBodyShape:calculateBoundingBox(orientation, position, box) end

---@param inertia JMatrix
---@param com JVector
---@param mass number
---@return void
function RigidBodyShape:calculateMassInertia(inertia, com, mass) end

---@param origin JVector
---@param direction JVector
---@param normal JVector
---@param lambda number
---@return boolean
function RigidBodyShape:localRayCast(origin, direction, normal, lambda) end

---@param origin JVector
---@param direction JVector
---@param normal JVector
---@param lambda number
---@return boolean
function RigidBodyShape:rayCast(origin, direction, normal, lambda) end

---@class IDebugDrawer
local IDebugDrawer = {}
---@param pA JVector
---@param pB JVector
---@return void
function IDebugDrawer:drawSegment(pA, pB) end

---@param pA JVector
---@param pB JVector
---@param pC JVector
---@return void
function IDebugDrawer:drawTriangle(pA, pB, pC) end

---@param p JVector
---@return void
function IDebugDrawer:drawPoint(p) end

---@class JBoundingBox
---@field center JVector
---@field min JVector
---@field max JVector
---@field largeBox JBoundingBox
---@field smallBox JBoundingBox
---@field epsilon number
local JBoundingBox = {}
---@return string
function JBoundingBox:toString() end

---@param orientation JMatrix
---@return void
function JBoundingBox:transform(orientation) end

---@param box JBoundingBox
---@param orientation JMatrix
---@return JBoundingBox
function JBoundingBox.createTransformed(box, orientation) end

---@param origin JVector
---@param direction JVector
---@return boolean
function JBoundingBox:segmentIntersect(origin, direction) end

---@param origin JVector
---@param direction JVector
---@return boolean
function JBoundingBox:rayIntersect(origin, direction) end

---@param point JVector
---@return boolean
function JBoundingBox:contains(point) end

---@param destination Span
---@return void
function JBoundingBox:getCorners(destination) end

---@param point JVector
---@return void
function JBoundingBox:addPoint(point) end

---@param box JBoundingBox
---@param point JVector
---@return void
function JBoundingBox.addPointInPlace(box, point) end

---@param points IEnumerable
---@return JBoundingBox
function JBoundingBox.createFromPoints(points) end

---@param left JBoundingBox
---@param right JBoundingBox
---@return boolean
function JBoundingBox.notDisjoint(left, right) end

---@param left JBoundingBox
---@param right JBoundingBox
---@return boolean
function JBoundingBox.disjoint(left, right) end

---@param outer JBoundingBox
---@param inner JBoundingBox
---@return boolean
function JBoundingBox.encompasses(outer, inner) end

---@param original JBoundingBox
---@param additional JBoundingBox
---@return JBoundingBox
function JBoundingBox.createMerged(original, additional) end

---@return number
function JBoundingBox:getVolume() end

---@return number
function JBoundingBox:getSurfaceArea() end

---@param other JBoundingBox
---@return boolean
function JBoundingBox:equals(other) end

---@return number
function JBoundingBox:getHashCode() end

---@class DateTimeZoneHandling
---@field value__ number
---@field local DateTimeZoneHandling
---@field utc DateTimeZoneHandling
---@field unspecified DateTimeZoneHandling
---@field roundtripKind DateTimeZoneHandling
local DateTimeZoneHandling = {}
---@class DateParseHandling
---@field value__ number
---@field none DateParseHandling
---@field dateTime DateParseHandling
---@field dateTimeOffset DateParseHandling
local DateParseHandling = {}
---@class FloatParseHandling
---@field value__ number
---@field double FloatParseHandling
---@field decimal FloatParseHandling
local FloatParseHandling = {}
---@class JsonToken
---@field value__ number
---@field none JsonToken
---@field startObject JsonToken
---@field startArray JsonToken
---@field startConstructor JsonToken
---@field propertyName JsonToken
---@field comment JsonToken
---@field raw JsonToken
---@field integer JsonToken
---@field float JsonToken
---@field string JsonToken
---@field boolean JsonToken
---@field null JsonToken
---@field undefined JsonToken
---@field endObject JsonToken
---@field endArray JsonToken
---@field endConstructor JsonToken
---@field date JsonToken
---@field bytes JsonToken
local JsonToken = {}
---@class IReferenceResolver
local IReferenceResolver = {}
---@param context Object
---@param reference string
---@return Object
function IReferenceResolver:resolveReference(context, reference) end

---@param context Object
---@param value Object
---@return string
function IReferenceResolver:getReference(context, value) end

---@param context Object
---@param value Object
---@return boolean
function IReferenceResolver:isReferenced(context, value) end

---@param context Object
---@param reference string
---@param value Object
---@return void
function IReferenceResolver:addReference(context, reference, value) end

---@class ISerializationBinder
local ISerializationBinder = {}
---@param assemblyName string
---@param typeName string
---@return Type
function ISerializationBinder:bindToType(assemblyName, typeName) end

---@param serializedType Type
---@param assemblyName string
---@param typeName string
---@return void
function ISerializationBinder:bindToName(serializedType, assemblyName, typeName) end

---@class ITraceWriter
---@field levelFilter TraceLevel
local ITraceWriter = {}
---@param level TraceLevel
---@param message string
---@param ex Exception
---@return void
function ITraceWriter:trace(level, message, ex) end

---@class TypeNameHandling
---@field value__ number
---@field none TypeNameHandling
---@field objects TypeNameHandling
---@field arrays TypeNameHandling
---@field all TypeNameHandling
---@field auto TypeNameHandling
local TypeNameHandling = {}
---@class TypeNameAssemblyFormatHandling
---@field value__ number
---@field simple TypeNameAssemblyFormatHandling
---@field full TypeNameAssemblyFormatHandling
local TypeNameAssemblyFormatHandling = {}
---@class PreserveReferencesHandling
---@field value__ number
---@field none PreserveReferencesHandling
---@field objects PreserveReferencesHandling
---@field arrays PreserveReferencesHandling
---@field all PreserveReferencesHandling
local PreserveReferencesHandling = {}
---@class ReferenceLoopHandling
---@field value__ number
---@field error ReferenceLoopHandling
---@field ignore ReferenceLoopHandling
---@field serialize ReferenceLoopHandling
local ReferenceLoopHandling = {}
---@class MissingMemberHandling
---@field value__ number
---@field ignore MissingMemberHandling
---@field error MissingMemberHandling
local MissingMemberHandling = {}
---@class NullValueHandling
---@field value__ number
---@field include NullValueHandling
---@field ignore NullValueHandling
local NullValueHandling = {}
---@class DefaultValueHandling
---@field value__ number
---@field include DefaultValueHandling
---@field ignore DefaultValueHandling
---@field populate DefaultValueHandling
---@field ignoreAndPopulate DefaultValueHandling
local DefaultValueHandling = {}
---@class ObjectCreationHandling
---@field value__ number
---@field auto ObjectCreationHandling
---@field reuse ObjectCreationHandling
---@field replace ObjectCreationHandling
local ObjectCreationHandling = {}
---@class ConstructorHandling
---@field value__ number
---@field default ConstructorHandling
---@field allowNonPublicDefaultConstructor ConstructorHandling
local ConstructorHandling = {}
---@class MetadataPropertyHandling
---@field value__ number
---@field default MetadataPropertyHandling
---@field readAhead MetadataPropertyHandling
---@field ignore MetadataPropertyHandling
local MetadataPropertyHandling = {}
---@class JsonConverterCollection
---@field count number
---@field item JsonConverter
local JsonConverterCollection = {}
---@class IContractResolver
local IContractResolver = {}
---@param type Type
---@return JsonContract
function IContractResolver:resolveContract(type) end

---@class Formatting
---@field value__ number
---@field none Formatting
---@field indented Formatting
local Formatting = {}
---@class DateFormatHandling
---@field value__ number
---@field isoDateFormat DateFormatHandling
---@field microsoftDateFormat DateFormatHandling
local DateFormatHandling = {}
---@class FloatFormatHandling
---@field value__ number
---@field string FloatFormatHandling
---@field symbol FloatFormatHandling
---@field defaultValue FloatFormatHandling
local FloatFormatHandling = {}
---@class StringEscapeHandling
---@field value__ number
---@field default StringEscapeHandling
---@field escapeNonAscii StringEscapeHandling
---@field escapeHtml StringEscapeHandling
local StringEscapeHandling = {}
---@class WriteState
---@field value__ number
---@field error WriteState
---@field closed WriteState
---@field object WriteState
---@field array WriteState
---@field constructor WriteState
---@field property WriteState
---@field start WriteState
local WriteState = {}
---@class Arbiter
---@field body1 RigidBody
---@field body2 RigidBody
---@field handle JHandle
local Arbiter = {}
---@class SpanData
---@field totalBytesAllocated number
---@field activeRigidBodies Span
---@field inactiveRigidBodies Span
---@field rigidBodies Span
---@field activeContacts Span
---@field inactiveContacts Span
---@field contacts Span
---@field activeConstraints Span
---@field inactiveConstraints Span
---@field constraints Span
---@field activeSmallConstraints Span
---@field inactiveSmallConstraints Span
---@field smallConstraints Span
local SpanData = {}
---@class ThreadModelType
---@field value__ number
---@field regular ThreadModelType
---@field persistent ThreadModelType
local ThreadModelType = {}
---@class ReadOnlyPartitionedSet
---@field activeCount number
---@field count number
---@field elements ReadOnlySpan
---@field active ReadOnlySpan
---@field inactive ReadOnlySpan
---@field item Island
local ReadOnlyPartitionedSet = {}
---@param element Island
---@return boolean
function ReadOnlyPartitionedSet:contains(element) end

---@param element Island
---@return boolean
function ReadOnlyPartitionedSet:isActive(element) end

---@return Enumerator
function ReadOnlyPartitionedSet:getEnumerator() end

---@class DynamicTree
---@field proxies ReadOnlyPartitionedSet
---@field root number
---@field filter fun(arg0: IDynamicTreeProxy, arg1: IDynamicTreeProxy): boolean
---@field updatedProxyCount number
---@field hashSetInfo ValueTuple
---@field nodes Node
---@field debugTimings Double
---@field nullNode number
---@field initialSize number
---@field pruningFraction number
---@field expandFactor number
---@field expandEps number
local DynamicTree = {}
---@param action fun(arg0: IDynamicTreeProxy, arg1: IDynamicTreeProxy): void
---@param multiThread boolean
---@return void
function DynamicTree:enumerateOverlaps(action, multiThread) end

---@param multiThread boolean
---@param dt number
---@return void
function DynamicTree:update(multiThread, dt) end

---@param proxy any
---@param active boolean
---@return void
function DynamicTree:addProxy(proxy, active) end

---@param proxy any
---@return boolean
function DynamicTree:isActive(proxy) end

---@param proxy any
---@return void
function DynamicTree:activateProxy(proxy) end

---@param proxy any
---@return void
function DynamicTree:deactivateProxy(proxy) end

---@param proxy IDynamicTreeProxy
---@return void
function DynamicTree:removeProxy(proxy) end

---@return number
function DynamicTree:calculateCost() end

---@param action fun(arg0: TreeBox, arg1: number): void
---@return void
function DynamicTree:enumerateTreeBoxes(action) end

---@param hits any
---@param rayOrigin JVector
---@param rayDirection JVector
---@return void
function DynamicTree:query(hits, rayOrigin, rayDirection) end

---@param sweeps number
---@param chance number
---@param incremental boolean
---@return void
function DynamicTree:optimize(sweeps, chance, incremental) end

---@param origin JVector
---@param direction JVector
---@param pre RayCastFilterPre
---@param post RayCastFilterPost
---@param proxy IDynamicTreeProxy
---@param normal JVector
---@param lambda number
---@return boolean
function DynamicTree:rayCast(origin, direction, pre, post, proxy, normal, lambda) end

---@class INarrowPhaseFilter
local INarrowPhaseFilter = {}
---@param shapeA RigidBodyShape
---@param shapeB RigidBodyShape
---@param pointA JVector
---@param pointB JVector
---@param normal JVector
---@param penetration number
---@return boolean
function INarrowPhaseFilter:filter(shapeA, shapeB, pointA, pointB, normal, penetration) end

---@class IBroadPhaseFilter
local IBroadPhaseFilter = {}
---@param proxyA IDynamicTreeProxy
---@param proxyB IDynamicTreeProxy
---@return boolean
function IBroadPhaseFilter:filter(proxyA, proxyB) end

---@class IDynamicTreeProxy
---@field nodePtr number
---@field velocity JVector
---@field worldBoundingBox JBoundingBox
local IDynamicTreeProxy = {}
---@class SolveMode
---@field value__ number
---@field none SolveMode
---@field linearBody1 SolveMode
---@field angularBody1 SolveMode
---@field linearBody2 SolveMode
---@field angularBody2 SolveMode
---@field fullBody1 SolveMode
---@field fullBody2 SolveMode
---@field linear SolveMode
---@field angular SolveMode
---@field full SolveMode
local SolveMode = {}
---@class Shape
---@field worldBoundingBox JBoundingBox
---@field velocity JVector
---@field shapeId number
local Shape = {}
---@param dt number
---@return void
function Shape:updateWorldBoundingBox(dt) end

---@param origin JVector
---@param direction JVector
---@param normal JVector
---@param lambda number
---@return boolean
function Shape:rayCast(origin, direction, normal, lambda) end

---@param direction JVector
---@param result JVector
---@return void
function Shape:supportMap(direction, result) end

---@param point JVector
---@return void
function Shape:getCenter(point) end

---@class JsonContract
---@field underlyingType Type
---@field createdType Type
---@field isReference Nullable
---@field converter JsonConverter
---@field internalConverter JsonConverter
---@field onDeserializedCallbacks IList
---@field onDeserializingCallbacks IList
---@field onSerializedCallbacks IList
---@field onSerializingCallbacks IList
---@field onErrorCallbacks IList
---@field defaultCreator fun(): Object
---@field defaultCreatorNonPublic boolean
local JsonContract = {}
---@class Enumerator
---@field current Island
local Enumerator = {}
---@return void
function Enumerator:dispose() end

---@return boolean
function Enumerator:moveNext() end

---@return void
function Enumerator:reset() end

---@class Node
---@field length number
---@field longLength number
---@field rank number
---@field syncRoot Object
---@field isReadOnly boolean
---@field isFixedSize boolean
---@field isSynchronized boolean
local Node = {}
---@param arg0 number
---@return Node
function Node:get(arg0) end

---@param arg0 number
---@param arg1 Node
---@return void
function Node:set(arg0, arg1) end

---@param arg0 number
---@return Node
function Node:address(arg0) end

---@class TreeBox
---@field vectorMin Vector128
---@field vectorMax Vector128
---@field center JVector
---@field min JVector
---@field minW number
---@field max JVector
---@field maxW number
---@field epsilon number
local TreeBox = {}
---@return JBoundingBox
function TreeBox:asJBoundingBox() end

---@param point JVector
---@return boolean
function TreeBox:contains(point) end

---@param box JBoundingBox
---@return boolean
function TreeBox:notDisjoint(box) end

---@param box JBoundingBox
---@return boolean
function TreeBox:disjoint(box) end

---@param box JBoundingBox
---@return boolean
function TreeBox:encompasses(box) end

---@param origin JVector
---@param direction JVector
---@return boolean
function TreeBox:segmentIntersect(origin, direction) end

---@param origin JVector
---@param direction JVector
---@return boolean
function TreeBox:rayIntersect(origin, direction) end

---@return string
function TreeBox:toString() end

---@return number
function TreeBox:getSurfaceArea() end

---@param first TreeBox
---@param second TreeBox
---@return number
function TreeBox.mergedSurface(first, second) end

---@param first TreeBox
---@param second TreeBox
---@param result TreeBox
---@return void
function TreeBox.createMerged(first, second, result) end

---@param first TreeBox
---@param second TreeBox
---@return boolean
function TreeBox.equals(first, second) end

---@return number
function TreeBox:getHashCode() end

---@class RayCastFilterPre
---@field target Object
---@field method MethodInfo
local RayCastFilterPre = {}
---@param result IDynamicTreeProxy
---@return boolean
function RayCastFilterPre:invoke(result) end

---@param result IDynamicTreeProxy
---@param callback AsyncCallback
---@param object Object
---@return IAsyncResult
function RayCastFilterPre:beginInvoke(result, callback, object) end

---@param result IAsyncResult
---@return boolean
function RayCastFilterPre:endInvoke(result) end

---@class RayCastFilterPost
---@field target Object
---@field method MethodInfo
local RayCastFilterPost = {}
---@param result RayCastResult
---@return boolean
function RayCastFilterPost:invoke(result) end

---@param result RayCastResult
---@param callback AsyncCallback
---@param object Object
---@return IAsyncResult
function RayCastFilterPost:beginInvoke(result, callback, object) end

---@param result IAsyncResult
---@return boolean
function RayCastFilterPost:endInvoke(result) end

---@class RayCastResult
---@field entity IDynamicTreeProxy
---@field lambda number
---@field normal JVector
local RayCastResult = {}
