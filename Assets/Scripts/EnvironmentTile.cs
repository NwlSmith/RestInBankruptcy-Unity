using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "New EnvironmentTile", menuName = "EnvironmentTiles")]
public class EnvironmentTile : ScriptableObject
{
    public GameObject tempObjectToSpawn;
    public GameObject[] objectsRow0 = new GameObject[rowSize];
    public GameObject[] objectsRow1 = new GameObject[rowSize];
    public GameObject[] objectsRow2 = new GameObject[rowSize];
    public GameObject[] objectsRow3 = new GameObject[rowSize];
    public GameObject[] objectsRow4 = new GameObject[rowSize];

    private static int rowSize = 5;
}