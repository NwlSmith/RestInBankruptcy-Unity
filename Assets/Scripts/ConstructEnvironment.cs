using UnityEngine;
using Random = UnityEngine.Random;


public class ConstructEnvironment : MonoBehaviour
{
    private Vector2 _dimensions = new Vector2(20, 20);

    private const float tileSize = 4;

    private GameObject _parentGameObject;

    public GameObject[] _gravestoneTiles;
    public GameObject[] _treeTiles;
    public GameObject[] _xEdgeTiles;
    public GameObject[] _yEdgeTiles;
    public GameObject _cornerTile;
    public GameObject _doorTile;
    public GameObject[] _xWalkwayTiles;
    public GameObject[] _yWalkwayTiles;
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
                if (x == 0 && y == 0)
                {
                    SpawnTile(_cornerTile, location);
                }
                else if (x == _dimensions.x - 1 && y == 0)
                {
                    SpawnRotAdjustedTile(_cornerTile, location, 90f);
                }
                else if (x == 0 && y == _dimensions.y - 1)
                {
                    SpawnRotAdjustedTile(_cornerTile, location, 180f);
                }
                else if (x == _dimensions.x - 1 && y == _dimensions.y - 1)
                {
                    SpawnRotAdjustedTile(_cornerTile, location, 360f);
                }
                else if (x == 0)
                {
                    SpawnRandomTile(_xEdgeTiles, location);
                }
                else if (x == _dimensions.x - 1)
                {
                    SpawnRandomRotAdjustedTile(_xEdgeTiles, location, 180);
                }
                else if (y == 0)
                {
                    SpawnRandomTile(_yEdgeTiles, location);
                }
                else if (y == _dimensions.y - 1)
                {
                    SpawnRandomRotAdjustedTile(_yEdgeTiles, location, 180);
                }
                // X walkway tiles
                else if (x == 1 || x == _dimensions.x - 2 || x == _dimensions.x / 2)
                {
                    SpawnRandomTile(_xWalkwayTiles, location);
                }
                // Y walkway tiles
                else if (y == 1 || y == _dimensions.y - 2 || y == _dimensions.y / 2)
                {
                    SpawnRandomTile(_yWalkwayTiles, location);
                }
                else
                {
                    if (Random.Range(0, 5) == 0)
                    {
                        SpawnRandomScaleAndRotAdjustedTile(_treeTiles, location, Random.Range(0f, 360f), Random.Range(1f, 2f), Random.Range(1f, 2f));
                    }
                    else
                    {
                        SpawnRandomTile(_gravestoneTiles, location);
                    }
                }
            }
        }
    }

    private void SpawnRandomTile(GameObject[] tiles, Vector2 coordinates)
    {
        SpawnTile(tiles[Random.Range(0, tiles.Length)], coordinates);
    }
    
    private void SpawnRandomRotAdjustedTile(GameObject[] tiles, Vector2 coordinates, float rotationAdjustment = 0f)
    {
        SpawnRotAdjustedTile(tiles[Random.Range(0, tiles.Length)], coordinates, rotationAdjustment);
    }
    
    private void SpawnRandomScaleAndRotAdjustedTile(GameObject[] tiles, Vector2 coordinates, float rotationAdjustment = 0f, float scaleHorAdjustment = 1f, float scaleVerAdjustment = 1f)
    {
        SpawnScaleAndRotAdjustedTile(tiles[Random.Range(0, tiles.Length)], coordinates, rotationAdjustment, scaleHorAdjustment, scaleVerAdjustment);
    }
    
    private void SpawnTile(GameObject tileToSpawn, Vector2 coordinates)
    {
        GameObject newGO = Instantiate(tileToSpawn);
        newGO.transform.parent = _parentGameObject.transform;
        Vector3 newPosition = new Vector3();
        newPosition.x = coordinates.x * tileSize;
        newPosition.y = 100f;
        newPosition.z = coordinates.y * tileSize;

        newGO.transform.position = newPosition;

        for (int i = 0; i < newGO.transform.childCount; i++)
        {
            GameObject childGO = newGO.transform.GetChild(i).gameObject;
            Vector3 newChildPosition = childGO.transform.position;
            
            if (Physics.Raycast(newChildPosition, Vector3.down, out RaycastHit hit, 200f))
            {
                newChildPosition.y = hit.point.y;
                childGO.transform.position = newChildPosition;
                childGO.transform.rotation = Quaternion.FromToRotation (Vector3.up, hit.normal) * childGO.transform.rotation;
            }
            else
            {
                Debug.LogError($"Positioning child {childGO.name} of {newGO.name} at {newChildPosition.ToString()} failed, disabling object");
                newGO.SetActive(false);
            }
        }
    }
    
    private void SpawnRotAdjustedTile(GameObject tileToSpawn, Vector2 coordinates, float rotationAdjustment = 0f)
    {
        GameObject newGO = Instantiate(tileToSpawn);
        newGO.transform.parent = _parentGameObject.transform;
        Vector3 newPosition = new Vector3();
        newPosition.x = coordinates.x * tileSize;
        newPosition.y = 100f;
        newPosition.z = coordinates.y * tileSize;

        newGO.transform.position = newPosition;
        newGO.transform.eulerAngles = Vector3.up * rotationAdjustment;

        for (int i = 0; i < newGO.transform.childCount; i++)
        {
            GameObject childGO = newGO.transform.GetChild(i).gameObject;
            Vector3 newChildPosition = childGO.transform.position;
            
            if (Physics.Raycast(newChildPosition, Vector3.down, out RaycastHit hit, 200f))
            {
                newChildPosition.y = hit.point.y;
                childGO.transform.position = newChildPosition;
                
                childGO.transform.rotation = Quaternion.FromToRotation (Vector3.up, hit.normal) * childGO.transform.rotation;
            }
            else
            {
                Debug.LogError($"Positioning child {childGO.name} of {newGO.name} at {newChildPosition.ToString()} failed, disabling object");
                newGO.SetActive(false);
            }
        }
    }
    
    private void SpawnScaleAndRotAdjustedTile(GameObject tileToSpawn, Vector2 coordinates, float rotationAdjustment = 0f, float scaleHorAdjustment = 1f, float scaleVerAdjustment = 1f)
    {
        GameObject newGO = Instantiate(tileToSpawn);
        newGO.transform.parent = _parentGameObject.transform;
        Vector3 newPosition = new Vector3();
        newPosition.x = coordinates.x * tileSize;
        newPosition.y = 100f;
        newPosition.z = coordinates.y * tileSize;

        newGO.transform.position = newPosition;

        for (int i = 0; i < newGO.transform.childCount; i++)
        {
            GameObject childGO = newGO.transform.GetChild(i).gameObject;
            Vector3 newChildPosition = childGO.transform.position;
            
            if (Physics.Raycast(newChildPosition, Vector3.down, out RaycastHit hit, 200f))
            {
                newChildPosition.y = hit.point.y;
                childGO.transform.position = newChildPosition;
                childGO.transform.localScale = new Vector3(scaleHorAdjustment, scaleVerAdjustment, scaleHorAdjustment);
                childGO.transform.eulerAngles = Vector3.up * rotationAdjustment;
                childGO.transform.rotation = Quaternion.FromToRotation (Vector3.up, hit.normal) * childGO.transform.rotation;
            }
            else
            {
                Debug.LogError($"Positioning child {childGO.name} of {newGO.name} at {newChildPosition.ToString()} failed, disabling object");
                newGO.SetActive(false);
            }
        }
    }

}
