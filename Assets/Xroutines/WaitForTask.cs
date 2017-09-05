﻿using UnityEngine;
using System;
using System.Threading;
using System.Collections;

public class WaitForTask : IEnumerator
{
	private object objLock;

	private bool started;
	private bool done;
	private bool threaded;

	private Action[] actions;

	private Thread backgroundThread;
	public WaitForTask(bool threaded = false,params Action[] actions)
	{
		this.started = false;
		this.done = false;
		//TODO: check for platform, set threaded to false if not supported
		this.threaded = threaded;
		this.actions = actions;
		if (threaded) {
			this.objLock = new object ();
		} 
	}

	public void execute(object state){
		if (threaded) {
			lock (objLock) {
				backgroundThread = Thread.CurrentThread;
			}
		} 
		for (int i = 0; i < actions.Length; i++) {
			actions[i].Invoke ();
		}
		if (threaded) {
			lock (objLock) {
				done = true;
			}
		} else {
			done = true;
		}
	}

	public void Stop() {
		if (!threaded) {
			return;
		}
		lock (objLock) {
			if (!done && backgroundThread != null && backgroundThread.IsAlive) {
				backgroundThread.Abort();
				backgroundThread = null;
				done = true;
			}
		}

	}

	public bool keepWaiting
	{
		get
		{
			if (threaded) {
				lock (objLock) {
					return !done;
				}
			}
			return !done;
		}
	}

	#region IEnumerator implementation
	public bool MoveNext ()
	{
		if (!this.started) {
			this.started = true;
			if (this.threaded) {
				ThreadPool.QueueUserWorkItem (new WaitCallback (execute));
			} else {
				execute (null);
			}
		}
		return this.keepWaiting;
	}

	public void Reset ()
	{
	}
	public object Current {
		get {
			return null;
		}
	}
	#endregion
}
