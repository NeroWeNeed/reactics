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

    [UpdateInGroup(typeof(PlayerInputProcessingSystemGroup))]
    [UpdateBefore(typeof(MainBattleSystemInputProcessor))]
    public class CameraInputProcessor : SystemBase
    {
        protected override void OnUpdate() 
        {
            var inputData = GetSingleton<InputData>();

            Entities.ForEach((ref CameraMovementData moveData, ref CameraRotationData rotData) =>
            {
                //fiddling with the camera while it's rotating is a great way to break *literally* everything. please don't.
                if (!rotData.rotating)
                {
                    moveData.panMovementDirection = inputData.pan;
                    moveData.gridMovementDirection = inputData.tileMovement;
                    moveData.zoomDirectionAndStrength = inputData.zoom;
                    rotData.rotationDirection = inputData.rotation;
                }
            }).WithName("CameraInputsJob").Run();
        }
    }

    //Processes inputs and shoves them into systems for the game to actually do stuff.
    [UpdateInGroup(typeof(PlayerInputProcessingSystemGroup))]
    public class MainBattleSystemInputProcessor : SystemBase
    {
        protected override void OnUpdate() 
        {
            //woops it's singletons
            var mapData = GetSingleton<MapData>();
            var inputData = GetSingleton<InputData>();
            var cursorData = GetSingleton<CursorData>();
            var cameraMoveData = GetSingleton<CameraMovementData>();
            
            //I've been thinking of a way to do this as read relationships (with component tags and such) instead of the way it currently works.
            //But I was taking way too long to test it out and try to implement it, so yeah. I'll probably look into that again at some point if it seems better.
            //should probably split into separate system or w/e.
            //this seems really *fucking* gross. but we'll see, I guess. now I'm starting to remember why this whole system is so goddamn ugly.
            ComponentDataFromEntity<MapBody> mapBodyData = GetComponentDataFromEntity<MapBody>(true); //WAS FALSE. CHANGE BACK IF BREAK.
            ComponentDataFromEntity<ActionMeterData> actionMeterData = GetComponentDataFromEntity<ActionMeterData>(true);
            ComponentDataFromEntity<UnitStatData> unitDataData = GetComponentDataFromEntity<UnitStatData>(true);
            Entities.ForEach((Entity entity, ref UnitManagerData unitManagerData, ref MapElement mapElement) =>
            {
                if (inputData.currentActionMap == ActionMaps.BattleControls)
                {
                if (inputData.select)
                {
                    if (!unitManagerData.commanding)
                    {
                        //Does a unit exist where we selected? If so, select it..
                        if (GetComponent<MapCollisionState>(mapElement.value).value.TryGetValue(cursorData.currentHoverPoint, out Entity mapBodyEntity))
                        {
                            UnitStatData unitData = unitDataData[mapBodyEntity];
                            unitManagerData.selectedUnit = mapBodyEntity;
                            unitManagerData.commanding = true;
                            unitManagerData.moveRange = 4;//unitData.Movement;
                        }
                        //Not sure if map bodies can (or should) be other things, but if they can it'd be relatively easy to account for that here.
                    }
                    else if (!unitManagerData.moveTileSelected && !mapData.GetTile(cursorData.currentHoverPoint).Inaccessible)
                    {
                        //Attempt to set the move tile to the cursor position.
                        Point currentPoint = mapBodyData[unitManagerData.selectedUnit].point;
                        Point movePoint = cursorData.currentHoverPoint;
                        ushort moveRange = unitManagerData.moveRange;

                        //Check if tile is in range, and set it if it is.
                        //we should make this a func or something.
                        if (movePoint.Distance(currentPoint) < moveRange)
                        {
                            //Set the move tile to this point and set moveReady to true.
                            unitManagerData.moveTile = movePoint;
                            unitManagerData.moveTileSelected = true;

                            //InputData inputData = inputData;
                            inputData.currentActionMap = ActionMaps.CommandControls;
                            //inputData = inputData;
                        }
                    }
                    else if (!mapData.GetTile(cursorData.currentHoverPoint).Inaccessible)
                    {
                        //Attempt to issue a command on this tile.
                        Point currentPoint = mapBodyData[unitManagerData.selectedUnit].point;
                        if (unitManagerData.moveTile.InRange(cursorData.currentHoverPoint, unitManagerData.effect.range) &&
                            !mapData.GetTile(cursorData.currentHoverPoint).Inaccessible)
                        {
                            if (unitManagerData.moveTile.ComparePoints(cursorData.currentHoverPoint))
                            {
                                if (unitManagerData.effect.affectsSelf)
                                    unitManagerData.effectReady = true;
                                else
                                    unitManagerData.effectReady = false;
                            }
                            else if (unitManagerData.effect.affectsTiles && unitManagerData.effect.affectsAllies)
                            {
                                //Change when differentation possible
                                unitManagerData.effectReady = true;
                            }
                            else if (unitManagerData.effect.affectsTiles && unitManagerData.effect.affectsEnemies) 
                            {
                                //Change when differentation possible
                                unitManagerData.effectReady = true;
                            }
                            else //Covers only tiles, or only units
                            {
                                /*
                                condensed version of below code, way less readable though so left it commented for now
                                bool onlyAffectsTiles = true;
                                if (!unitManagerData.effect.affectsTiles)
                                    onlyAffectsTiles = false;

                                if (GetComponent<MapCollisionState>(mapElement.value).value.TryGetValue(cursorData2.currentHoverPoint, out Entity mapBodyEntity))
                                    unitManagerData.effectReady = !onlyAffectsTiles;
                                else
                                    unitManagerData.effectReady = onlyAffectsTiles;
                                */

                                if (GetComponent<MapCollisionState>(mapElement.value).value.TryGetValue(cursorData.currentHoverPoint, out Entity mapBodyEntity))
                                {
                                    //A map body is here, and that's bad, because a map body shouldn't be here if it only affects tiles.
                                    if (unitManagerData.effect.affectsTiles && !unitManagerData.effect.affectsAllies && !unitManagerData.effect.affectsEnemies)
                                        unitManagerData.effectReady = false;
                                    else if (!unitManagerData.effect.affectsTiles)
                                    {
                                        unitManagerData.effectReady = true;
                                    }
                                }
                                else
                                {
                                    if (unitManagerData.effect.affectsTiles && !unitManagerData.effect.affectsAllies && !unitManagerData.effect.affectsEnemies)
                                        unitManagerData.effectReady = true;
                                    else if (!unitManagerData.effect.affectsTiles)
                                    {
                                        //enemy/ally differentiation here, probably.
                                        unitManagerData.effectReady = false;
                                    }
                                }
                            }
                        }
                    }
                    if (unitManagerData.effectReady)
                    {
                        EntityManager.AddComponentData<FindingPathInfo>(unitManagerData.selectedUnit, new FindingPathInfo {
                            destination = unitManagerData.moveTile,
                            speed = 5f,
                            maxElevationDifference = 1,
                            currentlyTraveled = 1,
                            maxTravel = unitManagerData.moveRange
                        });
                        //this is where we'd want to get an effect somehow and like. put it there.
                        EntityManager.AddComponentData(unitManagerData.selectedUnit, new Reactics.Komota.Projectile {
                            effect = unitManagerData.effect,
                            targetUnit = unitManagerData.targetedUnit,
                        });
                        //This is temporary. normally we'd want to keep it as we'd be writing to the server here.
                        //The server would then take the unit manager data, and if the checks went through, write these components to the entity.
                        unitManagerData = new UnitManagerData();
                    }
                }
                else if (inputData.cancel) //Cancel input in Battle Controls
                {
                    //Case A: Pressed Cancel while choosing a tile to move to. In this case, we no longer want to command the unit.
                    if (!unitManagerData.effectSelected)
                    {
                        unitManagerData = new UnitManagerData();
                    }
                    //Case B: Pressed cancel while choosing a tile to use the effect on.
                    //In this case, move the camera back to the move tile, and "bring the menu back up."
                    else
                    {
                        unitManagerData.effectSelected = false;

                        inputData.currentActionMap = ActionMaps.CommandControls;

                        //This signals to the camera to return back to the move tile we selected.
                        cameraMoveData.returnPoint = unitManagerData.moveTile;
                        cameraMoveData.returnToPoint = true;
                    }
                }
                }
                else if (inputData.currentActionMap == ActionMaps.CommandControls)
                {
                    /*
                    Most of this is fake menu stuff. so I don't care if it disappears, really. Coincidentally this is the sort of stuff you'll be fiddling with, I assume.
                    */
                    if (inputData.menuMovementDirection.y > 0)
                        inputData.menuOption += 1;
                    else if (inputData.menuMovementDirection.y < 0)
                        inputData.menuOption -= 1;

                    //if (tag.Exists(unitManagerData.selectedUnit))
                    //(code block)
                    if (inputData.select)
                    {
                        if (inputData.CurrentMenuOption() == 0)
                        {
                                //Soon the server should be handling this isntead.
                                EntityManager.AddComponentData(unitManagerData.selectedUnit, new FindingPathInfo {
                                    destination = unitManagerData.moveTile,
                                    speed = 5f,
                                    maxElevationDifference = 1,
                                    currentlyTraveled = 1,
                                    maxTravel = unitManagerData.moveRange
                                });

                                inputData.currentActionMap = ActionMaps.BattleControls;

                                //remove when server maybe? maybe not.
                                unitManagerData = new UnitManagerData();
                        }
                        else
                        {
                            //For now this means we're selecting an action.
                            //Normally we would like. do something here regarding getting the action from wherever it's stored.
                            unitManagerData.effectSelected = true;
                            unitManagerData.effect = new Reactics.Komota.Effect {
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
                        }
                    }
                    else if (inputData.cancel) //Cancel Input for Command Controls
                    {
                        inputData.currentActionMap = ActionMaps.BattleControls;
                        //Case A: No longer relevant, actually, I think. Leaving it in for now just in case, but... yeah.
                        if (!unitManagerData.moveTileSelected)
                        {
                            unitManagerData.commanding = false;
                        }
                        //Case B: Was selecting an effect to use. In this case, go back to selecting a tile to move to.
                        else
                        {
                            unitManagerData.moveTileSelected = false;
                        }
                    }
                }
            }).WithStructuralChanges().Run();

            SetSingleton<InputData>(inputData);
            SetSingleton<CameraMovementData>(cameraMoveData);
        }
    }
}