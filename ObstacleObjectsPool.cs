using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleObjectsPool : MonoBehaviour
{
    [Header("Big Object")]
    [SerializeField] private GameObject _gourneyPrefab;
    
    #region Get Set   
    // MID SECTION
    public GameObject GourneyPrefab { get { return _gourneyPrefab; } set { _gourneyPrefab = value; } }    
    #endregion
}
