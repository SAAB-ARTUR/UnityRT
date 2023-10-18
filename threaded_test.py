from aioconsole import get_standard_streams
import asyncio
import matplotlib.pyplot as plt
import numpy as np
import queue as Q
import sys
import os
from datetime import datetime
import flatbuffers
import Assets.Scripts.api.ObserveSchema_generated as observe_schema

processing = None
queue = Q.LifoQueue(maxsize=10)



fig = plt.figure()
ax = fig.add_subplot(projection = "3d")
# plt.ion()
plt.show(block = False)
line, = ax.plot3D([], [], [], linewidth  =0, marker = ".")

count = 0
datap = None
world = None

import sys
import threading

last_line = ''
new_line_event = threading.Event()

close = False



def keep_last_line():
    global last_line, new_line_event, datap, count, world, close
    
    #sys.stdin.buffer.mode = "rb"

    while not close:

        sys.stdin.mode = "rb"
        data = sys.stdin.buffer.read()
        #data2 = open("SAVE_FILENAME.whatever", "rb").read()
        print(data)
        #print(data2)
        world = observe_schema.World.GetRootAs(data)
        print(world)
        sys.stdin.buffer.flush()

    """
    for line in sys.stdin:
        last_line = line
        datap = float(line)
        count += 1
        line.
        # new_line_event.set()
    """

keep_last_line_thread = threading.Thread(target=keep_last_line)
keep_last_line_thread.daemon = True
keep_last_line_thread.start()



async def get_reader():



    loop = asyncio.get_running_loop()
    rstream = asyncio.StreamReader(limit=64, loop=loop)
    protocol = asyncio.StreamReaderProtocol(rstream, loop=loop)
    await loop.connect_read_pipe(lambda: protocol, sys.stdin)
    return rstream


def flush_input():
    try:
        import msvcrt
        while msvcrt.kbhit():
            msvcrt.getch()
    except ImportError:
        import sys, termios    #for linux/unix
        termios.tcflush(sys.stdin, termios.TCIOFLUSH)


async def main():
    
    global datap, count
    reader, writer = await get_standard_streams()
    #reader = await get_reader()

    #breakpoint()

    #f = os.fdopen(sys.stdin.fileno(), 'rb', buffering=1024)

    

    while True:

        await asyncio.sleep(0.0001)

        
        #res = f.readline()
        #f.flush()

        
        
        res = await reader.readline()
        p = res.decode("utf-8").strip()
       
        # print("Python " + p + "P>>" + str(datetime.now()))

        
        #res = (await reader.readline()).decode() 

        #flush_input()

        #queue.put([count,float(res)])
        #count +=1
        #datap = float(res)
        # writer.write("\t" + "queue" + str(queue) + "\n")
        # await writer.drain()


def extract_sender_pos(world: observe_schema.World) -> tuple[float, float, float] | None:

    if world is not None:
        p = world.Sender().Position()
        return (p.X(), p.Y(), p.Z())

def update_bound(minb, maxb, b):

    if b < minb:
        minb = b
    if b > maxb:
        maxb = b

    return (minb, maxb)


async def handle_queue(): 

    global datap, count
    size = 100
    data = np.empty((100, 2)) + 0

    maxv = -np.inf
    minv = np.inf

    minx, maxx = np.inf, -np.inf
    miny, maxy = np.inf, -np.inf
    minz, maxz = np.inf, -np.inf

    

    x = np.arange(size)

    while True:
        await asyncio.sleep(0.01)
        if world is not None:

            (x,y,z) = extract_sender_pos(world)
            minx, maxx = update_bound(minx, maxx, x)
            
            miny, maxy = update_bound(miny, maxy, y)
            
            minz, maxz = update_bound(minz, maxz, z)
            ax.set_xlim(minx, maxx)
            ax.set_ylim(miny, maxy)
            ax.set_zlim(minz, maxz)

            #aprint(x,y,z)

            line.set_data_3d([x], [y], [z])


            """
            #print("Processing : " + str(queue.pop()))
            #qd = queue.get()
            qd = datap
            datap = None
            # print("qd : " + str(np.array(qd)))
            data[:-1, :] = data[1:, :]
            
            data[-1, :] = [count, qd]

            if qd > maxv:
                maxv = qd
            if qd < minv:
                minv = qd

            line.set_data(data[:, 0], data[:, 1])

            """
            
            fig.canvas.draw()            
            plt.pause(0.01)
            

async def runner():

    #await asyncio.gather(handle_queue(), main())
    await asyncio.gather(handle_queue())




#if sys.platform:
#    asyncio.set_event_loop_policy(asyncio.WindowsSelectorEventLoopPolicy())
asyncio.run(runner())
close = True