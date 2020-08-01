using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Reactics.Battle;
using Reactics.Battle.Map;
using Reactics.Commons;
using Reactics.Tests;
using Unity.Collections;
using Unity.Entities;
using UnityEditor;
//using Unity.Entities.Tests;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EffectSystemTestFixture : ECSTestFixture {

    public const string EFFECT_PATH = "Assets/ResourceData/Effects/Effect 1.asset";

    public const string MAP_PATH = "Assets/ResourceData/Maps/DebugMap.asset";
    [Test]
    public void SampleTest() {
        var resourceSystem = this.World.GetResourceSystem();
        var effectResource = resourceSystem.Load<EffectAsset>(EFFECT_PATH, 10000);
        var targetEntity = EntityManager.CreateEntity();
        var mapResource = AssetDatabase.LoadAssetAtPath<MapAsset>(MAP_PATH);
        var mapEntity = EntityManager.CreateEntity();
        EntityManager.AddComponentData(mapEntity, new MapData
        {
            value = mapResource.CreateBlob()
        });

        var body = new MapBody
        {
            point = new Point(3, 4),
            direction = MapBodyDirection.South
        };
        EntityManager.AddComponentData(targetEntity, body);
        EntityManager.AddComponentData(targetEntity, new MapElement { value = mapEntity });
        var sourceEntity = EntityManager.CreateEntity();
        EntityManager.AddComponentData(sourceEntity, new MapBody
        {
            point = new Point(3, 3),
            direction = MapBodyDirection.North
        });
        EntityManager.AddComponentData(sourceEntity, new MapElement { value = mapEntity });
        var doEffectEntity = EntityManager.CreateEntity();
        EntityManager.AddComponentData(doEffectEntity, new Effect { value = effectResource });
        EntityManager.AddComponentData(doEffectEntity, new EffectSource { value = sourceEntity });
        EntityManager.AddComponentData(doEffectEntity, new EffectTarget<MapBodyTarget> { value = new MapBodyTarget { entity = targetEntity, mapBody = body } });
        EntityManager.AddComponentData(doEffectEntity, new MapElement { value = mapEntity });
        World.Update();

        foreach (var item in EntityManager.GetAllEntities(Allocator.Temp)) {
            Debug.Log(item);
            foreach (var type in EntityManager.GetComponentTypes(item)) {
                Debug.Log($" - {type}");
            }
        }
    }

}