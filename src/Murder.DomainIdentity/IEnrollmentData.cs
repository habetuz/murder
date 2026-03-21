namespace Murder.DomainIdentity;

public interface IEnrollmentData<out TMethod>
    where TMethod : IAuthenticationMethodKey
{
}