using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace com.felixwee.easysocket
{
    public class EasySocket : MonoBehaviour
    {

        private static EasySocket _instance = null;

        public static EasySocket Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("EasySocket");
                    _instance = go.AddComponent<EasySocket>();
                    DontDestroyOnLoad(_instance);
                }
                return _instance;
            }
        }

        /****************日志输出相关**************/

        public static void Log(params object[] args)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in args)
            {
                sb.Append(item + " ");
            }
            Debug.Log("【EasySocket】" + sb.ToString());
        }

        /// <summary>
        /// 输出警告日志
        /// </summary>
        /// <param name="args"></param>
        public static void LogWarning(params object[] args)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in args)
            {
                sb.Append(item + " ");
            }
            Debug.LogWarning("【EasySocket】" + sb.ToString());
        }

        /// <summary>
        /// 输出错误日志
        /// </summary>
        /// <param name="args"></param>
        public static void LogError(params object[] args)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in args)
            {
                sb.Append(item + " ");
            }
            Debug.LogError("【EasySocket】" + sb.ToString());
        }
    }
}
