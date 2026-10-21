using System.Security;
using System.Security.Permissions;

// Lets the mono JIT skip access checks so publicized GameLibs members
// (e.g. GenericSkill._skillFamily) can be used directly.
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
[module: UnverifiableCode]
