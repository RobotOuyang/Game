using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
using System.IO;

namespace LuaFramework
{
	
	public class ThreadLoadManager : Manager {

		public delegate void ThreadLoadDel(byte[] dt);

		public class ThreadInfo
		{
			public string path;
			public byte[] dt = null;
			public Thread thread = null;
			public int threadIndex = -1;
			public ThreadLoadDel callBack;
		}

		private int threadLoadIndex = -1;
		private int res_num = 0;

		public List<ThreadInfo> reLoadQueue = new List<ThreadInfo> ();
		public List<ThreadInfo> LoadQueue = new List<ThreadInfo>();
		public Queue<int> freeThread = new Queue<int> ();

		void Start () {
			InitThreadLoadManager();
		}

		void LateUpdate()
        {
            ThreadLoadTimerCallBack();
        }

		private void ReSetThreadLoadManager()
		{
			threadLoadIndex = -1;
		}

		private void ThreadLoadTimerCallBack()
		{
			for(int i = LoadQueue.Count - 1; i >= 0; i--)
			{
				if(LoadQueue[i].dt != null)
				{
					var ifn = LoadQueue [i];
					ifn.callBack (ifn.dt);
					freeThread.Enqueue (ifn.threadIndex);
					LoadQueue.RemoveAt (i);
				}
			}
			if(LoadQueue.Count < 10 && reLoadQueue.Count > 0)
			{
				var reLoadInfo = reLoadQueue[reLoadQueue.Count - 1];
				reLoadInfo.threadIndex = freeThread.Dequeue ();
				reLoadQueue.RemoveAt(reLoadQueue.Count - 1);
				LoadQueue.Add (reLoadInfo);
				StartThread (reLoadInfo);
			}
		}

		private void StartLoadThread0()
		{
			ThreadTodo (0);
		}

		private void StartLoadThread1()
		{
			ThreadTodo (1);
		}

		private void StartLoadThread2()
		{
			ThreadTodo (2);
		}

		private void StartLoadThread3()
		{
			ThreadTodo (3);
		}

		private void StartLoadThread4()
		{
			ThreadTodo (4);
		}

		private void StartLoadThread5()
		{
			ThreadTodo (5);
		}

		private void StartLoadThread6()
		{
			ThreadTodo (6);
		}

		private void StartLoadThread7()
		{
			ThreadTodo (7);
		}

		private void StartLoadThread8()
		{
			ThreadTodo (8);
		}

		private void StartLoadThread9()
		{
			ThreadTodo (9);
		}

		private void ThreadTodo(int threadIndex)
		{
			ThreadInfo currInfo = null;
			for(int i = 0; i < LoadQueue.Count; i++)
			{
				if(LoadQueue[i].threadIndex == threadIndex)
				{
					currInfo = LoadQueue [i];
					break;
				}
			}	
			if(currInfo == null)
			{
				Debug.LogError ("严重错乱逻辑，currInfo is null.");
				return;
			}
			var bt = System.IO.File.ReadAllBytes (currInfo.path);
			res_num = res_num + 1;
			Debug.LogError(res_num);
			currInfo.thread = null;
			currInfo.dt = bt;
		}

		private void StartThread(ThreadInfo info)
		{
			switch(info.threadIndex)
			{
			case 0:
				info.thread = new Thread (StartLoadThread0);
				info.thread.Start ();
				break;
			case 1:
				info.thread = new Thread (StartLoadThread1);
				info.thread.Start ();
				break;
			case 2:
				info.thread = new Thread (StartLoadThread2);
				info.thread.Start ();
				break;
			case 3:
				info.thread = new Thread (StartLoadThread3);
				info.thread.Start ();
				break;
			case 4:
				info.thread = new Thread (StartLoadThread4);
				info.thread.Start ();
				break;
			case 5:
				info.thread = new Thread (StartLoadThread5);
				info.thread.Start ();
				break;
			case 6:
				info.thread = new Thread (StartLoadThread6);
				info.thread.Start ();
				break;
			case 7:
				info.thread = new Thread (StartLoadThread7);
				info.thread.Start ();
				break;
			case 8:
				info.thread = new Thread (StartLoadThread8);
				info.thread.Start ();
				break;
			case 9:
				info.thread = new Thread (StartLoadThread9);
				info.thread.Start ();
				break;
			}
		}

		public void InitThreadLoadManager()
		{
			freeThread.Enqueue (0);
			freeThread.Enqueue (1);
			freeThread.Enqueue (2);
			freeThread.Enqueue (3);
			freeThread.Enqueue (4);
			freeThread.Enqueue (5);
			freeThread.Enqueue (6);
			freeThread.Enqueue (7);
			freeThread.Enqueue (8);
			freeThread.Enqueue (9);
			// threadLoadIndex = TimerTools.Instance.AddAction (ThreadLoadTimerCallBack, 0f, true, "ThreadLoadTimerCallBack");
		}

		public void LoadBytes(string path, ThreadLoadDel callBack)
		{
			ThreadInfo info = new ThreadInfo ();
			info.path = path;
			info.callBack = callBack;
			info.dt = null;
			reLoadQueue.Add (info);
		}
	}
}