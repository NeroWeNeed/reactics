using Reactics.Commons;
using Unity.Collections;
using UnityEngine;



public class NativeCollectionDebugger : MonoBehaviour
{
    private void Start()
    {
        NativeHeap<int> heap = new NativeHeap<int>(Allocator.Temp);
        int n;

        heap.Dispose();
    }
}