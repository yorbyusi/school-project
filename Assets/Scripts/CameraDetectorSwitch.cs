using UnityEngine;

public class TriggerObjectActivator : MonoBehaviour
{
    public GameObject object1; // The object to activate on enter
    public GameObject object2; // The object to activate on exit

    private void Start()
    {
        // // Set initial state (optional)
        // if (object1 != null) object1.SetActive(false);
        // if (object2 != null) object2.SetActive(true);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (object1 != null) object1.SetActive(true);
            if (object2 != null) object2.SetActive(false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (object1 != null) object1.SetActive(false);
            if (object2 != null) object2.SetActive(true);
        }
    }
}
