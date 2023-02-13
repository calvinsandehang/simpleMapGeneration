using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootingPlaceObjectsPool : MonoBehaviour
{
    [Header("Big Object")]
    [SerializeField] private GameObject _cupboardPrefab;
    [Header("Small Object")]
    [SerializeField] private GameObject _trashCanPrefab;

    #region Get Set   
    // MID SECTION
    public GameObject CupboardPrefab { get { return _cupboardPrefab; } set { _cupboardPrefab = value; } }
    public GameObject TrashcanPrefab { get { return _trashCanPrefab; } set { _trashCanPrefab = value; } }
    #endregion
}
