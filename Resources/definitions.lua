---@meta

---@type Obj
self = nil

---@type Level
level = nil

---@type Camera
cam = nil

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

---@class Obj
---@field icon string
---@field color ScytheColor
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
---@field isSelected boolean
local Obj = {}
---@return void
function Obj:delete() end

---@return void
function Obj:recordedDelete() end

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

---@param t Table
---@return Obj
function Obj:find(t) end

---@param t Table
---@return Component
function Obj:findComponent(t) end

---@param name string
---@return Component
function Obj:makeComponent(name) end

---@param name string
---@return string
function Obj:safeNameForChild(name) end

---@class Level
---@field root Obj
local Level = {}
---@return void
function Level:save() end

---@param name string
---@param parent Obj
---@return Obj
function Level.makeObject(name, parent) end

---@param name string
---@param parent Obj
---@return Obj
function Level:recordedBuildObject(name, parent) end

---@param source Obj
---@return void
function Level:recordedCloneObject(source) end

---@param t Table
---@return Obj
function Level:find(t) end

---@param t Table
---@return Component
function Level:findComponent(t) end

---@class Camera : Component
---@field labelIcon string
---@field labelScytheColor ScytheColor
---@field name string
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
---@param is2D boolean
---@return void
function Camera:loop(is2D) end

---@class LuaF2
---@field new Func
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
---@field new Func
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
function LuaKb.down(keyName) end

---@param keyName string
---@return boolean
function LuaKb.pressed(keyName) end

---@param keyName string
---@return boolean
function LuaKb.released(keyName) end

---@param keyName string
---@return boolean
function LuaKb.up(keyName) end

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

---@class Component
---@field name string
---@field labelIcon string
---@field labelScytheColor ScytheColor
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
---@return void
function Component:loop(is2D) end

---@return void
function Component:quit() end

---@class Animation : Component
---@field labelIcon string
---@field labelScytheColor ScytheColor
---@field path string
---@field track number
---@field isPlaying boolean
---@field looping boolean
---@field name string
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

---@param is2D boolean
---@return void
function Animation:loop(is2D) end

---@return void
function Animation:quit() end

---@class Light : Component
---@field labelIcon string
---@field labelScytheColor ScytheColor
---@field enabled boolean
---@field type number
---@field scytheColor ScytheColor
---@field intensity number
---@field range number
---@field name string
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
---@return void
function Light:update() end

---@param is2D boolean
---@return void
function Light:loop(is2D) end

---@class Model : Component
---@field labelIcon string
---@field labelScytheColor ScytheColor
---@field path string
---@field scytheColor ScytheColor
---@field isTransparent boolean
---@field alphaCutoff number
---@field name string
---@field isSelected boolean
---@field up Vector3
---@field fwd Vector3
---@field right Vector3
---@field fwdFlat Vector3
---@field rightFlat Vector3
---@field pos Vector3
---@field rot Quaternion
---@field rlModel Model
---@field obj Obj
---@field isLoaded boolean
local Model = {}
---@return boolean
function Model:load() end

---@param is2D boolean
---@return void
function Model:loop(is2D) end

---@return void
function Model:drawTransparent() end

---@return void
function Model:quit() end

---@class Script : Component
---@field path string
---@field name string
---@field labelIcon string
---@field labelScytheColor ScytheColor
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
---@field obj Obj
---@field isLoaded boolean
local Script = {}
---@return void
function Script.register() end

---@return boolean
function Script:load() end

---@param is2D boolean
---@return void
function Script:loop(is2D) end

---@param action Action
---@return void
function Script:safeLuaCall(action) end

---@class Transform : Component
---@field labelIcon string
---@field labelScytheColor ScytheColor
---@field pos Vector3
---@field euler Vector3
---@field scale Vector3
---@field rot Quaternion
---@field worldPos Vector3
---@field worldRot Quaternion
---@field worldEuler Vector3
---@field name string
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

---@param is2D boolean
---@return void
function Transform:loop(is2D) end

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

---@class ScytheColor
---@field r number
---@field g number
---@field b number
---@field a number
local ScytheColor = {}
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
---@field raylib Camera3D
local Camera3D = {}
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
