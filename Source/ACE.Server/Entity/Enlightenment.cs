using System;
using System.Linq;

using ACE.DatLoader;
using ACE.Entity.Enum;
using ACE.Entity.Enum.Properties;
using ACE.Server.WorldObjects;
using ACE.Server.Managers;
using ACE.Server.Network.GameMessages.Messages;
using System.Net.Http.Headers;

namespace ACE.Server.Entity
{
    public class Enlightenment
    {
        // https://asheron.fandom.com/wiki/Enlightenment

        // Reset your character to level 1, losing all experience and luminance but gaining a title, two points in vitality and one point in all of your skills.
        // In order to be eligible for enlightenment, you must be level 275, Master rank in a Society, and have all luminance auras with the exception of the skill credit auras.

        // As stated in the Spring 2014 patch notes, Enlightenment is a process for the most devoted players of Asheron's Call to continue enhancing characters which have been "maxed out" in terms of experience and abilities.
        // It was not intended to be a quest that every player would undertake or be interested in.

        // Requirements:
        // - Level 275
        // - Have all luminance auras (crafting aura included) except the 2 skill credit auras. (20 million total luminance)
        // - Have mastery rank in a society
        // - Have 25 unused pack spaces
        // - Max # of times for enlightenment: 5

        // You lose:
        // - All experience, reverting to level 1.
        // - All luminance, and luminance auras with the exception of the skill credit auras.
        // - The ability to use aetheria (until you attain sufficient level and re-open aetheria slots).
        // - The ability to gain luminance (until you attain level 200 and re-complete Nalicana's Test).
        // - The ability to equip and use items which have skill and level requirements beyond those of a level 1 character.
        //   Any equipped items are moved to your pack automatically.

        // You keep:
        // - All augmentations obtained through Augmentation Gems.
        // - Skill credits from luminance auras, Aun Ralirea, and Chasing Oswald quests.
        // - All quest flags with the exception of aetheria and luminance.

        // You gain:
        // - A new title each time you enlighten
        // - +2 to vitality
        // - +1 to all of your skills
        // - An attribute reset certificate

        public static void HandleEnlightenment(WorldObject npc, Player player)
        {
            if (!VerifyRequirements(player))
                return;

            DequipAllItems(player);

            RemoveAbility(player);

            AddPerks(npc, player);

            player.SaveBiotaToDatabase();
        }

        public static bool VerifyRequirements(Player player)
        {
            //if (player.Level < 275)
            if (player.Level < 1000)
            {
                player.Session.Network.EnqueueSend(new GameMessageSystemChat($"You must be level 1000 for ascension.", ChatMessageType.Broadcast));
                return false;
            }

            if (player.Enlightenment >= 5)
            {
                player.Session.Network.EnqueueSend(new GameMessageSystemChat($"You have already reached the maximum ascension level!", ChatMessageType.Broadcast));
                return false;
            }

            int titlesCheck = VerifyTitlesRequirement(player);
            if (titlesCheck != 1)
            {
                player.Session.Network.EnqueueSend(new GameMessageSystemChat($"You do not hold enough titles telling of your deeds. You are not yet worthy of the next step on your path of ascension. Return when you have garnered more esteem.", ChatMessageType.Broadcast));
                player.SendMessage($"You have {player.NumCharacterTitles} of {titlesCheck} required titles for your next ascension level",ChatMessageType.Broadcast);

                return false;
            }

            string lumCheck = VerifyAvailableLuminance(player);
            if (lumCheck != "true")
            {
                player.Session.Network.EnqueueSend(new GameMessageSystemChat($"You must have {lumCheck} Luminance available for ascension.", ChatMessageType.Broadcast));
                return false;
            }

            if (!VerifyLumAugs(player))
            {
                player.Session.Network.EnqueueSend(new GameMessageSystemChat($"You must have all luminance auras for ascension.", ChatMessageType.Broadcast));
                return false;
            }

            if (!VerifySocietyMaster(player))
            {
                player.Session.Network.EnqueueSend(new GameMessageSystemChat($"You must be a Master of one of the Societies of Dereth for ascension.", ChatMessageType.Broadcast));
                return false;
            }

            if (player.GetFreeInventorySlots() < 25)
            {
                player.Session.Network.EnqueueSend(new GameMessageSystemChat($"You must have at least 25 free inventory slots in your main pack for ascension.", ChatMessageType.Broadcast));
                return false;
            }
            return true;
        }

        public static string VerifyAvailableLuminance(Player player)
        {

            switch (player.Enlightenment)
            {
                case 1:
                    if (player.AvailableLuminance < 500000000)
                        return "500,000,000";
                    break;
                case 2:
                    if (player.AvailableLuminance < 800000000)
                        return "800,000,000";
                    break;
                case 3:
                    if (player.AvailableLuminance < 1200000000)
                        return "1,200,000,000";
                    break;
                case 4:
                    if (player.AvailableLuminance < 1500000000)
                        return "1,500,000,000";
                    break;
                case 5:
                    if (player.AvailableLuminance < 2000000000)
                        return "2,000,000,000";
                    break;
                default:
                    if (player.AvailableLuminance < 1000)
                        return "1,000";
                    break;
            }

            return "true";
        }

        public static int VerifyTitlesRequirement(Player player)
        {
            switch (player.Enlightenment)
            {
                case 0:
                    if (player.NumCharacterTitles < 75)
                    {
                        return 75;
                    }
                    else
                    {
                        return 1;
                    }
                case 1:
                    if (player.NumCharacterTitles < 115)
                    {
                        return 115;
                    }
                    else
                    {
                        return 1;
                    }
                case 2:
                    if (player.NumCharacterTitles < 155)
                    {
                        return 155;
                    }
                    else
                    {
                        return 1;
                    }
                case 3:
                    if (player.NumCharacterTitles < 195)
                    {
                        return 195;
                    }
                    else
                    {
                        return 1;
                    }
                case 4:
                    if (player.NumCharacterTitles < 235)
                    {
                        return 235;
                    }
                    else
                    {
                        return 1;
                    }
                default:
                    return 75;
            }
        }

        public static bool VerifySocietyMaster(Player player)
        {
            return player.SocietyRankCelhan == 1001 || player.SocietyRankEldweb == 1001 || player.SocietyRankRadblo == 1001;
        }

        public static bool VerifyLumAugs(Player player)
        {
            //var lumAugCredits = 0;

            //lumAugCredits += player.LumAugAllSkills;
            //lumAugCredits += player.LumAugSurgeChanceRating;
            //lumAugCredits += player.LumAugCritDamageRating;
            //lumAugCredits += player.LumAugCritReductionRating;
            //lumAugCredits += player.LumAugDamageRating;
            //lumAugCredits += player.LumAugDamageReductionRating;
            //lumAugCredits += player.LumAugItemManaUsage;
            //lumAugCredits += player.LumAugItemManaGain;
            //lumAugCredits += player.LumAugHealingRating;
            //lumAugCredits += player.LumAugSkilledCraft;
            //lumAugCredits += player.LumAugSkilledSpec;

            //return lumAugCredits >= 65;

            if (player.LumAugAllSkills < 21)
                return false;
            if (player.LumAugSurgeChanceRating < 5)
                return false;
            if (player.LumAugCritDamageRating < 10)
                return false;
            if (player.LumAugCritReductionRating < 10)
                return false;
            if (player.LumAugDamageRating < 25)
                return false;
            if (player.LumAugDamageReductionRating < 25)
                return false;
            if (player.LumAugItemManaUsage < 5)
                return false;
            if (player.LumAugItemManaGain < 5)
                return false;
            if (player.LumAugHealingRating < 5)
                return false;
            if (player.LumAugSkilledCraft < 5)
                return false;
            if (player.LumAugSkilledSpec < 10)
                return false;

            return true;

        }

        public static void DequipAllItems(Player player)
        {
            var equippedObjects = player.EquippedObjects.Keys.ToList();

            foreach (var equippedObject in equippedObjects)
                player.HandleActionPutItemInContainer(equippedObject.Full, player.Guid.Full, 0);
        }

        public static void RemoveAbility(Player player)
        {
            //RemoveSociety(player);
            //RemoveLuminance(player);
            //RemoveAetheria(player);
            RemoveAttributes(player);
            RemoveSkills(player);
            RemoveLevel(player);
        }

        //public static void RemoveSociety(Player player)
        //{
        //    player.QuestManager.Erase("SocietyMember");
        //    player.QuestManager.Erase("CelestialHandMember");
        //    player.QuestManager.Erase("EnlightenedCelestialHandMaster");
        //    player.QuestManager.Erase("EldrytchWebMember");
        //    player.QuestManager.Erase("EnlightenedEldrytchWebMaster");
        //    player.QuestManager.Erase("RadiantBloodMember");
        //    player.QuestManager.Erase("EnlightenedRadiantBloodMaster");

        //    if (player.SocietyRankCelhan == 1001)
        //        player.QuestManager.Stamp("EnlightenedCelestialHandMaster"); // after rejoining society, player can get promoted instantly to master when speaking to promotions officer
        //    if (player.SocietyRankEldweb == 1001)
        //        player.QuestManager.Stamp("EnlightenedEldrytchWebMaster");   // after rejoining society, player can get promoted instantly to master when speaking to promotions officer
        //    if (player.SocietyRankRadblo == 1001)
        //        player.QuestManager.Stamp("EnlightenedRadiantBloodMaster");  // after rejoining society, player can get promoted instantly to master when speaking to promotions officer

        //    player.Faction1Bits = null;
        //    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, PropertyInt.Faction1Bits, 0));
        //    player.SocietyRankCelhan = null;
        //    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, PropertyInt.SocietyRankCelhan, 0));
        //    player.SocietyRankEldweb = null;
        //    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, PropertyInt.SocietyRankEldweb, 0));
        //    player.SocietyRankRadblo = null;
        //    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, PropertyInt.SocietyRankRadblo, 0));
        //}

        public static void RemoveLevel(Player player)
        {
            // Available and total exp reset
            player.AvailableExperience = 0;
            player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt64(player, PropertyInt64.AvailableExperience, player.AvailableExperience ?? 0));
            player.TotalExperience = 0;
            player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt64(player, PropertyInt64.TotalExperience, player.TotalExperience ?? 0));
            // Available Lum reset
            player.AvailableLuminance = 0;
            player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt64(player, PropertyInt64.AvailableLuminance, player.AvailableLuminance ?? 0));

            player.Level = 1;
            player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, PropertyInt.Level, player.Level ?? 0));
        }

        //public static void RemoveAetheria(Player player)
        //{
        //    player.QuestManager.Erase("EFULNorthManaFieldUsed");
        //    player.QuestManager.Erase("EFULSouthManaFieldUsed");
        //    player.QuestManager.Erase("EFULEastManaFieldUsed");
        //    player.QuestManager.Erase("EFULWestManaFieldUsed");
        //    player.QuestManager.Erase("EFULCenterManaFieldUsed");

        //    player.QuestManager.Erase("EFMLNorthManaFieldUsed");
        //    player.QuestManager.Erase("EFMLSouthManaFieldUsed");
        //    player.QuestManager.Erase("EFMLEastManaFieldUsed");
        //    player.QuestManager.Erase("EFMLWestManaFieldUsed");
        //    player.QuestManager.Erase("EFMLCenterManaFieldUsed");

        //    player.QuestManager.Erase("EFLLNorthManaFieldUsed");
        //    player.QuestManager.Erase("EFLLSouthManaFieldUsed");
        //    player.QuestManager.Erase("EFLLEastManaFieldUsed");
        //    player.QuestManager.Erase("EFLLWestManaFieldUsed");
        //    player.QuestManager.Erase("EFLLCenterManaFieldUsed");

        //    player.AetheriaFlags = AetheriaBitfield.None;
        //    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, PropertyInt.AetheriaBitfield, 0));

        //    player.SendMessage("Your mastery of Aetheric magics fades.", ChatMessageType.Broadcast);
        //}

        public static void RemoveAttributes(Player player)
        {
            var propertyCount = Enum.GetNames(typeof(PropertyAttribute)).Length;
            for (var i = 1; i < propertyCount; i++)
            {
                var attribute = (PropertyAttribute)i;

                player.Attributes[attribute].Ranks = 0;
                player.Attributes[attribute].ExperienceSpent = 0;
                player.Session.Network.EnqueueSend(new GameMessagePrivateUpdateAttribute(player, player.Attributes[attribute]));
            }

            propertyCount = Enum.GetNames(typeof(PropertyAttribute2nd)).Length;
            for (var i = 1; i < propertyCount; i += 2)
            {
                var attribute = (PropertyAttribute2nd)i;

                player.Vitals[attribute].Ranks = 0;
                player.Vitals[attribute].ExperienceSpent = 0;
                player.Session.Network.EnqueueSend(new GameMessagePrivateUpdateVital(player, player.Vitals[attribute]));
            }

            player.SendMessage("Your attribute training fades.", ChatMessageType.Broadcast);
        }

        public static void RemoveSkills(Player player)
        {
            var propertyCount = Enum.GetNames(typeof(Skill)).Length;
            for (var i = 1; i < propertyCount; i++)
            {
                var skill = (Skill)i;

                player.ResetSkill(skill, false);
            }

            player.AvailableExperience = 0;
            player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt64(player, PropertyInt64.AvailableExperience, 0));

            var heritageGroup = DatManager.PortalDat.CharGen.HeritageGroups[(uint)player.Heritage];
            var availableSkillCredits = 0;

            availableSkillCredits += (int)heritageGroup.SkillCredits; // base skill credits allowed

            availableSkillCredits += player.QuestManager.GetCurrentSolves("ArantahKill1");       // additional quest skill credit
            availableSkillCredits += player.QuestManager.GetCurrentSolves("OswaldManualCompleted");  // additional quest skill credit
            availableSkillCredits += player.QuestManager.GetCurrentSolves("LumAugSkillQuest");   // additional quest skill credits
            availableSkillCredits += ((player.Enlightenment + 1) * 2);    // add skill credits for enlightenment

            player.AvailableSkillCredits = availableSkillCredits;

            player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, PropertyInt.AvailableSkillCredits, player.AvailableSkillCredits ?? 0));
        }

        //public static void RemoveLuminance(Player player)
        //{
        //    player.QuestManager.Erase("OracleLuminanceRewardsAccess_1110");
        //    player.QuestManager.Erase("LoyalToShadeOfLadyAdja");
        //    player.QuestManager.Erase("LoyalToKahiri");
        //    player.QuestManager.Erase("LoyalToLiamOfGelid");
        //    player.QuestManager.Erase("LoyalToLordTyragar");

        //    player.LumAugDamageRating = 0;
        //    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, PropertyInt.LumAugDamageRating, 0));
        //    player.LumAugDamageReductionRating = 0;
        //    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, PropertyInt.LumAugDamageReductionRating, 0));
        //    player.LumAugCritDamageRating = 0;
        //    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, PropertyInt.LumAugCritDamageRating, 0));
        //    player.LumAugCritReductionRating = 0;
        //    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, PropertyInt.LumAugCritReductionRating, 0));
        //    //player.LumAugSurgeEffectRating = 0;
        //    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, PropertyInt.LumAugSurgeEffectRating, 0));
        //    player.LumAugSurgeChanceRating = 0;
        //    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, PropertyInt.LumAugSurgeChanceRating, 0));
        //    player.LumAugItemManaUsage = 0;
        //    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, PropertyInt.LumAugItemManaUsage, 0));
        //    player.LumAugItemManaGain = 0;
        //    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, PropertyInt.LumAugItemManaGain, 0));
        //    player.LumAugVitality = 0;
        //    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, PropertyInt.LumAugVitality, 0));
        //    player.LumAugHealingRating = 0;
        //    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, PropertyInt.LumAugHealingRating, 0));
        //    player.LumAugSkilledCraft = 0;
        //    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, PropertyInt.LumAugSkilledCraft, 0));
        //    player.LumAugSkilledSpec = 0;
        //    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, PropertyInt.LumAugSkilledSpec, 0));
        //    player.LumAugAllSkills = 0;
        //    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, PropertyInt.LumAugAllSkills, 0));

        //    player.AvailableLuminance = null;
        //    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt64(player, PropertyInt64.AvailableLuminance, 0));
        //    player.MaximumLuminance = null;
        //    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt64(player, PropertyInt64.MaximumLuminance, 0));

        //    player.SendMessage("Your Luminance and Luminance Auras fade from your spirit.", ChatMessageType.Broadcast);
        //}

        // public static uint AttributeResetCertificate => 46421;

        public static void AddPerks(WorldObject npc, Player player)
        {
            // +1 to all skills
            // this could be handled through InitLevel, since we are always using deltas when modifying that field
            // (ie. +5/-5, instead of specifically setting to 5 trained / 10 specialized in SkillAlterationDevice)
            // however, it just feels safer to handle this dynamically in CreatureSkill, based on Enlightenment (similar to augs)
            //var enlightenment = player.Enlightenment + 1;
            //player.UpdateProperty(player, PropertyInt.Enlightenment, enlightenment);

            player.Enlightenment += 1;
            player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, PropertyInt.Enlightenment, player.Enlightenment));

            //player.SendMessage("You have become enlightened and view the world with new eyes.", ChatMessageType.Broadcast);
            player.SendMessage("You have ascended and view the world with new eyes.", ChatMessageType.Broadcast);
            player.SendMessage("Your available skill credits have been adjusted.", ChatMessageType.Broadcast);

            string lvl = "";

            // add title
            switch (player.Enlightenment)
            {
                case 1:
                    player.AddTitle(CharacterTitle.Awakened);
                    lvl = "1st";
                    player.LumAugAllSkills += 10;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, PropertyInt.LumAugAllSkills, player.LumAugAllSkills));
                    player.OverpowerResist = 5;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, PropertyInt.OverpowerResist, (int)player.OverpowerResist));
                    player.Vitals[PropertyAttribute2nd.MaxHealth].StartingValue += 4;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdateAttribute2ndLevel(player, Vital.MaxHealth, player.Vitals[PropertyAttribute2nd.MaxHealth].StartingValue));
                    player.Vitals[PropertyAttribute2nd.MaxStamina].StartingValue += 8;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdateAttribute2ndLevel(player, Vital.MaxStamina, player.Vitals[PropertyAttribute2nd.MaxStamina].StartingValue));
                    player.Vitals[PropertyAttribute2nd.MaxMana].StartingValue += 8;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdateAttribute2ndLevel(player, Vital.MaxMana, player.Vitals[PropertyAttribute2nd.MaxMana].StartingValue));
                    player.AugmentationIncreasedSpellDuration += 1;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, PropertyInt.AugmentationIncreasedSpellDuration, player.AugmentationIncreasedSpellDuration));
                    player.AugmentationBonusXp += 1;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, PropertyInt.AugmentationBonusXp, player.AugmentationBonusXp));
                    player.MaximumLuminance = 500000000;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt64(player, PropertyInt64.MaximumLuminance, (long)player.MaximumLuminance));
                    break;
                case 2:
                    player.AddTitle(CharacterTitle.Enlightened);
                    lvl = "2nd";
                    player.LumAugAllSkills += 10;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, PropertyInt.LumAugAllSkills, player.LumAugAllSkills));
                    player.OverpowerResist += 5;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, PropertyInt.OverpowerResist, (int)player.OverpowerResist));
                    player.Vitals[PropertyAttribute2nd.MaxHealth].StartingValue += 8;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdateAttribute2ndLevel(player, Vital.MaxHealth, player.Vitals[PropertyAttribute2nd.MaxHealth].StartingValue));
                    player.Vitals[PropertyAttribute2nd.MaxStamina].StartingValue += 16;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdateAttribute2ndLevel(player, Vital.MaxStamina, player.Vitals[PropertyAttribute2nd.MaxStamina].StartingValue));
                    player.Vitals[PropertyAttribute2nd.MaxMana].StartingValue += 16;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdateAttribute2ndLevel(player, Vital.MaxMana, player.Vitals[PropertyAttribute2nd.MaxMana].StartingValue));
                    player.AugmentationIncreasedSpellDuration += 1;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, PropertyInt.AugmentationIncreasedSpellDuration, player.AugmentationIncreasedSpellDuration));
                    player.AugmentationBonusXp += 1;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, PropertyInt.AugmentationBonusXp, player.AugmentationBonusXp));
                    player.MaximumLuminance = 800000000;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt64(player, PropertyInt64.MaximumLuminance, (long)player.MaximumLuminance));
                    break;
                case 3:
                    player.AddTitle(CharacterTitle.Illuminated);
                    lvl = "3rd";
                    player.LumAugAllSkills += 10;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, PropertyInt.LumAugAllSkills, player.LumAugAllSkills));
                    player.OverpowerResist += 5;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, PropertyInt.OverpowerResist, (int)player.OverpowerResist));
                    player.Vitals[PropertyAttribute2nd.MaxHealth].StartingValue += 12;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdateAttribute2ndLevel(player, Vital.MaxHealth, player.Vitals[PropertyAttribute2nd.MaxHealth].StartingValue));
                    player.Vitals[PropertyAttribute2nd.MaxStamina].StartingValue += 24;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdateAttribute2ndLevel(player, Vital.MaxStamina, player.Vitals[PropertyAttribute2nd.MaxStamina].StartingValue));
                    player.Vitals[PropertyAttribute2nd.MaxMana].StartingValue += 24;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdateAttribute2ndLevel(player, Vital.MaxMana, player.Vitals[PropertyAttribute2nd.MaxMana].StartingValue));
                    player.AugmentationIncreasedSpellDuration += 1;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, PropertyInt.AugmentationIncreasedSpellDuration, player.AugmentationIncreasedSpellDuration));
                    player.AugmentationBonusXp += 1;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, PropertyInt.AugmentationBonusXp, player.AugmentationBonusXp));
                    player.MaximumLuminance = 1200000000;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt64(player, PropertyInt64.MaximumLuminance, (long)player.MaximumLuminance));
                    break;
                case 4:
                    player.AddTitle(CharacterTitle.Transcended);
                    lvl = "4th";
                    player.LumAugAllSkills += 10;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, PropertyInt.LumAugAllSkills, player.LumAugAllSkills));
                    player.OverpowerResist += 5;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, PropertyInt.OverpowerResist, (int)player.OverpowerResist));
                    player.Vitals[PropertyAttribute2nd.MaxHealth].StartingValue += 16;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdateAttribute2ndLevel(player, Vital.MaxHealth, player.Vitals[PropertyAttribute2nd.MaxHealth].StartingValue));
                    player.Vitals[PropertyAttribute2nd.MaxStamina].StartingValue += 32;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdateAttribute2ndLevel(player, Vital.MaxStamina, player.Vitals[PropertyAttribute2nd.MaxStamina].StartingValue));
                    player.Vitals[PropertyAttribute2nd.MaxMana].StartingValue += 32;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdateAttribute2ndLevel(player, Vital.MaxMana, player.Vitals[PropertyAttribute2nd.MaxMana].StartingValue));
                    player.AugmentationIncreasedSpellDuration += 1;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, PropertyInt.AugmentationIncreasedSpellDuration, player.AugmentationIncreasedSpellDuration));
                    player.AugmentationBonusXp += 1;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, PropertyInt.AugmentationBonusXp, player.AugmentationBonusXp));
                    player.MaximumLuminance = 1500000000;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt64(player, PropertyInt64.MaximumLuminance, (long)player.MaximumLuminance));
                    break;
                case 5:
                    player.AddTitle(CharacterTitle.CosmicConscious);
                    lvl = "5th";
                    player.LumAugAllSkills += 10;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, PropertyInt.LumAugAllSkills, player.LumAugAllSkills));
                    player.OverpowerResist += 5;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, PropertyInt.OverpowerResist, (int)player.OverpowerResist));
                    player.Vitals[PropertyAttribute2nd.MaxHealth].StartingValue += 20;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdateAttribute2ndLevel(player, Vital.MaxHealth, player.Vitals[PropertyAttribute2nd.MaxHealth].StartingValue));
                    player.Vitals[PropertyAttribute2nd.MaxStamina].StartingValue += 40;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdateAttribute2ndLevel(player, Vital.MaxStamina, player.Vitals[PropertyAttribute2nd.MaxStamina].StartingValue));
                    player.Vitals[PropertyAttribute2nd.MaxMana].StartingValue += 40;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdateAttribute2ndLevel(player, Vital.MaxMana, player.Vitals[PropertyAttribute2nd.MaxMana].StartingValue));
                    player.AugmentationIncreasedSpellDuration += 1;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, PropertyInt.AugmentationIncreasedSpellDuration, player.AugmentationIncreasedSpellDuration));
                    player.AugmentationBonusXp += 1;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, PropertyInt.AugmentationBonusXp, player.AugmentationBonusXp));
                    player.MaximumLuminance = 2000000000;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt64(player, PropertyInt64.MaximumLuminance, (long)player.MaximumLuminance));
                    break;
                default:
                    player.AddTitle(CharacterTitle.CosmicConscious);
                    lvl = $"{player.Enlightenment}th";
                    player.LumAugAllSkills += 5;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdatePropertyInt(player, PropertyInt.LumAugAllSkills, player.LumAugAllSkills));
                    player.Vitals[PropertyAttribute2nd.MaxHealth].StartingValue += 5;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdateAttribute2ndLevel(player, Vital.MaxHealth, player.Vitals[PropertyAttribute2nd.MaxHealth].StartingValue));
                    player.Vitals[PropertyAttribute2nd.MaxStamina].StartingValue += 10;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdateAttribute2ndLevel(player, Vital.MaxStamina, player.Vitals[PropertyAttribute2nd.MaxStamina].StartingValue));
                    player.Vitals[PropertyAttribute2nd.MaxMana].StartingValue += 10;
                    player.Session.Network.EnqueueSend(new GameMessagePrivateUpdateAttribute2ndLevel(player, Vital.MaxMana, player.Vitals[PropertyAttribute2nd.MaxMana].StartingValue));
                    break;
            }

            // player.GiveFromEmote(npc, AttributeResetCertificate, 1);

            //var msg = $"{player.Name} has achieved the {lvl} level of Enlightenment!";
            var msg = $"{player.Name} has achieved the {lvl} level of Ascension!";
            if (player.Account.AccessLevel < 3)
            {
                PlayerManager.BroadcastToAll(new GameMessageSystemChat(msg, ChatMessageType.WorldBroadcast));
                PlayerManager.LogBroadcastChat(Channel.AllBroadcast, null, msg);
            }
            player.SendMessage($"You have risen to the {lvl} tier of ascension!", ChatMessageType.Broadcast);
            // +2 vitality
            // handled automatically via PropertyInt.Enlightenment * 2

            /*var vitality = player.LumAugVitality + 2;
            player.UpdateProperty(player, PropertyInt.LumAugVitality, vitality);*/
        }
    }
}
