using UnityEngine;
using UnityEngine.UI;

public class MessageBehaviour : MonoBehaviour
{
    private Toggle toggle;

    // Start is called before the first frame update
    void Start()
    {
        toggle = transform.parent.GetComponentInChildren<Toggle>();
        toggle.onValueChanged.AddListener(delegate
        {
            ToggleValueChanged(toggle);
        });
    }

    private void ToggleValueChanged(Toggle toggle)
    {
        GetComponent<Image>().enabled = !toggle.isOn;
    }
}
