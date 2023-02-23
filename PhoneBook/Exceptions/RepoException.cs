namespace PhoneBook.Exceptions
{
    public class RepoException : Exception
    {
        public int StatusCode { get; }
        public RepoException(int statusCode, string message) : base(message)
        { 
            StatusCode = statusCode;
        }
    }
}
