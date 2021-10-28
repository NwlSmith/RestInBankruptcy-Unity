using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;


public class ConstructEnvironment : MonoBehaviour
{
    private Vector2 _dimensions = new Vector2(20, 20);

    private const float tileSize = 1.95f;

    private GameObject _parentGameObject;

    public EnvironmentTile[] _middleTiles;
    public EnvironmentTile[] _xEdgeTiles;
    public EnvironmentTile[] _yEdgeTiles;
    public EnvironmentTile[] _cornerTiles;

    public GameObject testGO;

    private void Start()
    {
        Run();
    }

    private void Run()
    {
        Debug.Log("Spawning shit");

        _parentGameObject = Instantiate(new GameObject("Environment Tile Holder"), Vector3.zero, Quaternion.identity);

        for (int x = 0; x < _dimensions.x; x++)
        {
            for (int y = 0; y < _dimensions.y; y++)
            {
                Vector2 location = new Vector2(x, y);
                if ((x == 0 || x == _dimensions.x - 1) && (y == 0 || y == _dimensions.y - 1))
                {
                    SpawnRandomTile(_cornerTiles, location);
                }
                else if (x == 0 || x == _dimensions.x - 1)
                {
                    SpawnRandomTile(_xEdgeTiles, location);
                }
                else if (y == 0 || y == _dimensions.y - 1)
                {
                    SpawnRandomTile(_yEdgeTiles, location);
                }
                else
                {
                    SpawnRandomTile(_middleTiles, location);
                }
            }
        }
    }

    private void SpawnRandomTile(EnvironmentTile[] tiles, Vector2 coordinates)
    {
        SpawnTile(tiles[Random.Range(0, tiles.Length)], coordinates);
    }

    private void SpawnTile(EnvironmentTile tileToSpawn, Vector2 coordinates)
    {
        GameObject newGO = Instantiate(tileToSpawn.tempObjectToSpawn);
        newGO.transform.parent = _parentGameObject.transform;
        Vector3 newPosition = new Vector3();
        newPosition.x = coordinates.x * tileSize;
        newPosition.y = 100f;
        newPosition.z = coordinates.y * tileSize;

        if (Physics.Raycast(newPosition, Vector3.down, out RaycastHit hit, 200f))
        {
            newPosition.y = hit.point.y;
            newGO.transform.position = newPosition;
        }
        else
        {
            Debug.LogError($"Positioning {newGO.name} at {newPosition.ToString()} failed, disabling object");
            newGO.SetActive(false);
        }
    }

    private void SpawnTestObj(GameObject toSpawn, Vector2 coordinates)
    {
        GameObject newGO = Instantiate(toSpawn, _parentGameObject.transform);
        Vector3 newPosition = new Vector3(coordinates.x * tileSize, 100f, coordinates.y * tileSize);

        if (Physics.Raycast(newPosition, Vector3.down, out RaycastHit hit, 200f))
        {
            newPosition.y = hit.point.y;
            newGO.transform.position = newPosition;
        }
        else
        {
            Debug.LogError($"Positioning {newGO.name} failed, disabling object");
            newGO.SetActive(false);
        }
    }

}
