using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MapSectionPool : MonoBehaviour
{
    [Header("Start section")]
    [SerializeField] private GameObject _startSection1Prefab;
    [Header("Mid section")]
    [SerializeField] private GameObject _midSection1Prefab, _midSection2Prefab, _midSection3Prefab;
    [Header("Room")]
    [SerializeField] private GameObject _room1Prefab, _room2Prefab, _room3Prefab;
    [Header("Block section")]
    [SerializeField] private GameObject _blockSection1Prefab;
    [Header("End section")]
    [SerializeField] private GameObject _endSection1Prefab;

    #region Get Set
    // START SECTION
    public GameObject StartSection1Prefab { get { return _startSection1Prefab; } set { _startSection1Prefab = value; } }

    // MID SECTION
    public GameObject MidSection1Prefab {get { return _midSection1Prefab; } set { _midSection1Prefab = value; }}
    public GameObject MidSection2Prefab { get { return _midSection2Prefab; } set { _midSection2Prefab = value; } }
    public GameObject MidSection3Prefab { get { return _midSection3Prefab; } set { _midSection3Prefab = value; } }

    // ROOM
    public GameObject Room1Prefab { get { return _room1Prefab; } set { _room1Prefab = value; } }
    public GameObject Room2Prefab { get { return _room2Prefab; } set { _midSection2Prefab = value; } }
    public GameObject Room3Prefab { get { return _room3Prefab; } set { _room3Prefab = value; } }

    // BLOCK SECTION
    public GameObject BlockSection1Prefab { get { return _blockSection1Prefab; } set { _blockSection1Prefab = value; } }
    // END SECTION
    public GameObject EndSection1Prefab { get { return _endSection1Prefab; } set { _endSection1Prefab = value; } }
    #endregion

}
