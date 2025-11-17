namespace TodoBackend.Core.DTOs
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public AuthResponse? Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();

        public static AuthResult SuccessResult(AuthResponse data)
        {
            return new AuthResult { Success = true, Data = data };
        }

        public static AuthResult FailureResult(IEnumerable<string> errors)
        {
            return new AuthResult { Success = false, Errors = errors.ToList() };
        }

        public static AuthResult FailureResult(string error)
        {
            return new AuthResult { Success = false, Errors = new List<string> { error } };
        }
    }
}