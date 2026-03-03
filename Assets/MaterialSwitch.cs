using UnityEngine;

public class MaterialSwitch : MonoBehaviour, IWorldSwitchListener
{
    [SerializeField] private Material defaultWorldMaterial;
    [SerializeField] private Material hellWorldMaterial;
    [SerializeField] private Renderer renderer;

    public void OnSwitchWorld(bool isInHellWorld)
    {
        if (isInHellWorld)
        {
            renderer.material = hellWorldMaterial;
        }
        else
        {
            renderer.material = defaultWorldMaterial;
        }
    }
    private void OnEnable()
    {
        WorldSwitcher.switchWorld += OnSwitchWorld;
    }

    private void OnDisable()
    {
        WorldSwitcher.switchWorld -= OnSwitchWorld;
    }
}
