from aioconsole import get_standard_streams
import asyncio
import matplotlib.pyplot as plt
import numpy as np
import queue as Q
import sys
import os
from datetime import datetime


processing = None
queue = Q.LifoQueue(maxsize=10)



fig, ax = plt.subplots()
# plt.ion()
plt.show(block = False)
line, = ax.plot([], [], linewidth  =0, marker = ".")

count = 0
datap = None

import sys
import threading

last_line = ''
new_line_event = threading.Event()


def keep_last_line():
    global last_line, new_line_event, datap, count
    for line in sys.stdin:
        last_line = line
        datap = float(line)
        count += 1
        # new_line_event.set()


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




async def handle_queue(): 

    global datap, count
    size = 100
    data = np.empty((100, 2)) + 0

    maxv = -np.inf
    minv = np.inf

    

    x = np.arange(size)

    while True:
        await asyncio.sleep(0.001)
        if datap is not None:
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
            ax.set_xlim(count - size, count)
            ax.set_ylim(minv, maxv)

            
            fig.canvas.draw()            
            plt.pause(0.001)

async def runner():

    #await asyncio.gather(handle_queue(), main())
    await asyncio.gather(handle_queue())




#if sys.platform:
#    asyncio.set_event_loop_policy(asyncio.WindowsSelectorEventLoopPolicy())
asyncio.run(runner())

