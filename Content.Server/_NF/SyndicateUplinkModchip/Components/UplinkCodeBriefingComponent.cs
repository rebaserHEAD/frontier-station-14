using Content.Shared.Roles;

namespace Content.Server._NF.SyndicateUplinkModchip.Components;

/// <summary>
///     Mind role component that shows the PDA uplink code in the character briefing (same UI as traitor uplink code).
///     Code is set at runtime when the player uses a Syndicate Uplink Modchip on a PDA.
/// </summary>
[RegisterComponent]
public sealed partial class UplinkCodeBriefingComponent : BaseMindRoleComponent
{
    /// <summary>
    ///     The ringtone code to show in the briefing. Set after the role is added.
    /// </summary>
    [DataField]
    public string? Code;
}
