using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

/*
pick up inputs in your component systems onupdate function and then route the input from there into jobs

we have something called manual update mode that you can switch the system into which decouples 
the input system from the player loop and you can just run input updates under your own control


but this isn't a class. so we can't really do it this way. in addition, we aren't sending input data from here...
the input data is set up in the new system. so we have to figure out how to pass that to the other component system, somehow. but how? 
there's got to be a way to do this.

ok so what we thought of was... having moving as like a data thing in our data component thing...
we probably need to use manual update.

nero:
point it to a function that pushes the data to the dots system

leave input system in manual mode, haev it update a static struct, manually poll that once a frame from ecs

map select unit button to function, turn that to data,push to dots system.

basically the same as copenhagen guy, and basically the same idea i had. but like. how does dots even work. how do we get the jobs to run. guess we have to watch those videos.
though we should do that after we've figured out the cursor meme. probably. in the interest of having things being done.
*/

public struct PlayerInputData : IComponentData 
{
    
}