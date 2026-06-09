using UnityEngine;
using TMPro;

public class RoverDataCollection : MonoBehaviour
{
    public string dataInfo = "Environmental reading: Scanning...";
    public GameObject uiPanel;
    public TextMeshProUGUI infoText;

    private bool isCollected = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isCollected)
        {
            CollectData();
        }
    }

    private void CollectData()
    {
        isCollected = true;
        if (uiPanel != null) uiPanel.SetActive(true);
        if (infoText != null) infoText.text = dataInfo;
        
        Debug.Log("Data Collected: " + dataInfo);
        
        // Disable visual cue
        GetComponent<Renderer>().enabled = false;
        
        // Hide UI after some time
        Invoke("HideUI", 5f);
    }

    private void HideUI()
    {
        if (uiPanel != null) uiPanel.SetActive(false);
    }
}
