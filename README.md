# UnityRT

Inspiration: https://github.com/INedelcu/RayTracingMeshInstancingSimple/tree/main

För att få projektet att köra behövs en Windows-dator och detta https://docs.unity3d.com/Manual/UsingDX11GL3Features.html behövs göras i Unity-projektet.  
Troligtvis saknas paketet Unity.Mathematics: i Unity Window->Package Manager-> + -> Install package from git URL. Lägg till com.unity.mathematics.  
Eventuellt kan Unity klaga på att instancing inte finns, se då till att GPU instancing är enabled för alla material. I Unity, klicka på ett material och se till att "Enable GPU Instancing" är markerad i inspektorvyn.  

8 september: Koden har uppdaterats till att vara baserad på compute shaders istället (.compute-filer). C#-koden har anpassats lite för detta men reultatet av koden är densamma som tidigare.

1 september: Koden som finns just nu är körbar (förutsatt att allt som behövs finns i unity) och just nu är koden baserad på ett C#-skript (Main.cs) och flera GPU-shaders (.raytrace och .shader filer). När man klickar på play i Unity-fönstret kommer två vyer visas, den till vänster kommer visa en scen som innehåller några plan och sfärer. Planen ska representera en enkel havsyta och en enkel havsbotten. Mellan planen finns också ett valfritt antal osynliga plan som ska representera indelningen av vattenvolymen i lager. Den ena sfären (den gröna) representerar just nu en signalkälla och den andra sfären (den röda) gör ingenting just nu. Den högra vyn visar resultatet från raytracingen (som utförs i shaders). Rays skickas från den gröna sfären i kamerans riktning (alltså den högra vyns riktning) och interagerar med objekten i scenen. De olika planen har olika material kopplade till sig som i sin tur har olika shaders kopplade till sig. Baserat på vad en ray träffar kommer olika shaders anropas. Just nu sätter dessa shaders endast en färg på objektet (så ytan, botten och vattenplanen kommer få olika färger satta på sig när en ray träffar dom). Just nu kan rays endast träffa ett objekt, sen är dom färdiga.  
Med hjälp av högerklick och tangenterna W,A,S,D,Q,E kan man styra den vänstra vyn. Med hjälp av högerklick medan tangenten punkt/kolon hålls ner och tangenterna U,H,J,K,Y,I kan man flytta den gröna sfären. 
