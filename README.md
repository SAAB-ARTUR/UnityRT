# UnityRT

An executable file is located in the build-folder called "Building3DEnvironment.exe". This should be runnable even without Unity installed.

Settings panel instructions:
In the top left corner of the screen a small button labelled "Settings" is found. Click on the button to find a number of options that can be used to change the RayTracer's behaviour.

![Screenshot 2023-12-14 135652](https://github.com/SAAB-ARTUR/UnityRT/assets/125650725/9d64e026-7978-4483-a988-735617f59491)

There are currently 3 main categories ["Source parameters", "World parameters", "RT Model parameters"]



Install Unity via https://unity.com/download

Project requires a Windows computer and 

För att få projektet att köra behövs en Windows-dator och detta https://docs.unity3d.com/Manual/UsingDX11GL3Features.html behövs göras i Unity-projektet.
Troligtvis saknas paketet Unity.Mathematics: i Unity Window->Package Manager-> + -> Install package from git URL. Lägg till com.unity.mathematics.
Eventuellt kan Unity klaga på att instancing inte finns, se då till att GPU instancing är enabled för alla material. I Unity, klicka på ett material och se till att "Enable GPU Instancing" är markerad i inspektorvyn.

GÄLLANDE DISPATCH (anropet som startar shader-koden):
Funktionen Dispatch anropas med ett antal parametrar där den första säger vilken kernel som ska köras. Just nu finns det bara en kernel definierad så kernel 0 kommer vara CSMain. Övriga parametrar definierar hur många threadgroups som ska startas. I shader-koden finns det sedan ovanför CSMain några siffror (typ [8,8,1]), dessa siffror definierar hur stor varje threadgroup är.  
Exempel: Om threadGroupsX = 16, threadGroupsY = 16, threadGroupsZ = 1 och det i shadern står [8,8,1] kommer 16x16x1 = 256 threadgroups starta där de är uppdelade som en 16x16x1 matris. Varje threadgroup kommer sen bestå av en 8x8x1 matris som säger att varje threadgroup innehåller 8x8x1 = 64 threads. Varje thread startar en egen kopia av CSMain. Om man tänker detta som rays, dvs att varje thread skapar sin egna ray kommer 16x16x1 x 8x8x1 = 16384 rays att skickas. Länk som visar hur indexering av threadgroups och threads funkar: https://learn.microsoft.com/en-us/windows/win32/api/d3d11/nf-d3d11-id3d11devicecontext-dispatch

Kontroller:  
Med hjälp av högerklick och tangenterna W,A,S,D,Q,E kan man styra den vänstra vyn. Med hjälp av högerklick medan tangenten punkt/kolon hålls ner och tangenterna U,H,J,K,Y,I kan man flytta den gröna sfären. Klicka C för att skicka rays. Genom att klicka R kan användaren toggla mellan en fri-kamera eller en kamera som är låst till att följa sändaren. 

Controls:  
-C to send rays.  
-R to toggle between 'free camera' mode and 'follow source' mode.  
-W,A,S,D,Q,E to control the free camera (when in 'free camera' mode).  
-U,H,J,K,Y,I to control the source.  
-Left shift to speed up movement.  
