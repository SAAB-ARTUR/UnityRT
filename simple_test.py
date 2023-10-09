import sys
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

plt.show(block = False)

data = [0, 1]


limits = np.zeros((3,2))
limits[:, 0] = -10
limits[:, 1] = 10

def plot_senders(senders, world: schema.World):
    p = world.Sender().Position()
    pos = np.array([p.X(), p.Y(), p.Z()])
    senders.set_data_3d([pos[0]], [pos[1]], [pos[2]])

def plot_reciever(recievers, world: schema.World):
    p = world.Reciever().Position()
    pos = np.array([p.X(), p.Y(), p.Z()])
    recievers.set_data_3d([pos[0]], [pos[1]], [pos[2]])

def plot_rays(rays, world: schema.World):

    def ray_coord(coord: schema.Vec3):
        
        return [coord.X(), coord.Y(), coord.Z()]

    def extractRay(ray: schema.Ray):

        return [ray_coord(ray.XCartesian(ii)) for ii in range(ray.XCartesianLength())]

    # Get the rays
    if world.RayCollectionsLength() > 0:
        num_rays = world.RayCollections(0).RaysLength()
        raydata = [
         extractRay(world.RayCollections(0).Rays(ii)) for ii in range(num_rays)]
        

        if len(raydata) > 0:
            rays.set_segments(raydata)
            print(raydata[0][1])

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
    plot_senders(senders, world)
    plot_reciever(recievers, world)
    plot_rays(rays, world)

    fix_axis()

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
    