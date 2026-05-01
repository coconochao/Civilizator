using System;
using System.Collections.Generic;
using Civilizator.Presentation;
using Civilizator.Simulation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Civilizator.UI
{
    /// <summary>
    /// Creates the runtime HUD for SampleScene if the scene does not already contain one.
    /// The overlay includes the policy controls and diagnostics that the V1 docs describe.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RuntimeHudBootstrap : MonoBehaviour
    {
        private const string RootObjectName = "Civilizator Runtime HUD";
        private const string EventSystemObjectName = "Civilizator EventSystem";
        private const float PanelWidth = 540f;
        private const float PanelTopMargin = 16f;
        private const float PanelBottomMargin = 16f;
        private const float PanelSideMargin = 16f;
        private const float RowSpacing = 8f;

        private static readonly Profession[] ProducerProfessions =
        {
            Profession.Woodcutter,
            Profession.Miner,
            Profession.Hunter,
            Profession.Farmer
        };

        private static readonly string[] ProfessionLabels =
        {
            "Woodcutter",
            "Miner",
            "Hunter",
            "Farmer",
            "Builder",
            "Soldier"
        };

        private static readonly Color PanelColor = new Color(0.07f, 0.09f, 0.12f, 0.94f);
        private static readonly Color SubPanelColor = new Color(0.12f, 0.15f, 0.20f, 0.88f);
        private static readonly Color AccentColor = new Color(0.22f, 0.48f, 0.76f, 1f);
        private static readonly Color TextColor = new Color(0.96f, 0.97f, 0.99f, 1f);
        private static readonly Color DimTextColor = new Color(0.73f, 0.79f, 0.86f, 1f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureSceneHudExists()
        {
            if (FindAnyObjectByType<RuntimeHudBootstrap>() != null)
            {
                return;
            }

            var root = new GameObject(RootObjectName);
            root.AddComponent<RuntimeHudBootstrap>();
        }

        private SimulationTickDriver _driver;
        private World _world;
        private SimulationFacade _facade;

        private RectTransform _uiRoot;
        private bool _isBuilt;
        private bool _isBindingControls;

        private Text _statusText;
        private Text _stocksText;
        private Text _productionText;
        private Text _populationText;
        private Text _housingText;
        private Text _activityText;
        private Text _productivityText;

        private readonly List<ProfessionTargetRow> _professionTargetRows = new List<ProfessionTargetRow>();
        private readonly List<ProducerThresholdRow> _producerThresholdRows = new List<ProducerThresholdRow>();

        private Slider _reproductionSlider;
        private Text _reproductionValueText;
        private Slider _soldierPatrolSlider;
        private Text _soldierPatrolValueText;
        private Slider _towerEmphasisSlider;
        private Text _towerEmphasisValueText;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            BuildIfNeeded();
        }

        private void LateUpdate()
        {
            EnsureDriverBinding();
            RefreshDiagnostics();
        }

        private void BuildIfNeeded()
        {
            if (_isBuilt)
            {
                return;
            }

            _isBuilt = true;

            EnsureCanvasAndEventSystem();

            if (_uiRoot == null)
            {
                var uiRootObject = new GameObject("Civilizator UI Root", typeof(RectTransform));
                uiRootObject.transform.SetParent(transform, false);
                _uiRoot = uiRootObject.GetComponent<RectTransform>();
                _uiRoot.anchorMin = Vector2.zero;
                _uiRoot.anchorMax = Vector2.one;
                _uiRoot.offsetMin = Vector2.zero;
                _uiRoot.offsetMax = Vector2.zero;
            }

            var canvas = _uiRoot.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = _uiRoot.gameObject.AddComponent<Canvas>();
            }
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;

            var scaler = _uiRoot.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = _uiRoot.gameObject.AddComponent<CanvasScaler>();
            }
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            if (_uiRoot.GetComponent<GraphicRaycaster>() == null)
            {
                _uiRoot.gameObject.AddComponent<GraphicRaycaster>();
            }

            var panel = CreatePanel(_uiRoot, "HUD Panel", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(PanelSideMargin, PanelBottomMargin), new Vector2(PanelWidth + PanelSideMargin, -PanelTopMargin), PanelColor);
            var panelLayout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            panelLayout.padding = new RectOffset(16, 16, 16, 16);
            panelLayout.spacing = 10f;
            panelLayout.childAlignment = TextAnchor.UpperLeft;
            panelLayout.childControlWidth = true;
            panelLayout.childControlHeight = true;
            panelLayout.childForceExpandWidth = true;
            panelLayout.childForceExpandHeight = false;
            CreateTitle(panel, "Civilizator V1");
            _statusText = CreateBodyText(panel, "Waiting for simulation...");

            var content = CreatePanel(panel, "HUD Content", new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero, Color.clear);
            var contentLayout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            contentLayout.padding = new RectOffset(0, 0, 0, 0);
            contentLayout.spacing = 8f;
            contentLayout.childAlignment = TextAnchor.UpperLeft;
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = true;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;

            var contentFitter = content.gameObject.AddComponent<ContentSizeFitter>();
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var contentLayoutElement = content.gameObject.AddComponent<LayoutElement>();
            contentLayoutElement.flexibleHeight = 1f;
            contentLayoutElement.minHeight = 0f;

            CreateSectionHeader(content, "Player Controls");
            CreateProfessionTargetsSection(content);
            CreateSingleSliderSection(content, "Reproduction Rate", 0f, 1f, value => ApplyReproductionRate(value), value => $"Rate: {FormatPercent(value)}", out _reproductionSlider, out _reproductionValueText);
            CreateSingleSliderSection(content, "Soldier Patrol Share", 0f, 1f, value => ApplySoldierControl(ControlTarget.PatrolShare, value), value => $"Patrol: {FormatPercent(value)}", out _soldierPatrolSlider, out _soldierPatrolValueText);
            CreateSingleSliderSection(content, "Tower Build Emphasis", 0f, 1f, value => ApplySoldierControl(ControlTarget.TowerEmphasis, value), value => $"Tower emphasis: {FormatPercent(value)}", out _towerEmphasisSlider, out _towerEmphasisValueText);
            CreateProducerThresholdSection(content);

            CreateSectionHeader(content, "Diagnostics");
            _stocksText = CreateMetricBlock(content, "Central Stocks");
            _productionText = CreateMetricBlock(content, "Production Rates");
            _populationText = CreateMetricBlock(content, "Population");
            _housingText = CreateMetricBlock(content, "Housing");
            _activityText = CreateMetricBlock(content, "Activity");
            _productivityText = CreateMetricBlock(content, "Productivity");
        }

        private void EnsureCanvasAndEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var eventSystemObject = new GameObject(EventSystemObjectName);
                eventSystemObject.AddComponent<EventSystem>();
                eventSystemObject.AddComponent<InputSystemUIInputModule>();
                DontDestroyOnLoad(eventSystemObject);
            }
        }

        private void EnsureDriverBinding()
        {
            if (_driver == null)
            {
                _driver = FindAnyObjectByType<SimulationTickDriver>();
            }

            if (_driver == null || _driver.World == null)
            {
                return;
            }

            if (_world != _driver.World)
            {
                BindWorld(_driver.World);
            }
        }

        private void BindWorld(World world)
        {
            _world = world;
            _facade = world != null ? new SimulationFacade(world) : null;

            if (_world == null)
            {
                return;
            }

            SyncControlsFromWorld();
            RefreshDiagnostics();
        }

        private void SyncControlsFromWorld()
        {
            if (_world == null)
            {
                return;
            }

            _isBindingControls = true;
            try
            {
                float[] targets = _world.ProfessionTargets.GetTargetsCopy();
                for (int i = 0; i < _professionTargetRows.Count && i < targets.Length; i++)
                {
                    _professionTargetRows[i].Slider.SetValueWithoutNotify(targets[i]);
                    _professionTargetRows[i].ValueText.text =
                        $"{ProfessionLabels[i]} target: {FormatPercent(targets[i])}";
                }

                _reproductionSlider.SetValueWithoutNotify(_world.ReproductionSettings.ReproductionRate);
                _reproductionValueText.text = $"Rate: {FormatPercent(_world.ReproductionSettings.ReproductionRate)}";

                _soldierPatrolSlider.SetValueWithoutNotify(_world.SoldierControls.PatrolTargetShare);
                _soldierPatrolValueText.text = $"Patrol: {FormatPercent(_world.SoldierControls.PatrolTargetShare)}";

                _towerEmphasisSlider.SetValueWithoutNotify(_world.SoldierControls.TowerBuildEmphasis);
                _towerEmphasisValueText.text = $"Tower emphasis: {FormatPercent(_world.SoldierControls.TowerBuildEmphasis)}";

                for (int i = 0; i < _producerThresholdRows.Count; i++)
                {
                    Profession profession = _producerThresholdRows[i].Profession;
                    float start = ProducerThresholds.GetStartThreshold(profession);
                    float stop = ProducerThresholds.GetStopThreshold(profession);
                    _producerThresholdRows[i].StartSlider.SetValueWithoutNotify(start);
                    _producerThresholdRows[i].StopSlider.SetValueWithoutNotify(stop);
                    _producerThresholdRows[i].ValueText.text =
                        $"{ProfessionLabels[(int)profession]} thresholds: start {FormatPercent(start)}, stop {FormatPercent(stop)}";
                }
            }
            finally
            {
                _isBindingControls = false;
            }
        }

        private void RefreshDiagnostics()
        {
            if (_world == null || _facade == null)
            {
                if (_statusText != null)
                {
                    _statusText.text = "Waiting for SimulationTickDriver...";
                }

                return;
            }

            if (_statusText != null)
            {
                string gameOverText = _facade.IsGameOver ? $"Game over: {_facade.GameOverReason}" : "Running";
                _statusText.text =
                    $"Cycle: {_facade.CurrentCycle}\n" +
                    $"Elapsed: {_facade.TotalSimulationSeconds:0.##} s\n" +
                    $"{gameOverText}";
            }

            if (_stocksText != null)
            {
                _stocksText.text = CentralStockDisplay.CentralStockDisplayFormatter.Format(_facade.CentralStocks);
            }

            if (_productionText != null)
            {
                ProductionRateDisplay.ProductionRateSnapshot rateSnapshot = BuildProductionRateSnapshot();
                _productionText.text = ProductionRateDisplay.ProductionRateDisplayFormatter.Format(rateSnapshot);
            }

            if (_populationText != null)
            {
                var population = _facade.PopulationByStage;
                _populationText.text = PopulationDisplay.PopulationDisplayFormatter.Format(
                    population.children,
                    population.adults,
                    population.elders);
            }

            if (_housingText != null)
            {
                var housing = _facade.HousingStats;
                _housingText.text = HousingDisplay.HousingDisplayFormatter.Format(
                    housing.assignedAdults,
                    housing.unassignedAdults);
            }

            if (_activityText != null)
            {
                var snapshot = ActivityBreakdownDisplay.ActivitySnapshot.FromWorld(
                    _world.Agents,
                    _world.NaturalNodes,
                    _world.Buildings,
                    _world.Storage,
                    _world.ProfessionTargets,
                    1000);
                _activityText.text = ActivityBreakdownDisplay.ActivityBreakdownDisplayFormatter.Format(snapshot);
            }

            if (_productivityText != null)
            {
                var snapshot = ProductivityDisplay.ProductivitySnapshot.FromAgents(_world.Agents);
                _productivityText.text = ProductivityDisplay.ProductivityDisplayFormatter.Format(snapshot);
            }
        }

        private ProductionRateDisplay.ProductionRateSnapshot BuildProductionRateSnapshot()
        {
            float woodcutter = 0f;
            float miner = 0f;
            float hunter = 0f;
            float farmer = 0f;

            foreach (Agent agent in _world.Agents)
            {
                if (agent == null || !agent.IsAlive || !ProductionSystem.IsProducerProfession(agent.Profession))
                {
                    continue;
                }

                NaturalNode nearestNode = ProductionSystem.FindNearestRelevantNode(agent, _world.NaturalNodes);
                int currentStock = _world.Storage.GetStock(ProductionSystem.GetRequiredResourceForProfession(agent.Profession));
                bool shouldImprove = ProductionSystem.ShouldSwitchToImprovement(agent, nearestNode, currentStock, 1000);
                if (shouldImprove)
                {
                    continue;
                }

                float rate = agent.GetProductivityMultiplier() * SimulationClock.SecondsPerCycle;
                switch (agent.Profession)
                {
                    case Profession.Woodcutter:
                        woodcutter += rate;
                        break;
                    case Profession.Miner:
                        miner += rate;
                        break;
                    case Profession.Hunter:
                        hunter += rate;
                        break;
                    case Profession.Farmer:
                        farmer += rate;
                        break;
                }
            }

            float overall = woodcutter + miner + hunter + farmer;
            return new ProductionRateDisplay.ProductionRateSnapshot(overall, woodcutter, miner, hunter, farmer);
        }

        private void ApplyReproductionRate(float value)
        {
            if (_world == null || _isBindingControls)
            {
                return;
            }

            _world.ReproductionSettings.ReproductionRate = value;
            _world.ApplyReproductionSettings();
            _reproductionValueText.text = $"Rate: {FormatPercent(_world.ReproductionSettings.ReproductionRate)}";
        }

        private void ApplySoldierControl(ControlTarget target, float value)
        {
            if (_world == null || _isBindingControls)
            {
                return;
            }

            switch (target)
            {
                case ControlTarget.PatrolShare:
                    _world.SoldierControls.PatrolTargetShare = value;
                    break;
                case ControlTarget.TowerEmphasis:
                    _world.SoldierControls.TowerBuildEmphasis = value;
                    break;
            }

            _world.ApplySoldierControls();
            _soldierPatrolValueText.text = $"Patrol: {FormatPercent(_world.SoldierControls.PatrolTargetShare)}";
            _towerEmphasisValueText.text = $"Tower emphasis: {FormatPercent(_world.SoldierControls.TowerBuildEmphasis)}";
        }

        private void ApplyProfessionTarget(int professionIndex, float value)
        {
            if (_world == null || _isBindingControls)
            {
                return;
            }

            var targets = _world.ProfessionTargets.GetTargetsCopy();
            targets[professionIndex] = value;
            _world.ProfessionTargets.SetTargets(targets);
            SyncControlsFromWorld();
        }

        private void ApplyProducerThreshold(Profession profession)
        {
            if (_world == null || _isBindingControls)
            {
                return;
            }

            ProducerThresholdRow row = FindProducerThresholdRow(profession);
            if (row == null)
            {
                return;
            }

            float start = row.StartSlider.value;
            float stop = row.StopSlider.value;
            if (start >= stop)
            {
                if (ReferenceEquals(row.LastEditedSlider, row.StartSlider))
                {
                    stop = Mathf.Clamp(start + 0.01f, 0f, 1f);
                }
                else
                {
                    start = Mathf.Clamp(stop - 0.01f, 0f, 1f);
                }
            }

            ProducerThresholds.SetThresholds(profession, start, stop);
            row.StartSlider.SetValueWithoutNotify(start);
            row.StopSlider.SetValueWithoutNotify(stop);
            row.ValueText.text =
                $"{ProfessionLabels[(int)profession]} thresholds: start {FormatPercent(start)}, stop {FormatPercent(stop)}";
        }

        private ProducerThresholdRow FindProducerThresholdRow(Profession profession)
        {
            foreach (ProducerThresholdRow row in _producerThresholdRows)
            {
                if (row.Profession == profession)
                {
                    return row;
                }
            }

            return null;
        }

        private void CreateProfessionTargetsSection(Transform parent)
        {
            CreateSectionHeader(parent, "Profession Targets");

            for (int i = 0; i < ProfessionLabels.Length; i++)
            {
                int professionIndex = i;
                var row = new ProfessionTargetRow();
                row.Container = CreateSubPanel(parent, $"{ProfessionLabels[i]} Target Row");
                CreateRowTitle(row.Container, ProfessionLabels[i]);
                row.Slider = CreateSlider(row.Container, 0f, 1f, value =>
                {
                    row.ValueText.text = $"{ProfessionLabels[professionIndex]} target: {FormatPercent(value)}";
                    ApplyProfessionTarget(professionIndex, value);
                });
                row.ValueText = CreateRowValueText(row.Container, $"{ProfessionLabels[i]} target: 0%");
                _professionTargetRows.Add(row);
            }
        }

        private void CreateProducerThresholdSection(Transform parent)
        {
            CreateSectionHeader(parent, "Producer Thresholds");

            for (int i = 0; i < ProducerProfessions.Length; i++)
            {
                Profession profession = ProducerProfessions[i];
                var row = new ProducerThresholdRow(profession);
                row.Container = CreateSubPanel(parent, $"{ProfessionLabels[(int)profession]} Threshold Row");
                CreateRowTitle(row.Container, ProfessionLabels[(int)profession]);

                row.StartSlider = CreateSlider(row.Container, 0f, 1f, value =>
                {
                    row.LastEditedSlider = row.StartSlider;
                    row.ValueText.text = $"{ProfessionLabels[(int)profession]} thresholds: start {FormatPercent(value)}, stop {FormatPercent(row.StopSlider.value)}";
                    ApplyProducerThreshold(profession);
                });
                row.StartSlider.gameObject.name = $"{ProfessionLabels[(int)profession]} Start Slider";

                row.StopSlider = CreateSlider(row.Container, 0f, 1f, value =>
                {
                    row.LastEditedSlider = row.StopSlider;
                    row.ValueText.text = $"{ProfessionLabels[(int)profession]} thresholds: start {FormatPercent(row.StartSlider.value)}, stop {FormatPercent(value)}";
                    ApplyProducerThreshold(profession);
                });
                row.StopSlider.gameObject.name = $"{ProfessionLabels[(int)profession]} Stop Slider";

                row.ValueText = CreateRowValueText(row.Container, $"{ProfessionLabels[(int)profession]} thresholds: start 0%, stop 0%");
                _producerThresholdRows.Add(row);
            }
        }

        private void CreateSingleSliderSection(
            Transform parent,
            string title,
            float minValue,
            float maxValue,
            Action<float> onChanged,
            Func<float, string> valueFormatter,
            out Slider slider,
            out Text valueText)
        {
            var container = CreateSubPanel(parent, title + " Row");
            CreateRowTitle(container, title);
            Text localValueText = null;
            slider = CreateSlider(container, minValue, maxValue, value =>
            {
                if (localValueText != null)
                {
                    localValueText.text = valueFormatter(value);
                }

                onChanged?.Invoke(value);
            });
            localValueText = CreateRowValueText(container, valueFormatter(slider.value));
            valueText = localValueText;
        }

        private void CreateSectionHeader(Transform parent, string title)
        {
            var header = CreateText(parent, title, 19, FontStyle.Bold, AccentColor);
            header.alignment = TextAnchor.UpperLeft;
        }

        private Text CreateMetricBlock(Transform parent, string title)
        {
            var container = CreateSubPanel(parent, title + " Block");
            CreateRowTitle(container, title);
            var text = CreateBodyText(container, string.Empty);
            text.text = "Waiting for simulation...";
            return text;
        }

        private void CreateTitle(Transform parent, string title)
        {
            var text = CreateText(parent, title, 22, FontStyle.Bold, TextColor);
            text.alignment = TextAnchor.UpperLeft;
        }

        private void CreateRowTitle(Transform parent, string title)
        {
            var text = CreateText(parent, title, 16, FontStyle.Bold, TextColor);
            text.alignment = TextAnchor.UpperLeft;
        }

        private Text CreateRowValueText(Transform parent, string value)
        {
            var text = CreateText(parent, value, 14, FontStyle.Normal, DimTextColor);
            text.alignment = TextAnchor.UpperLeft;
            return text;
        }

        private Text CreateBodyText(Transform parent, string value)
        {
            var text = CreateText(parent, value, 14, FontStyle.Normal, TextColor);
            text.alignment = TextAnchor.UpperLeft;
            return text;
        }

        private Text CreateText(Transform parent, string value, int fontSize, FontStyle style, Color color)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);

            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;

            var rect = text.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0f, fontSize * 1.5f);

            var layoutElement = go.AddComponent<LayoutElement>();
            layoutElement.minHeight = fontSize * 1.25f;
            layoutElement.preferredHeight = fontSize * 1.5f;
            layoutElement.flexibleWidth = 1f;
            return text;
        }

        private Slider CreateSlider(Transform parent, float minValue, float maxValue, Action<float> onChanged)
        {
            var sliderRoot = new GameObject("Slider", typeof(RectTransform), typeof(Slider), typeof(Image));
            sliderRoot.transform.SetParent(parent, false);

            var rootRect = sliderRoot.GetComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(0f, 24f);

            var layoutElement = sliderRoot.AddComponent<LayoutElement>();
            layoutElement.minHeight = 24f;
            layoutElement.preferredHeight = 24f;
            layoutElement.flexibleWidth = 1f;

            var background = sliderRoot.GetComponent<Image>();
            background.color = new Color(0.17f, 0.19f, 0.24f, 1f);

            var slider = sliderRoot.GetComponent<Slider>();
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.wholeNumbers = false;

            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderRoot.transform, false);
            var fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0.05f, 0.25f);
            fillAreaRect.anchorMax = new Vector2(0.95f, 0.75f);
            fillAreaRect.offsetMin = Vector2.zero;
            fillAreaRect.offsetMax = Vector2.zero;

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            var fillImage = fill.GetComponent<Image>();
            fillImage.color = AccentColor;
            var fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            var handleSlideArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleSlideArea.transform.SetParent(sliderRoot.transform, false);
            var handleSlideRect = handleSlideArea.GetComponent<RectTransform>();
            handleSlideRect.anchorMin = new Vector2(0f, 0f);
            handleSlideRect.anchorMax = new Vector2(1f, 1f);
            handleSlideRect.offsetMin = new Vector2(8f, 0f);
            handleSlideRect.offsetMax = new Vector2(-8f, 0f);

            var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(handleSlideArea.transform, false);
            var handleImage = handle.GetComponent<Image>();
            handleImage.color = new Color(0.95f, 0.96f, 0.98f, 1f);
            var handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(18f, 18f);

            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImage;
            slider.direction = Slider.Direction.LeftToRight;

            slider.onValueChanged.AddListener(value => onChanged?.Invoke(value));

            return slider;
        }

        private ScrollRectHandle CreateScrollView(Transform parent, string name)
        {
            var root = CreatePanel(parent, name, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, -70f), new Vector2(-PanelSideMargin, -PanelBottomMargin), SubPanelColor);
            root.name = name;

            var viewport = CreatePanel(root, "Viewport", new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(8f, 8f), new Vector2(-8f, -8f), Color.clear);
            var mask = viewport.gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            var content = CreatePanel(viewport, "Content", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector2(0f, 0f), Color.clear);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.anchoredPosition = Vector2.zero;

            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = RowSpacing;

            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scrollRect = root.gameObject.AddComponent<ScrollRect>();
            scrollRect.viewport = viewport;
            scrollRect.content = content;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            var layoutElement = root.gameObject.AddComponent<LayoutElement>();
            layoutElement.flexibleHeight = 1f;
            layoutElement.preferredHeight = 0f;
            layoutElement.minHeight = 160f;

            return new ScrollRectHandle(root, viewport, content, scrollRect);
        }

        private RectTransform CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color backgroundColor)
        {
            var panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);

            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;

            var image = panel.GetComponent<Image>();
            image.color = backgroundColor;

            return rect;
        }

        private RectTransform CreateSubPanel(Transform parent, string name)
        {
            var panel = CreatePanel(parent, name, new Vector2(0f, 0f), new Vector2(1f, 0f), Vector2.zero, Vector2.zero, SubPanelColor);
            var layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 4f;

            var fitter = panel.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var layoutElement = panel.gameObject.AddComponent<LayoutElement>();
            layoutElement.minHeight = 0f;

            return panel;
        }

        private string FormatPercent(float value)
        {
            return $"{value * 100f:0.##}%";
        }

        private enum ControlTarget
        {
            PatrolShare,
            TowerEmphasis
        }

        private sealed class ProfessionTargetRow
        {
            public RectTransform Container;
            public Slider Slider;
            public Text ValueText;
        }

        private sealed class ProducerThresholdRow
        {
            public ProducerThresholdRow(Profession profession)
            {
                Profession = profession;
            }

            public Profession Profession { get; }
            public RectTransform Container;
            public Slider StartSlider;
            public Slider StopSlider;
            public Text ValueText;
            public Slider LastEditedSlider;
        }

        private readonly struct ScrollRectHandle
        {
            public ScrollRectHandle(RectTransform root, RectTransform viewport, RectTransform content, ScrollRect scrollRect)
            {
                Root = root;
                Viewport = viewport;
                Content = content;
                ScrollRect = scrollRect;
            }

            public RectTransform Root { get; }
            public RectTransform Viewport { get; }
            public RectTransform Content { get; }
            public ScrollRect ScrollRect { get; }
        }
    }
}
