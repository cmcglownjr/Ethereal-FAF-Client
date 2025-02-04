﻿namespace Ethereal.FA.Vault
{
    public class Scmap
    {
        public float[,] Heights;
        public ushort[] Heightmap;


        public int Height;
        public int Width;
        public float HeightScale;

        public static Scmap FromFile(string file, string previewTarget = null)
        {
            if (!File.Exists(file)) return null;
            var scmap = new Scmap();
            using var fs = new FileStream(file, FileMode.Open);
            using ScmapBinaryReader stream = new ScmapBinaryReader(fs);
            #region Header section
            var magicWord = stream.ReadInt32();
            if (magicWord != 0x1a70614d)
            {
                return null;
            }
            //? always 2
            var VersionMajor = stream.ReadInt32();
            //? always EDFE EFBE
            var unknown10 = stream.ReadInt32();
            //? always 2
            var unknown11 = stream.ReadInt32();
            var mapWidth = stream.ReadSingle();
            var mapHeight = stream.ReadSingle();
            //? always 0
            var Unknown12 = stream.ReadInt32();
            //? always 0
            var Unknown13 = stream.ReadInt16();
            int previewImageLength = stream.ReadInt32();
            var previewData = stream.ReadBytes(previewImageLength);
            var preview = file.Replace("scmap", "png");
            if (!File.Exists(preview)) DDSImage.ConvertToPng(previewData, preview);
            if (!string.IsNullOrWhiteSpace(previewTarget)) File.Copy(preview, previewTarget);
            var VersionMinor = stream.ReadInt32();
            #endregion
            #region Heightmap section
            scmap.Width = stream.ReadInt32();
            scmap.Height = stream.ReadInt32();
            //Height Scale, usually 1/128
            scmap.HeightScale = stream.ReadSingle();

            //heightmap dimension is always 1 more than texture dimension!
            //var heightmapData = stream.ReadInt16Array((int)((Height + 1) * (Width + 1)));
            var len = (scmap.Width + 1) * (scmap.Height + 1);
            //SixLabors.ImageSharp.

            ushort[] g = new ushort[len];
            for (int i = 0; i < len; i++)
            {
                g[i] = (ushort)stream.ReadInt16();
            }
            scmap.Heightmap = g;
            #region werf
            float[,] heights = new float[scmap.Height, scmap.Width];
            float HeightWidthMultiply = scmap.Height / (float)scmap.Width;
            for (int yy = 0; yy < scmap.Height; yy++)
            {
                for (int xx = 0; xx < scmap.Height; xx++)
                {
                    var localY = (int)((scmap.Height - 1 - yy) * HeightWidthMultiply);
                    heights[yy, xx] = GetHeight(localY, xx, g, scmap.Width) * scmap.HeightScale;

                    //if (HeightWidthMultiply == 0.5f && y > 0 && y % 2f == 0)
                    //{
                    //	heights[y - 1, x] = Lerp(heights[y, x], heights[y - 2, x], 0.5f);
                    //}
                }
            }
            scmap.Heights = heights;
            return scmap;
            #endregion


            if (VersionMinor >= 56)
                //Always 0?
                stream.ReadByte();

            #endregion
            #region Texture section
            //Terrain Shader, usually "TTerrain"
            var TerrainShader = stream.ReadStringNull();
            var TexPathBackground = stream.ReadStringNull();
            var TexPathSkyCubemap = stream.ReadStringNull();
            #endregion
            #region Env Cubes
            ScmapEnvCube[] envCubes;
            if (VersionMinor >= 56)
            {
                var count = stream.ReadInt32();
                envCubes = new ScmapEnvCube[count];
                for (int i = 0; i < count; i++)
                {
                    envCubes[0] = new ScmapEnvCube(stream.ReadStringNull(), stream.ReadStringNull());
                }
            }
            else
            {
                envCubes = new ScmapEnvCube[]
                {
                    new ScmapEnvCube(stream.ReadStringNull(),stream.ReadStringNull()),
                    new ScmapEnvCube(stream.ReadStringNull(),stream.ReadStringNull())
                };
            }
            #endregion
            #region Settings
            var LightingMultiplier = stream.ReadSingle();
            var SunDirection = stream.ReadVector3();
            var SunAmbience = stream.ReadVector3();
            var SunColor = stream.ReadVector3();
            var ShadowFillColor = stream.ReadVector3();
            var SpecularColor = stream.ReadVector4();
            var Bloom = stream.ReadSingle();
            var FogColor = stream.ReadVector3();
            var FogStart = stream.ReadSingle();
            var FogEnd = stream.ReadSingle();
            #endregion
        }

        public static int HeightmapId(int x, int y, float width) => (int)(x + y * (width + 1));

        public static ushort GetHeight(int x, int y, ushort[] heights, float width) => heights[HeightmapId(x, y, width)];
    }
}
