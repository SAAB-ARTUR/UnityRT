import flatbuffers 
import Assets.Scripts.api.ObserveSchema_generated as schema
import threading
import sys
import asyncio


buf = open("SAVE_FILENAME.whatever", "rb").read()
world = schema.World.GetRootAs(buf)

print(world.Sender())
