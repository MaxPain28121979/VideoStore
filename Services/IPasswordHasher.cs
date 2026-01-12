namespace VideoStore.Services
{
    public interface IPasswordHasher<T>
    {
        string HashPassword(T user, string password);
        bool VerifyHashedPassword(T user, string hashedPassword, string providedPassword);
    }
}
