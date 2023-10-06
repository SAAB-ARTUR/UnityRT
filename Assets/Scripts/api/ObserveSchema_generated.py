# automatically generated by the FlatBuffers compiler, do not modify

# namespace: Artur

import flatbuffers
from flatbuffers.compat import import_numpy
np = import_numpy()

class Vec3(object):
    __slots__ = ['_tab']

    @classmethod
    def SizeOf(cls):
        return 24

    # Vec3
    def Init(self, buf, pos):
        self._tab = flatbuffers.table.Table(buf, pos)

    # Vec3
    def X(self): return self._tab.Get(flatbuffers.number_types.Float64Flags, self._tab.Pos + flatbuffers.number_types.UOffsetTFlags.py_type(0))
    # Vec3
    def Y(self): return self._tab.Get(flatbuffers.number_types.Float64Flags, self._tab.Pos + flatbuffers.number_types.UOffsetTFlags.py_type(8))
    # Vec3
    def Z(self): return self._tab.Get(flatbuffers.number_types.Float64Flags, self._tab.Pos + flatbuffers.number_types.UOffsetTFlags.py_type(16))

def CreateVec3(builder, x, y, z):
    builder.Prep(8, 24)
    builder.PrependFloat64(z)
    builder.PrependFloat64(y)
    builder.PrependFloat64(x)
    return builder.Offset()


class CylindricalCoord(object):
    __slots__ = ['_tab']

    @classmethod
    def SizeOf(cls):
        return 24

    # CylindricalCoord
    def Init(self, buf, pos):
        self._tab = flatbuffers.table.Table(buf, pos)

    # CylindricalCoord
    def R(self): return self._tab.Get(flatbuffers.number_types.Float64Flags, self._tab.Pos + flatbuffers.number_types.UOffsetTFlags.py_type(0))
    # CylindricalCoord
    def Phi(self): return self._tab.Get(flatbuffers.number_types.Float64Flags, self._tab.Pos + flatbuffers.number_types.UOffsetTFlags.py_type(8))
    # CylindricalCoord
    def Z(self): return self._tab.Get(flatbuffers.number_types.Float64Flags, self._tab.Pos + flatbuffers.number_types.UOffsetTFlags.py_type(16))

def CreateCylindricalCoord(builder, r, phi, z):
    builder.Prep(8, 24)
    builder.PrependFloat64(z)
    builder.PrependFloat64(phi)
    builder.PrependFloat64(r)
    return builder.Offset()


class AngleSpan(object):
    __slots__ = ['_tab']

    @classmethod
    def SizeOf(cls):
        return 32

    # AngleSpan
    def Init(self, buf, pos):
        self._tab = flatbuffers.table.Table(buf, pos)

    # AngleSpan
    def NPhi(self): return self._tab.Get(flatbuffers.number_types.Int32Flags, self._tab.Pos + flatbuffers.number_types.UOffsetTFlags.py_type(0))
    # AngleSpan
    def PhiSpan(self): return self._tab.Get(flatbuffers.number_types.Float64Flags, self._tab.Pos + flatbuffers.number_types.UOffsetTFlags.py_type(8))
    # AngleSpan
    def NTheta(self): return self._tab.Get(flatbuffers.number_types.Int32Flags, self._tab.Pos + flatbuffers.number_types.UOffsetTFlags.py_type(16))
    # AngleSpan
    def ThetaSpan(self): return self._tab.Get(flatbuffers.number_types.Float64Flags, self._tab.Pos + flatbuffers.number_types.UOffsetTFlags.py_type(24))

def CreateAngleSpan(builder, nPhi, phiSpan, nTheta, thetaSpan):
    builder.Prep(8, 32)
    builder.PrependFloat64(thetaSpan)
    builder.Pad(4)
    builder.PrependInt32(nTheta)
    builder.PrependFloat64(phiSpan)
    builder.Pad(4)
    builder.PrependInt32(nPhi)
    return builder.Offset()


class SphericalDir(object):
    __slots__ = ['_tab']

    @classmethod
    def SizeOf(cls):
        return 16

    # SphericalDir
    def Init(self, buf, pos):
        self._tab = flatbuffers.table.Table(buf, pos)

    # SphericalDir
    def Phi(self): return self._tab.Get(flatbuffers.number_types.Float64Flags, self._tab.Pos + flatbuffers.number_types.UOffsetTFlags.py_type(0))
    # SphericalDir
    def Theta(self): return self._tab.Get(flatbuffers.number_types.Float64Flags, self._tab.Pos + flatbuffers.number_types.UOffsetTFlags.py_type(8))

def CreateSphericalDir(builder, phi, theta):
    builder.Prep(8, 16)
    builder.PrependFloat64(theta)
    builder.PrependFloat64(phi)
    return builder.Offset()


class Sender(object):
    __slots__ = ['_tab']

    @classmethod
    def GetRootAs(cls, buf, offset=0):
        n = flatbuffers.encode.Get(flatbuffers.packer.uoffset, buf, offset)
        x = Sender()
        x.Init(buf, n + offset)
        return x

    @classmethod
    def GetRootAsSender(cls, buf, offset=0):
        """This method is deprecated. Please switch to GetRootAs."""
        return cls.GetRootAs(buf, offset)
    # Sender
    def Init(self, buf, pos):
        self._tab = flatbuffers.table.Table(buf, pos)

    # Sender
    def Position(self):
        o = flatbuffers.number_types.UOffsetTFlags.py_type(self._tab.Offset(4))
        if o != 0:
            x = o + self._tab.Pos
            obj = Vec3()
            obj.Init(self._tab.Bytes, x)
            return obj
        return None

    # Sender
    def AngleSpan(self):
        o = flatbuffers.number_types.UOffsetTFlags.py_type(self._tab.Offset(6))
        if o != 0:
            x = o + self._tab.Pos
            obj = AngleSpan()
            obj.Init(self._tab.Bytes, x)
            return obj
        return None

    # Sender
    def LookAt(self):
        o = flatbuffers.number_types.UOffsetTFlags.py_type(self._tab.Offset(8))
        if o != 0:
            x = o + self._tab.Pos
            obj = SphericalDir()
            obj.Init(self._tab.Bytes, x)
            return obj
        return None

def SenderStart(builder):
    builder.StartObject(3)

def SenderAddPosition(builder, position):
    builder.PrependStructSlot(0, flatbuffers.number_types.UOffsetTFlags.py_type(position), 0)

def SenderAddAngleSpan(builder, angleSpan):
    builder.PrependStructSlot(1, flatbuffers.number_types.UOffsetTFlags.py_type(angleSpan), 0)

def SenderAddLookAt(builder, lookAt):
    builder.PrependStructSlot(2, flatbuffers.number_types.UOffsetTFlags.py_type(lookAt), 0)

def SenderEnd(builder):
    return builder.EndObject()



class Reciever(object):
    __slots__ = ['_tab']

    @classmethod
    def GetRootAs(cls, buf, offset=0):
        n = flatbuffers.encode.Get(flatbuffers.packer.uoffset, buf, offset)
        x = Reciever()
        x.Init(buf, n + offset)
        return x

    @classmethod
    def GetRootAsReciever(cls, buf, offset=0):
        """This method is deprecated. Please switch to GetRootAs."""
        return cls.GetRootAs(buf, offset)
    # Reciever
    def Init(self, buf, pos):
        self._tab = flatbuffers.table.Table(buf, pos)

    # Reciever
    def Position(self):
        o = flatbuffers.number_types.UOffsetTFlags.py_type(self._tab.Offset(4))
        if o != 0:
            x = o + self._tab.Pos
            obj = Vec3()
            obj.Init(self._tab.Bytes, x)
            return obj
        return None

def RecieverStart(builder):
    builder.StartObject(1)

def RecieverAddPosition(builder, position):
    builder.PrependStructSlot(0, flatbuffers.number_types.UOffsetTFlags.py_type(position), 0)

def RecieverEnd(builder):
    return builder.EndObject()



class Ray(object):
    __slots__ = ['_tab']

    @classmethod
    def GetRootAs(cls, buf, offset=0):
        n = flatbuffers.encode.Get(flatbuffers.packer.uoffset, buf, offset)
        x = Ray()
        x.Init(buf, n + offset)
        return x

    @classmethod
    def GetRootAsRay(cls, buf, offset=0):
        """This method is deprecated. Please switch to GetRootAs."""
        return cls.GetRootAs(buf, offset)
    # Ray
    def Init(self, buf, pos):
        self._tab = flatbuffers.table.Table(buf, pos)

    # Ray
    def Nbot(self):
        o = flatbuffers.number_types.UOffsetTFlags.py_type(self._tab.Offset(4))
        if o != 0:
            return self._tab.Get(flatbuffers.number_types.Int32Flags, o + self._tab.Pos)
        return 0

    # Ray
    def Ntop(self):
        o = flatbuffers.number_types.UOffsetTFlags.py_type(self._tab.Offset(6))
        if o != 0:
            return self._tab.Get(flatbuffers.number_types.Int32Flags, o + self._tab.Pos)
        return 0

    # Ray
    def Ncaust(self):
        o = flatbuffers.number_types.UOffsetTFlags.py_type(self._tab.Offset(8))
        if o != 0:
            return self._tab.Get(flatbuffers.number_types.Int32Flags, o + self._tab.Pos)
        return 0

    # Ray
    def Delay(self):
        o = flatbuffers.number_types.UOffsetTFlags.py_type(self._tab.Offset(10))
        if o != 0:
            return self._tab.Get(flatbuffers.number_types.Float64Flags, o + self._tab.Pos)
        return 0.0

    # Ray
    def Curve(self):
        o = flatbuffers.number_types.UOffsetTFlags.py_type(self._tab.Offset(12))
        if o != 0:
            return self._tab.Get(flatbuffers.number_types.Float64Flags, o + self._tab.Pos)
        return 0.0

    # Ray
    def NormalizedDistance(self):
        o = flatbuffers.number_types.UOffsetTFlags.py_type(self._tab.Offset(14))
        if o != 0:
            return self._tab.Get(flatbuffers.number_types.Float64Flags, o + self._tab.Pos)
        return 0.0

    # Ray
    def StartAngle(self):
        o = flatbuffers.number_types.UOffsetTFlags.py_type(self._tab.Offset(16))
        if o != 0:
            return self._tab.Get(flatbuffers.number_types.Float64Flags, o + self._tab.Pos)
        return 0.0

    # Ray
    def XCylindrical(self, j):
        o = flatbuffers.number_types.UOffsetTFlags.py_type(self._tab.Offset(18))
        if o != 0:
            x = self._tab.Vector(o)
            x += flatbuffers.number_types.UOffsetTFlags.py_type(j) * 24
            obj = CylindricalCoord()
            obj.Init(self._tab.Bytes, x)
            return obj
        return None

    # Ray
    def XCylindricalLength(self):
        o = flatbuffers.number_types.UOffsetTFlags.py_type(self._tab.Offset(18))
        if o != 0:
            return self._tab.VectorLen(o)
        return 0

    # Ray
    def XCylindricalIsNone(self):
        o = flatbuffers.number_types.UOffsetTFlags.py_type(self._tab.Offset(18))
        return o == 0

    # Ray
    def XCartesian(self, j):
        o = flatbuffers.number_types.UOffsetTFlags.py_type(self._tab.Offset(20))
        if o != 0:
            x = self._tab.Vector(o)
            x += flatbuffers.number_types.UOffsetTFlags.py_type(j) * 24
            obj = Vec3()
            obj.Init(self._tab.Bytes, x)
            return obj
        return None

    # Ray
    def XCartesianLength(self):
        o = flatbuffers.number_types.UOffsetTFlags.py_type(self._tab.Offset(20))
        if o != 0:
            return self._tab.VectorLen(o)
        return 0

    # Ray
    def XCartesianIsNone(self):
        o = flatbuffers.number_types.UOffsetTFlags.py_type(self._tab.Offset(20))
        return o == 0

    # Ray
    def Beta(self):
        o = flatbuffers.number_types.UOffsetTFlags.py_type(self._tab.Offset(22))
        if o != 0:
            return self._tab.Get(flatbuffers.number_types.Float64Flags, o + self._tab.Pos)
        return 0.0

def RayStart(builder):
    builder.StartObject(10)

def RayAddNbot(builder, nbot):
    builder.PrependInt32Slot(0, nbot, 0)

def RayAddNtop(builder, ntop):
    builder.PrependInt32Slot(1, ntop, 0)

def RayAddNcaust(builder, ncaust):
    builder.PrependInt32Slot(2, ncaust, 0)

def RayAddDelay(builder, delay):
    builder.PrependFloat64Slot(3, delay, 0.0)

def RayAddCurve(builder, curve):
    builder.PrependFloat64Slot(4, curve, 0.0)

def RayAddNormalizedDistance(builder, normalizedDistance):
    builder.PrependFloat64Slot(5, normalizedDistance, 0.0)

def RayAddStartAngle(builder, startAngle):
    builder.PrependFloat64Slot(6, startAngle, 0.0)

def RayAddXCylindrical(builder, xCylindrical):
    builder.PrependUOffsetTRelativeSlot(7, flatbuffers.number_types.UOffsetTFlags.py_type(xCylindrical), 0)

def RayStartXCylindricalVector(builder, numElems):
    return builder.StartVector(24, numElems, 8)

def RayAddXCartesian(builder, xCartesian):
    builder.PrependUOffsetTRelativeSlot(8, flatbuffers.number_types.UOffsetTFlags.py_type(xCartesian), 0)

def RayStartXCartesianVector(builder, numElems):
    return builder.StartVector(24, numElems, 8)

def RayAddBeta(builder, beta):
    builder.PrependFloat64Slot(9, beta, 0.0)

def RayEnd(builder):
    return builder.EndObject()



class RayCollection(object):
    __slots__ = ['_tab']

    @classmethod
    def GetRootAs(cls, buf, offset=0):
        n = flatbuffers.encode.Get(flatbuffers.packer.uoffset, buf, offset)
        x = RayCollection()
        x.Init(buf, n + offset)
        return x

    @classmethod
    def GetRootAsRayCollection(cls, buf, offset=0):
        """This method is deprecated. Please switch to GetRootAs."""
        return cls.GetRootAs(buf, offset)
    # RayCollection
    def Init(self, buf, pos):
        self._tab = flatbuffers.table.Table(buf, pos)

    # RayCollection
    def Sender(self):
        o = flatbuffers.number_types.UOffsetTFlags.py_type(self._tab.Offset(4))
        if o != 0:
            x = self._tab.Indirect(o + self._tab.Pos)
            obj = Sender()
            obj.Init(self._tab.Bytes, x)
            return obj
        return None

    # RayCollection
    def Reciever(self):
        o = flatbuffers.number_types.UOffsetTFlags.py_type(self._tab.Offset(6))
        if o != 0:
            x = self._tab.Indirect(o + self._tab.Pos)
            obj = Reciever()
            obj.Init(self._tab.Bytes, x)
            return obj
        return None

    # RayCollection
    def Rays(self, j):
        o = flatbuffers.number_types.UOffsetTFlags.py_type(self._tab.Offset(8))
        if o != 0:
            x = self._tab.Vector(o)
            x += flatbuffers.number_types.UOffsetTFlags.py_type(j) * 4
            x = self._tab.Indirect(x)
            obj = Ray()
            obj.Init(self._tab.Bytes, x)
            return obj
        return None

    # RayCollection
    def RaysLength(self):
        o = flatbuffers.number_types.UOffsetTFlags.py_type(self._tab.Offset(8))
        if o != 0:
            return self._tab.VectorLen(o)
        return 0

    # RayCollection
    def RaysIsNone(self):
        o = flatbuffers.number_types.UOffsetTFlags.py_type(self._tab.Offset(8))
        return o == 0

def RayCollectionStart(builder):
    builder.StartObject(3)

def RayCollectionAddSender(builder, sender):
    builder.PrependUOffsetTRelativeSlot(0, flatbuffers.number_types.UOffsetTFlags.py_type(sender), 0)

def RayCollectionAddReciever(builder, reciever):
    builder.PrependUOffsetTRelativeSlot(1, flatbuffers.number_types.UOffsetTFlags.py_type(reciever), 0)

def RayCollectionAddRays(builder, rays):
    builder.PrependUOffsetTRelativeSlot(2, flatbuffers.number_types.UOffsetTFlags.py_type(rays), 0)

def RayCollectionStartRaysVector(builder, numElems):
    return builder.StartVector(4, numElems, 4)

def RayCollectionEnd(builder):
    return builder.EndObject()



class World(object):
    __slots__ = ['_tab']

    @classmethod
    def GetRootAs(cls, buf, offset=0):
        n = flatbuffers.encode.Get(flatbuffers.packer.uoffset, buf, offset)
        x = World()
        x.Init(buf, n + offset)
        return x

    @classmethod
    def GetRootAsWorld(cls, buf, offset=0):
        """This method is deprecated. Please switch to GetRootAs."""
        return cls.GetRootAs(buf, offset)
    # World
    def Init(self, buf, pos):
        self._tab = flatbuffers.table.Table(buf, pos)

    # World
    def Sender(self):
        o = flatbuffers.number_types.UOffsetTFlags.py_type(self._tab.Offset(4))
        if o != 0:
            x = self._tab.Indirect(o + self._tab.Pos)
            obj = Sender()
            obj.Init(self._tab.Bytes, x)
            return obj
        return None

    # World
    def Reciever(self):
        o = flatbuffers.number_types.UOffsetTFlags.py_type(self._tab.Offset(6))
        if o != 0:
            x = self._tab.Indirect(o + self._tab.Pos)
            obj = Reciever()
            obj.Init(self._tab.Bytes, x)
            return obj
        return None

    # World
    def RayCollections(self, j):
        o = flatbuffers.number_types.UOffsetTFlags.py_type(self._tab.Offset(8))
        if o != 0:
            x = self._tab.Vector(o)
            x += flatbuffers.number_types.UOffsetTFlags.py_type(j) * 4
            x = self._tab.Indirect(x)
            obj = RayCollection()
            obj.Init(self._tab.Bytes, x)
            return obj
        return None

    # World
    def RayCollectionsLength(self):
        o = flatbuffers.number_types.UOffsetTFlags.py_type(self._tab.Offset(8))
        if o != 0:
            return self._tab.VectorLen(o)
        return 0

    # World
    def RayCollectionsIsNone(self):
        o = flatbuffers.number_types.UOffsetTFlags.py_type(self._tab.Offset(8))
        return o == 0

def WorldStart(builder):
    builder.StartObject(3)

def WorldAddSender(builder, sender):
    builder.PrependUOffsetTRelativeSlot(0, flatbuffers.number_types.UOffsetTFlags.py_type(sender), 0)

def WorldAddReciever(builder, reciever):
    builder.PrependUOffsetTRelativeSlot(1, flatbuffers.number_types.UOffsetTFlags.py_type(reciever), 0)

def WorldAddRayCollections(builder, rayCollections):
    builder.PrependUOffsetTRelativeSlot(2, flatbuffers.number_types.UOffsetTFlags.py_type(rayCollections), 0)

def WorldStartRayCollectionsVector(builder, numElems):
    return builder.StartVector(4, numElems, 4)

def WorldEnd(builder):
    return builder.EndObject()


