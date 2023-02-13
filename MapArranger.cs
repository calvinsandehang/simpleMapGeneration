using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(200)]
public class MapArranger : MonoBehaviour
{
    [SerializeField] private MapSeed _mapSeed;
    [SerializeField] private MapSectionPool _mapPool;    
    [SerializeField] private HidingPlaceObjectsPool _hidingPlaceObjectPool;
    [SerializeField] private LootingPlaceObjectsPool _lootingPlaceObjectPool;
    [SerializeField] private ObstacleObjectsPool _obstacleObjectsPool;
    [SerializeField] private LayerMask _roomLayer, _objectLayer;
    [SerializeField] private Transform _poolLocation, _startPosition;
    private Dictionary<int, GameObject> _corridorDict, _roomDict, _hidingPlaceDict, _lootingPlaceDict, _obstacleObjectDict;

    private int _sectionCount;
    private StartSection _startSection;

    private class StartSection
    {
        public GameObject startSectionGO;
        public ConnectorCorridor startSectionConnector;
        public Vector3 corridorConnectorPosition;
        public Quaternion corridorConnectorRotation;

        public StartSection(MapSectionPool mapPool)
        {
            startSectionGO = Instantiate(mapPool.StartSection1Prefab, Vector3.zero, Quaternion.identity);
            startSectionConnector = startSectionGO.GetComponentInChildren<ConnectorCorridor>();
            corridorConnectorPosition = startSectionConnector.gameObject.transform.position;
            corridorConnectorRotation = startSectionConnector.gameObject.transform.rotation;
        }

    }
    private class CorridorSection
    {
        public GameObject midSectionGO;
        public ConnectorCorridor midSectionConnector;
        public Vector3 midConnectorPosition;
        public Quaternion midConnectorRotation;
        // a list that store all the instantiated MidSection prefab in the map
        public List<GameObject> instantiatedMidSection = new List<GameObject>();
        // a list that store all the corridorConnector in a MidSectionPrefab. This list will be cleared after the program finish instantiating other connecting corridor
        public List<ConnectorCorridor> corridorConnector = new List<ConnectorCorridor>();

        // store every spawn point for hiding place in a corridor and a room
        public List<SpawnPoint_Hiding> totalHidingPlacesSP = new List<SpawnPoint_Hiding>();

        public float hidingPlaceIONum;
        public float totalhidingPlaceIONum;

        public float lootingPlaceIONum;
        public float totalLootingPlaceIONum;

        public float obstacleObjectIONum;
        public float totalObstacleObjectIONum;
    }
    private class RoomSection
    {
        public GameObject roomGO;
        // a list that store all the roomConnector in a MidSectionPrefab. This list will be cleared after the program finish instantiating the rooms
        public List<ConnectorRoom> roomConnector = new List<ConnectorRoom>();
        public Vector3 connectorRoomWorldPos;
        public Quaternion connectorRoomWorldRot;
        public BoxCollider roomCollider;
        public Vector3 roomSize;
        public LocatorRoomCollider colliderLocator;
        public Vector3 colliderPosition;
        public Collider[] colliders;

        // store spawn point for hiding place in a corridor and a room
        public List<SpawnPoint_Hiding> hidingPlacesSP = new List<SpawnPoint_Hiding>();
        // store every spawn point for hiding place in a corridor and a room
        public List<SpawnPoint_Hiding> totalHidingPlacesSPInRoom = new List<SpawnPoint_Hiding>();

        // store spawn point for hiding place in a corridor and a room
        public List<SpawnPoint_Looting> lootingPlacesSP = new List<SpawnPoint_Looting>();
        // store every spawn point for hiding place in a corridor and a room
        public List<SpawnPoint_Looting> totalLootingPlacesSPInRoom = new List<SpawnPoint_Looting>();

        public float hidingPlaceIONum;
        public float lootingPlaceIONum;

        // store spawn point for hiding place in a corridor and a room
        public List<SpawnPoint_Obstacle> obstacleObjectsSP = new List<SpawnPoint_Obstacle>();
        // store every spawn point for hiding place in a corridor and a room
        public List<SpawnPoint_Obstacle> totalObstacleObjectsSPInRoom = new List<SpawnPoint_Obstacle>();

        public float obstacleObjectIONum;
        public float totalObstacleObjectIONum;
    }

    private class HidingPlaceIO 
    {
        // store spawn point for hiding place in a corridor and a room
        public List<SpawnPoint_Hiding> hidingPlacesSP = new List<SpawnPoint_Hiding>();
        public Vector3 connectorRoomWorldPos;
        public Quaternion connectorRoomWorldRot;
        public BoxCollider hidingPlaceIOCollider;
        public Vector3 hidingPlaceIOSize;                
        public Collider[] colliders;
        public GameObject hidingPlaceGO;
    }

    private class LootingPlaceIO
    {
        // store spawn point for hiding place in a corridor and a room
        public List<SpawnPoint_Looting> lootingPlacesSP = new List<SpawnPoint_Looting>();
        public Vector3 connectorRoomWorldPos;
        public Quaternion connectorRoomWorldRot;
        public BoxCollider lootingPlaceIOCollider;
        public Vector3 lootingPlaceIOSize;
        public Collider[] colliders;
        public GameObject lootingPlaceGO;
    }

    private class ObstacleObjectIO
    {
        // store spawn point for hiding place in a corridor and a room
        public List<SpawnPoint_Obstacle> obstacleObjectsSP = new List<SpawnPoint_Obstacle>();
        public Vector3 connectorRoomWorldPos;
        public Quaternion connectorRoomWorldRot;
        public BoxCollider obstacleObjectIOCollider;
        public Vector3 obstacleObjectIOSize;
        public Collider[] colliders;
        public GameObject obstacleObjectGO;
    }
    private void Awake()
    {
        CorridorDictionary();
        RoomDictionary();
        HidingPlaceDictionary();
        LootingPlaceDictionary();
        ObstacleObjectsDictionary();
    }

    // Start is called before the first frame update
    void Start()
    {
        // arrange the sections
        ArrangeSections(_sectionCount);
    }


    private void ArrangeSections(int sectionCount)
    {        
        // the program won't spawn a new corridor if the angle between the ConnectorCorridor gameObject and the goal is > 120 degree        
        StartSection startSection = new StartSection(_mapPool);

        HandleCorridor(sectionCount, startSection.corridorConnectorPosition, startSection.corridorConnectorRotation);        
    }
   
    private CorridorSection HandleCorridor(int sectionCount, Vector3 startPos, Quaternion startRot)
    {
        CorridorSection corridor = new CorridorSection();
        HidingPlaceIO hidingPlace = new HidingPlaceIO();
        LootingPlaceIO lootingPlace = new LootingPlaceIO();
        ObstacleObjectIO obstacleObject = new ObstacleObjectIO();

        GameObject goal = GameObject.FindGameObjectWithTag("Goal");
        // install the first corridor
        corridor.instantiatedMidSection.Add(Instantiate(CorridorGenerator(), startPos, startRot));

        // for loop that spawn the corridors and its connected rooms
        for (int i = 1; i < sectionCount; i++)
        {
            #region collecting initial data
            /////////// COLLECTING INTIAL DATA /////////// 

            // get all the corridor and room connector component from a section, put it in a list
            corridor.corridorConnector.AddRange(corridor.instantiatedMidSection[i - 1].GetComponentsInChildren<ConnectorCorridor>());
            // shuffle the component on the list to make it more random
            corridor.corridorConnector.Shuffle();

            // get all the hiding place spawn point component from a section, put it in a list
            hidingPlace.hidingPlacesSP.AddRange(corridor.instantiatedMidSection[i - 1].GetComponentsInChildren<SpawnPoint_Hiding>());
            // hiding place count in the corridor 
            corridor.hidingPlaceIONum = hidingPlace.hidingPlacesSP.Count;

            // get all the looting place spawn point component from a section, put it in a list
            lootingPlace.lootingPlacesSP.AddRange(corridor.instantiatedMidSection[i - 1].GetComponentsInChildren<SpawnPoint_Looting>());
            corridor.lootingPlaceIONum = lootingPlace.lootingPlacesSP.Count;

            // get all the obstacle object spawn point component from a section, put it in a list
            obstacleObject.obstacleObjectsSP.AddRange(corridor.instantiatedMidSection[i - 1].GetComponentsInChildren<SpawnPoint_Obstacle>());
            corridor.obstacleObjectIONum = obstacleObject.obstacleObjectsSP.Count;

            #endregion

            #region spawn room
            /////////// SPAWN ROOM IN THE PREVIOUSLY SPAWNED CORRIDOR /////////// 

            // handle spawning room in the corridor
            RoomSection parentRoom = HandleParentRoom(corridor, i);
            #endregion

            #region spawn hiding place interactable object
            /////////// COLLECTING DATA FOR HIDING PLACE OBJECTS /////////// 
            // we collect the hiding place spawn point in the corridor (above at the "collectiong initial data")
            // Below, we collect the hiding data from the room and combined it


            // hiding place count in the rooms connected to the corridor
            parentRoom.hidingPlaceIONum = parentRoom.totalHidingPlacesSPInRoom.Count;
            // total hiding place in the corridor and room
            corridor.totalhidingPlaceIONum = corridor.hidingPlaceIONum + parentRoom.hidingPlaceIONum;
            // add hiding place spawn point in the rooms connected to the corridor to the list
            hidingPlace.hidingPlacesSP.AddRange(parentRoom.totalHidingPlacesSPInRoom);
            hidingPlace.hidingPlacesSP.Shuffle();
            
            // spawn the hidingPlaceIO object
            HidingPlaceIO hidingPlaceIO = HandleHidingPlace(hidingPlace, corridor.totalhidingPlaceIONum);

            #endregion

            #region spawn looting place interactable object
            /////////// COLLECTING DATA FOR LOOTING PLACE OBJECTS /////////// 
            // we collect the looting place spawn point in the corridor (above at the "collectiong initial data")
            // Below, we collect the looting data from the room and combined it


            // looting place count in the rooms connected to the corridor
            parentRoom.lootingPlaceIONum = parentRoom.totalLootingPlacesSPInRoom.Count;
            // total looting place in the corridor and room
            corridor.totalLootingPlaceIONum = corridor.lootingPlaceIONum + parentRoom.lootingPlaceIONum;
            // add looting place spawn point in the rooms connected to the corridor to the list
            lootingPlace.lootingPlacesSP.AddRange(parentRoom.totalLootingPlacesSPInRoom);
            lootingPlace.lootingPlacesSP.Shuffle();

            // spawn the lootingPlaceIO object
            LootingPlaceIO lootingPlaceIO = HandleLootingPlace(lootingPlace, corridor.totalLootingPlaceIONum);
            #endregion

            #region spawn obstacle object interactable object
            /////////// COLLECTING DATA FOR OBSTACLE OBJECTS /////////// 
            // we collect the obstacle object spawn point in the corridor (above at the "collectiong initial data")
            // Below, we collect the obstacle data from the room and combined it


            // obstacle object count in the rooms connected to the corridor
            parentRoom.obstacleObjectIONum = parentRoom.totalObstacleObjectsSPInRoom.Count;
            // total obstacle object in the corridor and room
            corridor.totalObstacleObjectIONum = corridor.obstacleObjectIONum + parentRoom.obstacleObjectIONum;
            // add obstacle object spawn point in the rooms connected to the corridor to the list
            obstacleObject.obstacleObjectsSP.AddRange(parentRoom.totalObstacleObjectsSPInRoom);
            obstacleObject.obstacleObjectsSP.Shuffle();

            // spawn the obstacleObjectIO object
            ObstacleObjectIO obstacleObjectIO = HandleObstacleObject(obstacleObject, corridor.totalObstacleObjectIONum);

            #endregion
            #region spawn the corridor
            ////////////////////////////////////////
            // SPAWN THE CORRIDOR //////////////////
            ////////////////////////////////////////

            // truePath = the path that lead to finish line
            bool truePath = true;

            // loop all the corridor connector in a section. The corridor connector is randomly chosen due to Shuffle() method above
            for (int k = 0; k < corridor.corridorConnector.Count; k++)
            {
                // check the angle between the corridorConnector and the goal
                float angle = Vector3.Angle(corridor.corridorConnector[k].gameObject.transform.forward, goal.transform.position - corridor.corridorConnector[k].gameObject.transform.position);

                // if truepath still true, the system can still spawn a corridor that lead to the finish line
                if (truePath)
                {
                    // only corridorConndector that has angle <= 120 relative to the Goal is allowed to spawn corridor that lead to the finish line
                    // this mechanic avoid corridor from overlaping and we can predit the behavior of the corridor spawning
                    // that is always goes to positive Z direction
                    if (angle <= 120)
                    {
                        // convert the connectorCorridor local position to world position
                        Vector3 connectorCorridorWorldPos = corridor.corridorConnector[k].transform.parent.TransformPoint(corridor.corridorConnector[k].transform.localPosition);
                        // compute the conncetorCorridor rotation
                        Quaternion connectorCorridorWorldRot = corridor.corridorConnector[k].transform.rotation;
                        // instantiate the corridor that lead to the finish line
                        corridor.midSectionGO = Instantiate(CorridorGenerator(), connectorCorridorWorldPos, connectorCorridorWorldRot);
                        corridor.instantiatedMidSection.Add(corridor.midSectionGO);
                        // when the corridor that lead to the finish line is spawn, we cannot spawn another corridor of the same type
                        truePath = false;
                    }
                    else
                    {
                        // instantiate a dead end corridor
                        Instantiate(_mapPool.BlockSection1Prefab, corridor.corridorConnector[k].transform.parent.TransformPoint(corridor.corridorConnector[k].transform.localPosition), corridor.corridorConnector[k].transform.rotation);
                    }
                }
                else
                {
                    // instantiate a dead end corridor
                    Instantiate(_mapPool.BlockSection1Prefab, corridor.corridorConnector[k].transform.parent.TransformPoint(corridor.corridorConnector[k].transform.localPosition), corridor.corridorConnector[k].transform.rotation);
                }
            }

            #endregion

            // clear the corridor connector because it will be used for the next section corridor connector
            corridor.corridorConnector.Clear();
            // clear the hiding place spawn point because it will be used for the next section corridor connector
            hidingPlace.hidingPlacesSP.Clear();
            parentRoom.totalHidingPlacesSPInRoom.Clear();

        }

        return corridor;
    }

    private RoomSection HandleParentRoom(CorridorSection corridor, int i)
    {
        RoomSection parentRoom = new RoomSection();

        #region initialization
        // clear the previous section hiding place spawn point
        parentRoom.hidingPlacesSP.Clear();

        // get all the corridor and ro0m connector component from a section, put it in a list  
        parentRoom.roomConnector.AddRange(corridor.instantiatedMidSection[i - 1].GetComponentsInChildren<ConnectorRoom>());
        // shuffle the component on the list to make it more random            
        parentRoom.roomConnector.Shuffle();

        #endregion
        // Add room into the previously placed corridor

        // Among the room connector that is available, we take random number that decide how many room will be spawn in certain corridor
        int roomRandomNumber = Random.Range(0, parentRoom.roomConnector.Count);
        //Debug.Log("roomRandomNumber = " + roomRandomNumber);
                
        // loop roomConnector, only certain roomConnector depending on the random number, can be 0, can be all room connector
        
        for (int j = 0; j < Mathf.Min(roomRandomNumber, parentRoom.roomConnector.Count); j++)
        {
            // compute the connector position and rotation in world space
            Vector3 connectorRoomWorldPos = parentRoom.roomConnector[j].transform.parent.TransformPoint(parentRoom.roomConnector[j].transform.localPosition);
            Quaternion connectorRoomWorldRot = parentRoom.roomConnector[j].transform.rotation;

            // instantiate the room at room conncetor position and rotation
            parentRoom.roomGO = Instantiate(RoomGenerator(), connectorRoomWorldPos, connectorRoomWorldRot);

            //detect if overlapping with other room
            parentRoom.roomCollider = parentRoom.roomGO.GetComponent<BoxCollider>();
            parentRoom.roomSize = (parentRoom.roomCollider.size) * 0.95f; // times 0.95 so that adjacent room does not considered as overlap  

            parentRoom.colliderLocator = parentRoom.roomGO.GetComponentInChildren<LocatorRoomCollider>();
            parentRoom.colliderPosition = parentRoom.colliderLocator.transform.position;

            // need to turn off the collider before detecting other collider, otherwise the Physics.OverlapBox will detect this room detector and think that it is overlapping with other room
            parentRoom.roomCollider.enabled = false;

            // detect if other collider is overlapping with the collider detector
            parentRoom.colliders = Physics.OverlapBox(parentRoom.colliderPosition, parentRoom.roomSize / 2f, Quaternion.identity, _roomLayer);

            // if there is an overlapping collider, the array length is more than 0
            if (parentRoom.colliders.Length > 0)
            {
                // deactivate overlapping room
                parentRoom.roomGO.SetActive(false);
                Debug.Log(" Room overlap with other room : " + parentRoom.roomGO.name);

            }
            else 
            {
                parentRoom.roomCollider.enabled = true;
                // get all the hiding place spawn point component from a parentRoom, put it in a list
                parentRoom.hidingPlacesSP.AddRange(parentRoom.roomGO.GetComponentsInChildren<SpawnPoint_Hiding>());
                // clear Debug.LogWarning(parentRoom.hidingPlacesSP.Count);
                //HandleChildRoom(parentRoom);

                RoomSection hidingPlaceInfo = HandleChildRoom(parentRoom);

                parentRoom.totalHidingPlacesSPInRoom.AddRange(hidingPlaceInfo.hidingPlacesSP);
                Debug.LogWarning(parentRoom.totalHidingPlacesSPInRoom.Count);
                Debug.Log("HandleChildRoom(mainRoom);");
            }
            
        }
        // clear the room connector because it will be use for the next section room connector
        parentRoom.roomConnector.Clear();

        return parentRoom;
    }
    private RoomSection HandleChildRoom(RoomSection parentRoom) 
    {
        RoomSection childRoom = new RoomSection();

        #region initialization
        // clear the previous section hiding place spawn point
        childRoom.hidingPlacesSP.Clear();
        // get the hiding place spawn point list from the parent room
        childRoom.hidingPlacesSP.AddRange(parentRoom.hidingPlacesSP);

        // get all room connector component from a parent Room, put it in a list  
        childRoom.roomConnector.AddRange(parentRoom.roomGO.GetComponentsInChildren<ConnectorRoom>());

        // shuffle the component on the list to make it more random            
        //childRoom.roomConnector.Shuffle();
        #endregion
        // Add room into the previously placed corridor
        int childRoomRandomNumber;
        // Among the room connector that is available, we take random number that decide how many room will be spawn in certain corridor
        if (childRoom.roomConnector.Count > 1)
        {
            childRoomRandomNumber = Random.Range(0, childRoom.roomConnector.Count);
            //Debug.Log("roomRandomNumber = " + roomRandomNumber);
        }
        else 
        {
            childRoomRandomNumber = Random.Range(0, childRoom.roomConnector.Count + 1);
            //Debug.Log("roomRandomNumber = " + roomRandomNumber);
        }

        Debug.Log("Mathf.Min(childRoomRandomNumber, childRoom.roomConnector.Count)"+ Mathf.Min(childRoomRandomNumber, childRoom.roomConnector.Count));
        // loop roomConnector, only certain roomConnector depending on the random number, can be 0, can be all room connector
        for (int j = 0; j < Mathf.Min(childRoomRandomNumber, childRoom.roomConnector.Count); j++)
        {
            // compute the connector position and rotation in world space
            Vector3 connectorRoomWorldPos = childRoom.roomConnector[j].transform.parent.TransformPoint(childRoom.roomConnector[j].transform.localPosition);
            Quaternion connectorRoomWorldRot = childRoom.roomConnector[j].transform.rotation;

            // instantiate the room at room conncetor position and rotation
            childRoom.roomGO = Instantiate(RoomGenerator(), connectorRoomWorldPos, connectorRoomWorldRot);
            Debug.Log("Instantiate ChildRoom");

            //detect if overlapping with other room
            childRoom.roomCollider = childRoom.roomGO.GetComponent<BoxCollider>();
            childRoom.roomSize = (childRoom.roomCollider.size) * 0.95f; // times 0.95 so that adjacent room does not considered as overlap  

            childRoom.colliderLocator = childRoom.roomGO.GetComponentInChildren<LocatorRoomCollider>();
            childRoom.colliderPosition = childRoom.colliderLocator.transform.position;

            // need to turn off the collider before detecting other collider, otherwise the Physics.OverlapBox will detect this room detector and think that it is overlapping with other room
            childRoom.roomCollider.enabled = false;

            // detect if other collider is overlapping with the collider detector
            childRoom.colliders = Physics.OverlapBox(childRoom.colliderPosition, childRoom.roomSize / 2f, Quaternion.identity, _roomLayer);

            // if there is an overlapping collider, the array length is more than 0
            if (childRoom.colliders.Length > 0)
            {
                // deactivate overlapping room
                childRoom.roomGO.SetActive(false);
                //Debug.Log(" Child Room overlap with other room : " + childRoom.roomGO.name);
            }
            else
            {
                childRoom.roomCollider.enabled = true;
                childRoom.hidingPlacesSP.AddRange(childRoom.roomGO.GetComponentsInChildren<SpawnPoint_Hiding>());

            }

        }
        // clear the room connector because it will be use for the next section room connector
        childRoom.roomConnector.Clear();

        return childRoom;
    }
    private HidingPlaceIO HandleHidingPlace(HidingPlaceIO hidingPlace, float totalHidingPlace ) 
    {
        // Among the hiding place spawn point that are available, we take random number that decide how many hiding place will be spawned in the corridor and connected rooms
        int randomNumber = Random.Range(0, hidingPlace.hidingPlacesSP.Count);

        // loop hiding place spawn point, only certain spawn point depending on the random number, can be 0, can be all room connector
        for (int j = 0; j < Mathf.Min(randomNumber, totalHidingPlace); j++)
        {
            // compute the connector position and rotation in world space
            Vector3 spawnPointWorldPos = hidingPlace.hidingPlacesSP[j].transform.parent.TransformPoint(hidingPlace.hidingPlacesSP[j].transform.localPosition);
            Quaternion spawnPointWorldRot = hidingPlace.hidingPlacesSP[j].transform.rotation;

            // instantiate the room at room conncetor position and rotation
            hidingPlace.hidingPlaceGO = Instantiate(HidingPlaceGenerator(), spawnPointWorldPos, spawnPointWorldRot);

            //detect if overlapping with other objects
            hidingPlace.hidingPlaceIOCollider = hidingPlace.hidingPlaceGO.GetComponent<BoxCollider>();
            hidingPlace.hidingPlaceIOSize = (hidingPlace.hidingPlaceIOCollider.size) * 0.95f; // times 0.95 so that adjacent room does not considered as overlap  
                        
            // need to turn off the collider before detecting other collider, otherwise the Physics.OverlapBox will detect this room detector and think that it is overlapping with other room
            hidingPlace.hidingPlaceIOCollider.enabled = false;

            // detect if other collider is overlapping with the collider detector
            hidingPlace.colliders = Physics.OverlapBox(spawnPointWorldPos, hidingPlace.hidingPlaceIOSize / 2f, Quaternion.identity, _objectLayer);

            // if there is an overlapping collider, the array length is more than 0
            if (hidingPlace.colliders.Length > 0)
            {
                // deactivate overlapping room
                hidingPlace.hidingPlaceGO.SetActive(false);
                Debug.Log(" Hiding Place overlap with other object : " + hidingPlace.hidingPlaceGO.name);

            }
            else
            {
                hidingPlace.hidingPlaceIOCollider.enabled = true;                
            }

        }
        // clear the hiding place spawn point in the list because it will be used for the next section room connector
        hidingPlace.hidingPlacesSP.Clear();

        return hidingPlace;
    }

    private LootingPlaceIO HandleLootingPlace(LootingPlaceIO lootingPlace, float totalLootingPlace)
    {
        // Among the looting place spawn point that are available, we take random number that decide how many looting place will be spawned in the corridor and connected rooms
        int randomNumber = Random.Range(0, lootingPlace.lootingPlacesSP.Count);

        // loop looting place spawn point, only certain spawn point depending on the random number, can be 0, can be all room connector
        for (int j = 0; j < Mathf.Min(randomNumber, totalLootingPlace); j++)
        {
            // compute the connector position and rotation in world space
            Vector3 spawnPointWorldPos = lootingPlace.lootingPlacesSP[j].transform.parent.TransformPoint(lootingPlace.lootingPlacesSP[j].transform.localPosition);
            Quaternion spawnPointWorldRot = lootingPlace.lootingPlacesSP[j].transform.rotation;

            // instantiate the room at room conncetor position and rotation
            lootingPlace.lootingPlaceGO = Instantiate(LootingPlaceGenerator(), spawnPointWorldPos, spawnPointWorldRot);

            //detect if overlapping with other objects
            lootingPlace.lootingPlaceIOCollider = lootingPlace.lootingPlaceGO.GetComponent<BoxCollider>();
            lootingPlace.lootingPlaceIOSize = (lootingPlace.lootingPlaceIOCollider.size) * 0.95f; // times 0.95 so that adjacent room does not considered as overlap  

            // need to turn off the collider before detecting other collider, otherwise the Physics.OverlapBox will detect this room detector and think that it is overlapping with other room
            lootingPlace.lootingPlaceIOCollider.enabled = false;

            // detect if other collider is overlapping with the collider detector
            lootingPlace.colliders = Physics.OverlapBox(spawnPointWorldPos, lootingPlace.lootingPlaceIOSize / 2f, Quaternion.identity, _objectLayer);

            // if there is an overlapping collider, the array length is more than 0
            if (lootingPlace.colliders.Length > 0)
            {
                // deactivate overlapping room
                lootingPlace.lootingPlaceGO.SetActive(false);
                Debug.Log(" Looting Place overlap with other object : " + lootingPlace.lootingPlaceGO.name);

            }
            else
            {
                lootingPlace.lootingPlaceIOCollider.enabled = true;
            }

        }
        // clear the looting place spawn point in the list because it will be used for the next section room connector
        lootingPlace.lootingPlacesSP.Clear();

        return lootingPlace;
    }

    private ObstacleObjectIO HandleObstacleObject(ObstacleObjectIO obstacleObject, float totalObstaclePlace)
    {
        // Among the obstacle place spawn point that are available, we take random number that decide how many obstacle place will be spawned in the corridor and connected rooms
        int randomNumber = Random.Range(0, obstacleObject.obstacleObjectsSP.Count);

        // loop obstacle place spawn point, only certain spawn point depending on the random number, can be 0, can be all room connector
        for (int j = 0; j < Mathf.Min(randomNumber, totalObstaclePlace); j++)
        {
            //chance to appear
            int chanceNum = Random.Range(0, 100);

            if (chanceNum < 50)
            {

            }
            else
            {
                // compute the connector position and rotation in world space
                Vector3 spawnPointWorldPos = obstacleObject.obstacleObjectsSP[j].transform.parent.TransformPoint(obstacleObject.obstacleObjectsSP[j].transform.localPosition);
                Quaternion spawnPointWorldRot = obstacleObject.obstacleObjectsSP[j].transform.rotation;

                // instantiate the room at room conncetor position and rotation
                obstacleObject.obstacleObjectGO = Instantiate(ObstacleObjectGenerator(), spawnPointWorldPos, spawnPointWorldRot);

                //detect if overlapping with other objects
                obstacleObject.obstacleObjectIOCollider = obstacleObject.obstacleObjectGO.GetComponent<BoxCollider>();
                obstacleObject.obstacleObjectIOSize = (obstacleObject.obstacleObjectIOCollider.size) * 0.95f; // times 0.95 so that adjacent room does not considered as overlap  

                // need to turn off the collider before detecting other collider, otherwise the Physics.OverlapBox will detect this room detector and think that it is overlapping with other room
                obstacleObject.obstacleObjectIOCollider.enabled = false;

                // detect if other collider is overlapping with the collider detector
                obstacleObject.colliders = Physics.OverlapBox(spawnPointWorldPos, obstacleObject.obstacleObjectIOSize / 2f, Quaternion.identity, _objectLayer);

                // if there is an overlapping collider, the array length is more than 0
                if (obstacleObject.colliders.Length > 0)
                {
                    // deactivate overlapping room
                    obstacleObject.obstacleObjectGO.SetActive(false);
                    Debug.Log(" Obstacle Place overlap with other object : " + obstacleObject.obstacleObjectGO.name);

                }
                else
                {
                    obstacleObject.obstacleObjectIOCollider.enabled = true;
                }
            }
            

        }
        // clear the obstacle place spawn point in the list because it will be used for the next section room connector
        obstacleObject.obstacleObjectsSP.Clear();

        return obstacleObject;
    }

    // we store the corridor prefab in the dictionary
    private void CorridorDictionary() 
    {
        // Create a dictionary that maps strings to GameObjects
        _corridorDict = new Dictionary<int, GameObject>();

        // Add some items to the dictionary
        _corridorDict.Add(0, _mapPool.MidSection1Prefab);
        _corridorDict.Add(1, _mapPool.MidSection2Prefab);
        _corridorDict.Add(2, _mapPool.MidSection3Prefab);
    }

    // we randomly choose one corridor for ArrangeSection() method to spawn
    private GameObject CorridorGenerator()
    {
        // Choose a random item from the dictionary
        System.Random random = new System.Random();
        int index = random.Next(_corridorDict.Count);
        GameObject randomCorridor = _corridorDict[index];

        // Return the value of the random item
        return randomCorridor;
    }

    // we store the room prefab in the dictionary
    private void RoomDictionary()
    {
        // Create a dictionary that maps strings to GameObjects
        _roomDict = new Dictionary<int, GameObject>();

        // Add some items to the dictionary
        _roomDict.Add(0, _mapPool.Room1Prefab);
        _roomDict.Add(1, _mapPool.Room2Prefab);
        _roomDict.Add(2, _mapPool.Room3Prefab);
    }

    // we randomly choose one room for ArrangeSection() method to spawn
    private GameObject RoomGenerator()
    {
        // Choose a random item from the dictionary
        System.Random random = new System.Random();
        int index = random.Next(_roomDict.Count);
        GameObject randomRoom = _roomDict[index];

        // Return the value of the random item
        return randomRoom;
    }

    private void HidingPlaceDictionary()
    {
        // Create a dictionary that maps strings to GameObjects
        _hidingPlaceDict = new Dictionary<int, GameObject>();

        // Add some items to the dictionary
        _hidingPlaceDict.Add(0, _hidingPlaceObjectPool.SingleLockerPrefab);
        _hidingPlaceDict.Add(1, _hidingPlaceObjectPool.TripleLockerPrefab);
    }

    private GameObject HidingPlaceGenerator()
    {
        // Choose a random item from the dictionary
        System.Random random = new System.Random();
        int index = random.Next(_hidingPlaceDict.Count);
        Debug.Log("_hidingPlaceDict.Count" + _hidingPlaceDict.Count);
        GameObject randomHidingPlace = _hidingPlaceDict[index];

        // Return the value of the random item
        return randomHidingPlace;
    }

    private void LootingPlaceDictionary()
    {
        // Create a dictionary that maps strings to GameObjects
        _lootingPlaceDict = new Dictionary<int, GameObject>();

        // Add some items to the dictionary
        _lootingPlaceDict.Add(0, _lootingPlaceObjectPool.CupboardPrefab);
        _lootingPlaceDict.Add(1, _lootingPlaceObjectPool.TrashcanPrefab);
    }

    private GameObject LootingPlaceGenerator()
    {
        // Choose a random item from the dictionary
        System.Random random = new System.Random();
        int index = random.Next(_lootingPlaceDict.Count);
        Debug.Log("_hidingPlaceDict.Count" + _lootingPlaceDict.Count);
        GameObject randomHidingPlace = _lootingPlaceDict[index];

        // Return the value of the random item
        return randomHidingPlace;
    }

    private void ObstacleObjectsDictionary()
    {
        // Create a dictionary that maps strings to GameObjects
        _obstacleObjectDict = new Dictionary<int, GameObject>();

        // Add some items to the dictionary
        _obstacleObjectDict.Add(0, _obstacleObjectsPool.GourneyPrefab);        
    }

    private GameObject ObstacleObjectGenerator()
    {
        // Choose a random item from the dictionary
        System.Random random = new System.Random();
        int index = random.Next(_obstacleObjectDict.Count);
        Debug.Log("_hidingPlaceDict.Count" + _obstacleObjectDict.Count);
        GameObject randomObstacleObject = _obstacleObjectDict[index];

        // Return the value of the random item
        return randomObstacleObject;
    }

    private void OnEnable()
    {
        _mapSeed.MapInfo += SectionCount;
    }

    private void OnDisable()
    {
        _mapSeed.MapInfo -= SectionCount;
    }

    private void SectionCount(int sectionCount)
    {
        _sectionCount = sectionCount;
    }

}
