using Content.Server.Antag;
using Content.Server._NF.UplinkUnlocker.Components;
using Content.Server.PDA.Ringer;
using Content.Server.Roles;
using Content.Server.Traitor.Uplink;
using Content.Shared.Interaction;
using Content.Shared.Mind;
using Content.Shared.PDA;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server._NF.UplinkUnlocker;

/// <summary>
///     Handles using the Syndicate Uplink Modchip on a PDA to add the traitor syndicate uplink (ringtone + store).
///     Grants a special role so the uplink code appears in the character briefing (same as traitor).
/// </summary>
public sealed class UplinkUnlockerSystem : EntitySystem
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly UplinkSystem _uplink = default!;
    [Dependency] private readonly RoleSystem _role = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    private const string UplinkCodeRoleId = "MindRoleUplinkCode";

    private static readonly ProtoId<StoreCategoryPrototype> ModchipStoreCategory = "UplinkSyndicateModchip";

    private static readonly SoundSpecifier TraitorActivationSound = new SoundPathSpecifier("/Audio/Ambience/Antag/traitor_start.ogg");

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<UplinkUnlockerComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<UplinkCodeBriefingComponent, GetBriefingEvent>(OnGetBriefing);
    }

    private void OnGetBriefing(Entity<UplinkCodeBriefingComponent> ent, ref GetBriefingEvent args)
    {
        if (string.IsNullOrEmpty(ent.Comp.Code))
            return;
        args.Append(Loc.GetString("traitor-role-uplink-code-short", ("code", ent.Comp.Code)));
    }

    private void OnAfterInteract(Entity<UplinkUnlockerComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach || args.Target is not { } target)
            return;

        if (!HasComp<PdaComponent>(target))
            return;

        var comp = ent.Comp;
        var user = args.User;

        if (!_uplink.AddUplink(user, comp.Balance, target, comp.GiveDiscounts))
            return;

        // Frontier: restrict PDA store to modchip catalog only (encryption key, LPB pinpointer, business cards)
        if (TryComp<StoreComponent>(target, out var store))
        {
            store.Categories.Clear();
            store.Categories.Add(ModchipStoreCategory);
            Dirty(target, store);
        }

        var ev = new GenerateUplinkCodeEvent();
        RaiseLocalEvent(target, ref ev);

        if (ev.Code is { } code)
        {
            var codeStr = string.Join("-", code).Replace("sharp", "#");

            if (_mind.TryGetMind(user, out var mindId, out var mindComp))
            {
                _role.MindAddRole(mindId, UplinkCodeRoleId, mindComp, silent: true);
                if (_role.MindHasRole<UplinkCodeBriefingComponent>((mindId, mindComp), out var roleEnt))
                {
                    roleEnt.Value.Comp2.Code = codeStr;
                    Dirty(roleEnt.Value.Owner, roleEnt.Value.Comp2);
                }

                _antag.SendBriefing(user, Loc.GetString("traitor-role-uplink-code-short", ("code", codeStr)), null, TraitorActivationSound);
            }
        }

        if (comp.UseSound is { } useSound)
            _audio.PlayPvs(useSound, target);

        Del(ent.Owner);
        args.Handled = true;
    }
}
