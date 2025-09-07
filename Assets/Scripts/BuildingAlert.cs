using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingAlert : MonoBehaviour
{
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private LayerMask preyMask;
    [SerializeField] private LayerMask predatorMask;
    [SerializeField] private LayerMask rhinoMask;

    private int combinedMask;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update(){
        CheckConditions();
    }

    protected virtual void CheckConditions()
    {
        //Debug.Log("Checking for prey within detection range: " + detectionRange);

        Collider[] colliders = new Collider[10];
        combinedMask = preyMask | predatorMask | rhinoMask;
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, detectionRange, colliders,combinedMask);

        for (int i = 0; i < numColliders; i++)
        {
            Collider col = colliders[i];
            Debug.Log("Checking collider: " + col.name);

            // 判斷是否是 Prey
            Prey prey = col.GetComponent<Prey>();
            if (prey != null)
            {
                prey.AlertByBuilding(gameObject); // ✅ 偵測到 Prey
                return;
            }

            Predator predator = col.GetComponent<Predator>();
            if (predator != null)
            {
                predator.AlertByBuilding(gameObject); // ✅ 偵測到 BabyPrey
                return;
            }
            BabyPrey babyPrey = col.GetComponent<BabyPrey>();
            if (babyPrey != null)
            {
                babyPrey.AlertByBuilding(gameObject); // ✅ 偵測到 BabyPrey
                return;
            }

            RhinoController rhino = col.GetComponent<RhinoController>();
            if (rhino != null)
            {
                rhino.AlertByBuilding(gameObject); // ✅ 偵測到 BabyPrey
                return;
            }
            

        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
