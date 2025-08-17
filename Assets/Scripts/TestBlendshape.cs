using UnityEngine;

public class EmotionTester : MonoBehaviour
{
    public BlendShapeController faceController;

    void Start()
    {
        // faceController.Smile();  // Call the smile expression on start
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            faceController.Smile();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            faceController.Angry();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            faceController.Shy();
        }
    }
}
