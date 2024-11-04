using System;
using System.Threading;
using System.Threading.Tasks;

public class Throttler
{
    private double interval;
    private double lastExecutionTime = 0;
    private System.Threading.Timer timer;

    public Throttler(double interval)
    {
        this.interval = interval;
    }

    public void Exec(Action fn)
    {
        double currentTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        if (currentTime - lastExecutionTime > interval)
        {
            fn.Invoke();
            lastExecutionTime = currentTime;
        }
        else
        {
            timer?.Dispose();
            var delay = (int)(interval - (currentTime - lastExecutionTime));
            timer = new System.Threading.Timer(_ =>
            {
                fn.Invoke();
                timer?.Dispose();
            }, null, delay, Timeout.Infinite);
        }
    }
}