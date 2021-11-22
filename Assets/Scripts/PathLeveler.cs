using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathLeveler : MonoBehaviour
{
    [SerializeField] private Transform parent;
    void Start()
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            LevelPathSegment(parent.GetChild(i));
        }
    }
    
    private void LevelPathSegment(Transform pathSegment)
    {
        Vector3 newPosition = new Vector3();
        newPosition.x = pathSegment.position.x;
        newPosition.y = 100f;
        newPosition.z = pathSegment.position.z;

        pathSegment.position = newPosition;
        
        if (Physics.Raycast(pathSegment.position, Vector3.down, out RaycastHit hit, 200f))
        {
            newPosition.y = hit.point.y - .04f;
            pathSegment.position = newPosition;
            
            pathSegment.rotation = Quaternion.FromToRotation (Vector3.up, hit.normal) * pathSegment.rotation;
            return;
            Vector3 rot = pathSegment.eulerAngles;
            pathSegment.up = hit.normal;
            Vector3 newRot = pathSegment.eulerAngles;
            newRot.y = rot.y;
            pathSegment.eulerAngles = newRot;
        }
        
    }
}
