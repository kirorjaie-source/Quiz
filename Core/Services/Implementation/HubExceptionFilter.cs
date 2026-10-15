using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementation
{
    // بيحوّل أي exception في الـ Hub لرسالة مفهومة توصل للكلاينت (HubException)
    public class HubExceptionFilter : IHubFilter
    {
        public async ValueTask<object?> InvokeMethodAsync(
            HubInvocationContext context,
            Func<HubInvocationContext, ValueTask<object?>> next)
        {
            try
            {
                return await next(context);
            }
            catch (HubException)
            {
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                throw new HubException("You are not allowed to do this");
            }
            catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException)
            {
                throw new HubException(ex.Message);
            }
            catch (Exception)
            {
                throw new HubException("Something went wrong");
            }
        }
    }
}
