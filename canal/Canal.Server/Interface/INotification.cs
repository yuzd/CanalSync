using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Canal.Server.Interface
{

    public interface INotificationHandler<TNotification> where TNotification : INotification
    {
        Task Handle(List<TNotification> notification);
    }


    public interface INotification
    {
    }
}
