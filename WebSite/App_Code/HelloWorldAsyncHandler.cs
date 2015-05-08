using System;
using System.Web;
using System.Threading;

namespace ololo
{

    class HelloWorldAsyncHandler : IHttpAsyncHandler
    {
        public bool IsReusable { get { return false; } }

        public HelloWorldAsyncHandler()
        {
        }
        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, Object extraData)
        {
            context.Response.Write("<p>Begin IsThreadPoolThread is " + Thread.CurrentThread.IsThreadPoolThread + "</p>\r\n");
            AsynchOperation asynch = new AsynchOperation(cb, context, extraData);
            asynch.StartAsyncWork();
            return asynch;
        }

        public void EndProcessRequest(IAsyncResult result)
        {
        }

        public void ProcessRequest(HttpContext context)
        {
            throw new InvalidOperationException();
        }
    }

    class AsynchOperation : IAsyncResult
    {
        private bool _completed;
        private Object _state;
        private AsyncCallback _callback;
        private HttpContext _context;

        bool IAsyncResult.IsCompleted { get { return _completed; } }
        WaitHandle IAsyncResult.AsyncWaitHandle { get { return null; } }
        Object IAsyncResult.AsyncState { get { return _state; } }
        bool IAsyncResult.CompletedSynchronously { get { return false; } }

        public AsynchOperation(AsyncCallback callback, HttpContext context, Object state)
        {
            _callback = callback;
            _context = context;
            _state = state;
            _completed = false;
        }

        public void StartAsyncWork()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(StartAsyncTask), null);
        }

        private void StartAsyncTask(Object workItemState)
        {

            _context.Response.Write("<p>Completion IsThreadPoolThread is " + Thread.CurrentThread.IsThreadPoolThread + "</p>\r\n");

            _context.Response.Write("Hello World from Async Handler!");
            _completed = true;
            _callback(this);
        }
    }
}