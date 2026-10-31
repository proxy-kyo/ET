import json
import random
from collections import deque
from pathlib import Path

from PIL import Image

WIDTH = HEIGHT = 128
TILE_SIZE = 32
COLORS = ["#ff659b45", "#ffb48a58", "#ffe0cd8b", "#ff397db8", "#ff747b86", "#ff245a30", "#ffaaa39b"]


def main():
    rng = random.Random(1)
    ground = [1] * (WIDTH * HEIGHT)
    block = [0] * (WIDTH * HEIGHT)
    walls = set()
    roads = set()
    centers = [(24, 24), (112, 104)]
    for y in range(HEIGHT):
        for x in range(WIDTH):
            i = y * WIDTH + x
            if 53 <= x <= 75 and 53 <= y <= 75:
                ground[i] = 2 if (x + y) % 5 else 1
            if ((x in (52, 76) and 52 <= y <= 76) or
                    (y in (52, 76) and 52 <= x <= 76)):
                walls.add((x, y))
                block[i] = 5
            if 63 <= x <= 65 or 63 <= y <= 65:
                roads.add((x, y))
                ground[i] = 2
                block[i] = 0
                walls.discard((x, y))
            if x in (95, 100):
                ground[i] = 3
            if 96 <= x <= 99:
                ground[i] = block[i] = 4
            if 94 <= x <= 101 and (63 <= y <= 65 or 23 <= y <= 25):
                ground[i] = 2
                block[i] = 0
                roads.add((x, y))

    protected = set()
    for x, y in walls | roads:
        for dy in range(-2, 3):
            for dx in range(-2, 3):
                protected.add((x + dx, y + dy))
    candidates = []
    outside_walkable = 0
    for y in range(HEIGHT):
        for x in range(WIDTH):
            if 52 <= x <= 76 and 52 <= y <= 76:
                continue
            if block[y * WIDTH + x] == 0:
                outside_walkable += 1
                if ((x, y) not in protected and ground[y * WIDTH + x] == 1 and
                        all(max(abs(x - cx), abs(y - cy)) > 2 for cx, cy in centers)):
                    candidates.append((x, y))
    available = set(candidates)
    tree_target = round(outside_walkable * 0.15)
    trees = 0
    while trees < tree_target:
        cx, cy = rng.choice(candidates)
        radius = rng.randint(3, 7)
        for y in range(max(0, cy - radius), min(HEIGHT, cy + radius + 1)):
            for x in range(max(0, cx - radius), min(WIDTH, cx + radius + 1)):
                if trees == tree_target:
                    break
                if ((x, y) in available and (x - cx) ** 2 + (y - cy) ** 2 <= radius ** 2
                        and rng.random() < 0.72):
                    block[y * WIDTH + x] = 6
                    available.remove((x, y))
                    trees += 1
    for x, y in rng.sample([cell for cell in candidates if cell in available], 120):
        block[y * WIDTH + x] = 7

    reached = {(64, 64)}
    queue = deque(reached)
    def walkable(x, y):
        return 0 <= x < WIDTH and 0 <= y < HEIGHT and block[y * WIDTH + x] == 0
    assert walkable(64, 64)
    while queue:
        x, y = queue.popleft()
        for dy in (-1, 0, 1):
            for dx in (-1, 0, 1):
                cell = (x + dx, y + dy)
                if ((dx == dy == 0) or cell in reached or not walkable(*cell)):
                    continue
                if dx and dy and (not walkable(x + dx, y) or not walkable(x, y + dy)):
                    continue
                reached.add(cell)
                queue.append(cell)
    blocked_count = sum(gid != 0 for gid in block)
    ratio = len(reached) / (WIDTH * HEIGHT - blocked_count)
    assert ratio >= 0.95, ratio
    assert all(cell in reached for cell in [(68, 66), (84, 64), *centers])

    objects = [{"id": 1, "name": "spawn", "type": "spawn", "class": "spawn", "point": True,
                "x": 64 * TILE_SIZE + 16, "y": (HEIGHT - 1 - 64) * TILE_SIZE + 16, "width": 0, "height": 0,
                "rotation": 0, "visible": True}]
    for object_id, name, x, y in [(2, "forest_sw", 16, 16), (3, "forest_ne", 104, 96)]:
        objects.append({"id": object_id, "name": name, "type": "monster_area", "class": "monster_area",
                        "x": x * TILE_SIZE, "y": (HEIGHT - y - 16) * TILE_SIZE, "width": 16 * TILE_SIZE,
                        "height": 16 * TILE_SIZE, "rotation": 0, "visible": True})
    # 上面按 grid 坐标（y 朝上）生成；Tiled 的行从上往下数，写文件时翻转行号。
    def tiled_rows(data):
        return [data[(HEIGHT - 1 - row) * WIDTH + x] for row in range(HEIGHT) for x in range(WIDTH)]
    layers = []
    for layer_id, name, data in [(1, "ground", ground), (2, "block", block)]:
        layers.append({"id": layer_id, "name": name, "type": "tilelayer", "width": WIDTH,
                       "height": HEIGHT, "x": 0, "y": 0, "opacity": 1, "visible": True, "data": tiled_rows(data)})
    layers.append({"id": 3, "name": "objects", "type": "objectgroup", "draworder": "topdown",
                   "x": 0, "y": 0, "opacity": 1, "visible": True, "objects": objects})
    tileset = {"firstgid": 1, "name": "LegendTiles", "image": "LegendTiles.png", "tilewidth": TILE_SIZE,
               "tileheight": TILE_SIZE, "columns": 7, "tilecount": 7, "imagewidth": 224, "imageheight": 32,
               "margin": 0, "spacing": 0,
               "tiles": [{"id": tile_id, "properties": [{"name": "color", "type": "color", "value": color}]}
                         for tile_id, color in enumerate(COLORS)]}
    tiled_map = {"type": "map", "version": "1.10", "tiledversion": "1.10.2", "orientation": "orthogonal",
                 "renderorder": "right-down", "width": WIDTH, "height": HEIGHT, "tilewidth": TILE_SIZE,
                 "tileheight": TILE_SIZE, "infinite": False, "nextlayerid": 4, "nextobjectid": 4,
                 "layers": layers, "tilesets": [tileset]}
    destination = Path(__file__).resolve().parents[1] / "Assets/GameRes/Map"
    destination.mkdir(parents=True, exist_ok=True)
    (destination / "LegendMap1.json").write_text(json.dumps(tiled_map, indent=2) + "\n", encoding="utf-8")
    image = Image.new("RGBA", (224, 32))
    noise = random.Random(1)
    for tile_id, color in enumerate(COLORS):
        rgb = tuple(int(color[offset:offset + 2], 16) for offset in (3, 5, 7))
        for y in range(TILE_SIZE):
            for x in range(TILE_SIZE):
                delta = noise.randint(-7, 7)
                image.putpixel((tile_id * TILE_SIZE + x, y),
                               tuple(max(0, min(255, channel + delta)) for channel in rgb) + (255,))
    image.save(destination / "LegendTiles.png")
    print(f"blocked={blocked_count}, reachable={len(reached)}/{WIDTH * HEIGHT - blocked_count} ({ratio:.2%})")


if __name__ == "__main__":
    main()
