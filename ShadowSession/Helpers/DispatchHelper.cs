namespace ShadowSession.Helpers
{
    public static class DispatchHelper 
    {
        public static void InvokeDispatch(Delegate action, params object[] args)
        {
            var dispatch = System.Windows.Application.Current?.Dispatcher;

            if (dispatch == null)
            {
                action.DynamicInvoke(args);
            }
            else
            {
                dispatch.Invoke(action, args);
            }
        }
    }
}
