namespace BitFab.KW1281Test.Models
{
    public class ActuatorTestControl
    {
        private TaskCompletionSource<bool>? _tcs;

        public bool StopRequested { get; private set; }

        public void RequestNext()
        {
            _tcs?.TrySetResult(true);
        }

        public void RequestStop()
        {
            StopRequested = true;
            _tcs?.TrySetResult(false);
        }

        public Task<bool> WaitForNextStepAsync()
        {
            _tcs = new TaskCompletionSource<bool>();
            return _tcs.Task;
        }
    }
}
