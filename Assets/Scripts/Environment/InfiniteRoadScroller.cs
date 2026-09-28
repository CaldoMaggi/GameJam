using System.Collections.Generic;
using CastleAssault.Art;
using CastleAssault.Core;
using UnityEngine;

namespace CastleAssault.Environment
{
    /// <summary>
    /// Genera y recicla tiles de carretera empedrada, bordes de muro y fondo de tierra
    /// en scroll infinito siguiendo al jugador hacia arriba.
    /// Usa Object Pooling para mantener el rendimiento constante.
    /// </summary>
    public class InfiniteRoadScroller : MonoBehaviour
    {
        [SerializeField] private float _tileHeight = 2.0f;
        [SerializeField] private float _roadWidth = 5.0f;
        [SerializeField] private int _tileBufferCount = 14;
        [SerializeField] private Transform _cameraTransform;

        private readonly List<GameObject> _roadTiles = new List<GameObject>();
        private readonly List<GameObject> _leftWallTiles = new List<GameObject>();
        private readonly List<GameObject> _rightWallTiles = new List<GameObject>();
        private readonly List<GameObject> _bgTiles = new List<GameObject>();

        private float _nextSpawnY;
        private float _lastDestroyY;

        private Sprite _roadSprite;
        private Sprite _wallSprite;
        private Sprite _whitePixel;

        private void Awake()
        {
            _roadSprite = PixelSpriteFactory.RoadSprite;
            _wallSprite = PixelSpriteFactory.WallSprite;
            _whitePixel = PixelSpriteFactory.WhitePixel;
        }

        private void Start()
        {
            if (_cameraTransform == null && Camera.main != null)
                _cameraTransform = Camera.main.transform;

            // Sembrar tiles iniciales cubriendo la vista entera
            float startY = (_cameraTransform != null ? _cameraTransform.position.y : 0f) - _tileHeight * 2;
            _nextSpawnY = startY;
            _lastDestroyY = startY - _tileHeight;

            for (int i = 0; i < _tileBufferCount; i++)
                SpawnRowAt(_nextSpawnY + i * _tileHeight);

            _nextSpawnY += _tileBufferCount * _tileHeight;
        }

        private void Update()
        {
            if (_cameraTransform == null) return;

            float camTop = _cameraTransform.position.y + 7f;
            float camBottom = _cameraTransform.position.y - 7f;

            // Spawnear tiles delante de la cámara
            while (_nextSpawnY < camTop + _tileHeight * 3)
            {
                SpawnRowAt(_nextSpawnY);
                _nextSpawnY += _tileHeight;
            }

            // Reciclar tiles que quedaron muy atrás
            RecycleTilesBehind(camBottom - _tileHeight * 2);
        }

        private void SpawnRowAt(float y)
        {
            // Fondo de tierra a ambos lados del camino
            CreateTile(_bgTiles, -_roadWidth - 1.5f, y, 3.0f, _tileHeight,
                new Color(0.22f, 0.38f, 0.18f), -2, "BG");
            CreateTile(_bgTiles, _roadWidth + 1.5f, y, 3.0f, _tileHeight,
                new Color(0.22f, 0.38f, 0.18f), -2, "BG");

            // Camino de adoquines
            CreateTile(_roadTiles, 0f, y, _roadWidth * 2f, _tileHeight,
                Color.white, -1, "Road", _roadSprite);

            // Muros del castillo a los lados
            CreateTile(_leftWallTiles, -_roadWidth - 0.0f, y, 1.0f, _tileHeight,
                Color.white, 0, "WallL", _wallSprite);
            CreateTile(_rightWallTiles, _roadWidth + 0.0f, y, 1.0f, _tileHeight,
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
