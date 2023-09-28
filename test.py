import sys
import fileinput

print("Hello from Python!")

for line in fileinput.input():
	
	print("Python got:" + line)

print("Done! Okeyy byeee")
print("Python was killed by unity 🤨")