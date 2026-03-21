namespace Murder.DomainIdentity;

public interface IIncomingCredential<out TMethod>
    where TMethod : IAuthenticationMethodKey
{
}