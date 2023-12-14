# UnityRT 

An executable file is located in the build-folder called "Building3DEnvironment.exe". This should be runnable even without Unity installed.

## Requirements
The following steps are necessary to work with the project in Unity:

  1. Project requires a Windows computer. This is due to DirectX being used. 
  2. Install Unity via https://unity.com/download
  3. Follow steps in https://docs.unity3d.com/Manual/UsingDX11GL3Features.html and select DirectX12 (may be listed as "Direct3D12").
  4. Add package Unity.Mathematics: In Unity select Window->Package Manager-> + -> Install package from git URL. Add com.unity.mathematics.
  5. Unity may complain that GPU instancing is not available. Make sure that GPU instancing is enabled by selecting a game object, inspect its material component and check the "Enable GPU Instancing" box.

Running these steps should enable you to run the program in the Unity using the play button. 

## Some important details about Unity:
Unity uses a left handed coordinate system where the xz-plane forms the horizontal plane. The y-axis is directed upwards, meaning that "increasing depth = decreasing y" and a positive y-coordinate indicates a position above the surface. Therefore the code in this project is written in accordance to Unity's coordinate system. 

## Settings panel instructions:
In the top left corner of the screen a small button labelled "Settings" is found. Click on the button to find a number of options that can be used to change the RayTracer's behaviour.

![Screenshot 2023-12-14 135652](https://github.com/SAAB-ARTUR/UnityRT/assets/125650725/9d64e026-7978-4483-a988-735617f59491)

### RT Model parameters
  - "Model" dropdown menu, a user has the option to choose from three different ray tracing models. These are: Bellhop, Hovem and Hovem-RTAS. The first two are 2D implementations with Eigenray computations showing transmission loss, delay, distance, surface       bounces, bottom bounces, cuastics. Hovem-RTAS is a 3D path simulator which only calculates the path of the transmitted rays, no signal processing, i.e calculating receieved signal at transmitter, is implemented for this model. 
  - Max # bottom hits: Sets how many times a ray can interact with the bottom. Exceeding this number stops the ray.  
  - Max # surface hits: Sets how many times a ray can interact with the surface. Exceeding this number stops the ray.  
  - \# integration steps: Maximum number of integration steps allowed for a ray.  
  - \# integration step size: **Only relevant for the Bellhop model**. Sets maximum step size in the Bellhop model. 

### Source paramaters
  - Ntheta: Number of rays to transmit (preferrably multiple of 8) **Note:** For models Bellhop and Hovem Ntheta rays will be sent towards each target. For model Hovem-RTAS Ntheta * Ntheta rays will be sent in the direction the source is facing.
  - Theta: Angular span.
  - Visualize rays: Toggle visualization of rays
  - Show contributing rays: Toggle ray visualization between showing all rays and only rays deemed to be contributing at the receiver by the ray tracing model. **Likely not relevant for the Hovem-RTAS model as it cannot deem rays contributing yet**
  - Send rays continously: Send rays as often as possible. **Proceed with caution**

### World parameters
  - Range: Sets the size of the surface and bottom. The water volume is square with centre in the origin. Setting range to 300 means that the surface/bottom extends from -150 to 150 in the x- and z-direction. Surface and bottom are the same size and are placed directly above/beneath eachother. **Only relevant for the Bellhop and Hovem models. When using the Hovem-RTAS model the size of the surface/bottom is set by the bathymetry-file.**
  - SSP: Sound speed profile, an example of a sound speed profile is located in "Assets/SSP/summer.txt". The first column denotes the depth and the second column denotes the speed of sound at that depth. Depths are listed as negative values. The gap between listed depths does not have to be uniform. Future work: Handle an SSP that is dependent on more variables than only depth.
  - Targets: List of targets (red sphere's). Separate targets with ":" and define a target as "{x, y, z}".
  - Bathymetry: Read a custom bathymetry .stl-file https://en.wikipedia.org/wiki/STL_(file_format). Currently only ASCII-format is supported. Examples of custom bathymetry files are located in "Assets/Bathymetry/". The bathymetry file defines the dimensions of the bottom (Range-value is overridden). **Only relevant for the Hovem-RTAS model. The other models will simply have a planar bottom.** 

### Commands
  - The "Enable"-box starts something cool (Daniel) **Only implemented for Model: Bellhop
  - The "Callback" inputfield fills no purpose. At first it was added so that commands could be sent to the other software instead of simply using the "Enable"-box. Maybe something for the future if someone finds a smart use for it.

## Controls (These are bad and someone should do something about these):  
- C to send rays.  
- R to toggle between 'free camera' mode and 'follow source' mode.  
- W,A,S,D,Q,E to control the free camera (when in 'free camera' mode).  
- U,H,J,K,Y,I to control the source.  
- Left shift to speed up movement.  
- While in 'free camera' mode holding down the right mouse button and moving the mouse will rotate the camera.
- Holding down the right mouse button and moving the mouse while also holding down the ":"-key will rotate the sender.

## Note about Computeshader.dispatch()
A ComputeShader in Unity has a function Dispatch() which is used to start the code written on the GPU. The function has several parameters where the first parameter states the index of the kernel which is to be executed on the GPU. (The kernels are implemented in the file MainCompute.compute which is located in "Assets/Shaders/". A kernel is defined using the line "#pragma kernel kernel_name" and they are automatically given an index based on their ordering from 0 and onwards.) The remaining 3 parameters that are used indicates how many threadgroups that should be created when running the GPU kernel. These are in order: threadgroupsX, threadgroupsY and threadgroupsZ. Above the definitions of the kernel functions (defined in "Assets/Shaders/MainCompute.compute") the line "[numthreads(X, Y, Z)]" is declared, these numbers define the size of each threadgroup.  
Example: If threadgroupsX = 16, threadgroupsY = 16, threadgroupsZ = 1, and [numthreads(8, 8, 1)] is written above the kernel, then 16x16x1 = 256 threadgroups are going to be created. Each threadgroup will consist of 8x8x1 = 64 threads and each thread will run its own instance of the GPU kernel. Since the kernel computes one ray this would mean that 16x16x1 x 8x8x1 = 16384 rays will be computed.  
https://learn.microsoft.com/en-us/windows/win32/api/d3d11/nf-d3d11-id3d11devicecontext-dispatch shows how the indexing of these threads work.

## Note about the RayTracing Acceleration Structure (RTAS)
Documentation: https://microsoft.github.io/DirectX-Specs/d3d/Raytracing.html
