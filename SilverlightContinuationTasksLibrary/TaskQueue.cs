using System;
using System.Windows.Threading;
using System.Collections.Generic;

namespace ContinuationTasks
{
    public class TaskQueue
    {
        private class DelayTask
        {
            public Action ToRunAction;
            public TimeSpan DelaySpan;
            public Func<bool> CanRun;
            public int MaxAttempts;
            public int AttemptCount;
        };

        private DispatcherTimer _DelayTimer;
        private List<DelayTask> _TaskQueue;
        private int _CurrentTaskIndex;

        public TaskQueue()
        {
            _TaskQueue = new List<DelayTask>();
            _DelayTimer = new DispatcherTimer();
            _DelayTimer.Tick += new EventHandler(On_DelayTimer_Tick);
            _CurrentTaskIndex = 0;
        }

        private void On_DelayTimer_Tick(object sender, EventArgs e)
        {
            DelayTask currentTask = _TaskQueue[_CurrentTaskIndex];

            if (currentTask.CanRun == null)
                RunTask();
            else
                if (currentTask.MaxAttempts > 0)
                {
                    currentTask.AttemptCount++;
                    if (currentTask.CanRun() || currentTask.AttemptCount >= currentTask.MaxAttempts)
                        RunTask();
                }
                else
                    if (currentTask.CanRun())
                        RunTask();
        }

        private void RunTask()
        {
            _DelayTimer.Stop();
            _TaskQueue[_CurrentTaskIndex].ToRunAction();

            _CurrentTaskIndex++;
            if (_CurrentTaskIndex < _TaskQueue.Count)
            {
                _TaskQueue[_CurrentTaskIndex].AttemptCount = 0;
                Start(false);
            }
        }

        public void AddTask(Action toRunAction, int delayMilliseconds = 100, Func<bool> canRun = null, int maxAttempts = 0)
        {
            if (toRunAction != null && delayMilliseconds > 0)
                _TaskQueue.Add(new DelayTask() { ToRunAction = toRunAction, DelaySpan = TimeSpan.FromMilliseconds(delayMilliseconds), CanRun = canRun, MaxAttempts = maxAttempts, AttemptCount = 0 });
        }

        public void Start(bool restart = false)
        {
            if (_TaskQueue.Count > 0)
            {
                if (restart)
                    _CurrentTaskIndex = 0;

                _DelayTimer.Interval = _TaskQueue[_CurrentTaskIndex].DelaySpan;
                _DelayTimer.Start();
            }
        }

        public void Reset()
        {
            if (_DelayTimer.IsEnabled)
                _DelayTimer.Stop();

            _TaskQueue.Clear();
            _CurrentTaskIndex = 0;
        }

        public void RunSingleTask(Action toRunAction, int delayMilliseconds = 100, Func<bool> canRun = null, int maxAttempts = 0)
        {
            Reset();
            AddTask(toRunAction, delayMilliseconds, canRun, maxAttempts);
            Start(false);
        }
    }
}

////////////////////////////////////////////////////////////////////////////////////////////////////
//
//	Copyright 2012 Abel Cheng
//	This source code is subject to terms and conditions of the MIT License.
//	You must not remove this notice, or any other, from this software.
//
//	Original Author:	Abel Cheng <abelcys@gmail.com>
//	Created Date:		2012-12-19
//	Source Host:		http://sctl.codeplex.com
//	Change Log:
//	Author				Date			Comment
//
//
//
//
//	(Keep Code Clean)
//
////////////////////////////////////////////////////////////////////////////////////////////////////
