using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using MongoDB.Bson;
using Unity.Mathematics;

namespace ET
{
    public static class LegendMapHelper
    {
        public static void Load(Scene scene, string mapName)
        {
            // ponytail: 直接读工程内文件，只适用于编辑器和 dotnet；打包到手机时改走 YooAssets。
            BsonDocument document = BsonDocument.Parse(File.ReadAllText($"Packages/cn.etetet.legend/Assets/GameRes/Map/{mapName}.json"));
            int width = document["width"].AsInt32;
            int height = document["height"].AsInt32;
            int tileWidth = document["tilewidth"].AsInt32;
            if (document["orientation"].AsString != "orthogonal" || document["infinite"].AsBoolean ||
                width <= 0 || height <= 0 || tileWidth <= 0 || document["tileheight"].AsInt32 != tileWidth)
            {
                throw new FormatException("Invalid Legend map dimensions or orientation");
            }
            BsonArray layers = document["layers"].AsBsonArray;
            BsonArray ground = layers.Single(layer => layer["name"].AsString == "ground")["data"].AsBsonArray;
            BsonArray block = layers.Single(layer => layer["name"].AsString == "block")["data"].AsBsonArray;
            if (ground.Count != width * height || block.Count != ground.Count)
            {
                throw new FormatException("Invalid Legend map layer size");
            }
            BsonValue tileset = document["tilesets"].AsBsonArray.Single();
            int firstGid = tileset["firstgid"].AsInt32;
            Dictionary<int, int> colors = new();
            foreach (BsonValue tile in tileset["tiles"].AsBsonArray)
            {
                string color = tile["properties"].AsBsonArray.Single(property => property["name"].AsString == "color")["value"].AsString;
                colors.Add(firstGid + tile["id"].AsInt32, unchecked((int)uint.Parse(color.Substring(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture)));
            }
            BsonValue spawn = layers.Single(layer => layer["name"].AsString == "objects")["objects"].AsBsonArray
                .Single(obj => obj["name"].AsString == "spawn");
            GridMapComponent grid = scene.AddComponent<GridMapComponent, int, int, float>(width, height, LegendMapConst.CellSize);
            LegendMapComponent map = scene.AddComponent<LegendMapComponent>();
            // Tiled 的行从上往下数，grid 的 y 朝上，读取时翻转行号，游戏里的朝向才和 Tiled 里一致。
            map.Spawn = new int2((int)(spawn["x"].ToDouble() / tileWidth), height - 1 - (int)(spawn["y"].ToDouble() / tileWidth));
            map.CellColors = new int[ground.Count];
            for (int i = 0; i < ground.Count; ++i)
            {
                int tiled = (height - 1 - i / width) * width + i % width;
                int groundGid = (int)(ground[tiled].ToInt64() & 0x1FFFFFFF);
                int blockGid = (int)(block[tiled].ToInt64() & 0x1FFFFFFF);
                grid.Blocked[i] = blockGid != 0;
                map.CellColors[i] = colors[blockGid != 0 ? blockGid : groundGid];
            }
        }
    }
}
