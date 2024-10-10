using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PianoPlayer : MonoBehaviour
{
    Ray ray;
    RaycastHit hit;

    void Update()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            if (Input.GetMouseButtonDown(0))
            {
                PianoKey l_key = hit.collider.GetComponentInParent<PianoKey>();
                if (l_key) l_key.Play();
            }
        }
    }
}
