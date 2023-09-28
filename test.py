import sys
import fileinput
import matplotlib.pyplot as plt 
import numpy as np
import asyncio

fig, ax = plt.subplots()
plt.ion()
plt.show(block = False)




print("Hello from Python!")


linenum = 0

datapoints = []

plot = None

for line in fileinput.input():
	
	try:

		datapoints.append([linenum, float(line)])

		dp = np.array(datapoints)

		if plot is None:
			plot, = ax.plot(dp[:, 0], dp[:, 1])
		else:
			plot.set_data(dp[:, 0], dp[:, 1])



		ax.set_xlim([0, linenum])
		ax.set_ylim([np.min(dp[:, 1]), np.max(dp[:, 1])])
		fig.canvas.draw()
		plt.pause(0.0001)

		linenum += 1
	except:
		pass






print("Done! Okeyy byeee")
print("Python was killed by unity 🤨")