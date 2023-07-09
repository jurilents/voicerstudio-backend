using Microsoft.AspNetCore.Mvc;

namespace VoicerStudio.Api.Controllers.Core;

public class FromCredentialsHeader : FromHeaderAttribute
{
    public FromCredentialsHeader()
    {
        Name = "X-Credentials";
    }
}