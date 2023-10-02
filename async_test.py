from aioconsole import get_standard_streams
import asyncio
import matplotlib.pyplot as plt
import numpy as np
import queue as Q


processing = None
queue = Q.LifoQueue(maxsize=10)



fig, ax = plt.subplots()
# plt.ion()
plt.show(block = False)
line, = ax.plot([], [], linewidth  =0, marker = ".")

count = 0
datap = None

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

    while True:

        

        reader, writer = await get_standard_streams()
        res = await reader.readline()


        #flush_input()

        #queue.put([count,float(res)])
        count +=1
        datap = float(res)
        # writer.write("\t" + "queue" + str(queue) + "\n")
        # await writer.drain()



async def handle_queue(): 

    global datap, count
    size = 100
    data = np.empty((100, 2)) + 0

    while True:
        await asyncio.sleep(0.01)
        if datap is not None:
            #print("Processing : " + str(queue.pop()))
            #qd = queue.get()
            qd = datap
            datap = None
            # print("qd : " + str(np.array(qd)))
            data[:-1, :] = data[1:, :]
            
            data[-1, :] = [count, qd]

            line.set_data(data[:, 0], data[:, 1])
            ax.set_xlim(np.min(data[:,0]), np.max(data[:, 0]))
            ax.set_ylim(np.min(data[:,1]), np.max(data[:, 1]))
            
            fig.canvas.draw()            
            plt.pause(0.01)

async def runner():

    await asyncio.gather(handle_queue(), main())

asyncio.run(runner())

