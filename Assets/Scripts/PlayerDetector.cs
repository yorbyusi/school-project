using UnityEngine;

public class JobSelectorTriggerTMP : MonoBehaviour
{
    public GameObject uiPanel;
    public JobSelectionUI jobSelectionUI;
    public bool locked = false;

    private string playerTag = "Player";

    // Simpan instance & nama job yang aktif (optional, tapi rapi)
    private GameObject currentJobInstance;
    private string currentJobName;

    private void OnTriggerStay(Collider other)
    {
        if (locked) return;
        if (!other.CompareTag(playerTag)) return;

        if (uiPanel != null) uiPanel.SetActive(true);
        //Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (jobSelectionUI != null)
            jobSelectionUI.SetCurrentTarget(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (uiPanel != null) uiPanel.SetActive(false);
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;

        if (jobSelectionUI != null)
            jobSelectionUI.ClearTarget();
    }

    public void AssignPrefab(GameObject prefab, string jobName)
    {
        // 1) Bersihkan semua instance lama (apapun job-nya)
        //    — aman kalau pernah ganti dari Menulis → Menggambar → dst.
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform c = transform.GetChild(i);
            if (c.name.EndsWith("_Instance"))
                Destroy(c.gameObject);
        }

        // 2) (opsional) kalau simpan referensi, pastikan juga dihancurkan
        if (currentJobInstance != null)
        {
            Destroy(currentJobInstance);
            currentJobInstance = null;
        }

        // 3) Spawn baru
        if (prefab != null)
        {
            currentJobInstance = Instantiate(prefab, transform);
            currentJobInstance.name = jobName + "_Instance";
            currentJobName = jobName;
        }
    }
}
