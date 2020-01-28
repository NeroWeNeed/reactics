using UnityEngine;


public class SPTRTest : MonoBehaviour
{

[SerializeField]    
        private Camera cam;

        

        void Update()
        {
            
            Ray ray = cam.ScreenPointToRay(new Vector3(200, 200, 0));
            Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);
        }
    
}