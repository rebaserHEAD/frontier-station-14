using Content.Shared.FixedPoint;
using Robust.Shared.Audio;

namespace Content.Server._NF.SyndicateUplinkModchip.Components;

/// <summary>
///     Syndicate Uplink Modchip: when used on a PDA, adds the traitor syndicate uplink (same as SS14: ringtone code + store).
///     Consumed on use.
/// </summary>
[RegisterComponent]
public sealed partial class SyndicateUplinkModchipComponent : Component
{
    /// <summary>
    ///     Telecrystal balance to grant when unlocking the PDA uplink.
    /// </summary>
    [DataField]
    public FixedPoint2 Balance = 0;

    /// <summary>
    ///     Whether to apply traitor uplink discounts.
    /// </summary>
    [DataField]
    public bool GiveDiscounts = false;

    /// <summary>
    ///     Sound played when the modchip is successfully used on a PDA. Prototype uses emag (sparks) by default.
    /// </summary>
    [DataField]
    public SoundSpecifier? UseSound = new SoundPathSpecifier("/Audio/Machines/id_insert.ogg");
}
