namespace tactgame.com.Models
{
    public class ResponseData
    {
        #region Properties

        /// <summary>
        /// Gets or sets the request status.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Gets or sets the request data.
        /// </summary>
        public object ResponseMessage { get; set; }

        #endregion

        public ResponseData(bool isSuccess, object data = null)
        {
            IsSuccess = isSuccess;
            ResponseMessage = data;
        }
    }
}