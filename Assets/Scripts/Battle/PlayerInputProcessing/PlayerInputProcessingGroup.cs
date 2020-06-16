using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Reactics.Battle.Map;

namespace Reactics.Battle {
    /// <summary>
    /// Component group for processing player inputs created in the PlayerInputSystemGroup. 
    /// </summary>
    [UpdateAfter(typeof(PlayerInputSystemGroup))]
    [UpdateBefore(typeof(BattleSimulationSystemGroup))]
    [UpdateInGroup(typeof(BattleSystemGroup))]
    public class PlayerInputProcessingSystemGroup : ComponentSystemGroup {

    }

    //Processes inputs and shoves them into systems for the game to actually do stuff.
    //I left a lot of comments in here because I think they'll be helpful eventually maybe. just ignore most of them probably
    [UpdateInGroup(typeof(PlayerInputProcessingSystemGroup))]
    public class PlayerInputProcessorSystem : SystemBase
    {
        EntityCommandBufferSystem entityCommandBufferSystem;

        protected override void OnCreate()
        {
            entityCommandBufferSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate() 
        {
            var mapData = GetSingleton<MapData>();

            NativeArray<InputData> inputDataArray = new NativeArray<InputData>(1, Allocator.TempJob);
            inputDataArray[0] = GetSingleton<InputData>();

            NativeArray<UnitManagerData> unitManagerDataArray = new NativeArray<UnitManagerData>(1, Allocator.TempJob);
            unitManagerDataArray[0] = GetSingleton<UnitManagerData>();

            //Passes inputs related to camera movement.
            Entities.ForEach((ref CameraMovementData moveData, ref CameraRotationData rotData) =>
            {
                //fiddling with the camera while it's rotating is a great way to break *literally* everything. please don't.
                if (!rotData.rotating)
                {
                    moveData.panMovementDirection = inputDataArray[0].pan;
                    moveData.gridMovementDirection = inputDataArray[0].tileMovement;
                    moveData.zoomDirectionAndStrength = inputDataArray[0].zoom;
                    rotData.rotationDirection = inputDataArray[0].rotation;
                }
            }).WithName("CameraInputsJob").Run();
            
            if (inputDataArray[0].currentActionMap == ActionMaps.BattleControls)
            {
                if (inputDataArray[0].select)
                {
                    var cursorData = GetSingleton<CursorData>();

                    //If no tiles are currently being commanded then obviously we're trying to select this one, if there's a unit there.
                    if (!unitManagerDataArray[0].commanding)
                    {
                        //We are selecting a map body to begin issuing a command to. Only do so if the action meter is full.
                        Entities.ForEach((Entity entity, ref MapBody mapBody, /*ref MoveTilesTag tag,*/ in UnitData unitData, in ActionMeter actionMeter) =>
                        {
                            //tag.toggle = true;
                            if (actionMeter.Active() && mapBody.point.ComparePoints(cursorData.currentHoverPoint))
                            {
                                UnitManagerData unitManagerData = new UnitManagerData();
                                unitManagerData.selectedUnit = entity;
                                unitManagerData.commanding = true;
                                unitManagerData.moveRange = unitData.Movement();
                                unitManagerDataArray[0] = unitManagerData;
                            }
                        }).Run();
                    }
                    else if (!unitManagerDataArray[0].moveTileSelected && !mapData.GetTile(cursorData.currentHoverPoint).Inaccessible)
                    {
                        //We are selecting a tile to move to. Make sure it's in range of our movement.
                        Point currentPoint = GetComponentDataFromEntity<MapBody>(true)[unitManagerDataArray[0].selectedUnit].point;
                        Point movePoint = cursorData.currentHoverPoint;
                        ushort moveRange = unitManagerDataArray[0].moveRange;

                        //Check if tile is in range, and set it if it is.
                        //we should make this a func or something.
                        if (movePoint.Distance(currentPoint) < moveRange)
                        {
                            //Set the move tile to this point and set moveReady to true.
                            UnitManagerData unitManagerData = unitManagerDataArray[0];
                            unitManagerData.moveTile = movePoint;
                            unitManagerData.moveTileSelected = true;
                            unitManagerDataArray[0] = unitManagerData;

                            InputData inputData = inputDataArray[0];
                            inputData.currentActionMap = ActionMaps.CommandControls;
                            inputDataArray[0] = inputData;
                        }
                    }
                    else if (!mapData.GetTile(cursorData.currentHoverPoint).Inaccessible)
                    {
                        Point currentPoint = GetComponentDataFromEntity<MapBody>(true)[unitManagerDataArray[0].selectedUnit].point;
                        UnitManagerData unitManagerData = unitManagerDataArray[0];
                        if (unitManagerDataArray[0].moveTile.InRange(cursorData.currentHoverPoint, unitManagerDataArray[0].effect.range) &&
                            !mapData.GetTile(cursorData.currentHoverPoint).Inaccessible)
                        {
                            //The range is ok, so we should make sure now that the thing is targeting something it's allowed to
                            if (unitManagerDataArray[0].moveTile.ComparePoints(cursorData.currentHoverPoint))
                            {
                                //This could happen on commands that can affect self or others.
                                if (unitManagerDataArray[0].effect.affectsSelf)
                                    unitManagerData.effectReady = true;
                                else
                                    unitManagerData.effectReady = false;
                            }
                            else if (unitManagerDataArray[0].effect.affectsTiles && unitManagerDataArray[0].effect.affectsAllies)
                            {
                                //Change when we can differentiate
                                unitManagerData.effectReady = true;
                            }
                            else if (unitManagerDataArray[0].effect.affectsTiles && unitManagerDataArray[0].effect.affectsEnemies)
                            {
                                //Change when we can differentiate
                                unitManagerData.effectReady = true;
                            }
                            else if (unitManagerDataArray[0].effect.affectsTiles && !unitManagerDataArray[0].effect.affectsAllies && !unitManagerDataArray[0].effect.affectsEnemies)
                            {
                                unitManagerData.effectReady = true;
                                Entities.ForEach((Entity entity, ref UnitData unitData, ref MapBody mapBody, /*ref MoveTilesTag tag,*/ in ActionMeter actionMeter) =>
                                {
                                    //TODO: Fix this so it works on current tile of selected mapbody
                                    //In this case we're making sure there is not a mapbody here. if there is then we set it to false
                                    if (cursorData.currentHoverPoint.ComparePoints(mapBody.point))
                                    {
                                        unitManagerData.effectReady = false;
                                    }
                                }).Run();
                            }
                            else if (!unitManagerDataArray[0].effect.affectsTiles)
                            {
                                unitManagerData.effectReady = false;
                                Entities.ForEach((Entity entity, ref UnitData unitData, ref MapBody mapBody, /*ref MoveTilesTag tag,*/ in ActionMeter actionMeter) =>
                                {
                                    if (cursorData.currentHoverPoint.ComparePoints(mapBody.point))
                                    {
                                        //If affects allies and this is an ally, cool.
                                        //If affects enemies and this is an enemy, sick. (do the distinction later when we have that stuff.)
                                        //Or maybe don't. Maybe just have an (affectsUnits) field.
                                        if (unitManagerData.effect.affectsAllies || unitManagerData.effect.affectsEnemies)
                                        {
                                            unitManagerData.targetedUnit = entity;
                                            unitManagerData.effectReady = true;
                                        }
                                    }
                                }).Run();
                            }

                            unitManagerDataArray[0] = unitManagerData;
                            if (unitManagerDataArray[0].effectReady)
                            {
                                EntityManager.AddComponentData(unitManagerDataArray[0].selectedUnit, new FindingPathInfo {
                                    //maxDistance = unitManagerDataArray[0].moveRange,
                                    destination = unitManagerDataArray[0].moveTile,
                                    speed = 5f
                                });
                                //this is where we'd want to get an effect somehow and like. put it there.
                                EntityManager.AddComponentData(unitManagerDataArray[0].selectedUnit, new Projectile {
                                    effect = unitManagerDataArray[0].effect,
                                    targetUnit = unitManagerDataArray[0].targetedUnit,
                                });
                                //This is temporary. normally we'd want to keep it as we'd be writing to the server here.
                                //The server would then take the unit manager data, and if the checks went through, write these components to the entity.
                                unitManagerDataArray[0] = new UnitManagerData();
                            }
                        }
                    }
                }
            }
            else if (inputDataArray[0].currentActionMap == ActionMaps.CommandControls)
            {
                //Check if we got some movement input
                //There's no menu entity atm so we're just doing it on the input manager itself... bleh.

                //Absolutely a better way to do this (maybe just make a quick enum)
                //Right now 1 = Move input, 2 = Command input
                //NativeArray<int> selectedCommandType = new NativeArray<int>(1, Allocator.TempJob);
                /*Entities.ForEach((ref CameraMovementData moveData) => 
                {
                    moveData.CLAPTeleportPoint.x = moveData.cameraLookAtPoint.x;
                    moveData.CLAPTeleportPoint.y = moveData.cameraLookAtPoint.z;
                }).Schedule(inputDeps).Complete();*/

                //The only time we could ever press select on the menu is if we're selecting a command or if we're selecting a thing to stop moving.
                //So that's pretty nice.

                //Currently this runs without burst because of the ECB being used.
                //I read somewhere that they're trying to fix that so I'm leaving it for now.
                //Also I think this code is temporary, and it's written this way because I predicted that it might have to be a job based on teh UI.
                //If I was wrong then it's easy enough to un-job and just add the component normally.
                //Or if they never get to bursting the ecb then I can just set a flag, pass in a native array, and get the result like that, then add the component.
                //var ecb = entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent();
                //ComponentDataFromEntity<MoveTilesTag> tag = GetComponentDataFromEntity<MoveTilesTag>(false);
                Entities.ForEach((ref InputData inputDataData) =>
                {
                    InputData inputData = inputDataArray[0];
                    if (inputData.menuMovementDirection.y > 0)
                        inputData.menuOption += 1;
                    else if (inputData.menuMovementDirection.y < 0)
                        inputData.menuOption -= 1;

                    UnitManagerData unitManagerData = unitManagerDataArray[0];

                    /*if (tag.Exists(unitManagerData.selectedUnit))
                    {
                        //this is a neat feature but delete it...
                        //actually what if this is perfect and makes it so moving other blocks works... hmm....
                        //keep it for now.
                        MoveTilesTag thing = new MoveTilesTag{toggle = true};
                        tag[unitManagerData.selectedUnit] = thing;
                    }*/
                    if (inputData.select)
                    {
                        if (inputData.CurrentMenuOption() == 0)
                        {
                                /*
                                Soon the server should be handling this isntead.
                                The way I see it that goes like this:
                                here we write the server component, and keep commanding set to true, and return to battle controls...
                                The server checks on stuff, and if it's all good, it sets commanding to false, and executes the action (by adding the mbt and command components.)
                                Now that that's all said and done, we return to neutral. 
                                If it fails, then it just *doesn't* add those components and does nothing, so here it's as if we never pressed the button. Seems simple enough..?

                                Or maybe it's more like...
                                We add all three components HERE
                                And if the server decides that we shouldn't have added those two components it removes them and then rolls back to when we never did that as if it iddn't occur.
                                Probably more likely but we'll have to read about rollback some more...
                                */
                                EntityManager.AddComponentData(unitManagerDataArray[0].selectedUnit, new FindingPathInfo {
                                    //maxDistance = unitManagerDataArray[0].moveRange,
                                    destination = unitManagerDataArray[0].moveTile,
                                    speed = 5f
                                });

                                inputData.currentActionMap = ActionMaps.BattleControls;

                                //This is incredibly temporary, this guy right here. He won't be here.
                                unitManagerData = new UnitManagerData();
                        }
                        else
                        {
                            //For now this means we're selecting an action.
                            //Normally we would like. do something here regarding getting the action from wherever it's stored.
                            unitManagerData.effectSelected = true;
                            unitManagerData.effect = new Effect {
                                physicalDmg = 105,
                                magicDmg = 15,
                                trueDOT = 1,
                                harmful = false,
                                totalFrames = 301,
                                secondsPerTick = 2,
                                range = 7,
                                aoeRange = 3,
                                affectsTiles = false,
                                affectsEnemies = true,
                                affectsSelf = false,
                                affectsAllies = false,
                                cost = 50,
                                homing = false,
                                speed = 1,
                                arc = false,
                                pierce = true,
                                maxPierce = 1,
                                maxArcValue = 3,
                                movementModifier = -4
                            };
                            inputData.currentActionMap = ActionMaps.BattleControls;
                            //selectedCommandType[0] = 2;
                        }
                    }
                    unitManagerDataArray[0] = unitManagerData;
                    inputDataArray[0] = inputData;
                }).WithStructuralChanges().Run();
            }

            if (inputDataArray[0].cancel && unitManagerDataArray[0].commanding)
            {
                UnitManagerData unitManagerData = unitManagerDataArray[0];
                InputData inputData = inputDataArray[0];

                if (inputData.currentActionMap == ActionMaps.CommandControls)
                {
                    inputData.currentActionMap = ActionMaps.BattleControls;
                    if (!unitManagerData.moveTileSelected)
                    {
                        unitManagerData.commanding = false;
                    }
                    else
                    {
                        unitManagerData.moveTileSelected = false;
                    }
                }
                else //We were in battle controls
                {
                    if (!unitManagerData.effectSelected)
                    {
                        unitManagerData = new UnitManagerData();
                    }
                    else
                    {
                        unitManagerData.effectSelected = false;
                        inputData.currentActionMap = ActionMaps.CommandControls;
                        Entities.ForEach((ref CameraMovementData moveData) => 
                        {
                            if (unitManagerDataArray[0].moveTileSelected)
                                moveData.returnPoint = unitManagerDataArray[0].moveTile;
                                moveData.returnToPoint = true;
                        }).Run();
                    }
                }

                inputDataArray[0] = inputData;
                unitManagerDataArray[0] = unitManagerData;
            }

            SetSingleton<InputData>(inputDataArray[0]);
            SetSingleton<UnitManagerData>(unitManagerDataArray[0]);
            inputDataArray.Dispose();
            unitManagerDataArray.Dispose();
        }
    }
}