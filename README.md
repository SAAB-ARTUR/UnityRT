# UnityRT

An executable file is located in the build-folder called "Building3DEnvironment.exe". This should be runnable even without Unity installed.

Settings panel instructions:
In the top left corner of the screen a small button labelled "Settings" is found. Click on the button to find a number of options that can be used to change the RayTracer's behaviour.

![Screenshot 2023-12-14 135652](https://github.com/SAAB-ARTUR/UnityRT/assets/125650725/9d64e026-7978-4483-a988-735617f59491)

There are currently 3 main categories ["Source parameters", "World parameters", "RT Model parameters"]

### RT Model parameters
Pass

### Source paramaters
Pass 

### World parameters
Pass 

### Commands
The "Enable"-box starts something cool (Daniel) **Only implemented for Model: Bellhop
The "Callback" inputfield fills no purpose. At first it was added so that commands could be sent to the other software instead of simply using the "Enable"-box. Maybe something for the future if someone finds a smart use for it.


## Requirements
The following steps are necessary to work with the project in Unity:

  1. Project requires a Windows computer. This is due to DirectX being used. 
  2. Install Unity via https://unity.com/download
  3. Follow steps in https://docs.unity3d.com/Manual/UsingDX11GL3Features.html and select DirectX12 (may be listed as "Direct3D12").
  4. Add package Unity.Mathematics: In Unity select Window->Package Manager-> + -> Install package from git URL. Add com.unity.mathematics.
  5. Unity may complain that GPU instancing is not available. Make sure that GPU instancing is enabled by selecting a game object, inspect its material component and check the "Enable GPU Instancing" box.

Running these steps should enable you to run the program in the Unity using the play button. 

## Controls:  
-C to send rays.  
-R to toggle between 'free camera' mode and 'follow source' mode.  
-W,A,S,D,Q,E to control the free camera (when in 'free camera' mode).  
-U,H,J,K,Y,I to control the source.  
-Left shift to speed up movement.  

## Note about the RayTracing Acceleration Structure (RTAS)
Documentation: https://microsoft.github.io/DirectX-Specs/d3d/Raytracing.html

## Note about Computeshader.dispatch()
A ComputeShader in Unity has a function Dispatch() which is used to start the code written on the GPU. The function has several parameters where the first parameter states the index of the kernel which is to be executed on the GPU. (The kernels are implemented in the file MainCompute.compute which is located in Assets/Shaders/. A kernel is defined using the line "#pragma kernel kernel_name" and they are automatically given an index based on their ordering from 0 and onwards.) The remaining 3 parameters that are used indicates how many threadgroups that should be created when running the GPU kernel. These are in order: threadgroupsX, threadgroupsY and threadgroupsZ. Above the definitions of the kernel functions (defined in Assets/Shaders/MainCompute.compute) the line "[numthreads(X, Y, Z)]" is declared, these numbers define the size of each threadgroup.  
Example: If threadgroupsX = 16, threadgroupsY = 16, threadgroupsZ = 1, and [numthreads(8, 8, 1)] is written above the kernel, then 16x16x1 = 256 threadgroups are going to be created. Each threadgroup will consist of 8x8x1 = 64 threads and each thread will run its own instance of the GPU kernel. Since the kernel computes one ray this would mean that 16x16x1 x 8x8x1 = 16384 rays will be computed.  
https://learn.microsoft.com/en-us/windows/win32/api/d3d11/nf-d3d11-id3d11devicecontext-dispatch shows how the indexing of these threads work.
