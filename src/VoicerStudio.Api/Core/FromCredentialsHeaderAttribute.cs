using Microsoft.AspNetCore.Mvc;

namespace VoicerStudio.Api.Core;

public class FromCredentialsHeaderAttribute : FromHeaderAttribute
{
    public FromCredentialsHeaderAttribute()
    {
        Name = "X-Credentials";
    }
}