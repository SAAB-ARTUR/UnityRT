// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

namespace SAAB.Artur
{

using global::System;
using global::System.Collections.Generic;
using global::Google.FlatBuffers;

public struct Vec3 : IFlatbufferObject
{
  private Struct __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public void __init(int _i, ByteBuffer _bb) { __p = new Struct(_i, _bb); }
  public Vec3 __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public double X { get { return __p.bb.GetDouble(__p.bb_pos + 0); } }
  public double Y { get { return __p.bb.GetDouble(__p.bb_pos + 8); } }
  public double Z { get { return __p.bb.GetDouble(__p.bb_pos + 16); } }

  public static Offset<SAAB.Artur.Vec3> CreateVec3(FlatBufferBuilder builder, double X, double Y, double Z) {
    builder.Prep(8, 24);
    builder.PutDouble(Z);
    builder.PutDouble(Y);
    builder.PutDouble(X);
    return new Offset<SAAB.Artur.Vec3>(builder.Offset);
  }
}

public struct CylindricalCoord : IFlatbufferObject
{
  private Struct __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public void __init(int _i, ByteBuffer _bb) { __p = new Struct(_i, _bb); }
  public CylindricalCoord __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public double R { get { return __p.bb.GetDouble(__p.bb_pos + 0); } }
  public double Phi { get { return __p.bb.GetDouble(__p.bb_pos + 8); } }
  public double Z { get { return __p.bb.GetDouble(__p.bb_pos + 16); } }

  public static Offset<SAAB.Artur.CylindricalCoord> CreateCylindricalCoord(FlatBufferBuilder builder, double R, double Phi, double Z) {
    builder.Prep(8, 24);
    builder.PutDouble(Z);
    builder.PutDouble(Phi);
    builder.PutDouble(R);
    return new Offset<SAAB.Artur.CylindricalCoord>(builder.Offset);
  }
}

public struct AngleSpan : IFlatbufferObject
{
  private Struct __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public void __init(int _i, ByteBuffer _bb) { __p = new Struct(_i, _bb); }
  public AngleSpan __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public int NPhi { get { return __p.bb.GetInt(__p.bb_pos + 0); } }
  public double PhiSpan { get { return __p.bb.GetDouble(__p.bb_pos + 8); } }
  public int NTheta { get { return __p.bb.GetInt(__p.bb_pos + 16); } }
  public double ThetaSpan { get { return __p.bb.GetDouble(__p.bb_pos + 24); } }

  public static Offset<SAAB.Artur.AngleSpan> CreateAngleSpan(FlatBufferBuilder builder, int NPhi, double PhiSpan, int NTheta, double ThetaSpan) {
    builder.Prep(8, 32);
    builder.PutDouble(ThetaSpan);
    builder.Pad(4);
    builder.PutInt(NTheta);
    builder.PutDouble(PhiSpan);
    builder.Pad(4);
    builder.PutInt(NPhi);
    return new Offset<SAAB.Artur.AngleSpan>(builder.Offset);
  }
}

public struct SphericalDir : IFlatbufferObject
{
  private Struct __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public void __init(int _i, ByteBuffer _bb) { __p = new Struct(_i, _bb); }
  public SphericalDir __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public double Phi { get { return __p.bb.GetDouble(__p.bb_pos + 0); } }
  public double Theta { get { return __p.bb.GetDouble(__p.bb_pos + 8); } }

  public static Offset<SAAB.Artur.SphericalDir> CreateSphericalDir(FlatBufferBuilder builder, double Phi, double Theta) {
    builder.Prep(8, 16);
    builder.PutDouble(Theta);
    builder.PutDouble(Phi);
    return new Offset<SAAB.Artur.SphericalDir>(builder.Offset);
  }
}

public struct Sender : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_23_5_26(); }
  public static Sender GetRootAsSender(ByteBuffer _bb) { return GetRootAsSender(_bb, new Sender()); }
  public static Sender GetRootAsSender(ByteBuffer _bb, Sender obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
  public Sender __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public SAAB.Artur.Vec3? Position { get { int o = __p.__offset(4); return o != 0 ? (SAAB.Artur.Vec3?)(new SAAB.Artur.Vec3()).__assign(o + __p.bb_pos, __p.bb) : null; } }
  public SAAB.Artur.AngleSpan? AngleSpan { get { int o = __p.__offset(6); return o != 0 ? (SAAB.Artur.AngleSpan?)(new SAAB.Artur.AngleSpan()).__assign(o + __p.bb_pos, __p.bb) : null; } }
  public SAAB.Artur.SphericalDir? LookAt { get { int o = __p.__offset(8); return o != 0 ? (SAAB.Artur.SphericalDir?)(new SAAB.Artur.SphericalDir()).__assign(o + __p.bb_pos, __p.bb) : null; } }

  public static void StartSender(FlatBufferBuilder builder) { builder.StartTable(3); }
  public static void AddPosition(FlatBufferBuilder builder, Offset<SAAB.Artur.Vec3> positionOffset) { builder.AddStruct(0, positionOffset.Value, 0); }
  public static void AddAngleSpan(FlatBufferBuilder builder, Offset<SAAB.Artur.AngleSpan> angleSpanOffset) { builder.AddStruct(1, angleSpanOffset.Value, 0); }
  public static void AddLookAt(FlatBufferBuilder builder, Offset<SAAB.Artur.SphericalDir> lookAtOffset) { builder.AddStruct(2, lookAtOffset.Value, 0); }
  public static Offset<SAAB.Artur.Sender> EndSender(FlatBufferBuilder builder) {
    int o = builder.EndTable();
    return new Offset<SAAB.Artur.Sender>(o);
  }
}


static public class SenderVerify
{
  static public bool Verify(Google.FlatBuffers.Verifier verifier, uint tablePos)
  {
    return verifier.VerifyTableStart(tablePos)
      && verifier.VerifyField(tablePos, 4 /*Position*/, 24 /*SAAB.Artur.Vec3*/, 8, false)
      && verifier.VerifyField(tablePos, 6 /*AngleSpan*/, 32 /*SAAB.Artur.AngleSpan*/, 8, false)
      && verifier.VerifyField(tablePos, 8 /*LookAt*/, 16 /*SAAB.Artur.SphericalDir*/, 8, false)
      && verifier.VerifyTableEnd(tablePos);
  }
}
public struct Reciever : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_23_5_26(); }
  public static Reciever GetRootAsReciever(ByteBuffer _bb) { return GetRootAsReciever(_bb, new Reciever()); }
  public static Reciever GetRootAsReciever(ByteBuffer _bb, Reciever obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
  public Reciever __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public SAAB.Artur.Vec3? Position { get { int o = __p.__offset(4); return o != 0 ? (SAAB.Artur.Vec3?)(new SAAB.Artur.Vec3()).__assign(o + __p.bb_pos, __p.bb) : null; } }

  public static void StartReciever(FlatBufferBuilder builder) { builder.StartTable(1); }
  public static void AddPosition(FlatBufferBuilder builder, Offset<SAAB.Artur.Vec3> positionOffset) { builder.AddStruct(0, positionOffset.Value, 0); }
  public static Offset<SAAB.Artur.Reciever> EndReciever(FlatBufferBuilder builder) {
    int o = builder.EndTable();
    return new Offset<SAAB.Artur.Reciever>(o);
  }
}


static public class RecieverVerify
{
  static public bool Verify(Google.FlatBuffers.Verifier verifier, uint tablePos)
  {
    return verifier.VerifyTableStart(tablePos)
      && verifier.VerifyField(tablePos, 4 /*Position*/, 24 /*SAAB.Artur.Vec3*/, 8, false)
      && verifier.VerifyTableEnd(tablePos);
  }
}
public struct Ray : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_23_5_26(); }
  public static Ray GetRootAsRay(ByteBuffer _bb) { return GetRootAsRay(_bb, new Ray()); }
  public static Ray GetRootAsRay(ByteBuffer _bb, Ray obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
  public Ray __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public int Nbot { get { int o = __p.__offset(4); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : (int)0; } }
  public int Ntop { get { int o = __p.__offset(6); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : (int)0; } }
  public int Ncaust { get { int o = __p.__offset(8); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : (int)0; } }
  public double Delay { get { int o = __p.__offset(10); return o != 0 ? __p.bb.GetDouble(o + __p.bb_pos) : (double)0.0; } }
  public double Curve { get { int o = __p.__offset(12); return o != 0 ? __p.bb.GetDouble(o + __p.bb_pos) : (double)0.0; } }
  public double NormalizedDistance { get { int o = __p.__offset(14); return o != 0 ? __p.bb.GetDouble(o + __p.bb_pos) : (double)0.0; } }
  public double StartAngle { get { int o = __p.__offset(16); return o != 0 ? __p.bb.GetDouble(o + __p.bb_pos) : (double)0.0; } }
  public SAAB.Artur.CylindricalCoord? XCylindrical(int j) { int o = __p.__offset(18); return o != 0 ? (SAAB.Artur.CylindricalCoord?)(new SAAB.Artur.CylindricalCoord()).__assign(__p.__vector(o) + j * 24, __p.bb) : null; }
  public int XCylindricalLength { get { int o = __p.__offset(18); return o != 0 ? __p.__vector_len(o) : 0; } }
  public SAAB.Artur.Vec3? XCartesian(int j) { int o = __p.__offset(20); return o != 0 ? (SAAB.Artur.Vec3?)(new SAAB.Artur.Vec3()).__assign(__p.__vector(o) + j * 24, __p.bb) : null; }
  public int XCartesianLength { get { int o = __p.__offset(20); return o != 0 ? __p.__vector_len(o) : 0; } }
  public double Beta { get { int o = __p.__offset(22); return o != 0 ? __p.bb.GetDouble(o + __p.bb_pos) : (double)0.0; } }

  public static Offset<SAAB.Artur.Ray> CreateRay(FlatBufferBuilder builder,
      int nbot = 0,
      int ntop = 0,
      int ncaust = 0,
      double delay = 0.0,
      double curve = 0.0,
      double normalized_distance = 0.0,
      double start_angle = 0.0,
      VectorOffset x_cylindricalOffset = default(VectorOffset),
      VectorOffset x_cartesianOffset = default(VectorOffset),
      double beta = 0.0) {
    builder.StartTable(10);
    Ray.AddBeta(builder, beta);
    Ray.AddStartAngle(builder, start_angle);
    Ray.AddNormalizedDistance(builder, normalized_distance);
    Ray.AddCurve(builder, curve);
    Ray.AddDelay(builder, delay);
    Ray.AddXCartesian(builder, x_cartesianOffset);
    Ray.AddXCylindrical(builder, x_cylindricalOffset);
    Ray.AddNcaust(builder, ncaust);
    Ray.AddNtop(builder, ntop);
    Ray.AddNbot(builder, nbot);
    return Ray.EndRay(builder);
  }

  public static void StartRay(FlatBufferBuilder builder) { builder.StartTable(10); }
  public static void AddNbot(FlatBufferBuilder builder, int nbot) { builder.AddInt(0, nbot, 0); }
  public static void AddNtop(FlatBufferBuilder builder, int ntop) { builder.AddInt(1, ntop, 0); }
  public static void AddNcaust(FlatBufferBuilder builder, int ncaust) { builder.AddInt(2, ncaust, 0); }
  public static void AddDelay(FlatBufferBuilder builder, double delay) { builder.AddDouble(3, delay, 0.0); }
  public static void AddCurve(FlatBufferBuilder builder, double curve) { builder.AddDouble(4, curve, 0.0); }
  public static void AddNormalizedDistance(FlatBufferBuilder builder, double normalizedDistance) { builder.AddDouble(5, normalizedDistance, 0.0); }
  public static void AddStartAngle(FlatBufferBuilder builder, double startAngle) { builder.AddDouble(6, startAngle, 0.0); }
  public static void AddXCylindrical(FlatBufferBuilder builder, VectorOffset xCylindricalOffset) { builder.AddOffset(7, xCylindricalOffset.Value, 0); }
  public static void StartXCylindricalVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(24, numElems, 8); }
  public static void AddXCartesian(FlatBufferBuilder builder, VectorOffset xCartesianOffset) { builder.AddOffset(8, xCartesianOffset.Value, 0); }
  public static void StartXCartesianVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(24, numElems, 8); }
  public static void AddBeta(FlatBufferBuilder builder, double beta) { builder.AddDouble(9, beta, 0.0); }
  public static Offset<SAAB.Artur.Ray> EndRay(FlatBufferBuilder builder) {
    int o = builder.EndTable();
    return new Offset<SAAB.Artur.Ray>(o);
  }
}


static public class RayVerify
{
  static public bool Verify(Google.FlatBuffers.Verifier verifier, uint tablePos)
  {
    return verifier.VerifyTableStart(tablePos)
      && verifier.VerifyField(tablePos, 4 /*Nbot*/, 4 /*int*/, 4, false)
      && verifier.VerifyField(tablePos, 6 /*Ntop*/, 4 /*int*/, 4, false)
      && verifier.VerifyField(tablePos, 8 /*Ncaust*/, 4 /*int*/, 4, false)
      && verifier.VerifyField(tablePos, 10 /*Delay*/, 8 /*double*/, 8, false)
      && verifier.VerifyField(tablePos, 12 /*Curve*/, 8 /*double*/, 8, false)
      && verifier.VerifyField(tablePos, 14 /*NormalizedDistance*/, 8 /*double*/, 8, false)
      && verifier.VerifyField(tablePos, 16 /*StartAngle*/, 8 /*double*/, 8, false)
      && verifier.VerifyVectorOfData(tablePos, 18 /*XCylindrical*/, 24 /*SAAB.Artur.CylindricalCoord*/, false)
      && verifier.VerifyVectorOfData(tablePos, 20 /*XCartesian*/, 24 /*SAAB.Artur.Vec3*/, false)
      && verifier.VerifyField(tablePos, 22 /*Beta*/, 8 /*double*/, 8, false)
      && verifier.VerifyTableEnd(tablePos);
  }
}
public struct RayCollection : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_23_5_26(); }
  public static RayCollection GetRootAsRayCollection(ByteBuffer _bb) { return GetRootAsRayCollection(_bb, new RayCollection()); }
  public static RayCollection GetRootAsRayCollection(ByteBuffer _bb, RayCollection obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
  public RayCollection __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public SAAB.Artur.Sender? Sender { get { int o = __p.__offset(4); return o != 0 ? (SAAB.Artur.Sender?)(new SAAB.Artur.Sender()).__assign(__p.__indirect(o + __p.bb_pos), __p.bb) : null; } }
  public SAAB.Artur.Reciever? Reciever { get { int o = __p.__offset(6); return o != 0 ? (SAAB.Artur.Reciever?)(new SAAB.Artur.Reciever()).__assign(__p.__indirect(o + __p.bb_pos), __p.bb) : null; } }
  public SAAB.Artur.Ray? Rays(int j) { int o = __p.__offset(8); return o != 0 ? (SAAB.Artur.Ray?)(new SAAB.Artur.Ray()).__assign(__p.__indirect(__p.__vector(o) + j * 4), __p.bb) : null; }
  public int RaysLength { get { int o = __p.__offset(8); return o != 0 ? __p.__vector_len(o) : 0; } }

  public static Offset<SAAB.Artur.RayCollection> CreateRayCollection(FlatBufferBuilder builder,
      Offset<SAAB.Artur.Sender> senderOffset = default(Offset<SAAB.Artur.Sender>),
      Offset<SAAB.Artur.Reciever> recieverOffset = default(Offset<SAAB.Artur.Reciever>),
      VectorOffset raysOffset = default(VectorOffset)) {
    builder.StartTable(3);
    RayCollection.AddRays(builder, raysOffset);
    RayCollection.AddReciever(builder, recieverOffset);
    RayCollection.AddSender(builder, senderOffset);
    return RayCollection.EndRayCollection(builder);
  }

  public static void StartRayCollection(FlatBufferBuilder builder) { builder.StartTable(3); }
  public static void AddSender(FlatBufferBuilder builder, Offset<SAAB.Artur.Sender> senderOffset) { builder.AddOffset(0, senderOffset.Value, 0); }
  public static void AddReciever(FlatBufferBuilder builder, Offset<SAAB.Artur.Reciever> recieverOffset) { builder.AddOffset(1, recieverOffset.Value, 0); }
  public static void AddRays(FlatBufferBuilder builder, VectorOffset raysOffset) { builder.AddOffset(2, raysOffset.Value, 0); }
  public static VectorOffset CreateRaysVector(FlatBufferBuilder builder, Offset<SAAB.Artur.Ray>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static VectorOffset CreateRaysVectorBlock(FlatBufferBuilder builder, Offset<SAAB.Artur.Ray>[] data) { builder.StartVector(4, data.Length, 4); builder.Add(data); return builder.EndVector(); }
  public static VectorOffset CreateRaysVectorBlock(FlatBufferBuilder builder, ArraySegment<Offset<SAAB.Artur.Ray>> data) { builder.StartVector(4, data.Count, 4); builder.Add(data); return builder.EndVector(); }
  public static VectorOffset CreateRaysVectorBlock(FlatBufferBuilder builder, IntPtr dataPtr, int sizeInBytes) { builder.StartVector(1, sizeInBytes, 1); builder.Add<Offset<SAAB.Artur.Ray>>(dataPtr, sizeInBytes); return builder.EndVector(); }
  public static void StartRaysVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static Offset<SAAB.Artur.RayCollection> EndRayCollection(FlatBufferBuilder builder) {
    int o = builder.EndTable();
    return new Offset<SAAB.Artur.RayCollection>(o);
  }
}


static public class RayCollectionVerify
{
  static public bool Verify(Google.FlatBuffers.Verifier verifier, uint tablePos)
  {
    return verifier.VerifyTableStart(tablePos)
      && verifier.VerifyTable(tablePos, 4 /*Sender*/, SAAB.Artur.SenderVerify.Verify, false)
      && verifier.VerifyTable(tablePos, 6 /*Reciever*/, SAAB.Artur.RecieverVerify.Verify, false)
      && verifier.VerifyVectorOfTables(tablePos, 8 /*Rays*/, SAAB.Artur.RayVerify.Verify, false)
      && verifier.VerifyTableEnd(tablePos);
  }
}
public struct World : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_23_5_26(); }
  public static World GetRootAsWorld(ByteBuffer _bb) { return GetRootAsWorld(_bb, new World()); }
  public static World GetRootAsWorld(ByteBuffer _bb, World obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public static bool VerifyWorld(ByteBuffer _bb) {Google.FlatBuffers.Verifier verifier = new Google.FlatBuffers.Verifier(_bb); return verifier.VerifyBuffer("", false, WorldVerify.Verify); }
  public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
  public World __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public SAAB.Artur.Sender? Sender { get { int o = __p.__offset(4); return o != 0 ? (SAAB.Artur.Sender?)(new SAAB.Artur.Sender()).__assign(__p.__indirect(o + __p.bb_pos), __p.bb) : null; } }
  public SAAB.Artur.Reciever? Reciever { get { int o = __p.__offset(6); return o != 0 ? (SAAB.Artur.Reciever?)(new SAAB.Artur.Reciever()).__assign(__p.__indirect(o + __p.bb_pos), __p.bb) : null; } }
  public SAAB.Artur.RayCollection? RayCollections(int j) { int o = __p.__offset(8); return o != 0 ? (SAAB.Artur.RayCollection?)(new SAAB.Artur.RayCollection()).__assign(__p.__indirect(__p.__vector(o) + j * 4), __p.bb) : null; }
  public int RayCollectionsLength { get { int o = __p.__offset(8); return o != 0 ? __p.__vector_len(o) : 0; } }

  public static Offset<SAAB.Artur.World> CreateWorld(FlatBufferBuilder builder,
      Offset<SAAB.Artur.Sender> senderOffset = default(Offset<SAAB.Artur.Sender>),
      Offset<SAAB.Artur.Reciever> recieverOffset = default(Offset<SAAB.Artur.Reciever>),
      VectorOffset ray_collectionsOffset = default(VectorOffset)) {
    builder.StartTable(3);
    World.AddRayCollections(builder, ray_collectionsOffset);
    World.AddReciever(builder, recieverOffset);
    World.AddSender(builder, senderOffset);
    return World.EndWorld(builder);
  }

  public static void StartWorld(FlatBufferBuilder builder) { builder.StartTable(3); }
  public static void AddSender(FlatBufferBuilder builder, Offset<SAAB.Artur.Sender> senderOffset) { builder.AddOffset(0, senderOffset.Value, 0); }
  public static void AddReciever(FlatBufferBuilder builder, Offset<SAAB.Artur.Reciever> recieverOffset) { builder.AddOffset(1, recieverOffset.Value, 0); }
  public static void AddRayCollections(FlatBufferBuilder builder, VectorOffset rayCollectionsOffset) { builder.AddOffset(2, rayCollectionsOffset.Value, 0); }
  public static VectorOffset CreateRayCollectionsVector(FlatBufferBuilder builder, Offset<SAAB.Artur.RayCollection>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static VectorOffset CreateRayCollectionsVectorBlock(FlatBufferBuilder builder, Offset<SAAB.Artur.RayCollection>[] data) { builder.StartVector(4, data.Length, 4); builder.Add(data); return builder.EndVector(); }
  public static VectorOffset CreateRayCollectionsVectorBlock(FlatBufferBuilder builder, ArraySegment<Offset<SAAB.Artur.RayCollection>> data) { builder.StartVector(4, data.Count, 4); builder.Add(data); return builder.EndVector(); }
  public static VectorOffset CreateRayCollectionsVectorBlock(FlatBufferBuilder builder, IntPtr dataPtr, int sizeInBytes) { builder.StartVector(1, sizeInBytes, 1); builder.Add<Offset<SAAB.Artur.RayCollection>>(dataPtr, sizeInBytes); return builder.EndVector(); }
  public static void StartRayCollectionsVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static Offset<SAAB.Artur.World> EndWorld(FlatBufferBuilder builder) {
    int o = builder.EndTable();
    return new Offset<SAAB.Artur.World>(o);
  }
  public static void FinishWorldBuffer(FlatBufferBuilder builder, Offset<SAAB.Artur.World> offset) { builder.Finish(offset.Value); }
  public static void FinishSizePrefixedWorldBuffer(FlatBufferBuilder builder, Offset<SAAB.Artur.World> offset) { builder.FinishSizePrefixed(offset.Value); }
}


static public class WorldVerify
{
  static public bool Verify(Google.FlatBuffers.Verifier verifier, uint tablePos)
  {
    return verifier.VerifyTableStart(tablePos)
      && verifier.VerifyTable(tablePos, 4 /*Sender*/, SAAB.Artur.SenderVerify.Verify, false)
      && verifier.VerifyTable(tablePos, 6 /*Reciever*/, SAAB.Artur.RecieverVerify.Verify, false)
      && verifier.VerifyVectorOfTables(tablePos, 8 /*RayCollections*/, SAAB.Artur.RayCollectionVerify.Verify, false)
      && verifier.VerifyTableEnd(tablePos);
  }
}

}