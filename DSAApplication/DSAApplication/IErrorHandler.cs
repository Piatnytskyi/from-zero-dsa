using System;

namespace DSAApplication
{
    public interface IErrorHandler
    {
        void HandleError(Exception ex);
    }
}
