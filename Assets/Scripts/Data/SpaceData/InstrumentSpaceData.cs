using Events.EventDataStructures;
using Logging;
using UnityEngine;

[CreateAssetMenu(fileName = "InstrumentSpaceData", menuName = "Board Spaces/Instrument Space")]
public class InstrumentSpaceData : OwnableSpaceData
{
    [SerializeField] public int[] researchFundingLevels = new int[4];
    
    public override void OnLanded(Player player)
    {
        base.OnLanded(player);
        // because events are asynchronous and execute on delays,
        // we can be pretty safe in assuming that when this returns when a space is unowned,
        // that it will continue to be unowned through the rest of this execution block.
        // I guess thats not ideal design, and is kind of a 'race condition' of sorts, and we could
        // modify the method signatures or something to flag stuff.
        if (owner == null) return;

        if (owner.Equals(player))
        {
            // do nothing, I think based on rules
        }
        else
        {
            int instrumentsOwned = owner.GetNumberInstrumentsOwned();
            switch (instrumentsOwned)
            {
                case > 0 and < 5:
                    // charge player from researchFundingLevels at the correct index (i-1)
                    int chargeAmount = researchFundingLevels[instrumentsOwned - 1];
                    chargeOwnershipFeeEventChannel.RaiseEvent(new ChargeOwnershipFeeEvent(
                        player, owner,
                        chargeAmount, this));
                    break;
                case 0:
                    Logging.Logger.Error("InstrumentSpaceData.OnLanded", 
                        "Owner player does not have record of owning any Instruments.",
                        LogCategory.Economy, this);
                    break;
                default:
                    Logging.Logger.Error("InstrumentSpaceData.OnLanded", 
                        "Owner player more than 4 Instruments in inventory.",
                        LogCategory.Economy, this);
                    break;
            }
        }
    }

    public override void OnPassed(Player player)
    {
        // do nothing
    }

    public override SpaceHoverEvent OnHover()
    {
        var payload = base.OnHover();

        // I'm not going to be clever here, I'm just going to
        // write it out to the event like it is on the card.
        payload.AppendInformation(
            $"Research Funding:      {researchFundingLevels[0]}");
        payload.AppendInformation(
            $"If 2 S.I.'s are owned: {researchFundingLevels[1]}");
        payload.AppendInformation(
            $"If 3:                  {researchFundingLevels[2]}");
        payload.AppendInformation(
            $"If 4:                  {researchFundingLevels[4]}");
        
        // In the future, we could query the owner's number of instruments
        // owned to make the OnHover accurately reflect the research funding
        // when landed.
        
        spaceHoverEventChannel.RaiseEvent(payload);
        
        return payload;
    }
}
