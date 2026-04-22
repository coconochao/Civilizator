using System.Collections.Generic;
using Civilizator.Simulation;
using UnityEngine;

namespace Civilizator.Presentation
{
    /// <summary>
    /// Creates simple grid-aligned placeholder visuals for the map, buildings, natural nodes, and agents.
    /// The component is intentionally lightweight and uses primitive meshes so it can be replaced later
    /// with production art without changing the underlying placement math.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WorldPlaceholderView : MonoBehaviour
    {
        private const string RootName = "World Placeholder View";

        [SerializeField]
        private int _mapWidthTiles = GridPos.MapWidth;

        [SerializeField]
        private int _mapHeightTiles = GridPos.MapHeight;

        [SerializeField]
        private float _tileWorldSize = 1f;

        [SerializeField]
        private int _nodeSeed = 42;

        [SerializeField]
        private int _gridStride = 10;

        private bool _hasBuilt;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureSceneRootExists()
        {
            if (Object.FindAnyObjectByType<WorldPlaceholderView>() != null)
            {
                return;
            }

            GameObject root = new GameObject(RootName);
            root.AddComponent<WorldPlaceholderView>();
        }

        private static readonly BuildingPreview[] DefaultBuildings =
        {
            new BuildingPreview(BuildingKind.Central, new GridPos(48, 48)),
            new BuildingPreview(BuildingKind.House, new GridPos(45, 45)),
            new BuildingPreview(BuildingKind.Tower, new GridPos(52, 45)),
            new BuildingPreview(BuildingKind.Plantation, new GridPos(45, 52)),
            new BuildingPreview(BuildingKind.Farm, new GridPos(52, 52)),
            new BuildingPreview(BuildingKind.CattleFarm, new GridPos(58, 52)),
            new BuildingPreview(BuildingKind.Quarry, new GridPos(52, 58))
        };

        private static readonly AgentPreview[] DefaultAgents =
        {
            new AgentPreview(Profession.Woodcutter, new GridPos(47, 47)),
            new AgentPreview(Profession.Miner, new GridPos(50, 47)),
            new AgentPreview(Profession.Hunter, new GridPos(47, 50)),
            new AgentPreview(Profession.Farmer, new GridPos(50, 50)),
            new AgentPreview(Profession.Builder, new GridPos(49, 53)),
            new AgentPreview(Profession.Soldier, new GridPos(53, 49))
        };

        /// <summary>
        /// Converts a grid tile into a world-space position on the XZ plane.
        /// </summary>
        public static Vector3 TileToWorld(GridPos position, float tileWorldSize = 1f, float y = 0f)
        {
            return new Vector3(position.X * tileWorldSize, y, position.Y * tileWorldSize);
        }

        /// <summary>
        /// Gets the centered world position for a building footprint anchored at a grid tile.
        /// </summary>
        public static Vector3 FootprintCenter(GridPos anchor, int footprintSize, float tileWorldSize = 1f, float y = 0f)
        {
            float centerOffset = (footprintSize - 1) * 0.5f * tileWorldSize;
            return new Vector3(
                anchor.X * tileWorldSize + centerOffset,
                y,
                anchor.Y * tileWorldSize + centerOffset);
        }

        private void Awake()
        {
            BuildIfNeeded();
        }

        private void Start()
        {
            BuildIfNeeded();
        }

        /// <summary>
        /// Rebuilds the placeholder hierarchy if it has not already been created.
        /// </summary>
        public void BuildIfNeeded()
        {
            if (_hasBuilt)
            {
                return;
            }

            _hasBuilt = true;

            ClearChildren(transform);
            CreateFloor();
            CreateRegionGrid();
            CreateNaturalNodes();
            CreateBuildings();
            CreateAgents();
        }

        private void CreateFloor()
        {
            Vector3 center = FootprintCenter(
                new GridPos(0, 0),
                _mapWidthTiles,
                _tileWorldSize,
                -0.06f);

            GameObject floor = CreatePrimitive(
                "Floor",
                PrimitiveType.Cube,
                new Vector3(_mapWidthTiles * _tileWorldSize, 0.12f, _mapHeightTiles * _tileWorldSize),
                center,
                new Color(0.16f, 0.22f, 0.17f));

            floor.transform.SetParent(transform, false);
        }

        private void CreateRegionGrid()
        {
            if (_gridStride <= 0)
            {
                return;
            }

            float verticalLength = _mapHeightTiles * _tileWorldSize;
            float horizontalLength = _mapWidthTiles * _tileWorldSize;
            float lineHeight = 0.03f;
            float lineThickness = 0.05f;
            Color lineColor = new Color(0.31f, 0.43f, 0.36f);

            for (int x = 0; x <= _mapWidthTiles; x += _gridStride)
            {
                Vector3 position = new Vector3(x * _tileWorldSize, 0.02f, verticalLength * 0.5f);
                GameObject line = CreatePrimitive(
                    $"GridLine_X{x}",
                    PrimitiveType.Cube,
                    new Vector3(lineThickness, lineHeight, verticalLength),
                    position,
                    lineColor);
                line.transform.SetParent(transform, false);
            }

            for (int y = 0; y <= _mapHeightTiles; y += _gridStride)
            {
                Vector3 position = new Vector3(horizontalLength * 0.5f, 0.02f, y * _tileWorldSize);
                GameObject line = CreatePrimitive(
                    $"GridLine_Y{y}",
                    PrimitiveType.Cube,
                    new Vector3(horizontalLength, lineHeight, lineThickness),
                    position,
                    lineColor);
                line.transform.SetParent(transform, false);
            }
        }

        private void CreateNaturalNodes()
        {
            List<NaturalNode> nodes = WorldGenerator.GenerateNodes(_nodeSeed);
            Transform nodeRoot = CreateChildRoot("Natural Nodes");

            foreach (NaturalNode node in nodes)
            {
                GameObject marker = CreatePrimitive(
                    $"{node.Type}_Node_{node.Position.X}_{node.Position.Y}",
                    PrimitiveType.Sphere,
                    new Vector3(0.34f, 0.34f, 0.34f),
                    TileToWorld(node.Position, _tileWorldSize, 0.18f),
                    GetNodeColor(node.Type));

                marker.transform.SetParent(nodeRoot, false);
            }
        }

        private void CreateBuildings()
        {
            Transform buildingRoot = CreateChildRoot("Buildings");

            foreach (BuildingPreview building in DefaultBuildings)
            {
                int footprintSize = BuildingKindHelpers.GetFootprintSize(building.Kind);
                Vector3 footprintCenter = FootprintCenter(building.Anchor, footprintSize, _tileWorldSize, 0.0f);

                GameObject basePlate = CreatePrimitive(
                    $"{building.Kind}_Footprint",
                    PrimitiveType.Cube,
                    new Vector3(
                        footprintSize * _tileWorldSize * 0.94f,
                        0.08f,
                        footprintSize * _tileWorldSize * 0.94f),
                    footprintCenter + new Vector3(0f, 0.04f, 0f),
                    GetBuildingColor(building.Kind, true));
                basePlate.transform.SetParent(buildingRoot, false);

                float height = GetBuildingHeight(building.Kind);
                GameObject body = CreatePrimitive(
                    $"{building.Kind}_Body",
                    PrimitiveType.Cube,
                    new Vector3(
                        footprintSize * _tileWorldSize * 0.62f,
                        height,
                        footprintSize * _tileWorldSize * 0.62f),
                    footprintCenter + new Vector3(0f, height * 0.5f + 0.08f, 0f),
                    GetBuildingColor(building.Kind, false));
                body.transform.SetParent(buildingRoot, false);
            }
        }

        private void CreateAgents()
        {
            Transform agentRoot = CreateChildRoot("Agents");

            foreach (AgentPreview agent in DefaultAgents)
            {
                GameObject marker = CreatePrimitive(
                    $"{agent.Profession}_Agent",
                    PrimitiveType.Capsule,
                    new Vector3(0.34f, 0.72f, 0.34f),
                    TileToWorld(agent.Position, _tileWorldSize, 0.36f),
                    GetProfessionColor(agent.Profession));
                marker.transform.SetParent(agentRoot, false);
            }
        }

        private Transform CreateChildRoot(string childName)
        {
            Transform existing = transform.Find(childName);
            if (existing != null)
            {
                ClearChildren(existing);
                return existing;
            }

            GameObject child = new GameObject(childName);
            child.transform.SetParent(transform, false);
            return child.transform;
        }

        private static void ClearChildren(Transform parent)
        {
            List<GameObject> children = new List<GameObject>(parent.childCount);
            for (int i = 0; i < parent.childCount; i++)
            {
                children.Add(parent.GetChild(i).gameObject);
            }

            for (int i = 0; i < children.Count; i++)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(children[i]);
                }
                else
                {
                    Object.DestroyImmediate(children[i]);
                }
            }
        }

        private static GameObject CreatePrimitive(string name, PrimitiveType type, Vector3 scale, Vector3 position, Color color)
        {
            GameObject primitive = GameObject.CreatePrimitive(type);
            primitive.name = name;
            primitive.transform.position = position;
            primitive.transform.localScale = scale;

            Collider collider = primitive.GetComponent<Collider>();
            if (collider != null)
            {
                Object.Destroy(collider);
            }

            Renderer renderer = primitive.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = color;
            }

            return primitive;
        }

        private static Color GetNodeColor(NaturalNodeType type)
        {
            return type switch
            {
                NaturalNodeType.Tree => new Color(0.23f, 0.56f, 0.26f),
                NaturalNodeType.Plant => new Color(0.72f, 0.67f, 0.28f),
                NaturalNodeType.Animal => new Color(0.65f, 0.44f, 0.31f),
                NaturalNodeType.Ore => new Color(0.56f, 0.58f, 0.63f),
                _ => Color.white
            };
        }

        private static Color GetBuildingColor(BuildingKind kind, bool footprint)
        {
            return kind switch
            {
                BuildingKind.Central => footprint ? new Color(0.67f, 0.58f, 0.24f) : new Color(0.92f, 0.82f, 0.33f),
                BuildingKind.House => footprint ? new Color(0.34f, 0.46f, 0.67f) : new Color(0.62f, 0.76f, 0.93f),
                BuildingKind.Tower => footprint ? new Color(0.57f, 0.24f, 0.28f) : new Color(0.84f, 0.35f, 0.39f),
                BuildingKind.Plantation => footprint ? new Color(0.31f, 0.47f, 0.24f) : new Color(0.48f, 0.72f, 0.31f),
                BuildingKind.Farm => footprint ? new Color(0.56f, 0.49f, 0.25f) : new Color(0.89f, 0.78f, 0.34f),
                BuildingKind.CattleFarm => footprint ? new Color(0.46f, 0.32f, 0.24f) : new Color(0.76f, 0.54f, 0.39f),
                BuildingKind.Quarry => footprint ? new Color(0.35f, 0.37f, 0.41f) : new Color(0.64f, 0.67f, 0.72f),
                _ => Color.white
            };
        }

        private static Color GetProfessionColor(Profession profession)
        {
            return profession switch
            {
                Profession.Woodcutter => new Color(0.24f, 0.62f, 0.35f),
                Profession.Miner => new Color(0.56f, 0.45f, 0.29f),
                Profession.Hunter => new Color(0.65f, 0.33f, 0.29f),
                Profession.Farmer => new Color(0.84f, 0.73f, 0.28f),
                Profession.Builder => new Color(0.34f, 0.54f, 0.80f),
                Profession.Soldier => new Color(0.72f, 0.24f, 0.31f),
                _ => Color.white
            };
        }

        private static float GetBuildingHeight(BuildingKind kind)
        {
            return kind switch
            {
                BuildingKind.Central => 1.2f,
                BuildingKind.House => 0.7f,
                BuildingKind.Tower => 1.8f,
                BuildingKind.Plantation => 0.55f,
                BuildingKind.Farm => 0.55f,
                BuildingKind.CattleFarm => 0.55f,
                BuildingKind.Quarry => 0.65f,
                _ => 0.6f
            };
        }

        private readonly struct BuildingPreview
        {
            public BuildingPreview(BuildingKind kind, GridPos anchor)
            {
                Kind = kind;
                Anchor = anchor;
            }

            public BuildingKind Kind { get; }
            public GridPos Anchor { get; }
        }

        private readonly struct AgentPreview
        {
            public AgentPreview(Profession profession, GridPos position)
            {
                Profession = profession;
                Position = position;
            }

            public Profession Profession { get; }
            public GridPos Position { get; }
        }
    }
}
