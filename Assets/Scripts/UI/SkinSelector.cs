using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinSelector : MonoBehaviour
{
    [SerializeField] private int grid;
    [SerializeField] private Vector2 firstPosition;
    [SerializeField] private Vector2 offset;

    [SerializeField] private Sprite[] skins;

    [SerializeField] private GameObject skinSlotPrefab;

    private readonly List<GameObject> spawnedSlots = new();

    // Start is called before the first frame update
 void Start()
    {
        SpawnSkinSlots();
    }

    private void SpawnSkinSlots()
    {
        if (skinSlotPrefab == null || skins == null || skins.Length == 0)
        {
            Debug.LogWarning("SkinSelector: Prefab or skins not assigned!");
            return;
        }

        // Xóa slot cũ nếu có
        foreach (var slot in spawnedSlots)
        {
            if (slot != null)
                Destroy(slot);
        }
        spawnedSlots.Clear();

        // Spawn từng skin
        for (int i = 0; i < skins.Length; i++)
        {
            // Tính vị trí spawn

            Vector2 pos = firstPosition + new Vector2(offset.x * (i % grid), offset.y * (i/ grid));

            // Tạo slot
            GameObject slot = Instantiate(skinSlotPrefab, transform);
            slot.transform.localPosition = pos;

            // Gán sprite cho slot (nếu có Image hoặc SpriteRenderer)
            var sr = slot.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.sprite = skins[i];
            else
            {
                var img = slot.GetComponent<UnityEngine.UI.Image>();
                if (img != null)
                    img.sprite = skins[i];
            }

            spawnedSlots.Add(slot);
        }
    }
}
