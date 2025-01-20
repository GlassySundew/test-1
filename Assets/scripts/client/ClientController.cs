using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

public class ClientController : MonoBehaviour
{
    [SerializeField] private Transform AbilityBarContainer;

    [Header("Ability button prefabs")]
    [SerializeField] private Button ReloadBtn;

    [Header("Ability button prefabs")]
    [SerializeField] private GameObject AttackPrefab;
    [SerializeField] private GameObject BarrierPrefab;
    [SerializeField] private GameObject RegenerationPrefab;
    [SerializeField] private GameObject FireballPrefab;
    [SerializeField] private GameObject PurifyPrefab;

    [Header("Player UI")]
    [SerializeField] private Slider PlayerHealthSlider;
    [SerializeField] private TextMeshProUGUI PlayerHealthText;
    [SerializeField] private Transform PlayerStatusEffects;

    [Header("Enemy UI")]
    [SerializeField] private Slider EnemyHealthSlider;
    [SerializeField] private TextMeshProUGUI EnemyHealthText;
    [SerializeField] private Transform EnemyStatusEffects;

    private Dictionary<AbilityType, GameObject> AbilityPrefabs;
    private Dictionary<EffectType, GameObject> EffectPrefabs;
    private Dictionary<AbilityType, Button> AbilityButtons = new();
    private Dictionary<EffectType, Button> EffectIcons = new();

    private ServerAdapter ServerAdapter;
    private GameSessionState CurrentSession;

    private void Awake()
    {
        AbilityPrefabs = new Dictionary<AbilityType, GameObject>
            {
                { AbilityType.Attack, AttackPrefab },
                { AbilityType.Barrier, BarrierPrefab },
                { AbilityType.Regeneration, RegenerationPrefab },
                { AbilityType.Fireball, FireballPrefab },
                { AbilityType.Purify, PurifyPrefab }
            };

        EffectPrefabs = new Dictionary<EffectType, GameObject>
            {
                { EffectType.Burn, FireballPrefab },
                { EffectType.Regeneration, RegenerationPrefab },
                { EffectType.Barrier, BarrierPrefab },
            };

        ReloadBtn.onClick.AddListener(() => CreateNewSession());

        ServerAdapter = new ServerAdapter();

        CreateNewSession();
    }

    private void CreateNewSession()
    {
        Debug.Log("creating new session");

        if (CurrentSession != null)
        {
            CurrentSession.OnChanged -= OnCurrentSessionUpdated;
            CurrentSession.Disposed -= CreateNewSession;
        }
        CurrentSession = ServerAdapter.RequestNewGameSession();
        CurrentSession.OnChanged += OnCurrentSessionUpdated;
        CurrentSession.Disposed += CreateNewSession;
        OnCurrentSessionUpdated();
    }

    private void OnCurrentSessionUpdated()
    {
        var player = ServerAdapter.GetCurrentPlayer();
        CreateAbilityButtons(player.Abilities.Keys.ToList());

        var anyEnemy = ServerAdapter.GetAnyEnemy();

        UpdatePlayerHealth(player.CurrentHp, player.MaxHp);
        UpdateEnemyHealth(anyEnemy.CurrentHp, anyEnemy.MaxHp);
        UpdateUI();
    }

    private void CreateAbilityButtons(List<AbilityType> playerAbilities)
    {
        foreach (Transform child in AbilityBarContainer)
        {
            Destroy(child.gameObject);
        }
        AbilityButtons.Clear();

        foreach (AbilityType ability in playerAbilities)
        {
            if (AbilityPrefabs.TryGetValue(ability, out GameObject prefab))
            {
                GameObject buttonObj = Instantiate(prefab, AbilityBarContainer);
                Button button = buttonObj.GetComponent<Button>();

                button.onClick.AddListener(() => OnAbilityUsed(ability));

                AbilityButtons[ability] = button;
            }
            else
            {
                Debug.LogWarning($"Префаб для способности {ability} не найден!");
            }
        }
    }

    private void OnAbilityUsed(AbilityType abilityType)
    {
        if (!ServerAdapter.UseAbility(abilityType))
        {
            Debug.Log("Способность недоступна!");
            return;
        }

        ServerAdapter.EndPlayerTurn();
        UpdateUI();
    }

    private void UpdateUI()
    {
        UpdateAbilityButtons();
        UpdateStatusEffectContainers();
    }

    private void UpdateAbilityButtons()
    {
        foreach (var abilityPair in AbilityButtons)
        {
            AbilityType ability = abilityPair.Key;
            Button button = abilityPair.Value;

            int cooldown = ServerAdapter.GetCurrentPlayer().Abilities[ability].Cooldown;

            // Обновляем текст кулдауна
            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();

            if (cooldown > 0)
            {
                button.interactable = false;
                buttonText.text = $"{cooldown}"; // Показываем оставшиеся ходы
            }
            else
            {
                button.interactable = true;
                buttonText.text = ""; // Очищаем текст, если кулдаун закончился
            }
        }
    }

    private void UpdateStatusEffectContainers()
    {
        void UpdateContainer(Transform container, BattleEntity entity)
        {
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }
            AbilityButtons.Clear();

            foreach (Effect effect in entity.ActiveEffects)
            {
                if (EffectPrefabs.TryGetValue(effect.Type, out GameObject prefab))
                {
                    GameObject buttonObj = Instantiate(prefab, container);
                    Button button = buttonObj.GetComponent<Button>();

                    button.interactable = false;

                    EffectIcons[effect.Type] = button;
                    TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
                    buttonText.text = effect.RemainingTurns.ToString();
                }
            }
        }
        UpdateContainer(PlayerStatusEffects, ServerAdapter.GetCurrentPlayer());
        UpdateContainer(EnemyStatusEffects, ServerAdapter.GetAnyEnemy());
    }

    private void UpdatePlayerHealth(int currentHP, int maxHP)
    {
        PlayerHealthSlider.maxValue = maxHP;
        PlayerHealthSlider.value = currentHP;
        PlayerHealthText.text = $"{currentHP}";
    }

    private void UpdateEnemyHealth(int currentHP, int maxHP)
    {
        EnemyHealthSlider.maxValue = maxHP;
        EnemyHealthSlider.value = currentHP;
        EnemyHealthText.text = $"{currentHP}";
    }
}
