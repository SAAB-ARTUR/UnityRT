import sys
import matplotlib
import matplotlib.pyplot as plt
import numpy as np
import os
import flatbuffers 
import Assets.Scripts.api.ObserveSchema_generated as schema
from mpl_toolkits.mplot3d.art3d import Line3DCollection

fig = plt.figure()

ax = fig.add_subplot(projection = "3d")
senders, = ax.plot3D([], [], [], linewidth = 0, marker = ".")
recievers, = ax.plot3D([], [], [], linewidth = 0, marker = ".")
rays = Line3DCollection(np.empty((0, 3)))
ax.add_collection(rays)

figresp, axresp = plt.subplots()
lineresp, = axresp.plot([], [])

plt.show(block = False)

data = [0, 1]


limits = np.zeros((3,2))
limits[:, 0] = -10
limits[:, 1] = 10

def plot_senders(senders, world: schema.World):
    p = world.Sender().Position()
    pos = np.array([p.X(), -p.Z(), p.Y()])
    senders.set_data_3d([pos[0]], [pos[1]], [pos[2]])

def plot_reciever(recievers, world: schema.World):
    p = world.Reciever().Position()
    pos = np.array([p.X(), -p.Z(), p.Y()])
    recievers.set_data_3d([pos[0]], [pos[1]], [pos[2]])

def plot_rays(rays, world: schema.World):

    def ray_coord(coord: schema.Vec3):
        
        return [coord.X(), -coord.Z(), coord.Y()]

    def extractRay(ray: schema.Ray):

        return [ray_coord(ray.XCartesian(ii)) for ii in range(ray.XCartesianLength())]

    # Get the rays
    if world.RayCollectionsLength() > 0:
        num_rays = world.RayCollections(0).RaysLength()
        raydata = [
         extractRay(world.RayCollections(0).Rays(ii)) for ii in range(num_rays)
        ]
        
        raydata = [raydata[ii] for ii in range(len(raydata)) if len(raydata[ii]) > 0 ]

        if len(raydata) > 0:
            
            rays.set_segments(raydata)
            rays.do_3d_projection()
            #print(raydata[0][1])

def plot_resp(y):

    axresp.clear()
    axresp.plot(y)
    #lineresp.set_data(np.arange(len(y)), y)


def fix_axis():

    global limits

    poss = np.concatenate([senders.get_data_3d(), recievers.get_data_3d()], axis = 1).T
    # print("poss = ", poss.shape)
    for pos in poss: 
        #print(pos)
        limits[:, 0] = np.min([limits[:, 0], pos], axis = 0)
        limits[:, 1] = np.max([limits[:, 1], pos], axis = 0)

    ax.set_xlim(*limits[0, :])
    ax.set_ylim(*limits[1, :])
    ax.set_zlim(*limits[2, :])


def response_handled_message() -> bytes:

    import Assets.Scripts.api.ControlSchema_generated as control_schema

    builder = flatbuffers.Builder(1024)

    control_schema.ResponseHandledStart(builder)
    msg = control_schema.ResponseHandledEnd(builder)

    control_schema.MessageStart(builder)
    control_schema.MessageAddMessageType(builder, control_schema.MessageType().ResponseHandled)
    control_schema.MessageAddMessage(builder, msg)
    message = control_schema.MessageEnd(builder)

    builder.Finish(message)

    binary_message: bytearray
    binary_message = builder.Output()
    assert isinstance(binary_message, bytearray)

    return bytes(binary_message)

def trace_message():

    import Assets.Scripts.api.ControlSchema_generated as control_schema

    builder = flatbuffers.Builder(1024)

    control_schema.TraceNowStart(builder)
    msg = control_schema.TraceNowEnd(builder)

    control_schema.MessageStart(builder)
    control_schema.MessageAddMessageType(builder, control_schema.MessageType().TraceNow)
    control_schema.MessageAddMessage(builder, msg)
    message = control_schema.MessageEnd(builder)

    builder.Finish(message)

    binary_message: bytearray
    binary_message = builder.Output()
    assert isinstance(binary_message, bytearray)

    return bytes(binary_message)

def move_sender_message(position):

    import Assets.Scripts.api.ControlSchema_generated as control_schema

    builder = flatbuffers.Builder(1024)

    position = control_schema.CreateVec3(builder, *position)
    
    control_schema.SenderStart(builder)
    control_schema.SenderAddPosition(builder, position)
    msg = control_schema.SenderEnd(builder)

    control_schema.ControlMessageStart(builder)
    control_schema.ControlMessageAddSender(builder, msg)
    msg = control_schema.ControlMessageEnd(builder)

    control_schema.MessageStart(builder)
    control_schema.MessageAddMessageType(builder, control_schema.MessageType().ControlMessage)
    control_schema.MessageAddMessage(builder, msg)
    message = control_schema.MessageEnd(builder)

    builder.Finish(message)
    binary_message: bytearray = builder.Output()

    return bytes(binary_message)

def move_receiver_message(position):

    import Assets.Scripts.api.ControlSchema_generated as control_schema

    builder = flatbuffers.Builder(1024)

    position = control_schema.CreateVec3(builder, *position)

    control_schema.RecieverStart(builder)
    control_schema.RecieverAddPosition(builder, position)
    msg = control_schema.RecieverEnd(builder)

    control_schema.ControlMessageStart(builder)
    control_schema.ControlMessageAddReciever(builder, msg)
    msg = control_schema.ControlMessageEnd(builder)

    control_schema.MessageStart(builder)
    control_schema.MessageAddMessageType(builder, control_schema.MessageType().ControlMessage)
    control_schema.MessageAddMessage(builder, msg)
    message = control_schema.MessageEnd(builder)

    builder.Finish(message)
    binary_message: bytearray = builder.Output()

    return bytes(binary_message)

    


"""
msg = move_message()
open("MoveMessage.bin", "wb").write(msg)

buf = open("MoveMessage.bin", "rb").read()
import Assets.Scripts.api.ControlSchema_generated as control_schema
message = control_schema.Message.GetRootAs(buf)

import base64
print(base64.b64encode(buf).decode("ascii"))

# Read as a Control message
control_msg = control_schema.ControlMessage()
control_msg.Init(message.Message().Bytes, message.Message().Pos)

print(control_msg.Sender().Position().X())


msg = response_handled_message()
open("ResponseHandled.bin", "wb").write(msg)


msg = move_message()
open("MoveMessage.bin", "wb").write(msg)
"""



def send(message: bytearray):

    import base64
    sys.stdout.write(base64.b64encode(message).decode("ascii") + "\n")
    sys.stdout.flush()



# Signal processing to get the response
def result2Time(Amp, phase, delay, freqs) -> np.ndarray:

    # Define the signal that is carried by the rays

    Fs = 2e4;       # Sampling rate
    T  = 0.0005;      # Period
    Ns = int(Fs*T); # Number of samples
    
    print("Ns", Ns)
    
    f = np.arange(0, Ns)

    # Ricker pulse
    tt = np.arange(0, Ns)/Fs - T/4
    fc = 1000
    xx = (np.pi*fc*tt)**2
    xx = (1 - 2*xx) * np.exp(-xx)
    X = np.fft.fft(xx)


    tmin = np.min(delay)

    # Calculate the receiver signal
    # Preallocate Y
    Y = np.zeros(Ns).astype(complex)
    for jj in range(1, int(Ns/2)):

        Pk = 0
        omega = 2 * np.pi * f[jj]

        for ii in range(len(delay)):
            print("ii", ii)
            Pk = Pk + Amp[ii,jj] * np.exp(1j * (phase[ii,jj] - omega*(delay[ii]-tmin)))
        Y[jj] = Pk * X[jj]
        Y[-jj] = np.conj(Pk) * X[-jj]
    
    yy = np.real(np.fft.ifft(Y))

    return yy
        

def extractRay(world) -> tuple[np.ndarray, np.ndarray, np.ndarray]:

    empty = np.empty((10, 10))
    return (empty, empty, empty[:, 0], empty)


theta = 0

ran = False
while (not ran):
    
    ran = not ran
    
    theta += 0.1

    pos_sender = [10 * np.cos(theta), 10 * np.sin(theta), -25 + 10 * np.sin(theta)]   
    pos_receiver = [10 * np.sin(theta), 0, -25 + 10 * np.sin(theta)]

    #blength = int.from_bytes(sys.stdin.buffer.read())
    blength = int.from_bytes(os.read(sys.stdin.fileno(), 4), sys.byteorder, signed = True)
    #sys.stdin.flush()
    # print(blength)

    # buf2 = open("SAVE_FILENAME.whatever", "rb").read()

    # print(blength)

    buf = os.read(sys.stdin.fileno(), blength)

    # print(buf)

    #print("1" + str(buf))
    # print(buf2)

    world = schema.World.GetRootAs(buf)
    plot_senders(senders, world)
    plot_reciever(recievers, world)
    plot_rays(rays, world)
    plot_resp(result2Time(*extractRay(world)))


    fix_axis()

    fig.canvas.draw()
    figresp.canvas.draw()
    plt.pause(0.01)
    #sys.stdin.buffer.flush()
    # sys.stdin.flush()

    send(move_sender_message(pos_sender))
    send(move_receiver_message(pos_receiver))
    send(trace_message())
    send(response_handled_message())
    
    #os.write(sys.stdout.fileno(), response_handled_message()).
    


    #sys.stdout.buffer.write(response_handled_message())
    #sys.stdout.buffer.flush()
    #sys.stdout.flush()
    #sys.stdout.write("ok!\n")
    #sys.stdout.flush()
    
    """
    try:
        data.append(float(msg.strip()))
    except:
        pass

    line.set_data(np.arange(len(data)), data)
    fig.canvas.draw()

    ax.set_xlim(0, len(data))
    ax.set_ylim(np.min(data), np.max(data))

    plt.pause(0.001)
    """
plt.show()

    