using UnityEngine;

namespace PoVMonsterLevels;

internal sealed class LevelBillboard : MonoBehaviour
{
    private Character _character = null!;
    private TextMesh _text = null!;
    private float _nextRefresh;

    private void Awake()
    {
        _character = GetComponent<Character>();
        var label = new GameObject("PoVMonsterLevelLabel");
        label.transform.SetParent(transform, false);
        label.transform.localPosition = Vector3.up * (_character.GetHeight() + 0.45f);
        _text = label.AddComponent<TextMesh>();
        _text.anchor = TextAnchor.MiddleCenter;
        _text.alignment = TextAlignment.Center;
        _text.characterSize = 0.16f;
        _text.fontSize = 48;
        _text.richText = false;
    }

    private void LateUpdate()
    {
        if (!Plugin.ShowOverheadLevel.Value || Player.m_localPlayer == null)
        {
            _text.text = string.Empty;
            return;
        }
        var camera = Camera.main;
        if (camera != null) _text.transform.rotation = Quaternion.LookRotation(_text.transform.position - camera.transform.position);
        if (Time.time < _nextRefresh) return;
        _nextRefresh = Time.time + 0.25f;
        var level = CreatureLevel.Get(_character);
        if (level <= 0) { _text.text = string.Empty; return; }
        var playerLevel = PoVLevelAdapter.GetPlayerLevel(Player.m_localPlayer);
        _text.text = Plugin.LevelText.Value.Replace("@", level.ToString());
        _text.color = CreatureLevel.ColorFor(level, playerLevel);
    }
}
