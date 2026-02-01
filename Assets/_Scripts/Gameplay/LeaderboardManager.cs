using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.UI;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using UnityEngine;
using Unity.Services.Leaderboards.Exceptions;
using Enums;

// --- DÜZELTME 1: Deðiþkenler STRING oldu. Çünkü JSON "2" (String) gönderiyor. ---
[System.Serializable]
public class LeaderboardMetadata
{
    public string charID;
    public string atkID;
}

public struct LeaderboardEntryData
{
    public int score;
    public LeaderboardMetadata metadata;
}

public class LeaderboardManager : MonoBehaviour
{
    private const string leaderboardID = "leaderboard";

    [Header("Configuration")]
    [SerializeField] private LeaderboardIconDatabase iconDatabase;

    [Header("Leaderboard Data UI (Lists)")]
    [SerializeField] private List<TextMeshProUGUI> nameFields;
    [SerializeField] private List<TextMeshProUGUI> scoreFields;
    [SerializeField] private List<Image> characterIconFields;
    [SerializeField] private List<Image> attackTypeIconFields;

    [Header("Player Row (Out-of-range) UI")]
    [SerializeField] private GameObject playerRowParent;
    [SerializeField] private TMP_Text playerRankField;
    [SerializeField] private TMP_Text playerNameField;
    [SerializeField] private TMP_Text playerScoreField;
    [SerializeField] private Image playerCharacterIcon;
    [SerializeField] private Image playerAttackTypeIcon;

    [Header("Colors")]
    [SerializeField] private Color playerColor = Color.yellow;
    [SerializeField] private Color defaultNameColor = Color.white;
    [SerializeField] private Color transparentColor = new Color(1, 1, 1, 0);
    [SerializeField] private Color visibleColor = Color.white;

    private Dictionary<string, LeaderboardEntryData> leaderboardCache;
    private bool uiNeedsUpdate = true;
    private bool refreshInProgress = false;

    [Header("Timer")]
    [SerializeField] private float leaderboardUpdateTime = 90f;
    private float timer = 0f;

    private async void Start()
    {
        await InitializeLeaderboardServicesAsync();
        await RefreshLeaderboardAsync();
    }

    private void Update()
    {
        timer += Time.unscaledDeltaTime;
        if (timer >= leaderboardUpdateTime)
        {
            timer = 0f;
            _ = RefreshLeaderboardAsync();
        }

        if (uiNeedsUpdate)
        {
            UpdateUI();
            uiNeedsUpdate = false;
        }
    }

    public static async Task InitializeLeaderboardServicesAsync()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
            await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async Task RefreshLeaderboardAsync()
    {
        if (refreshInProgress) return;
        refreshInProgress = true;
        try
        {
            leaderboardCache = await GetLeaderboardAsync();
            uiNeedsUpdate = true;
        }
        catch (System.Exception e) { Debug.LogException(e); }
        finally { refreshInProgress = false; }
    }

    // --- FETCH DATA ---
    public async Task<Dictionary<string, LeaderboardEntryData>> GetLeaderboardAsync()
    {
        var result = new Dictionary<string, LeaderboardEntryData>();

        try
        {
            int limit = 100;
            int offset = 0;

            while (true)
            {
                var page = await LeaderboardsService.Instance.GetScoresAsync(
                    leaderboardID,
                    new GetScoresOptions { Limit = limit, Offset = offset, IncludeMetadata = true }
                );

                if (page?.Results == null || page.Results.Count == 0) break;

                foreach (var entry in page.Results)
                {
                    string playerName = entry.PlayerName ?? "Anon";
                    int hashIndex = playerName.IndexOf('#');
                    if (hashIndex > 0) playerName = playerName.Substring(0, hashIndex);

                    int score = (int)entry.Score;
                    Debug.Log($"Oyuncu: {playerName}, Gelen Metadata: {entry.Metadata}");

                    // --- DÜZELTME 2: Standart JsonUtility kullanýmý ---
                    LeaderboardMetadata meta = new LeaderboardMetadata();
                    if (!string.IsNullOrEmpty(entry.Metadata))
                    {
                        // JSON'da "2" string olduðu için Class'ta da string. Artýk okuyabilir.
                        meta = JsonUtility.FromJson<LeaderboardMetadata>(entry.Metadata);
                    }

                    LeaderboardEntryData newData = new LeaderboardEntryData
                    {
                        score = score,
                        metadata = meta
                    };

                    if (result.TryGetValue(playerName, out var oldData))
                    {
                        if (score > oldData.score) result[playerName] = newData;
                    }
                    else
                    {
                        result[playerName] = newData;
                    }
                }

                offset += page.Results.Count;
                if (page.Results.Count < limit) break;
            }
        }
        catch (System.Exception e) { Debug.LogException(e); }

        return result;
    }

    // --- SUBMIT SCORE ---
    public static async Task SubmitScoreAsync(int currentRunScore, int currentRunCharID, int currentRunAtkID)
    {
        try
        {
            await InitializeLeaderboardServicesAsync();

            string name = GameManager.Instance.SaveData.Name;
            if (string.IsNullOrWhiteSpace(name))
                name = $"Player_{UnityEngine.Random.Range(1000, 9999)}";
            await AuthenticationService.Instance.UpdatePlayerNameAsync(name);

            // Metadata hazýrla
            var metadataDict = new Dictionary<string, string>
        {
            { "charID", currentRunCharID.ToString() },
            { "atkID", currentRunAtkID.ToString() }
        };

            var scoreOptions = new AddPlayerScoreOptions { Metadata = metadataDict };

            // Direkt gönder
            await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardID, currentRunScore, scoreOptions);
            Debug.Log($"<color=green>Leaderboard Güncellendi: {currentRunScore}</color>");

        }
        catch (System.Exception e) { Debug.LogException(e); }
    }

    public async Task<bool> CheckForNameAsync(string nameToCheck)
    {
        if (string.IsNullOrWhiteSpace(nameToCheck)) return false;
        if (leaderboardCache == null) await RefreshLeaderboardAsync();
        if (leaderboardCache == null) return false;

        string cleanName = nameToCheck;
        int hashIndex = cleanName.IndexOf('#');
        if (hashIndex > 0) cleanName = cleanName.Substring(0, hashIndex);

        return leaderboardCache.ContainsKey(cleanName);
    }

    // --- UPDATE UI ---
    private void UpdateUI()
    {
        if (leaderboardCache == null || leaderboardCache.Count == 0)
        {
            ClearUI();
            return;
        }

        string playerName = GameManager.Instance.SaveData != null ? GameManager.Instance.SaveData.Name : "";

        var entries = leaderboardCache
            .OrderByDescending(kv => kv.Value.score)
            .ThenBy(kv => kv.Key);

        int maxRows = nameFields.Count;
        ClearUI();

        int idx = 0;
        int rank = 0;
        bool playerFound = false;
        bool playerInRange = false;

        foreach (var kvp in entries)
        {
            rank++;
            string currentName = kvp.Key;
            int currentScore = kvp.Value.score;
            LeaderboardMetadata currentMeta = kvp.Value.metadata;

            // --- 1. LÝSTE SATIRLARI ---
            if (idx < maxRows)
            {
                if (nameFields[idx])
                {
                    nameFields[idx].text = currentName;
                    nameFields[idx].color = (currentName == playerName) ? playerColor : defaultNameColor;
                }
                if (scoreFields[idx]) scoreFields[idx].text = currentScore.ToString();

                // --- DÜZELTME 3: Veri String geldiði için kullanýrken Int'e çeviriyoruz ---
                if (characterIconFields != null && idx < characterIconFields.Count && characterIconFields[idx])
                {
                    int cID = 0;
                    if (currentMeta != null) int.TryParse(currentMeta.charID, out cID);

                    Sprite sprite = iconDatabase.GetCharacterIcon(cID);
                    SetIcon(characterIconFields[idx], sprite);
                }

                if (attackTypeIconFields != null && idx < attackTypeIconFields.Count && attackTypeIconFields[idx])
                {
                    int aID = 0;
                    if (currentMeta != null) int.TryParse(currentMeta.atkID, out aID);

                    Sprite sprite = iconDatabase.GetAttackTypeIcon(aID);
                    SetIcon(attackTypeIconFields[idx], sprite);
                }
            }

            // --- 2. OYUNCU SATIRI ---
            if (currentName == playerName)
            {
                playerFound = true;
                if (idx < maxRows) playerInRange = true;
                else
                {
                    if (playerRowParent) playerRowParent.SetActive(true);
                    if (playerRankField) playerRankField.text = rank.ToString();
                    if (playerNameField)
                    {
                        playerNameField.color = playerColor;
                        playerNameField.text = currentName;
                    }
                    if (playerScoreField) playerScoreField.text = currentScore.ToString();

                    if (playerCharacterIcon)
                    {
                        int cID = 0;
                        if (currentMeta != null) int.TryParse(currentMeta.charID, out cID);

                        Sprite sprite = iconDatabase.GetCharacterIcon(cID);
                        SetIcon(playerCharacterIcon, sprite);
                    }

                    if (playerAttackTypeIcon)
                    {
                        int aID = 0;
                        if (currentMeta != null) int.TryParse(currentMeta.atkID, out aID);

                        Sprite sprite = iconDatabase.GetAttackTypeIcon(aID);
                        SetIcon(playerAttackTypeIcon, sprite);
                    }
                }
            }
            idx++;
        }

        if (playerRowParent && (playerInRange || !playerFound))
            playerRowParent.SetActive(false);
    }

    private void SetIcon(Image img, Sprite sprite)
    {
        if (sprite != null)
        {
            img.sprite = sprite;
            img.color = visibleColor;
        }
        else
        {
            img.color = transparentColor;
        }
    }

    private void ClearUI()
    {
        for (int i = 0; i < nameFields.Count; i++)
        {
            if (nameFields[i]) nameFields[i].text = "";
            if (scoreFields[i]) scoreFields[i].text = "";
            if (characterIconFields != null && i < characterIconFields.Count && characterIconFields[i])
                characterIconFields[i].color = transparentColor;
            if (attackTypeIconFields != null && i < attackTypeIconFields.Count && attackTypeIconFields[i])
                attackTypeIconFields[i].color = transparentColor;
        }
        if (playerRowParent) playerRowParent.SetActive(false);
    }
}