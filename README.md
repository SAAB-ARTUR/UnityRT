# UnityRT

Inspiration: https://github.com/INedelcu/RayTracingMeshInstancingSimple/tree/main

För att få projektet att köra behövs en Windows-dator och detta https://docs.unity3d.com/Manual/UsingDX11GL3Features.html behövs göras i Unity-projektet.
Troligtvis saknas paketet Unity.Mathematics: i Unity Window->Package Manager-> + -> Install package from git URL. Lägg till com.unity.mathematics.
Eventuellt kan Unity klaga på att instancing inte finns, se då till att GPU instancing är enabled för alla material. I Unity, klicka på ett material och se till att "Enable GPU Instancing" är markerad i inspektorvyn.

GÄLLANDE DISPATCH (anropet som startar shader-koden):
Funktionen Dispatch anropas med ett antal parametrar där den första säger vilken kernel som ska köras. Just nu finns det bara en kernel definierad så kernel 0 kommer vara CSMain. Övriga parametrar definierar hur många threadgroups som ska startas. I shader-koden finns det sedan ovanför CSMain några siffror (typ [8,8,1]), dessa siffror definierar hur stor varje threadgroup är.  
Exempel: Om threadGroupsX = 16, threadGroupsY = 16, threadGroupsZ = 1 och det i shadern står [8,8,1] kommer 16x16x1 = 256 threadgroups starta där de är uppdelade som en 16x16x1 matris. Varje threadgroup kommer sen bestå av en 8x8x1 matris som säger att varje threadgroup innehåller 8x8x1 = 64 threads. Varje thread startar en egen kopia av CSMain. Om man tänker detta som rays, dvs att varje thread skapar sin egna ray kommer 16x16x1 x 8x8x1 = 16384 rays att skickas. Länk som visar hur indexering av threadgroups och threads funkar: https://learn.microsoft.com/en-us/windows/win32/api/d3d11/nf-d3d11-id3d11devicecontext-dispatch

Kontroller:  
Med hjälp av högerklick och tangenterna W,A,S,D,Q,E kan man styra den vänstra vyn. Med hjälp av högerklick medan tangenten punkt/kolon hålls ner och tangenterna U,H,J,K,Y,I kan man flytta den gröna sfären. Klicka C för att skicka rays, eller klicka i rutan i maincamera-komponenten som gör att rays skickas vid varje Update()-anrop. Genom att klicka R kan användaren toggla mellan en fri-kamera eller en kamera som är låst till att följa sändaren. 

18 oktober: Bellhop är implementerat för raytracing av strålar mot varje mottagare i scenen.

20 september: En settings-panel har lagts till där användaren har smidig åtkomst till inställningar. Det är möjligt att läsa in en fil som definierar hastighetsprofilen för vattnet. Hastighetsprofilen definierar också antalet vattenplan samt vattendjupet. Det är dags att börja med shader-kod.

14 september: Rays kan nu träffa ett mål (definierad av en röd sfär). Sändaren kan inte placeras utanför miljön, användaren kan toggla mellan en fri-kamera och att följa sändaren. Världen är inte längre sändar-centrisk utan världen är låst och sändaren kan placeras inuti världen. Yt/botten-plan är dubbla för att de ska synas från båda håll i den normala vyn. Allmänt robustare kod. 

13 september: Compute-shadern bygger nu på inlineraytracing med en accelerationsstruktur. Unity-miljön är omgjord för att definiera källan som centrum i världen. 

12 september: Rays skickas nu från källan och användaren kan sätta två vinklar som avgör vilket spann som rays kommer skickas över. Användaren kan också ange antal rays per vinkel.

8 september: Studsar mot botten och yta har lagts till, just nu inverteras endast y-riktningen så studsarna är väldigt primitiva. Ett maxtal för antal interaktioner har lagts och det går att visualisera stusarna. Vändpunkter i vattnet finns ej.

7 september: Koden har uppdaterats till att vara baserad på compute shaders istället (.compute-filer). C#-koden har anpassats lite för detta men reultatet av koden är densamma som tidigare.

1 september: Koden som finns just nu är körbar (förutsatt att allt som behövs finns i unity) och just nu är koden baserad på ett C#-skript (Main.cs) och flera GPU-shaders (.raytrace och .shader filer). När man klickar på play i Unity-fönstret kommer två vyer visas, den till vänster kommer visa en scen som innehåller några plan och sfärer. Planen ska representera en enkel havsyta och en enkel havsbotten. Mellan planen finns också ett valfritt antal osynliga plan som ska representera indelningen av vattenvolymen i lager. Den ena sfären (den gröna) representerar just nu en signalkälla och den andra sfären (den röda) gör ingenting just nu. Den högra vyn visar resultatet från raytracingen (som utförs i shaders). Rays skickas från den gröna sfären i kamerans riktning (alltså den högra vyns riktning) och interagerar med objekten i scenen. De olika planen har olika material kopplade till sig som i sin tur har olika shaders kopplade till sig. Baserat på vad en ray träffar kommer olika shaders anropas. Just nu sätter dessa shaders endast en färg på objektet (så ytan, botten och vattenplanen kommer få olika färger satta på sig när en ray träffar dom). Just nu kan rays endast träffa ett objekt, sen är dom färdiga.
