using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HidingPlaceObjectsPool : MonoBehaviour
{    
    [Header("Locker")]
    [SerializeField] private GameObject _singleLockerPrefab, _tripleLockerPrefab;

    #region Get Set   
    // MID SECTION
    public GameObject SingleLockerPrefab { get { return _singleLockerPrefab; } set { _singleLockerPrefab = value; } }
    public GameObject TripleLockerPrefab { get { return _tripleLockerPrefab; } set { _tripleLockerPrefab = value; } }
    #endregion
}
