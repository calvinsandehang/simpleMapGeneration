using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detector_Window : MonoBehaviour
{
    [SerializeField] private List<GameObject> _windowBaricades;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<Detector_Window>())
        {
            if (_windowBaricades.Count > 0)
            {
                for (int i = 0; i < _windowBaricades.Count; i++)
                {
                    _windowBaricades[i].SetActive(false);
                }
            }
            else
            {
                Debug.LogWarning("Please add game objects to _doorBaricades list");
            }

        }
    }
}
