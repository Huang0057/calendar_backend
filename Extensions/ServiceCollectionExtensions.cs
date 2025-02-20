namespace Calendar.API.Exceptions
{
    public class ServiceException : Exception
    {
        public ServiceException(string message) : base(message)
        {
        }

        public ServiceException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class EntityNotFoundException : BusinessValidationException
    {
        public EntityNotFoundException(string message) : base(message)
        {
        }

        public EntityNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class DuplicateEntityException : BusinessValidationException
    {
        public DuplicateEntityException(string message) : base(message)
        {
        }

        public DuplicateEntityException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class BusinessValidationException : Exception
    {
        public BusinessValidationException(string message) : base(message)
        {
        }

        public BusinessValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}