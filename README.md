# UnityRT

Håller på att göra om raytracingen med compute shaders istället, det verkar gå iallafall. Just nu är inte raytracingen baserad på nån accelerationsstruktur eller BVH tyvärr. Det får bli senare arbete för att öka prestandan.

Att göra:
  - Lägga till max antal interaktioner för en ray. (Interaktioner räknas för tillfället som en träff mot yta eller botten, samt vändpunkter i vatten. Vändpunkter kommer dock inte finnas än eftersom det inte finns någon sound speed profile implementerad ännu.)
  - Lägga till studsar och försöka visualisera det. 

GÄLLANDE DISPATCH (anropet som startar shader-koden): 
Funktionen Dispatch anropas med ett antal parametrar där den första säger vilken kernel som ska köras. Just nu finns det bara en kernel definierad så kernel 0 kommer vara CSMain. Övriga parametrar definierar hur många threadgroups som ska startas. I shader-koden finns det sedan ovanför CSMain några siffror (typ [8,8,1]), dessa siffror definierar hur stor varje threadgroup är. 
Exempel: Om threadGroupsX = 16, threadGroupsY = 16, threadGroupsZ = 1 och det i shadern står [8,8,1] kommer 16x16x1 = 256 threadgroups starta där de är uppdelade som en 16x16x1 matris. Varje threadgroup kommer sen bestå av en 8x8x1 matris som säger att varje threadgroup innehåller 8x8x1 = 64 threads. Varje thread startar en egen kopia av CSMain. Om man tänker detta som rays, dvs att varje thread skapar sin egna ray kommer 16x16x1 x 8x8x1 = 16384 rays att skickas. Länk som visar hur indexering av threadgroups och threads funkar: https://learn.microsoft.com/en-us/windows/win32/api/d3d11/nf-d3d11-id3d11devicecontext-dispatch 
