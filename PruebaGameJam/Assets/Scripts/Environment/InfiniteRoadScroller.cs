using System.Collections.Generic;
using CastleAssault.Art;
using CastleAssault.Core;
using UnityEngine;

namespace CastleAssault.Environment
{
    /// <summary>
    /// Genera y recicla tiles de carretera empedrada, bordes de muro y fondo de tierra
    /// en scroll infinito siguiendo al jugador hacia arriba.
    /// </summary>
    public class InfiniteRoadScroller : MonoBehaviour
    {
        [SerializeField] private float _tileHeight = 2.0f;
        [SerializeField] private float _roadWidth = 2.5f;   // MITAD del ancho del camino
        [SerializeField] private int _tileBufferCount = 14;
        [SerializeField] private Transform _cameraTransform;

        private readonly List<GameObject> _roadTiles = new List<GameObject>();
        private readonly List<GameObject> _leftWallTiles = new List<GameObject>();
        private readonly List<GameObject> _rightWallTiles = new List<GameObject>();
        private readonly List<GameObject> _bgTiles = new List<GameObject>();

        private float _nextSpawnY;

        private Sprite _roadSprite;
        private Sprite _wallSprite;
        private Sprite _whitePixel;

        private void Awake()
        {
            _roadSprite = MakeTileable(PixelSpriteFactory.RoadSprite);
            _wallSprite = MakeTileable(PixelSpriteFactory.WallSprite);

            // Si la fábrica no entregó el sprite, generamos uno propio
            if (_roadSprite == null)
            {
                Debug.LogWarning("RoadSprite es null, usando adoquines generados por el scroller.");
                _roadSprite = BuildFallbackSprite(new Color(0.55f, 0.5f, 0.45f), new Color(0.3f, 0.27f, 0.24f));
            }
            if (_wallSprite == null)
            {
                Debug.LogWarning("WallSprite es null, usando muro generado por el scroller.");
                _wallSprite = BuildFallbackSprite(new Color(0.45f, 0.45f, 0.5f), new Color(0.2f, 0.2f, 0.25f));
            }
        }

        private Sprite BuildFallbackSprite(Color baseColor, Color mortar)
        {
            const int size = 16;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Repeat;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Filas de ladrillos desfasadas
                    bool isMortar = (y % 8 == 0) || (((x + ((y / 8) % 2) * 4) % 8) == 0);
                    float shade = 0.9f + ((x * 7 + y * 13) % 5) * 0.03f;
                    Color c = isMortar
                        ? mortar
                        : new Color(baseColor.r * shade, baseColor.g * shade, baseColor.b * shade, 1f);
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f), 16f, 0, SpriteMeshType.FullRect);
        }

        private void Start()
        {
            if (_cameraTransform == null && Camera.main != null)
                _cameraTransform = Camera.main.transform;

            float startY = (_cameraTransform != null ? _cameraTransform.position.y : 0f) - _tileHeight * 2;
            _nextSpawnY = startY;

            for (int i = 0; i < _tileBufferCount; i++)
                SpawnRowAt(_nextSpawnY + i * _tileHeight);

            _nextSpawnY += _tileBufferCount * _tileHeight;
        }

        private void Update()
        {
            if (_cameraTransform == null) return;

            float camTop = _cameraTransform.position.y + 7f;
            float camBottom = _cameraTransform.position.y - 7f;

            while (_nextSpawnY < camTop + _tileHeight * 3)
            {
                SpawnRowAt(_nextSpawnY);
                _nextSpawnY += _tileHeight;
            }

            RecycleTilesBehind(camBottom - _tileHeight * 2);
        }

        /// <summary>
        /// Recrea el sprite con Mesh Type = FullRect y textura en Repeat,
        /// requisito para SpriteDrawMode.Tiled.
        /// </summary>
        private Sprite MakeTileable(Sprite source)
        {
            if (source == null) return null;

            Texture2D tex = source.texture;
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Point;

            Vector2 pivot = new Vector2(
                source.pivot.x / source.rect.width,
                source.pivot.y / source.rect.height);

            return Sprite.Create(
                tex,
                source.rect,
                pivot,
                source.pixelsPerUnit,
                0,
                SpriteMeshType.FullRect);
        }

        private void SpawnRowAt(float y)
        {
            // Fondo verde a ambos lados del camino
            CreateTile(_bgTiles, -_roadWidth - 1.5f, y, 3.0f, _tileHeight,
                new Color(0.22f, 0.38f, 0.18f), -2, "BG");
            CreateTile(_bgTiles, _roadWidth + 1.5f, y, 3.0f, _tileHeight,
                new Color(0.22f, 0.38f, 0.18f), -2, "BG");

            // Camino de adoquines
            CreateTile(_roadTiles, 0f, y, _roadWidth * 2f, _tileHeight,
                Color.white, -1, "Road", _roadSprite);

            // Muros del castillo a los lados
            CreateTile(_leftWallTiles, -_roadWidth, y, 1.0f, _tileHeight,
                Color.white, 0, "WallL", _wallSprite);
            CreateTile(_rightWallTiles, _roadWidth, y, 1.0f, _tileHeight,
                Color.white, 0, "WallR", _wallSprite);
        }

        private void CreateTile(List<GameObject> pool, float x, float y, float w, float h,
            Color color, int sortOrder, string label, Sprite sprite = null)
        {
            GameObject go = new GameObject(label);
            go.transform.SetParent(transform, false);
            go.transform.position = new Vector3(x, y, 0f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite != null ? sprite : _whitePixel;
            sr.color = color;
            sr.sortingOrder = sortOrder;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(w, h);

            pool.Add(go);
        }

        private void RecycleTilesBehind(float minY)
        {
            RecycleList(_roadTiles, minY);
            RecycleList(_leftWallTiles, minY);
            RecycleList(_rightWallTiles, minY);
            RecycleList(_bgTiles, minY);
        }

        private void RecycleList(List<GameObject> list, float minY)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i] == null)
                {
                    list.RemoveAt(i);
                    continue;
                }
                if (list[i].transform.position.y < minY)
                {
                    Destroy(list[i]);
                    list.RemoveAt(i);
                }
            }
        }
    }
}