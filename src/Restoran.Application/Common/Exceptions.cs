namespace Restoran.Application.Common;

/// <summary>Objekti ei leitud (API tagastab 404).</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

/// <summary>Andmed on vastuolus, nt laud on juba broneeritud (API tagastab 409).</summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}

/// <summary>Äriloogika reegel ei luba tegevust (API tagastab 400).</summary>
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}

/// <summary>Kasutajal pole õigust seda objekti muuta (API tagastab 403).</summary>
public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message) { }
}
