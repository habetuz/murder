namespace Murder.DomainIdentity;

public class AuthenticatorBuilder
{
    private readonly Dictionary<Type, IAuthenticationMethod> _methods = [];

    public AuthenticatorBuilder Register<TCredential, TEnrollmentData, TMethod>(IAuthenticationMethod<TCredential, TEnrollmentData, TMethod> method)
        where TCredential : IIncomingCredential<TMethod>
        where TEnrollmentData : IEnrollmentData<TMethod>
        where TMethod : IAuthenticationMethodKey
    {
        _methods.Add(typeof(TMethod), new AuthenticationMethodAdapter<TCredential, TEnrollmentData, TMethod>(method));
        return this;
    }

    public Authenticator Build()
    {
        return new Authenticator(new Dictionary<Type, IAuthenticationMethod>(_methods));
    }

    private sealed class AuthenticationMethodAdapter<TCredential, TEnrollmentData, TMethod>(IAuthenticationMethod<TCredential, TEnrollmentData, TMethod> method)
        : IAuthenticationMethod
        where TCredential : IIncomingCredential<TMethod>
        where TEnrollmentData : IEnrollmentData<TMethod>
        where TMethod : IAuthenticationMethodKey
    {
        public Type MethodKeyType => typeof(TMethod);

        public IdentityId? Authenticate(object credential)
        {
            if (credential is not TCredential typedCredential)
                throw new AuthenticationMethodMismatchException(typeof(TCredential), credential.GetType());

            return method.Authenticate(typedCredential);
        }

        public object Enroll(object enrollmentData)
        {
            if (enrollmentData is not TEnrollmentData typedEnrollmentData)
                throw new AuthenticationMethodMismatchException(typeof(TEnrollmentData), enrollmentData.GetType());

            return method.Enroll(typedEnrollmentData);
        }
    }
}
