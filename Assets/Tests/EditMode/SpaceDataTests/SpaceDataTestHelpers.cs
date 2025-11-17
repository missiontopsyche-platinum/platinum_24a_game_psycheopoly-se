using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Tests.EditMode.SpaceDataTests
{
    public static class SpaceDataTestHelpers
    {
        public static List<ScriptableObject> PopulateSpaceDataFields(SpaceData newSpace, String name, Color spaceColor,
            List<ScriptableObject> eventChannels)
        {
            newSpace.spaceName = name;
            newSpace.spaceColor = spaceColor;
            newSpace.spaceHoverEventChannel = ScriptableObject.CreateInstance<SpaceHoverEventChannel>();
            eventChannels.Add(newSpace.spaceHoverEventChannel);
            newSpace.onSpaceExitEventChannel = ScriptableObject.CreateInstance<BooleanEventChannel>();
            eventChannels.Add(newSpace.onSpaceExitEventChannel);
            return eventChannels;
        }

        public static List<ScriptableObject> PopulateOwnableSpaceDataFields(OwnableSpaceData newSpace, int buyPrice,
            int collabPrice, List<ScriptableObject> eventChannels)
        {
            newSpace.buyPrice = buyPrice;
            newSpace.collaborationValue = collabPrice;
            newSpace.purchaseOwnableRequestEventChannel =
                ScriptableObject.CreateInstance<PurchaseOwnableRequestEventChannel>();
            eventChannels.Add(newSpace.purchaseOwnableRequestEventChannel);
            newSpace.chargeOwnershipFeeEventChannel = 
                ScriptableObject.CreateInstance<ChargeOwnershipFeeEventChannel>();
            eventChannels.Add(newSpace.chargeOwnershipFeeEventChannel);
            return eventChannels;
        }

        public static void DestroyEventChannels(List<ScriptableObject> eventChannels)
        {
            foreach (var channel in eventChannels)
                Object.DestroyImmediate(channel);
        }
    }
}