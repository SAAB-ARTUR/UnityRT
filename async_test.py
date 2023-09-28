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
line, = ax.plot([], [])




async def main():
    count = 0
    while True:
        reader, writer = await get_standard_streams()

        res = await reader.readline()


        queue.put([count,float(res)])
        count +=1
        writer.write("\t" + "queue" + str(queue) + "\n")
        await writer.drain()



async def handle_queue(): 

    data = np.empty((0, 2))

    while True:
        await asyncio.sleep(3)
        if not queue.empty():
            #print("Processing : " + str(queue.pop()))
            qd = queue.get()
            print("qd : " + str(np.array(qd)))
            data = np.concatenate([data, [qd]], axis = 0)
            print(data)
            line.set_data(data[:, 0], data[:, 1])
            ax.set_xlim(np.min(data[:,0]), np.max(data[:, 0]))
            ax.set_ylim(np.min(data[:,1]), np.max(data[:, 1]))
            
            fig.canvas.draw()            
            plt.pause(0.01)

async def runner():

    await asyncio.gather(handle_queue(), main())

asyncio.run(runner())

