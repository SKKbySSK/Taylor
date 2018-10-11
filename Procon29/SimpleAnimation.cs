using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Procon29
{
    class SimpleAnimation
    {
        public SimpleAnimation(TimeSpan duration, Action<double> animator)
        {
            Duration = duration;
            Animator = animator;
        }

        public SimpleAnimation(TimeSpan duration, Action<double> animator, Action completed) : this(duration, animator)
        {
            CompletedAction = completed;
        }

        public event EventHandler Completed;

        public TimeSpan Duration { get; } = TimeSpan.FromMilliseconds(300);

        public bool IsAnimating { get; private set; } = false;

        public bool Loop { get; set; } = false;

        private Action<double> Animator { get; }

        private Action CompletedAction { get; }

        private DateTime Started { get; set; }

        public void Start()
        {
            if (!IsAnimating)
            {
                Started = DateTime.Now;
                System.Windows.Media.CompositionTarget.Rendering += CompositionTarget_Rendering;
                IsAnimating = true;
            }
        }

        public void Stop()
        {
            System.Windows.Media.CompositionTarget.Rendering -= CompositionTarget_Rendering;

            if (IsAnimating)
            {
                CompletedAction?.Invoke();
                Completed?.Invoke(this, new EventArgs());
                IsAnimating = false;
            }
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            var delta = DateTime.Now - Started;
            if (delta >= Duration)
            {
                if (Loop)
                {
                    Started = DateTime.Now;
                }
                else
                {
                    System.Windows.Media.CompositionTarget.Rendering -= CompositionTarget_Rendering;
                    Animator(1);
                    IsAnimating = false;
                }

                CompletedAction?.Invoke();
                Completed?.Invoke(this, new EventArgs());
            }
            else
            {
                Animator(delta.TotalMilliseconds / Duration.TotalMilliseconds);
            }
        }
    }
}
