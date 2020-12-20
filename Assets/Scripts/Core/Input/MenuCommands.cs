using System;
using NeroWeNeed.UIDots;
using Unity.Burst;
using Unity.Entities;

[assembly:UICallback]
namespace Reactics.Core.Input {

    [BurstCompile]
    public unsafe static class MenuCommands {
        [BurstCompile]
        public static void SampleAction(Entity* cursor,Entity* element, void* ecb) {

        }
        [BurstCompile]
        public static void SampleAction2(Entity* cursor, Entity* element, void* ecb) {

        }
        [BurstCompile]
        public static void SampleAction3(Entity* cursor, Entity* element, void* ecb) {

        }
        [BurstCompile]
        public static void SampleAction4(Entity* cursor, Entity* element, void* ecb) {

        }
        [BurstCompile]
        public static void SampleAction5(Entity* cursor, Entity* element, void* ecb) {

        }
    }
}