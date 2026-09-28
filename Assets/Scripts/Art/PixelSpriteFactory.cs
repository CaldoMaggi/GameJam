using UnityEngine;

namespace CastleAssault.Art
{
    /// <summary>
    /// Genera sprites pixel-art procedurales de alta calidad y estilo medieval en tiempo de ejecución.
    /// Garantiza que el juego no dependa de assets externos y sea 100% autónomo.
    /// </summary>
    public static class PixelSpriteFactory
    {
        private static Sprite _knightSprite;
        private static Sprite _shieldAuraSprite;
        private static Sprite _arrowSprite;
        private static Sprite _rockSprite;
        private static Sprite _oilPuddleSprite;
        private static Sprite _reticleSprite;
        private static Sprite _bannerSprite;
        private static Sprite _heartSprite;
        private static Sprite _shieldIconSprite;
        private static Sprite _roadSprite;
        private static Sprite _wallSprite;
        private static Sprite _whitePixelSprite;

        public static Sprite WhitePixel
        {
            get
            {
                if (_whitePixelSprite == null)
                {
                    Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    Color[] colors = new Color[] { Color.white, Color.white, Color.white, Color.white };
                    tex.SetPixels(colors);
                    tex.filterMode = FilterMode.Point;
                    tex.Apply();
                    _whitePixelSprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 1f);
                }
                return _whitePixelSprite;
            }
        }

        public static Sprite KnightSprite => _knightSprite ??= CreateKnightSprite();
        public static Sprite ShieldAuraSprite => _shieldAuraSprite ??= CreateShieldAuraSprite();
        public static Sprite ArrowSprite => _arrowSprite ??= CreateArrowSprite();
        public static Sprite RockSprite => _rockSprite ??= CreateRockSprite();
        public static Sprite OilPuddleSprite => _oilPuddleSprite ??= CreateOilPuddleSprite();
        public static Sprite ReticleSprite => _reticleSprite ??= CreateReticleSprite();
        public static Sprite BannerSprite => _bannerSprite ??= CreateBannerSprite();
        public static Sprite HeartSprite => _heartSprite ??= CreateHeartSprite();
        public static Sprite ShieldIconSprite => _shieldIconSprite ??= CreateShieldIconSprite();
        public static Sprite RoadSprite => _roadSprite ??= CreateRoadSprite();
        public static Sprite WallSprite => _wallSprite ??= CreateWallSprite();

        private static Sprite CreateSpriteFromTexture(Texture2D tex, float pixelsPerUnit = 16f, Vector2? pivot = null)
        {
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            Vector2 p = pivot ?? new Vector2(0.5f, 0.5f);
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), p, pixelsPerUnit);
        }

        #region Knight Sprite (16x22)
        private static Sprite CreateKnightSprite()
        {
            int w = 16, h = 22;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color clear = Color.clear;
            Color silver = new Color(0.78f, 0.82f, 0.86f);
            Color darkMetal = new Color(0.35f, 0.40f, 0.48f);
            Color visor = new Color(0.12f, 0.14f, 0.18f);
            Color plume = new Color(0.85f, 0.2f, 0.2f); // Cresta roja del casco
            Color cape = new Color(0.2f, 0.35f, 0.75f); // Capa azul real
            Color armorGold = new Color(0.92f, 0.75f, 0.2f);
            Color boots = new Color(0.25f, 0.18f, 0.12f);

            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    tex.SetPixel(x, y, clear);

            // Cresta de plumas (y: 19..21)
            for (int y = 19; y < 22; y++)
            {
                tex.SetPixel(7, y, plume);
                tex.SetPixel(8, y, plume);
            }
            tex.SetPixel(9, 20, plume);

            // Casco metálico (y: 14..18)
            for (int y = 14; y < 19; y++)
                for (int x = 5; x <= 10; x++)
                    tex.SetPixel(x, y, silver);

            // Visera oscura
            for (int x = 6; x <= 9; x++)
                tex.SetPixel(x, 16, visor);
            tex.SetPixel(5, 14, darkMetal);
            tex.SetPixel(10, 14, darkMetal);

            // Torso / Armadura y capa (y: 7..13)
            for (int y = 7; y < 14; y++)
            {
                tex.SetPixel(4, y, cape);
                tex.SetPixel(11, y, cape);

                for (int x = 5; x <= 10; x++)
                {
                    tex.SetPixel(x, y, silver);
                }
            }
            tex.SetPixel(7, 10, armorGold);
            tex.SetPixel(8, 10, armorGold);
            tex.SetPixel(7, 9, armorGold);
            tex.SetPixel(8, 9, armorGold);

            // Escudo en la mano izquierda del sprite (x: 2..4, y: 7..13)
            for (int y = 7; y <= 13; y++)
            {
                for (int x = 2; x <= 4; x++)
                {
                    tex.SetPixel(x, y, armorGold);
                }
            }
            tex.SetPixel(3, 10, cape);

            // Piernas / Botas (y: 0..6)
            for (int y = 0; y < 7; y++)
            {
                Color legColor = (y < 3) ? boots : darkMetal;
                tex.SetPixel(6, y, legColor);
                tex.SetPixel(7, y, legColor);
                tex.SetPixel(8, y, legColor);
                tex.SetPixel(9, y, legColor);
            }

            tex.Apply();
            return CreateSpriteFromTexture(tex, 16f, new Vector2(0.5f, 0.4f));
        }
        #endregion

        #region Shield Aura (32x32)
        private static Sprite CreateShieldAuraSprite()
        {
            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float radius = size * 0.44f;
            Color glowInner = new Color(0.25f, 0.65f, 1f, 0.45f);
            Color glowBorder = new Color(0.7f, 0.9f, 1f, 0.95f);
            Color runeColor = new Color(1f, 0.9f, 0.4f, 0.9f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist <= radius)
                    {
                        if (dist >= radius - 2.5f)
                        {
                            tex.SetPixel(x, y, glowBorder);
                        }
                        else
                        {
                            float alpha = Mathf.Lerp(0.55f, 0.15f, dist / radius);
                            tex.SetPixel(x, y, new Color(glowInner.r, glowInner.g, glowInner.b, alpha));
                        }
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }

            tex.SetPixel(15, 29, runeColor); tex.SetPixel(16, 29, runeColor);
            tex.SetPixel(15, 2, runeColor); tex.SetPixel(16, 2, runeColor);
            tex.SetPixel(2, 15, runeColor); tex.SetPixel(2, 16, runeColor);
            tex.SetPixel(29, 15, runeColor); tex.SetPixel(29, 16, runeColor);

            tex.Apply();
            return CreateSpriteFromTexture(tex, 16f, new Vector2(0.5f, 0.5f));
        }
        #endregion

        #region Arrow Sprite (8x24)
        private static Sprite CreateArrowSprite()
        {
            int w = 8, h = 24;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color clear = Color.clear;
            Color wood = new Color(0.55f, 0.35f, 0.15f);
            Color metal = new Color(0.85f, 0.88f, 0.92f);
            Color darkMetal = new Color(0.4f, 0.45f, 0.5f);
            Color fletching = new Color(0.95f, 0.3f, 0.2f);

            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    tex.SetPixel(x, y, clear);

            // Punta de flecha apuntando hacia abajo (y: 0..4)
            tex.SetPixel(3, 0, metal);
            tex.SetPixel(4, 0, metal);
            for (int x = 2; x <= 5; x++) tex.SetPixel(x, 1, metal);
            for (int x = 1; x <= 6; x++) tex.SetPixel(x, 2, metal);
            for (int x = 2; x <= 5; x++) tex.SetPixel(x, 3, darkMetal);

            // Asta de madera (y: 4..19)
            for (int y = 4; y < 20; y++)
            {
                tex.SetPixel(3, y, wood);
                tex.SetPixel(4, y, wood);
            }

            // Emplumado rojo (y: 19..23)
            for (int y = 19; y < 24; y++)
            {
                tex.SetPixel(2, y, fletching);
                tex.SetPixel(3, y, wood);
                tex.SetPixel(4, y, wood);
                tex.SetPixel(5, y, fletching);
            }
            tex.SetPixel(1, 22, fletching);
            tex.SetPixel(6, 22, fletching);

            tex.Apply();
            return CreateSpriteFromTexture(tex, 16f, new Vector2(0.5f, 0.1f));
        }
        #endregion

        #region Rock / Boulder Sprite (24x24)
        private static Sprite CreateRockSprite()
        {
            int size = 24;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float radius = size * 0.42f;

            Color baseStone = new Color(0.45f, 0.42f, 0.40f);
            Color darkStone = new Color(0.28f, 0.25f, 0.24f);
            Color highlight = new Color(0.68f, 0.65f, 0.62f);
            Color crack = new Color(0.18f, 0.15f, 0.14f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float angle = Mathf.Atan2(y - center.y, x - center.x);
                    float deform = Mathf.Sin(angle * 5f) * 1.5f + Mathf.Cos(angle * 3f) * 1.0f;

                    if (dist <= radius + deform)
                    {
                        if (dist >= radius + deform - 1.5f)
                        {
                            tex.SetPixel(x, y, darkStone);
                        }
                        else if (y > center.y + 2 && x < center.x + 4)
                        {
                            tex.SetPixel(x, y, highlight);
                        }
                        else if (y < center.y - 2)
                        {
                            tex.SetPixel(x, y, darkStone);
                        }
                        else
                        {
                            tex.SetPixel(x, y, baseStone);
                        }
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }

            tex.SetPixel(10, 14, crack);
            tex.SetPixel(11, 13, crack);
            tex.SetPixel(12, 12, crack);
            tex.SetPixel(13, 12, crack);
            tex.SetPixel(14, 11, crack);
            tex.SetPixel(12, 15, crack);

            tex.Apply();
            return CreateSpriteFromTexture(tex, 16f, new Vector2(0.5f, 0.5f));
        }
        #endregion

        #region Oil Puddle Sprite (32x20)
        private static Sprite CreateOilPuddleSprite()
        {
            int w = 32, h = 20;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Vector2 center = new Vector2(w * 0.5f, h * 0.5f);
            Color darkOil = new Color(0.08f, 0.08f, 0.10f, 0.88f);
            Color sheen1 = new Color(0.20f, 0.15f, 0.28f, 0.85f);
            Color sheen2 = new Color(0.35f, 0.28f, 0.15f, 0.85f);
            Color bubble = new Color(0.45f, 0.40f, 0.50f, 0.95f);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float dx = (x - center.x) / (w * 0.45f);
                    float dy = (y - center.y) / (h * 0.45f);
                    float d = dx * dx + dy * dy;

                    if (d <= 1f)
                    {
                        if (d > 0.82f)
                        {
                            tex.SetPixel(x, y, new Color(darkOil.r, darkOil.g, darkOil.b, 0.5f));
                        }
                        else if (y > center.y + 1 && x > center.x - 4 && x < center.x + 8)
                        {
                            tex.SetPixel(x, y, sheen1);
                        }
                        else if (y < center.y && x > center.x - 8 && x < center.x)
                        {
                            tex.SetPixel(x, y, sheen2);
                        }
                        else
                        {
                            tex.SetPixel(x, y, darkOil);
                        }
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }

            tex.SetPixel(12, 10, bubble);
            tex.SetPixel(20, 8, bubble);
            tex.SetPixel(15, 6, bubble);

            tex.Apply();
            return CreateSpriteFromTexture(tex, 16f, new Vector2(0.5f, 0.5f));
        }
        #endregion

        #region Reticle Warning Sprite (24x24)
        private static Sprite CreateReticleSprite()
        {
            int size = 24;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float radius = size * 0.42f;
            Color redRing = new Color(1f, 0.15f, 0.15f, 0.95f);
            Color redFill = new Color(1f, 0.1f, 0.1f, 0.25f);
            Color brightWhite = Color.white;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist <= radius)
                    {
                        if (Mathf.Abs(dist - radius) <= 1.2f)
                        {
                            tex.SetPixel(x, y, redRing);
                        }
                        else
                        {
                            tex.SetPixel(x, y, redFill);
                        }
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }

            int c = size / 2;
            for (int i = -3; i <= 3; i++)
            {
                tex.SetPixel(c + i, c, brightWhite);
                tex.SetPixel(c, c + i, brightWhite);
            }

            tex.Apply();
            return CreateSpriteFromTexture(tex, 16f, new Vector2(0.5f, 0.5f));
        }
        #endregion

        #region Banner Collectible (+50m) (16x24)
        private static Sprite CreateBannerSprite()
        {
            int w = 16, h = 24;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color clear = Color.clear;
            Color gold = new Color(1f, 0.85f, 0.2f);
            Color goldDark = new Color(0.75f, 0.6f, 0.1f);
            Color redSilk = new Color(0.85f, 0.18f, 0.22f);
            Color pole = new Color(0.5f, 0.35f, 0.2f);

            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    tex.SetPixel(x, y, clear);

            for (int y = 0; y < 23; y++)
            {
                tex.SetPixel(2, y, pole);
                tex.SetPixel(3, y, goldDark);
            }
            tex.SetPixel(2, 23, gold);
            tex.SetPixel(3, 23, gold);

            for (int y = 10; y <= 21; y++)
            {
                int endX = 14;
                if (y < 13) endX = 14 - (13 - y) * 2;

                for (int x = 4; x <= endX; x++)
                {
                    tex.SetPixel(x, y, redSilk);
                }
            }

            for (int y = 15; y <= 18; y++)
            {
                tex.SetPixel(7, y, gold);
                tex.SetPixel(8, y, gold);
            }
            tex.SetPixel(9, 17, gold);
            tex.SetPixel(6, 16, gold);

            tex.Apply();
            return CreateSpriteFromTexture(tex, 16f, new Vector2(0.5f, 0.5f));
        }
        #endregion

        #region Heart Sprite (16x16)
        private static Sprite CreateHeartSprite()
        {
            int size = 16;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color clear = Color.clear;
            Color red = new Color(0.95f, 0.18f, 0.24f);
            Color redDark = new Color(0.65f, 0.08f, 0.12f);
            Color highlight = new Color(1f, 0.6f, 0.65f);

            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    tex.SetPixel(x, y, clear);

            int[] rows = new int[]
            {
                0b0000000000000000,
                0b0000000110000000,
                0b0000001111000000,
                0b0000011111100000,
                0b0000111111110000,
                0b0001111111111000,
                0b0011111111111100,
                0b0111111111111110,
                0b0111111111111110,
                0b1111111111111111,
                0b1111111111111111,
                0b1111111111111111,
                0b0111111001111110,
                0b0011110000111100,
                0b0000000000000000,
                0b0000000000000000
            };

            for (int y = 0; y < 16; y++)
            {
                int row = rows[y];
                for (int x = 0; x < 16; x++)
                {
                    if (((row >> (15 - x)) & 1) == 1)
                    {
                        if (y >= 10 && x < 6 && y < 13)
                            tex.SetPixel(x, y, highlight);
                        else if (y < 4 || x == 0 || x == 15)
                            tex.SetPixel(x, y, redDark);
                        else
                            tex.SetPixel(x, y, red);
                    }
                }
            }

            tex.Apply();
            return CreateSpriteFromTexture(tex, 16f, new Vector2(0.5f, 0.5f));
        }
        #endregion

        #region Shield Icon Sprite (16x16)
        private static Sprite CreateShieldIconSprite()
        {
            int size = 16;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color clear = Color.clear;
            Color gold = new Color(0.95f, 0.78f, 0.2f);
            Color blue = new Color(0.22f, 0.45f, 0.85f);
            Color silver = new Color(0.85f, 0.9f, 0.95f);

            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    tex.SetPixel(x, y, clear);

            for (int y = 2; y <= 14; y++)
            {
                int minX = (y < 8) ? (8 - y) / 2 : 2;
                int maxX = 15 - minX;
                for (int x = minX; x <= maxX; x++)
                {
                    if (x == minX || x == maxX || y == 14 || y <= 3)
                        tex.SetPixel(x, y, gold);
                    else if (x == 7 || x == 8)
                        tex.SetPixel(x, y, silver);
                    else
                        tex.SetPixel(x, y, blue);
                }
            }

            tex.Apply();
            return CreateSpriteFromTexture(tex, 16f, new Vector2(0.5f, 0.5f));
        }
        #endregion

        #region Road & Wall Textures (32x32 tiles)
        private static Sprite CreateRoadSprite()
        {
            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color dirtBase = new Color(0.38f, 0.35f, 0.30f);
            Color stone1 = new Color(0.52f, 0.49f, 0.45f);
            Color stone2 = new Color(0.60f, 0.57f, 0.52f);
            Color grout = new Color(0.25f, 0.22f, 0.19f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isGrout = (x % 8 == 0) || (y % 8 == 0 && (x / 8) % 2 == 0) || (y % 8 == 4 && (x / 8) % 2 == 1);
                    if (isGrout)
                    {
                        tex.SetPixel(x, y, grout);
                    }
                    else
                    {
                        int hash = (x * 13 + y * 27) % 7;
                        tex.SetPixel(x, y, hash > 3 ? stone2 : (hash > 1 ? stone1 : dirtBase));
                    }
                }
            }

            tex.Apply();
            return CreateSpriteFromTexture(tex, 16f, new Vector2(0.5f, 0.5f));
        }

        private static Sprite CreateWallSprite()
        {
            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color stoneDark = new Color(0.22f, 0.23f, 0.26f);
            Color stoneMid = new Color(0.36f, 0.38f, 0.42f);
            Color stoneLight = new Color(0.50f, 0.53f, 0.58f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isMortar = (x % 16 == 0) || (y % 8 == 0);
                    if (isMortar)
                    {
                        tex.SetPixel(x, y, stoneDark);
                    }
                    else
                    {
                        int n = (x * 7 + y * 31) % 5;
                        tex.SetPixel(x, y, n == 0 ? stoneLight : stoneMid);
                    }
                }
            }

            tex.Apply();
            return CreateSpriteFromTexture(tex, 16f, new Vector2(0.5f, 0.5f));
        }
        #endregion
    }
}
