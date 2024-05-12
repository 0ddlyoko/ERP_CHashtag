namespace lib;

// Inspired by https://dvoituron.com/2020/01/22/UnitTest-DateTime/
// Also by https://stackoverflow.com/a/4190793 for [ThreadStatic]
public class DateTimeProvider
{
    public static DateTime Now => DateTimeProviderContext.Current?.ContextDateTimeNow ?? DateTime.Now;
    public static DateTime UtcNow => Now.ToUniversalTime();
    public static DateTime Today => Now.Date;

    public class DateTimeProviderContext : IDisposable
    {
        internal readonly DateTime ContextDateTimeNow;
        private static AsyncLocal<Stack<DateTimeProviderContext>>? _threadScopeStack;

        public DateTimeProviderContext(DateTime contextDateTimeNow)
        {
            ContextDateTimeNow = contextDateTimeNow;
            _threadScopeStack ??= new AsyncLocal<Stack<DateTimeProviderContext>>();
            _threadScopeStack.Value ??= new Stack<DateTimeProviderContext>();
            _threadScopeStack.Value.Push(this);
        }

        public static DateTimeProviderContext? Current
        {
            get
            {
                if (_threadScopeStack!.Value!.Count == 0)
                    return null;
                return _threadScopeStack!.Value!.Peek();
            }
        }

        public void Dispose()
        {
            _threadScopeStack!.Value!.Pop();
        }
    }
}