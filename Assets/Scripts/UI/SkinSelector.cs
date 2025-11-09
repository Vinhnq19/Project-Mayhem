using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkinSelector : MonoBehaviour
{
    [SerializeField] private int id;
    [SerializeField] private int grid = 4;
    [SerializeField] private Vector2 firstPosition;
    [SerializeField] private Vector2 offset;
    [SerializeField] private SpriteRenderer skinImage;   // nhân vật

    [SerializeField] private Color[] colors;             // danh sách màu để chọn
    [SerializeField] private GameObject skinSlotPrefab;  // prefab chứa Image + Button

    private readonly List<GameObject> spawnedSlots = new();

    void Start()
    {
        SpawnColorSlots();
    }

    private void SpawnColorSlots()
    {
        if (skinSlotPrefab == null || colors == null || colors.Length == 0)
        {
            Debug.LogWarning("SkinSelector: Prefab or colors not assigned!");
            return;
        }

        // Xóa slot cũ
        foreach (var slot in spawnedSlots)
            if (slot != null) Destroy(slot);
        spawnedSlots.Clear();

        // Tạo slot cho từng màu
        for (int i = 0; i < colors.Length; i++)
        {
            Vector2 pos = firstPosition + new Vector2(offset.x * (i % grid), offset.y * (i / grid));
            GameObject slot = Instantiate(skinSlotPrefab, transform);
            slot.transform.localPosition = pos;

            // Đặt màu lên Image trong slot
            var imgs = slot.GetComponentsInChildren<Image>(true);
            if (imgs != null && imgs.Length > 0)
            {
                imgs[^1].color = colors[i];
            }

            int index = i;
            var btn = slot.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => OnColorSelected(index));
            }

            spawnedSlots.Add(slot);
        }
    }

    private void OnColorSelected(int index)
    {
        if (id == 1)
        {
            GameData.Instance.SetPlayer1Color(colors[index]);
        }
        else
        {
            GameData.Instance.SetPlayer2Color(colors[index]);
        }
        skinImage.color = colors[index];
    }
}
