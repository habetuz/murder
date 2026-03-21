namespace Murder.DomainIdentity;

public interface IStoredCredential<out TMethod>
    where TMethod : IAuthenticationMethodKey { }
