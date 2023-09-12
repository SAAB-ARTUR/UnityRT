Create a cginc-file by first creating a txt-file and then change the file extension to cginc.
Add code to cginc-file and include it in shader-file by adding #include "filename.cginc".
#include will simply copy the code from the cginc-file and paste it where the #include is placed, therefore it is important to place
the #include at a logical position. Meaning above any code that calls one of the functions declared in the cginc-file.