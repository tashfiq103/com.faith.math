﻿namespace com.faith.core
{
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using System.Collections;

    public class GameConfiguratorManager : MonoBehaviour
    {

        #region Public Varaibles

        public static CoreEnums.GameMode gameMode = CoreEnums.GameMode.DEBUG;
        public static CoreEnums.LogType logType = CoreEnums.LogType.Verbose;
        public static CoreEnums.DataSavingMode dataSavingMode = CoreEnums.DataSavingMode.PlayerPrefsData;

        public CoreEnums.GameMode GameMode { get { return gameMode; } }
        public CoreEnums.LogType LogType { get { return logType; } }
        public CoreEnums.DataSavingMode DataSavingMode { get { return dataSavingMode; } }

#if UNITY_EDITOR

        public bool isGameConfiguratorAssetVisible;

#endif

        public static GameConfiguratorManager   Instance;

        public CoreEnums.InstanceBehaviour      instanceBehaviour;
        public GameConfiguratorAsset            gameConfiguratorAsset;

        #region Private Variables

        

        private bool    _isAutomaticDataSnapShopControllerRunning = false;

        #endregion

        #endregion

        #region Mono Behaviour

        private void Awake()
        {
            Initialization();
        }


        private void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
                StartAutomaticDataSnapshotController();
            }
            else {

                StopAutomaticDataSnapshotController();

                if(gameConfiguratorAsset.dataSaveWhenApplicationLoseFocus) TakeDataSnapshop();
            }
              
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                StopAutomaticDataSnapshotController();
                if (gameConfiguratorAsset.dataSaveWhenApplicationLoseFocus) TakeDataSnapshop();
            }
            else {

                StartAutomaticDataSnapshotController();
            }
        }

        private void OnApplicationQuit()
        {
            StopAutomaticDataSnapshotController();
            if (gameConfiguratorAsset.dataSaveWhenApplicationQuit) TakeDataSnapshop();
        }

        #endregion

        #region Configuretion

        private void Initialization() {

            switch (instanceBehaviour)
            {

                case CoreEnums.InstanceBehaviour.UseAsReference:

                    break;
                case CoreEnums.InstanceBehaviour.CashedAsInstance:

                    Instance = this;

                    break;
                case CoreEnums.InstanceBehaviour.Singleton:

                    if (Instance == null)
                    {
                        Instance = this;
                        DontDestroyOnLoad(gameObject);
                    }
                    else
                    {

                        Destroy(gameObject);
                    }

                    break;
            }

            if (gameConfiguratorAsset == null)
            {
                CoreDebugger.Debug.LogError("'gameConfiguretorAsset' has not been assigned!");
                return;
            }

            gameMode        = gameConfiguratorAsset.gameMode;
            logType         = gameConfiguratorAsset.logType;
            dataSavingMode  = gameConfiguratorAsset.dataSavingMode;

            if (gameConfiguratorAsset.dataSaveWhenSceneUnloaded)
                SceneManager.sceneUnloaded += OnSceneUnloaded;

            StartAutomaticDataSnapshotController();
        }

        private void OnSceneUnloaded(Scene scene) {

            TakeDataSnapshop();
        }

        private void StartAutomaticDataSnapshotController() {

            if (dataSavingMode == CoreEnums.DataSavingMode.BinaryFormater && !_isAutomaticDataSnapShopControllerRunning)
            {
                _isAutomaticDataSnapShopControllerRunning = true;
                StartCoroutine(ControllerForTakingDataSnapshopInPeriodOfTime());
            }
        }

        private void StopAutomaticDataSnapshotController() {

            if (dataSavingMode == CoreEnums.DataSavingMode.BinaryFormater && _isAutomaticDataSnapShopControllerRunning) {

                _isAutomaticDataSnapShopControllerRunning = false;
            }
        }

        private IEnumerator ControllerForTakingDataSnapshopInPeriodOfTime() {

            float remainingTime                 = gameConfiguratorAsset.snapshotFrequenceyInSec;
            float cycleLength                   = 0.0167f;
            WaitForSecondsRealtime cycleDelay   = new WaitForSecondsRealtime(cycleLength);
            while (_isAutomaticDataSnapShopControllerRunning) {

                yield return cycleDelay;
                remainingTime -= cycleLength;
                
                if (remainingTime <= 0)
                {
                    remainingTime = gameConfiguratorAsset.snapshotFrequenceyInSec;
                    TakeDataSnapshop();
                }

                if (!_isAutomaticDataSnapShopControllerRunning)
                    break;

            }

            StopCoroutine(ControllerForTakingDataSnapshopInPeriodOfTime());
        }

        private void TakeDataSnapshop() {

            if(gameConfiguratorAsset.dataSavingMode == CoreEnums.DataSavingMode.BinaryFormater)
                BinaryFormatedData.SaveDataSnapshot();
        }

        #endregion
    }
}

