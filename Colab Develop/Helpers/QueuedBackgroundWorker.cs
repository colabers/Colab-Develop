using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;

namespace Colab.Develop
{
    [System.ComponentModel.DesignerCategory("Code")]
    public class QueuedBackgroundWorker : BackgroundWorker, INotifyPropertyChanged
    {
        private ConcurrentQueue<QueuedWork> queue = new ConcurrentQueue<QueuedWork>();
        public QueuedBackgroundWorker()
        {
            WorkerSupportsCancellation = true;
            DoWork += worker_DoWork;
        }

        public int QueueLength
        {
            get
            {
                return queue.Count;
            }
        }

        public Boolean Working { get; set; }
        
        public String CurrentWork { get; set; }
        
        public void Enqueue(String name, Action action)
        {
            queue.Enqueue(new QueuedWork() { Name = name, Action = action });
            if (!IsBusy)
            {
                RunWorkerAsync();
            }
            OnPropertyChanged("QueueLength");
        }

        public void CancelAll()
        {
            if (IsBusy)
            {
                CancelAsync();
            }
            QueuedWork dummy;
            while (queue.TryDequeue(out dummy)) { }
            OnPropertyChanged("QueueLength");
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!queue.IsEmpty && !CancellationPending)
            {
                QueuedWork work;
                if (queue.TryDequeue(out work))
                {
                    CurrentWork = work.Name;
                    Working = true;
                    OnPropertyChanged("QueueLength");
                    OnPropertyChanged("CurrentWork");
                    OnPropertyChanged("Working");
                    try
                    {
                        work.Action();
                    }
                    catch
                    { }
                }
            }
            CurrentWork = "";
            Working = false;
            OnPropertyChanged("CurrentWork");
            OnPropertyChanged("Working");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    class QueuedWork
    {
        public String Name { get; set; }
        public Action Action { get; set; }
    }
}
