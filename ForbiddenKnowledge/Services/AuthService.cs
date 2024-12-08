namespace ForbiddenKnowledge.Services
{
    public class AuthService
    {
        public bool IsAuthenticated { get; private set; }
        public string? Pseudonym { get; private set; }


        public void Authenticate(string pseudonym)
        {
            IsAuthenticated = true;
            Pseudonym = pseudonym;
        }

        public void Logout()
        {
            IsAuthenticated = false;
            Pseudonym = null;
        }






    }
}
