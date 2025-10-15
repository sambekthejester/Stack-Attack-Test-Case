using UnityEngine;
using TMPro;

public class BossText : MonoBehaviour
{
    [Header("Text Settings")]
    [SerializeField] Vector3 localOffset = new Vector3(0f, 3.5f, 0f);
    [SerializeField] float fontSize = 5f;
    [SerializeField] Color color = Color.red;
    [SerializeField] float outline = 0.3f;

    TextMeshPro tmp;
    Transform t;

    void Awake()
    {
        t = transform;
        EnsureTMP();
        ApplyStyle();

        var hp = GetComponent<BossHP>();
        if (hp != null)
        {
            hp.OnHPChanged.AddListener(SetHP);
            SetHP(hp.CurrentHealth, hp.MaxHealth);
        }
    }

    void LateUpdate()
    {
        if (!tmp) return;
        var cam = Camera.main;
        if (cam) tmp.transform.rotation = Quaternion.LookRotation(tmp.transform.position - cam.transform.position, Vector3.up);
    }

    void EnsureTMP()
    {
        tmp = GetComponentInChildren<TextMeshPro>(true);
        if (!tmp)
        {
            var go = new GameObject("BossHPText");
            go.transform.SetParent(t, false);
            go.transform.localPosition = localOffset;
            tmp = go.AddComponent<TextMeshPro>();
            tmp.alignment = TextAlignmentOptions.Center;
        }
        else tmp.transform.localPosition = localOffset;
    }

    void ApplyStyle()
    {
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.outlineWidth = outline;
    }

    public void SetHP(int current, int max)
    {
        if (!tmp) return;
        tmp.text = $"{current}";
    }
}
