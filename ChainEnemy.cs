using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Curve;
using EntityBehavior.Action;

public class ChainEnemy : MonoBehaviour {
    private bool active = false;
    private ICurve curve;
    private ElementInfo[] enemys;
    private float speed = 0f;
    private float time = 0f;
    private FunctionInfo[] functions;
    private bool funcRunOnFirstElement = true;

    public static GameObject[] Generate(GameObject prefab, Quaternion rot, int num, float distance, float duration, bool speedBase, ICurve curve, bool funcRunOnFirstElement,params Function[] functions) {
        ChainEnemy c = Create();
        return c.Set(prefab, rot, num, distance, duration, speedBase, curve, funcRunOnFirstElement, functions);
    }
    public static GameObject[] Generate(GameObject prefab, Quaternion rot, int num, float distance, float duration, bool speedBase, ICurve curve, bool funcRunOnFirstElement, params (float, Func<int, EA_IFunction[]>, bool)[] functions) {
        ChainEnemy c = Create();
        return c.Set(prefab, rot, num, distance, duration, speedBase, curve, funcRunOnFirstElement, functions);
    }
    public static GameObject[] Generate(GameObject prefab, Quaternion rot, int num, float distance, float duration, bool speedBase, ICurve curve) {
        ChainEnemy c = Create();
        return c.Set(prefab, rot, num, distance, duration, speedBase, curve);
    }
    private static ChainEnemy Create() {
        return new GameObject("ChainEnemy").AddComponent<ChainEnemy>();
    }

    public GameObject[] Set(GameObject prefab, Quaternion rot, int num, float distance, float duration, bool speedBase, ICurve curve, bool funcRunOnFirstElement, params Function[] functions) {
        this.funcRunOnFirstElement = funcRunOnFirstElement;
        this.functions = new FunctionInfo[functions.Length];
        for (int i = 0; i < functions.Length; i++)
            this.functions[i] = new FunctionInfo(functions[i], num);
        return Set(prefab, rot, num, distance, duration, speedBase, curve);
    }
    public GameObject[] Set(GameObject prefab, Quaternion rot, int num, float distance, float duration, bool speedBase, ICurve curve, bool funcRunOnFirstElement, params (float, Func<int, EA_IFunction[]>, bool)[] functions) {
        Function[] _functions = new Function[functions.Length];
        for (int i = 0; i < _functions.Length; i++) {
            (float d, Func<int, EA_IFunction[]> f, bool n) = functions[i];
            _functions[i] = new Function(d, f, n);
        }
        return Set(prefab, rot, num, distance, duration, speedBase, curve, funcRunOnFirstElement, _functions);
    }
    public GameObject[] Set(GameObject prefab, Quaternion rot, int num, float distance, float duration, bool speedBase, ICurve curve) {
        this.curve = curve;
        this.enemys = new ElementInfo[num];
        GameObject[] objs = new GameObject[this.enemys.Length];
        for (int i = 0; i < num; i++) {
            objs[i] = Instantiate(prefab, this.curve.GetPosition(0), rot, this.transform);
            objs[i].SetActive(false);
            this.enemys[i] = new ElementInfo(objs[i], distance);
        }
        if (speedBase) this.speed = duration;
        else this.speed = this.curve.GetCurveLength() / duration;
        active = true;
        return objs;
    }

    public class ElementInfo {
        private GameObject obj;
        public float distance { get; private set; }
        public bool ObjectIsNull { get { return this.obj == null; } }
        public ElementInfo(GameObject obj, float distance) {
            this.obj = obj;
            this.distance = distance;
        }
        public float Update(ICurve curve, float t) {
            float curveLength = curve.GetCurveLength();
            if (this.obj != null) {
                this.obj.transform.position = curve.GetPosition(t);
                this.obj.SetActive(0 <= t && t < curveLength);
            }
            if (t >= curveLength) Destroy();
            return t -= this.distance;
        }
        private void Destroy() {
            if (this.obj != null) MonoBehaviour.Destroy(this.obj);
        }
        public void FunctionExecute(EA_IFunction[] functions) {
            EntityActionCon actionCon = this.obj.ComponentEntityActionCon();
            actionCon.ResetAll();
            actionCon.Set(functions).BeginAction();
        }
    }

    public class FunctionInfo {
        private Function function;
        private bool[] executedArray = new bool[1];
        public FunctionInfo(Function function, int elementNum) {
            this.function = function;
            this.executedArray = new bool[elementNum];
            for (int i = 0; i < this.executedArray.Length; i++) this.executedArray[i] = false;
        }
        public void Update(ICurve curve, float t, bool runOnFirstElement, ElementInfo[] elements) {
            for (int i = 0; i < elements.Length; i++) {
                ElementInfo e = elements[i];
                if (!this.executedArray[i] && t > GetDistance(curve)) {
                    this.executedArray[i] = true;
                    e.FunctionExecute(GetFunctions(arg: i));
                }
                if (!runOnFirstElement) t -= e.distance;
            }
        }
        private float GetDistance(ICurve curve) => this.function.GetDistance(curve);
        private Func<int, EA_IFunction[]> GetFunctions => this.function.GetFunctions();

    }
    public struct Function {
        private float distance;
        private Func<int, EA_IFunction[]> func;
        private bool normalized;
        public Function(float distance, Func<int, EA_IFunction[]> func, bool normalized = false) {
            this.distance = distance;
            this.func = func;
            this.normalized = normalized;
        }
        public float GetDistance(ICurve curve) => normalized ? distance * curve.GetCurveLength() : distance;
        public Func<int, EA_IFunction[]> GetFunctions() => this.func;
    }

    // Update is called once per frame
    void Update() {
        if (!this.active) return;
        if (this.enemys == null) return;
        this.time += Time.deltaTime;
        float t = this.speed * this.time;
        if (this.functions != null) {
            foreach (var f in this.functions)
                f.Update(this.curve, t, this.funcRunOnFirstElement, this.enemys);
        }
        float _t = t;
        bool allNull = true;
        foreach (var e in this.enemys) {
            _t = e.Update(this.curve, _t);
            allNull &= e.ObjectIsNull;
        }
        if (allNull) Destroy(this.gameObject);
    }
}

