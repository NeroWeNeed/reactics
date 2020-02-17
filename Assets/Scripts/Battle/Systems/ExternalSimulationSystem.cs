using System;
using System.Threading;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Jobs;
using UnityEngine;

namespace Reactics.Battle
{

    public class ExternalSimulationSystem : ComponentSystem
    {
        /// <summary>
        /// Rate to update the simulation world
        /// </summary>
        public const int SIMULATION_RATE = 30;

        /// <summary>
        /// Maximum number of frames to keep in memory.
        /// </summary>
        private const int MAX_HISTORY = 8;

        /// <summary>
        /// Represents the Simulation World.
        /// </summary>
        /// <value></value>
        public World SimulationWorld { get; private set; }
        /// <summary>
        /// Current Simulation Frame
        /// </summary>
        /// <value></value>
        public ulong Frame { get; private set; }

        private NativeQueue<SimulationFrame> history;

        private Timer timer;

        private int simulationFramesQueued = 0;
        protected override void OnCreate()
        {
            Frame = 0;
            SimulationWorld = new World("Simulation World");
            history = new NativeQueue<SimulationFrame>(Allocator.Persistent);
            timer = new Timer(SimulateFrame, null, 1000/SIMULATION_RATE, 1000/SIMULATION_RATE);
        }




        protected override void OnDestroy()
        {

            SimulationWorld.Dispose();
            history.Dispose();
            timer.Dispose();

        }

        protected override void OnUpdate()
        {

            SimulationWorld.Update();

            MemoryBinaryWriter binaryWriter = new MemoryBinaryWriter();
            SerializeUtility.SerializeWorld(SimulationWorld.EntityManager, binaryWriter);

            unsafe
            {
                SimulationFrame frame = new SimulationFrame
                {
                    frame = Frame,
                    data = UnsafeUtility.Malloc(binaryWriter.Length, 4, Allocator.Persistent),
                    length = binaryWriter.Length
                };

                UnsafeUtility.MemCpy(frame.data, binaryWriter.Data, binaryWriter.Length);
                history.Enqueue(frame);
            }
                
            binaryWriter.Dispose();

            Frame++;
            if (history.Count > MAX_HISTORY)
                history.Dequeue().Dispose();
            Debug.Log("Simulated Frame " + Frame);

            Enabled = --simulationFramesQueued > 0;
        }

        private void SimulateFrame(object data)
        {

            Enabled = ++simulationFramesQueued > 0;
        }
        public unsafe struct SimulationFrame : IDisposable
        {
            public ulong frame;
            public void* data;

            public long length;

            public void Dispose()
            {
                unsafe
                {
                    UnsafeUtility.MemClear(data, length);
                }
            }
        }

    }

}