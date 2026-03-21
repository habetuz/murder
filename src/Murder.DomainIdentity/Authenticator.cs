namespace Murder.DomainIdentity;

public class Authenticator
{
    private readonly IReadOnlyDictionary<Type, IAuthenticationMethod> _methods;

    internal Authenticator(IReadOnlyDictionary<Type, IAuthenticationMethod> methods)
    {
        _methods = methods;
    }

    public IdentityId? Authenticate<TMethod>(IIncomingCredential<TMethod> credential)
        where TMethod : IAuthenticationMethodKey
    {
        var method = _methods[typeof(TMethod)];
        return method.Authenticate(credential!);
    }

    public EnrollmentResult<TMethod> Enroll<TMethod>(IEnrollmentData<TMethod> enrollmentData)
        where TMethod : IAuthenticationMethodKey
    {
        var method = _methods[typeof(TMethod)];
        return (EnrollmentResult<TMethod>)method.Enroll(enrollmentData!);
    }
}