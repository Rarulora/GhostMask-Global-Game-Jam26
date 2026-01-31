using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.UI;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using UnityEngine;

// 1. Structure used to convert TO/FROM JSON for Unity Metadata
[System.Serializable]
public class LeaderboardMetadata
{
    public int charID;
    public int atkID;
}

// 2. Internal structure to hold downloaded data in memory
public struct LeaderboardEntryData
{
    public int score;
    public LeaderboardMetadata metadata;
}

public class LeaderboardManager : MonoBehaviour
{
    private const string leaderboardID = "leaderboard"; // Must match dashboard ID

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

    // Cache for downloaded leaderboard data
    private Dictionary<string, LeaderboardEntryData> leaderboardCache;

    [Header("Flags")]
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
        {
            await UnityServices.InitializeAsync();
        }

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
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
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
        finally
        {
            refreshInProgress = false;
        }
    }

    // --- FETCH DATA ---
    public async Task<Dictionary<string, LeaderboardEntryData>> GetLeaderboardAsync()
    {
        var result = new Dictionary<string, LeaderboardEntryData>();

        try
        {
            int limit = 100; // Unity limit per page
            int offset = 0;

            while (true)
            {
                var page = await LeaderboardsService.Instance.GetScoresAsync(
                    leaderboardID,
                    new GetScoresOptions { Limit = limit, Offset = offset }
                );

                if (page?.Results == null || page.Results.Count == 0) break;

                foreach (var entry in page.Results)
                {
                    string playerName = entry.PlayerName ?? "Anonim";
                    // Handle duplicate names with #1234 tags
                    int hashIndex = playerName.IndexOf('#');
                    if (hashIndex > 0) playerName = playerName.Substring(0, hashIndex);

                    int score = (int)entry.Score;

                    // Parse Metadata JSON
                    LeaderboardMetadata meta = new LeaderboardMetadata();
                    if (!string.IsNullOrEmpty(entry.Metadata))
                    {
                        try
                        {
                            meta = JsonUtility.FromJson<LeaderboardMetadata>(entry.Metadata);
                        }
                        catch
                        {
                            Debug.LogWarning($"Metadata JSON error for player: {playerName}");
                        }
                    }

                    LeaderboardEntryData newData = new LeaderboardEntryData
                    {
                        score = score,
                        metadata = meta
                    };

                    // Handle duplicates: Keep the better score if name exists
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
    public static async Task SetNewEntry()
    {
        try
        {
            await InitializeLeaderboardServicesAsync();

            // 1. Get Best Run Data from Save System
            // Make sure GameManager.Instance.SaveData.bestRunData is NOT null before accessing
            var saveData = GameManager.Instance.SaveData;
            if (saveData == null || saveData.bestRunData == null)
            {
                Debug.LogWarning("SaveData or BestRunData is null. Cannot submit score.");
                return;
            }

            HighScoreData bestRun = saveData.bestRunData;

            // 2. Prepare Metadata Package
            LeaderboardMetadata meta = new LeaderboardMetadata
            {
                charID = (int)bestRun.character,   // Cast Enum to Int
                atkID = (int)bestRun.attackType    // Cast Enum to Int
            };
            
            string jsonMeta = JsonUtility.ToJson(meta);

            // 3. Handle Player Name
            string name = saveData.Name;
            if (string.IsNullOrWhiteSpace(name))
                name = $"Player_{UnityEngine.Random.Range(1000, 9999)}";

            await AuthenticationService.Instance.UpdatePlayerNameAsync(name);

            // 4. Submit Score + Metadata
            int score = Mathf.FloorToInt(bestRun.highScore);

            var scoreOptions = new AddPlayerScoreOptions 
            { 
                Metadata = jsonMeta 
            };

            await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardID, score, scoreOptions);
            Debug.Log($"Score Submitted: {score} | CharID: {meta.charID} | AtkID: {meta.atkID}");
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
    }

    // --- UPDATE UI ---
    private void UpdateUI()
    {
        if (leaderboardCache == null || leaderboardCache.Count == 0)
        {
            ClearUI();
            return;
        }

        // Get current player name for highlighting
        string playerName = GameManager.Instance.SaveData != null ? GameManager.Instance.SaveData.Name : "";

        // Sort by Score Descending
        var entries = leaderboardCache
            .OrderByDescending(kv => kv.Value.score)
            .ThenBy(kv => kv.Key);

        int maxRows = nameFields.Count; // Limit by UI slots
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

            // --- FILL LIST ROWS ---
            if (idx < maxRows)
            {
                // Name
                if (nameFields[idx])
                {
                    nameFields[idx].text = currentName;
                    nameFields[idx].color = (currentName == playerName) ? playerColor : defaultNameColor;
                }

                // Score
                if (scoreFields[idx])
                    scoreFields[idx].text = currentScore.ToString();

                // Icons (Safe Check for Nulls)
                if (characterIconFields != null && idx < characterIconFields.Count && characterIconFields[idx])
                {
                    Sprite sprite = (currentMeta != null) ? iconDatabase.GetCharacterIcon(currentMeta.charID) : null;
                    SetIcon(characterIconFields[idx], sprite);
                }

                if (attackTypeIconFields != null && idx < attackTypeIconFields.Count && attackTypeIconFields[idx])
                {
                    Sprite sprite = (currentMeta != null) ? iconDatabase.GetAttackTypeIcon(currentMeta.atkID) : null;
                    SetIcon(attackTypeIconFields[idx], sprite);
                }
            }

            // --- FILL PLAYER ROW (If player is found) ---
            if (currentName == playerName)
            {
                playerFound = true;
                if (idx < maxRows)
                {
                    playerInRange = true; // Player is already shown in the main list
                }
                else
                {
                    // Player is not in the top list, show the separate row
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
                        Sprite sprite = (currentMeta != null) ? iconDatabase.GetCharacterIcon(currentMeta.charID) : null;
                        SetIcon(playerCharacterIcon, sprite);
                    }

                    if (playerAttackTypeIcon)
                    {
                        Sprite sprite = (currentMeta != null) ? iconDatabase.GetAttackTypeIcon(currentMeta.atkID) : null;
                        SetIcon(playerAttackTypeIcon, sprite);
                    }
                }
            }
            idx++;
        }

        // Hide special row if player is in top list or not found at all
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