using BepInEx;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger = BepInEx.Logger;

using UnityEngine.Networking;

namespace KoikatsuA10
{
    [BepInPlugin(GUID: GUID, Name: "KoikatsuA10", Version: Version)]
    public class KoikatsuA10 : BaseUnityPlugin
    {
        public const string GUID = "com.bepis.bepinex.koikatsua10";
        public const string Version = "10.0";

        float lastSyncedSec = 0f;
        float syncIntervalSec = 0.5f;


        private Boolean isSyncDevice = false;
        //private A10Piston a10PistonDevice;

        void Awake()
        {
            Logger.Log(BepInEx.Logging.LogLevel.Info, "koikatsuA10 Awaked");
            this.lastSyncedSec = Time.time;
        }

        void Start()
        {
            Logger.Log(BepInEx.Logging.LogLevel.Info, "koikatsuA10 call Start(Send)");
        }

        private Transform chaMHipBone;

        private Transform chaFTouchBone;

        private Transform chaFHipBone;
        private Transform chaFToothBone;
        private Transform chaFHandRBone;
        private Transform chaFBustRBone;
        private Transform chaFBustLBone;


        List<string> scenePrefixies = new List<string> { "HScene", "VRHScene"};
        private HFlag hFlag;

        private GameObject hScene;

        private string beforeAnimStateName = "";
        void Update()
        {
            if (this.hScene && this.hScene.activeSelf)
            {
                UpdateInHScene();
            }
            if (lastSyncedSec + syncIntervalSec < Time.time)
            {
                GameObject temp = this.FindHSceneObject("");
                if(temp) { this.hScene = temp; }
                this.lastSyncedSec = Time.time;
            }
        }
        GameObject FindHSceneObject(string bodySuffix)
        {
            GameObject temp = null;
            foreach (var sceneKey in this.scenePrefixies)
            {
                string fullKey = bodySuffix.Length == 0 ? sceneKey : $"{sceneKey}/{bodySuffix}";
                temp = GameObject.Find(fullKey);
                if (temp) { break; }
            }
            return temp;
        }
        void UpdateInHScene() {
            if (lastSyncedSec + syncIntervalSec < Time.time)
            {
                if (!this.chaMHipBone)
                {
                    GameObject temp = this.FindHSceneObject("chaM_001/BodyTop/p_cf_body_bone/cf_j_root/cf_n_height/cf_j_hips");
                    if (temp)
                    {
                        this.chaMHipBone = temp.transform;
                        Logger.Log(BepInEx.Logging.LogLevel.Info, string.Format("M HipBone: {0}", this.chaMHipBone.position));
                    }
                }
                if (!this.chaFTouchBone)
                {
                    GameObject temp = this.FindHSceneObject( "chaF_001/BodyTop/p_cf_body_bone/cf_j_root/cf_n_height/cf_j_hips");
                    if (temp)
                    {
                        this.chaFTouchBone = temp.transform;
                        this.chaFHipBone = temp.transform;
                        Logger.Log(BepInEx.Logging.LogLevel.Info, string.Format("F HipBone: {0}", this.chaFTouchBone.position));
                    }
                }
                if(!this.chaFHandRBone)
                {
                    GameObject temp = this.FindHSceneObject("chaF_001/BodyTop/p_cf_body_bone/cf_j_root/cf_n_height/cf_j_hips/cf_j_spine01/cf_j_spine02/cf_j_spine03/cf_d_shoulder_R/cf_j_shoulder_R/cf_j_arm00_R/cf_j_forearm01_R/cf_j_hand_R/cf_s_hand_R");
                    if (temp)
                    {
                        this.chaFHandRBone = temp.transform;
                        Logger.Log(BepInEx.Logging.LogLevel.Info, string.Format("F HandRBone: {0}", this.chaFHandRBone.position));
                    }

                }
                if(!this.chaFToothBone)
                {
                    GameObject temp = this.FindHSceneObject("chaF_001/BodyTop/p_cf_body_bone/cf_j_root/cf_n_height/cf_j_hips/cf_j_spine01/cf_j_spine02/cf_j_spine03/cf_j_neck/cf_j_head/cf_s_head/p_cf_head_bone/ct_head/N_tonn_face/N_cf_haed/cf_O_tooth");
                    if (temp)
                    {
                        this.chaFToothBone = temp.transform;
                        Logger.Log(BepInEx.Logging.LogLevel.Info, string.Format("F ToothBone: {0}", this.chaFToothBone.position));
                    }


                }
                if(!this.chaFBustLBone)
                {
                    GameObject temp = this.FindHSceneObject("chaF_001/BodyTop/p_cf_body_bone/cf_j_root/cf_n_height/cf_j_hips/cf_j_spine01/cf_j_spine02/cf_j_spine03/cf_d_bust00/cf_s_bust00_L");
                    if (temp)
                    {
                        this.chaFBustLBone= temp.transform;
                        Logger.Log(BepInEx.Logging.LogLevel.Info, string.Format("F BustLBone: {0}", this.chaFBustLBone.position));
                    }

                }
                if(!this.chaFBustLBone)
                {
                    GameObject temp = this.FindHSceneObject("chaF_001/BodyTop/p_cf_body_bone/cf_j_root/cf_n_height/cf_j_hips/cf_j_spine01/cf_j_spine02/cf_j_spine03/cf_d_bust00/cf_s_bust00_R");
                    if (temp)
                    {
                        this.chaFBustRBone= temp.transform;
                        Logger.Log(BepInEx.Logging.LogLevel.Info, string.Format("F BustRBone: {0}", this.chaFBustRBone.position));
                    }

                }

                if (!(this.chaFHipBone && this.chaFTouchBone))
                {
                    Logger.Log(BepInEx.Logging.LogLevel.Info, "Hips bone not found");
                }


                if(!this.hFlag)
                {
                    var obj = GameObject.Find("HSceneProc");
                    if(!obj) { obj = GameObject.Find("VRHScene");  }
                    if (obj) { this.hFlag = obj.GetComponent<HFlag>(); }
                }
                this.lastSyncedSec = Time.time;
            }


            UpdateAnimateState();

            if (this.chaMHipBone && this.chaFTouchBone)
            {
                float currentFrameDistance = Vector3.Distance(this.chaFTouchBone.position, this.chaMHipBone.position);
                UpdateRelativeAcceleration(currentFrameDistance);
            }


            if (this.isSyncDevice)
            {
                // 方向の変わったフレームの場合、キューを追加
                // ガクガクするので、0.1f(100ms)未満をのストロークをフィルタリング
                if(this.beforePistonDirectionChanged == Time.time && this.directionChangeInterval >= 0.1f)
                {
                    // 直近で更新しているので、beforeから取って問題なし
                    StartCoroutine(AddPistonQueue(this.directionChangeInterval, this.beforePistonDirection));
                }
                //this.a10PistonDevice.Sync(Time.time);
            }
        }
        IEnumerator AddPistonQueue(float interval, int direction)
        {
           string hostNameFull = string.Format("http://{0}:{1}/api/addQueue?direction={2}&interval={3}", "localhost", 8080, direction, interval);
           UnityWebRequest request =  UnityWebRequest.Get(hostNameFull);
           UnityEngine.AsyncOperation async = request.Send();
            yield return async;
        }
        IEnumerator ClearPistonQueue()
        {
            string hostNameFull = string.Format("http://{0}:{1}/api/clearQueue", "localhost", 8080);
           UnityWebRequest request =  UnityWebRequest.Get(hostNameFull);
           UnityEngine.AsyncOperation async = request.Send();
            yield return async;
        }

        public void UpdateTouchMBone()
        {
            Dictionary<string, Transform> candidate = new Dictionary<string, Transform> {
                { "chaFHipBone" , this.chaFHipBone },
                { "chaFToothBone" , this.chaFToothBone },
                { "chaFHandRBone" , this.chaFHandRBone },
                { "chaFBustLBone" , this.chaFBustLBone },
                { "chaFBustRBone" , this.chaFBustRBone },
            };

            string minimumKey = null;
            float minimumDistance = float.MaxValue;
            Transform minimumDistanceBone = this.chaFTouchBone;

            foreach(var key in candidate.Keys){
                var transform = candidate[key];

                if (transform) {
                    var distance = Vector3.Distance(this.chaMHipBone.position, transform.position);

                    Logger.Log(BepInEx.Logging.LogLevel.Info, $"UpdateTouchMBone key: {key}, minimumDistance: {distance}");

                    if (minimumKey == null) {
                        minimumKey = key; minimumDistance = distance;

                    }
                    else
                    {
                        if (distance < minimumDistance)
                        {
                            minimumDistance = distance;
                            minimumKey = key;
                        }
                    }
                } else
                {
                    Logger.Log(BepInEx.Logging.LogLevel.Info, $"UpdateTouchMBone {key} is NotFound.");
                }
            }

            Logger.Log(BepInEx.Logging.LogLevel.Info, $"UpdateTouchMBone minimumKey: {minimumKey}, minimumDistance: {minimumDistance}");
            this.chaFTouchBone = minimumDistanceBone;

        }

        public void UpdateAnimateState()
        {
            var currentAnimStateName = this.hFlag ?  this.hFlag.nowAnimStateName : "-";

            if (!this.beforeAnimStateName.Equals(currentAnimStateName))
            {
                UpdateTouchMBone();

                // モードが変わったら、今の実行キューをクリアする
                StartCoroutine(ClearPistonQueue());
                // アレが動く状況は、Loopが付いているとき
                this.isSyncDevice = currentAnimStateName.Contains("Loop") ? true : false;

                Logger.Log(BepInEx.Logging.LogLevel.Info, string.Format("animStateChanged: {0} to {1}, isSyncDevice: {2}", this.beforeAnimStateName, currentAnimStateName, this.isSyncDevice));
                this.beforeAnimStateName = currentAnimStateName;
            }
        }

        public struct DistanceHistory
        {
            public float time;
            public float distance;
        };
        private static int referenceFrameLimit = 3;
        private Stack<DistanceHistory> histories = new Stack<DistanceHistory>(referenceFrameLimit);

        private float beforeFrameAccelaration = 0f;
        private float currentFrameAccelaration = 0f;

        private int beforePistonDirection = 0;
        private float beforePistonDirectionChanged = 0f;
        private float directionChangeInterval = 0f;

        void UpdateRelativeAcceleration (float currentFrameDistance)
        {
            var currentDistance = new DistanceHistory();
            currentDistance.time = Time.time;
            currentDistance.distance = currentFrameDistance;

            histories.Push(currentDistance);


            var beforeFrameTime = 0f;
            var beforeFrameDistance = 0f;

            var accelarations = new List<float>(referenceFrameLimit);
            foreach (var history in  histories)
            {
                if (beforeFrameTime != 0f)
                {
                    var timeDistance = history.time - beforeFrameTime;
                    // 空間距離。+の値なら遠ざかっている
                    var spaceDistance = history.distance - beforeFrameDistance;
                    // フレームごとの加速度
                    var accelaration = spaceDistance / timeDistance;

                    accelarations.Add(accelaration);
                }
                beforeFrameTime = history.time;
                beforeFrameDistance = history.distance;
            }

            this.beforeFrameAccelaration = this.currentFrameAccelaration;
            if (accelarations.Count() >= 2)
            {
                this.currentFrameAccelaration = accelarations.Aggregate<float>((total, item) => total + item) / referenceFrameLimit;
            }

            float accelarationDiff = this.currentFrameAccelaration - this.beforeFrameAccelaration;

            int currentPistonDirection = accelarationDiff < 0 ? -1 : 1;

            // 加速度が別方向になった場合のみ
            if (currentPistonDirection != this.beforePistonDirection)
            {
                this.directionChangeInterval = this.beforePistonDirectionChanged == 0 ? 0 : Time.time - this.beforePistonDirectionChanged;

                this.beforePistonDirectionChanged = Time.time;
                this.beforePistonDirection = currentPistonDirection;
            }
        }
    }
    class A10Piston : MonoBehaviour
    {
        private struct A10PistonCommand
        {
            public float interval;
            public int direction;
        };

        private Queue<A10PistonCommand> commandQueue = new Queue<A10PistonCommand>();

        private string HostName = "localhost";
        private int portNo = 8080;
        private string hostNameFull;

        void init()
        {
            this.hostNameFull = string.Format("http://{0}:{1}", HostName, portNo);
            StartCoroutine(SendInit());
        }
        IEnumerator SendInit()
        {
           UnityWebRequest request =  UnityWebRequest.Get(this.hostNameFull);
           UnityEngine.AsyncOperation async = request.Send();
            yield return async;
        }
        public void Open()
        {
            init();
        }
        public void AddQueue(float interval, int direction)
        {
            var command = new A10PistonCommand();
            command.interval = interval;
            command.direction = direction;
            commandQueue.Enqueue(command);
        }
        public void ClearQueue()
        {
            commandQueue.Clear();
        }
    }
}
