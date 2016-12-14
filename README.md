# ExecutionStopwatch
Calculates the CPU execution time for the current thread / process  
计算当前线程/进程的CPU执行时间

# Platform(适用的目标平台)
Only work in windows OS  and  dotNet framewrok. 只支持在Windows操作和.net框架下正常工作。  
Source code is written by C#, PInvoke, windows32 api .  

# Tips(提示)
- Basic usage, consistent with System.Diagnostics.Stopwatch.基本用法，跟System.Diagnostics.Stopwatch一致。  

  At the beginning of the timing position, through the Start method to press the meter; in order to get the elapsed time, through the Stop method pops up the meter, while ElapsedMilliseconds get the number of milliseconds.  
  在开始计时的位置，通过Start方法按下咪表；在想获取已经过时间的位置，通过Stop方法弹起咪表，同时通过ElapsedMilliseconds得到经过的毫秒数。
  
- If you do not need to clear to continue the meter, be sure to resume as soon as possible  through the Start() method.  
  如果需要不清零继续计时，请务必在尽早恢复Start()。
  
```
        var esw = new ExecutionStopwatch();
        esw.Start();
        //xxxx-A
        esw.Stop();
        var passTime = esw.ElapsedMilliseconds;
  
        esw.Start();             //<---- As soon as possible to restore Start (), do other logical judgments   尽早恢复Start()，再做其他逻辑判断
        if (passTime >= xxxxxx)
        {
             //yyyyy
        }
        //xxxx-B
        
        esw.Stop();
        var AllTime = esw.ElapsedMilliseconds;
```

- Do not call frequently in high frequency cycles. Because the call also consumes CPU resources. In this case, set the interval to a fixed time interval.  
  不要在高频循环里频繁调用。因为调用的时候也消耗CPU资源。这种情况下，请间隔固定时间段去计时。

