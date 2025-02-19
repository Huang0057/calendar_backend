namespace Calendar.API.Exceptions
{
    /// 代表在資料存取層發生的異常狀況
    /// 用於封裝和轉換底層資料庫異常
    public class RepositoryException : Exception
    {
        /// 使用指定的錯誤訊息初始化 RepositoryException              
        public RepositoryException(string message) 
            : base(message) 
        { }

        /// 使用指定的錯誤訊息和造成這個異常的內部異常，初始化 RepositoryException 

        public RepositoryException(string message, Exception innerException) 
            : base(message, innerException) 
        { }
    }
}