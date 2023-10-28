using Microsoft.AspNetCore.Mvc;

namespace VoicerStudio.Api.Core;

[ApiController]
[Produces("application/json")]
public abstract class V1Controller : ControllerBase { }