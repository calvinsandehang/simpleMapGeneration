using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(100)]
public class MapSeed : MonoBehaviour
{
    //[Tooltip("The total distance of the level in meter, must be multiple of the _sectionLength")]
    //[SerializeField] private int _levelDistance;
    [Tooltip("The length of each map section")]
    [SerializeField] private int _sectionCount, _goalDistance;
    [SerializeField] private GameObject _goal;
    [SerializeField] private GameObject _test;

    private int _roomCount;

    #region event
    public delegate void MapInformation(int count);
    public event MapInformation MapInfo;
    #endregion

    private void Start()
    {
        //  Room needed
        _roomCount = _sectionCount * 2;

        MapInfo?.Invoke(_sectionCount);
        Vector3 newGoal = new Vector3(0,0,_goalDistance);
        _goal.transform.position = newGoal;
    }

    
}
