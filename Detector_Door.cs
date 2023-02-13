using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detector_Door : MonoBehaviour
{
    [SerializeField] private List<GameObject> _doorBaricades;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<Detector_Door>())
        {
            if (_doorBaricades.Count > 0)
            {
                for (int i = 0; i < _doorBaricades.Count; i++)
                {
                    _doorBaricades[i].SetActive(false);
                }
            }
            else
            {
                Debug.LogWarning("Please add game objects to _doorBaricades list");
            }

        }
    }
}
