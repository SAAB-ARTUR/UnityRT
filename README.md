# UnityRT

Inspiration: https://github.com/INedelcu/RayTracingMeshInstancingSimple/tree/main

För att få projektet att köra behövs en Windows-dator och detta https://docs.unity3d.com/Manual/UsingDX11GL3Features.html behövs göras i Unity-projektet.  
Troligtvis saknas paketet Unity.Mathematics: i Unity Window->Package Manager-> + -> Install package from git URL. Lägg till com.unity.mathematics.  
Eventuellt kan Unity klaga på att instancing inte finns, se då till att GPU instancing är enabled för alla material. I Unity, klicka på ett material och se till att "Enable GPU Instancing" är markerad i inspektorvyn.  

1 september: Koden som finns just nu är körbar (förutsatt att allt som behövs finns i unity) och just nu är koden baserad på ett C#-skript (Main.cs) och flera GPU-shaders (.raytrace och .shader filer). När man klickar på play i Unity-fönstret kommer två vyer visas, den till vänster kommer visa en scen som innehåller några plan och sfärer. Planen ska representera en enkel havsyta och en enkel havsbotten. Mellan planen finns också ett valfritt antal osynliga plan som ska representera indelningen av vattenvolymen i lager. Den ena sfären (den gröna) representerar just nu en signalkälla och den andra sfären (den röda) gör ingenting just nu. Den högra vyn visar resultatet från raytracingen (som utförs i shaders). Rays skickas från den gröna sfären i kamerans riktning (alltså den högra vyns riktning) och interagerar med objekten i scenen. De olika planen har olika material kopplade till sig som i sin tur har olika shaders kopplade till sig. Baserat på vad en ray träffar kommer olika shaders anropas. Just nu sätter dessa shaders endast en färg på objektet (så ytan, botten och vattenplanen kommer få olika färger satta på sig när en ray träffar dom). Just nu kan rays endast träffa ett objekt, sen är dom färdiga.  
Med hjälp av högerklick och tangenterna W,A,S,D,Q,E kan man styra den vänstra vyn. Med hjälp av högerklick medan tangenten punkt/kolon hålls ner och tangenterna U,H,J,K,Y,I kan man flytta den gröna sfären. 

Just nu kan rays som sagt inte studsa, det är väl nästa steg. Dock har ett eventuellt problem upptäckts som undersöks just nu. Det verkar som att raytracing-shaders inte är byggda för att enkelt kunna skicka data mellan GPU och CPU utan de är gjorda för att vara en del av en rendering pipeline (inte helt säker på vad det innebär men det verkar ha med visuella saker att göra, och det visuella är ju inte vårt huvudsakliga intresse). Istället bör compute shaders användas istället eftersom de är gjorda för GPGPU-programmering (https://discussions.unity.com/t/send-float-value-from-shader-to-script/148108), men där är problemet att det just nu inte är lika klart hur raytracing ska genomföras. Raytracing är ganska klarlagd som det är gjort just nu, men som sagt är det frågetecken hur vi kan skicka våra resultat från shader till C#-skript. I en compute shader ska det vara enklare att skicka data men raytracing-implementationen är inte lika självklar. Så just behövs det undersökas många saker. 

Håller på att göra om raytracingen med compute shaders istället, det verkar gå iallafall. Just nu är inte raytracingen baserad på nån accelerationsstruktur eller BVH tyvärr. Det får bli senare arbete för att öka prestandan.

Att göra: 
- Visualisera rays vägar genom miljön med studsar.
- Lista ut hur buffern (som gör kommunikation mellan GPU och CPU möjlig) ska struktureras och indexeras.

