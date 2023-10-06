import sys
import matplotlib.pyplot as plt
import numpy as np
import os
import flatbuffers 
import Assets.Scripts.api.ObserveSchema_generated as schema


fig = plt.figure()

ax = fig.add_subplot(projection = "3d")


line, = ax.plot3D([], [], [], linewidth = 0, marker = ".")

plt.show(block = False)

data = [0, 1]


limits = np.zeros((3,2))
limits[:, 0] = -10
limits[:, 1] = 10

while True:
    sys.stdout.write("ok!\n")
    sys.stdout.flush()
    #blength = int.from_bytes(sys.stdin.buffer.read())
    blength = int.from_bytes(os.read(sys.stdin.fileno(), 4), sys.byteorder, signed = True)
    #sys.stdin.flush()
    # print(blength)

    # buf2 = open("SAVE_FILENAME.whatever", "rb").read()

    buf = os.read(sys.stdin.fileno(), blength)

    #print("1" + str(buf))
    # print(buf2)
    world = schema.World.GetRootAs(buf)
    p = world.Sender().Position()
    
    # print("Sender Pos Y: " + str(world.Sender().Position().Y()))

    pos = np.array([p.X(), p.Y(), p.Z()])

    line.set_data_3d([pos[0]], [pos[1]], [pos[2]])

    limits[:, 0] = np.min([limits[:, 0], pos], axis = 0)
    limits[:, 1] = np.max([limits[:, 1], pos], axis = 0)

    ax.set_xlim(*limits[0, :])
    ax.set_ylim(*limits[1, :])
    ax.set_zlim(*limits[2, :])

    fig.canvas.draw()
    plt.pause(0.001)
    #sys.stdin.buffer.flush()
    sys.stdin.flush()
    
    
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
    