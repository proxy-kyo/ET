import json
import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parents[3]
KEEP = {
    f"cn.etetet.{name}"
    for name in """core config proto loader sourcegenerator memorypack login db map aoi
        unit numeric grid actorlocation netinner servicediscovery startconfig router
        console http test unitybridge yooassets yiuiframework yiuiinvoke yiuiyooassets
        yiuitips yiuigm yiuiluban yiuireddot yiuiloopscrollrectasync""".split()
}


def validate():
    closure = set()
    external = set()
    pending = ["cn.etetet.legend"]
    while pending:
        name = pending.pop()
        if name in closure:
            continue
        assert name == "cn.etetet.legend" or name in KEEP, f"非保留包: {name}"
        closure.add(name)
        path = ROOT / "Packages" / name / "package.json"
        package = json.loads(path.read_text(encoding="utf-8-sig"))
        for dependency, version in package.get("dependencies", {}).items():
            if dependency.startswith("cn.etetet."):
                pending.append(dependency)
                if name == "cn.etetet.legend":
                    target = ROOT / "Packages" / dependency / "package.json"
                    actual = json.loads(target.read_text(encoding="utf-8-sig"))["version"]
                    assert version == actual, f"版本不一致: {dependency}: {version} != {actual}"
            else:
                assert dependency.startswith("com.unity."), f"未核实的外部依赖: {dependency}"
                external.add(dependency)

    main = set((ROOT / "MainPackage.txt").read_text(encoding="utf-8-sig").splitlines())
    assert {name for name in main if not name.startswith("com.unity.")} == closure, "MainPackage 与闭包不一致"
    assert external <= main, f"MainPackage 缺少 Unity 依赖: {external - main}"
    print(f"闭包 ⊆ 保留包：通过（{len(closure) - 1} 个依赖包 + legend）")
    print("\n".join(sorted(closure)))


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    validate()
