using UnityEngine;

public class RoomLight : MonoBehaviour
{
    public Light[] lights;   // danh sach den se sang

    void Start()
    {
        SetLights(false);   // luc dau tat het
    }

    public void TurnOn()
    {
        SetLights(true);
        Debug.Log("Den da bat sang.");
    }

    void SetLights(bool on)
    {
        foreach (Light l in lights)
            if (l != null) l.enabled = on;
    }
}