using blogpessoal.Model;

namespace blogpessoal.NovaPasta3
{
    public interface IAuthService
    {
        Task<UserLogin?> Autenticar(UserLogin userLogin); 
    }
}
