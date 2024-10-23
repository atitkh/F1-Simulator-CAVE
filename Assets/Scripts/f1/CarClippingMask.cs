using UnityEngine;

public class CarClippingMask : MonoBehaviour
{
    public Transform carTransform;  // The car's transform
    public Material mapMaterial;    // The material with the shader applied
    public float visibleRadius = 5f;  // Radius around the car to show

    void Update()
    {
        // Update the car's position in the shader
        mapMaterial.SetVector("_CarPosition", carTransform.position);

        // Update the radius if needed
        mapMaterial.SetFloat("_Radius", visibleRadius);
    }
}
