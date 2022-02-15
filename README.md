# ChainEnemy
※ You need EntityActionCon.

# Static Method

* GameObject[] Generate(GameObject prefab, Quaternion rot, int num, float distance, float duration, bool speedBase, ICurve curve)
* GameObject[] Generate(GameObject prefab, Quaternion rot, int num, float distance, float duration, bool speedBase, ICurve curve, bool funcRunOnFirstElement,params Function[] functions)
* GameObject[] Generate(GameObject prefab, Quaternion rot, int num, float distance, float duration, bool speedBase, ICurve curve, bool funcRunOnFirstElement, params (float, Func<int, EA_IFunction[]>, bool)[] functions)

# execute

```
EA_IFunction[] GetFunc1(int i) {
    return new EA_IFunction[] {new A_Shot(false, bullet, 1, 0.5f, new A_Shot.LaunchInfo(Vector3.zero, PlayerTrans, true, 0))
}

ChainEnemy.Generate(enemy, RotationLeft, 8, 2, 5, true, new StraightLine(V2(0, 0), V2(10, 0), V2(12, 10)), false,(10, GetFunc1, false));
```
