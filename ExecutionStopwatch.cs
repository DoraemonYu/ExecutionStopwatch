/* MIT Licence
 * https://github.com/DoraemonYu/ExecutionStopwatch
 */

using System;
using System.Runtime.InteropServices;

namespace System
{
    /// <summary>
    /// Calculates the CPU execution time for the current thread / process (non-thread-safe class)
    /// </summary>
    /// <summary xml:lang="zh">
    /// 计算当前线程/进程的CPU已执行时间(非线程安全类)
    /// </summary>
    public class ExecutionStopwatch
    {

        //------------Members------------

        #region 状态记录

        //开始和结束的时间点     Start and end time points
        private long m_startTimeStamp;
        private long m_endTimeStamp;

        //存储已经过的时间（用于支持多次Start、Stop）        Stores the elapsed time (used to support multiple Start and Stop)
        private long m_savedPassTime = 0;

        //标记是否正在执行      Flag is being executed
        private bool m_isRunning;

        //类型        Type
        readonly CalcTarget calcTarget;

        #endregion

        #region 结果值

        /// <summary>
        /// Elapsed time, milliseconds (recommended; make sure to call Stop to stop the meter before getting it)
        /// </summary>
        /// <summary xml:lang="zh">
        /// 经过的时间，毫秒(推荐使用；获取前，请确保已先调用Stop暂停咪表)
        /// </summary>
        public long ElapsedMilliseconds
        {
            get
            {
                return GetMillisecondsFromTick(m_savedPassTime);
            }
        }

        /// <summary>
        /// Elapsed Time (ElapsedMilliseconds is recommended for use)
        /// </summary>
        /// <summary xml:lang="zh">
        /// 经过的时间(推荐直接获取ElapsedMilliseconds)
        /// </summary>
        public TimeSpan Elapsed
        {
            get
            {
                return TimeSpan.FromMilliseconds(ElapsedMilliseconds);
            }
        }

        /// <summary>
        /// Is being executed
        /// </summary>
        /// <summary xml:lang="zh">
        /// 是否正在执行
        /// </summary>
        public bool IsRunning
        {
            get { return m_isRunning; }
        }


        #endregion


        //------------PInvoke------------

        #region win32 API
        
        /// <summary>
        /// Get the handle of current thread
        /// </summary>
        /// <summary xml:lang="zh">
        /// 获取当前线程句柄
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetCurrentThread();

        /// <summary>
        /// Gets the timestamp of the current thread
        /// </summary>
        /// <summary xml:lang="zh">
        /// 获取当前线程的时间点
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetThreadTimes(IntPtr hThread, out long lpCreationTime,
                                          out long lpExitTime, out long lpKernelTime, out long lpUserTime);
        /// <summary>
        /// Gets the timestamp of the current process
        /// </summary>
        /// <summary xml:lang="zh">
        /// 获取当前进程的时间点
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetProcessTimes(IntPtr hProcess, out long lpCreationTime, out long lpExitTime, 
                                          out long lpKernelTime, out long lpUserTime);

        #endregion


        //------------Structs------------

        #region 类型枚举

        /// <summary>
        /// Measurement objectives
        /// </summary>
        /// <summary xml:lang="zh">
        /// 计量目标
        /// </summary>
        public enum CalcTarget
        {
            /// <summary>
            /// Thread
            /// </summary>
            /// <summary xml:lang="zh">
            /// 线程级别
            /// </summary>
            ThreadLevel,

            /// <summary>
            /// Process
            /// </summary>
            /// <summary xml:lang="zh">
            /// 进程级别
            /// </summary>
            ProcessLevel,
        }

        #endregion


        //------------Control------------

        #region 构造函数

        /// <summary>
        /// Constructor
        /// </summary>
        /// <summary xml:lang="zh">
        /// 构造函数
        /// </summary>
        public ExecutionStopwatch(CalcTarget target = CalcTarget.ThreadLevel)
        {
            this.calcTarget = target;
        }

        #endregion

        #region 开始/继续 计时

        /// <summary>
        /// Start / Continue Timing (meter pressed)
        /// </summary>
        /// <summary xml:lang="zh">
        /// 开始/继续 计时(咪表按下)
        /// </summary>
        public void Start()
        {
            m_isRunning         = true;

            //标记起始点     Marks the starting point
            long timestamp      = GetTimes();
            m_startTimeStamp    = timestamp;
        }

        #endregion

        #region 暂停计时

        /// <summary>
        /// Time-out (meter up)
        /// </summary>
        /// <summary xml:lang="zh">
        /// 暂停计时(咪表弹起)
        /// </summary>
        public void Stop()
        {
            m_isRunning = false;

            //标记停止点     Mark the stop point
            long timestamp = GetTimes();
            m_endTimeStamp = timestamp;

            //累加存储本轮已经过时间       Cumulative storage This round has elapsed
            System.Threading.Interlocked.Add(ref m_savedPassTime, m_endTimeStamp - m_startTimeStamp);
        }

        #endregion

        #region 重置

        /// <summary>
        /// Reset (Meter Clear)
        /// </summary>
        /// <summary xml:lang="zh">
        /// 重置(咪表清零)
        /// </summary>
        public void Reset()
        {
            m_startTimeStamp    = 0;
            m_endTimeStamp      = 0;
            m_savedPassTime     = 0;
        }

        /// <summary>
        /// Reset and start timing
        /// </summary>
        /// <summary xml:lang="zh">
        /// 重置并开始计时
        /// </summary>
        public void Restart()
        {
            Reset();
            Start();
        }

        #endregion


        #region 通用方法

        /// <summary>
        /// Tick to milliseconds
        /// </summary>
        /// <summary xml:lang="zh">
        /// Tick转毫秒
        /// </summary>
        private static long GetMillisecondsFromTick(long time)
        {
            return time / 10000;
        }

        #endregion

        #region 计量时间

        /// <summary>
        /// Gets the current timestamp
        /// </summary>
        /// <summary xml:lang="zh">
        /// 获取当前时间点
        /// </summary>
        private long GetTimes()
        {
            switch (this.calcTarget)
            {
                default:
                case CalcTarget.ThreadLevel:
                    return GetThreadTimes();

                case CalcTarget.ProcessLevel:
                    return GetProcessTimes();
            }
        }

        /// <summary>
        /// Gets the current timestamp of the current thread
        /// </summary>
        /// <summary xml:lang="zh">
        /// 获取【当前线程】的当前时间点
        /// </summary>
        private static long GetThreadTimes()
        {
            IntPtr threadHandle = GetCurrentThread();
            if (threadHandle == IntPtr.Zero)
            {
                throw new Exception(string.Format("failed to get handle of target thread. error code: {0}", Marshal.GetLastWin32Error()));
            }

            long notIntersting;
            long kernelTime, userTime;

            var success = GetThreadTimes(threadHandle,
                                         out notIntersting,
                                         out notIntersting,
                                         out kernelTime, out userTime);

            if (!success)
            {
                throw new Exception(string.Format("failed to get timestamp. error code: {0}", Marshal.GetLastWin32Error()));
            }

            long result = kernelTime + userTime;
            return result;
        }

        /// <summary>
        /// Gets the current timestamp of the current process
        /// </summary>
        /// <summary xml:lang="zh">
        /// 获取【当前进程】的当前时间点
        /// </summary>
        private static long GetProcessTimes()
        {
            IntPtr processHandle = System.Diagnostics.Process.GetCurrentProcess().Handle;
            if (processHandle == IntPtr.Zero)
            {
                throw new Exception(string.Format("failed to get handle of target process. error code: {0}", Marshal.GetLastWin32Error()));
            }

            long notIntersting;
            long kernelTime, userTime;

            var success = GetProcessTimes(processHandle,
                                         out notIntersting,
                                         out notIntersting,
                                         out kernelTime, out userTime);

            if (!success)
            {
                throw new Exception(string.Format("failed to get timestamp. error code: {0}", Marshal.GetLastWin32Error()));
            }

            long result = kernelTime + userTime;
            return result;
        }

        #endregion

    }
}
