using UnityEngine;
using TMPro;

public class EnemyText : MonoBehaviour
{
    [Header("Text Settings")]
    [SerializeField] Vector3 localOffset = new Vector3(0f, 2f, 0f);
    [SerializeField] float fontSize = 3.5f;
    [SerializeField] Color color = Color.white;
    [SerializeField] float outline = 0.25f;

    TextMeshPro tmp;
    Transform t;

    void Awake()
    {
        t = transform;
        EnsureTMP();
        ApplyStyle();
    }

    void LateUpdate()
    {
        if (tmp == null) return;
        var cam = Camera.main;
        if (cam != null)
        {
            tmp.transform.rotation = Quaternion.LookRotation(
                tmp.transform.position - cam.transform.GetChild(0).position, Vector3.up
            );
        }
    }

    void EnsureTMP()
    {
        tmp = GetComponentInChildren<TextMeshPro>(includeInactive: true);
        if (tmp == null)
        {
            var go = new GameObject("HPText");
            go.transform.SetParent(t, false);
            go.transform.localPosition = localOffset;

            tmp = go.AddComponent<TextMeshPro>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableAutoSizing = false;
        }
        else
        {
            tmp.transform.localPosition = localOffset;
        }
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
