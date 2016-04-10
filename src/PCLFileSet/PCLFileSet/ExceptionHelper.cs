using System;
using System.Threading.Tasks;

namespace PCLFileSet
{
    public static class ExceptionHelper
    {
        /// <summary>
        /// Waits for the task to complete and captures <see cref="AggregateException"/> 
        /// if thrown and translates to the contained exception, if there is only one.
        /// </summary>
        /// <param name="taskToWaitFor">The task to wait for.</param>
        public static void WaitForTaskAndTranslateAggregateExceptions(this Task taskToWaitFor)
        {
            TranslateAggregateExceptions(taskToWaitFor.Wait);
        }

        /// <summary>
        /// Captures <see cref="AggregateException"/> if thrown and translates to the 
        /// contained exception, if there is only one.
        /// </summary>
        /// <param name="action">The action.</param>
        public static void TranslateAggregateExceptions(this Action action)
        {
            try
            {
                action();
            }
            catch (AggregateException agg)
            {
                if (agg.InnerExceptions.Count == 1)
                    throw agg.InnerExceptions[0];

                throw;
            }
        }
    }
}