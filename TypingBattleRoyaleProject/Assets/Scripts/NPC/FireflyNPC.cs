using UnityEngine;
using WotK.Brand;

public class FireflyNPC : MonoBehaviour
{
    public float moveRadius = 3f;
    public float moveSpeed = 0.4f;
    public float lightRange = 2f;

    Light pointLight;

    Vector3 centerPos;
    float seedX;
    float seedY;
    float seedZ;
    float pulseSpeed;

    void Awake()
    {
        pointLight = GetComponentInChildren<Light>();
    }

    public void Activate(BoxCollider area)
    {
        Bounds b = area.bounds;

        centerPos = new Vector3(
            Random.Range(b.min.x, b.max.x),
            Random.Range(b.min.y, b.max.y),
            Random.Range(b.min.z, b.max.z));

        transform.position = centerPos;

        seedX = Random.Range(0f, 100f);
        seedY = Random.Range(0f, 100f);
        seedZ = Random.Range(0f, 100f);

        pulseSpeed = Random.Range(0.5f, 1.5f);

        pointLight.range = lightRange;
        pointLight.color = WotKBrand.GatorOrange;

        gameObject.SetActive(true);
    }

    void Update()
    {
        float t = Time.time * moveSpeed;

        float x = Mathf.PerlinNoise(seedX, t) - 0.5f;
        float y = Mathf.PerlinNoise(seedY, t) - 0.5f;
        float z = Mathf.PerlinNoise(seedZ, t) - 0.5f;

        Vector3 offset = new Vector3(x, y, z) * moveRadius;
        transform.position = centerPos + offset;

        float pulse = Mathf.Lerp(
            0.4f,
            1f,
            (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f);

        pointLight.intensity = pulse;

    }
}