using UnityEngine;
using WotK.Brand;

public class BirdNPC : MonoBehaviour
{
    public float speed = 8f;
    public float sineAmplitude = 1f;
    public float sineFrequency = 2f;
    public float despawnDistance = 80f;

    Vector3 direction;
    Vector3 startPos;
    float timeOffset;
    Camera cam;
    SpriteRenderer sr;

    void Awake()
    {
        cam = Camera.main;
        sr = GetComponent<SpriteRenderer>();
    }

    public void Activate(Vector3 pos, Vector3 dir)
    {
        transform.position = pos;
        startPos = pos;
        direction = dir.normalized;
        timeOffset = Random.Range(0f, 100f);

        gameObject.SetActive(true);

        sr.color = WotKBrand.Black;
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;

        Vector3 pos = transform.position;
        pos.y = startPos.y + Mathf.Sin((Time.time + timeOffset) * sineFrequency) * sineAmplitude;
        transform.position = pos;

        if (cam != null)
            transform.forward = cam.transform.forward;

        if (Vector3.Distance(startPos, transform.position) > despawnDistance)
            gameObject.SetActive(false);
    }
}