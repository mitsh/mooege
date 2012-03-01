﻿/*
 * Copyright (C) 2011 - 2012 mooege project - http://www.mooege.org
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

using System;
using System.Collections.Generic;
using System.Data.SQLite;
using Mooege.Common.Helpers.Hash;
using Mooege.Common.Storage;
using Mooege.Core.MooNet.Accounts;
using Mooege.Core.MooNet.Helpers;
using Mooege.Core.MooNet.Objects;
using Mooege.Core.GS.Players;

namespace Mooege.Core.MooNet.Toons
{
    public class Toon : PersistentRPCObject
    {

        public IntPresenceField HeroClassField
            = new IntPresenceField(FieldKeyHelper.Program.D3, FieldKeyHelper.OriginatingClass.Hero, 1, 0);

        public IntPresenceField HeroLevelField
            = new IntPresenceField(FieldKeyHelper.Program.D3, FieldKeyHelper.OriginatingClass.Hero, 2, 0);

        public ByteStringPresenceField<D3.Hero.VisualEquipment> HeroVisualEquipmentField
            = new ByteStringPresenceField<D3.Hero.VisualEquipment>(FieldKeyHelper.Program.D3, FieldKeyHelper.OriginatingClass.Hero, 3, 0);

        public IntPresenceField HeroFlagsField
            = new IntPresenceField(FieldKeyHelper.Program.D3, FieldKeyHelper.OriginatingClass.Hero, 4, 0);

        public StringPresenceField HeroNameField
            = new StringPresenceField(FieldKeyHelper.Program.D3, FieldKeyHelper.OriginatingClass.Hero, 5, 0);

        public IntPresenceField Field6
            = new IntPresenceField(FieldKeyHelper.Program.D3, FieldKeyHelper.OriginatingClass.Hero, 6, 0, 0);

        public IntPresenceField Field7
            = new IntPresenceField(FieldKeyHelper.Program.D3, FieldKeyHelper.OriginatingClass.Hero, 7, 0, 0);

        /// <summary>
        /// D3 EntityID encoded id.
        /// </summary>
        public D3.OnlineService.EntityId D3EntityID { get; private set; }

        /// <summary>
        /// Toon handle struct.
        /// </summary>
        public ToonHandleHelper ToonHandle { get; private set; }

        /// <summary>
        /// Toon's name.
        /// </summary>
        private string _name;
        public string Name {
            get
            {
                return _name;
            }
            private set
            {
                this._name = value;
                this.HeroNameField.Value = value;
            }
        }

        /// <summary>
        /// Toon's hash-code.
        /// </summary>
        public int HashCode { get; set; }

        /// <summary>
        /// Toon's owner account.
        /// </summary>
        public GameAccount GameAccount { get; set; }

        /// <summary>
        /// Toon's class.
        /// </summary>
        private ToonClass _class;
        public ToonClass Class
        {
            get
            {
                return _class;
            }
            private set
            {
                _class = value;
                switch (_class)
                {
                    case ToonClass.EnumBarbarian:
                        this.HeroClassField.Value = (long)ToonClass.HeroBarbarian;
                        break;
                    case ToonClass.EnumDemonHunter:
                        this.HeroClassField.Value = (long)ToonClass.HeroDemonHunter;
                        break;
                    case ToonClass.EnumMonk:
                        this.HeroClassField.Value = (long)ToonClass.HeroMonk;
                        break;
                    case ToonClass.EnumWitchDoctor:
                        this.HeroClassField.Value = (long)ToonClass.HeroWitchDoctor;
                        break;
                    case ToonClass.EnumWizard:
                        this.HeroClassField.Value = (long)ToonClass.HeroWizard;
                        break;
                    default:
                        this.HeroClassField.Value = (long)ToonClass.HeroNone;
                        break;
                }
            }
        }

        /// <summary>
        /// Toon's flags.
        /// </summary>
        private ToonFlags _flags;
        public ToonFlags Flags
        {
            get
            {
                return _flags;
            }
            private set
            {
                _flags = value;
                this.HeroFlagsField.Value = (int)(this.Flags | ToonFlags.AllUnknowns);
            }
        }

        /// <summary>
        /// Toon's level.
        /// </summary>
        //TODO: Remove this as soon as everywhere the field is used
        private byte _level;
        public byte Level
        {
            get
            {
                return _level;
            }
            private set
            {
                this._level = value;
                this.HeroLevelField.Value = value;
            }
        }

        /// <summary>
        /// Total time played for toon.
        /// </summary>
        public uint TimePlayed { get; set; }

        /// <summary>
        /// Gold amount for toon.
        /// </summary>
        public int GoldAmount { get; set; }
        
        /// <summary>
        /// Last login time for toon.
        /// </summary>
        public uint LoginTime { get; set; }

        /// <summary>
        /// The visual equipment for toon.
        /// </summary>
        private D3.Hero.VisualEquipment _equipment;
        public D3.Hero.VisualEquipment Equipment
        {
            get
            {
                return _equipment;
            }
            protected set
            {
                this._equipment = value;
                this.HeroVisualEquipmentField.Value = value;
            }
        }

        /// <summary>
        /// Settings for toon.
        /// </summary>
        private D3.Client.ToonSettings _settings = D3.Client.ToonSettings.CreateBuilder().Build();
        public D3.Client.ToonSettings Settings
        {
            get
            {
                return this._settings;
            }
            set
            {
                this._settings = value;
            }
        }

        /// <summary>
        /// Toon digest.
        /// </summary>
        public D3.Hero.Digest Digest
        {
            get
            {
                return D3.Hero.Digest.CreateBuilder().SetVersion(901)
                                .SetHeroId(this.D3EntityID)
                                .SetHeroName(this.Name)
                                .SetGbidClass((int)this.ClassID)
                                .SetPlayerFlags((uint)this.Flags)
                                .SetLevel(this.Level)
                                .SetVisualEquipment(this.Equipment)
                                .SetLastPlayedAct(0)
                                .SetHighestUnlockedAct(0)
                                .SetLastPlayedDifficulty(0)
                                .SetHighestUnlockedDifficulty(0)
                                .SetLastPlayedQuest(-1)
                                .SetLastPlayedQuestStep(-1)
                                .SetTimePlayed(this.TimePlayed)
                                .Build();
            }
        }

        /// <summary>
        /// Hero Profile.
        /// </summary>
        public D3.Profile.HeroProfile Profile
        {
            get
            {
                return D3.Profile.HeroProfile.CreateBuilder()
                    .SetHardcore(false)
                    .SetHeroId(this.D3EntityID)
                    .SetHighestDifficulty(0)
                    .SetHighestLevel(this.Level)
                    .SetMonstersKilled(923)
                    .Build();
            }
        }

        public bool IsSelected
        {
            get
            {
                if (!this.GameAccount.IsOnline) return false;
                else
                {
                    if (this.GameAccount.CurrentToon != null)
                        return this.GameAccount.CurrentToon.D3EntityID == this.D3EntityID;
                    else
                        return false;
                }
            }
        }

        public Toon(ulong persistentId, string name, int hashCode, byte @class, byte gender, byte level, long accountId, uint timePlayed, int goldAmount) // Toon with given persistent ID
            : base(persistentId)
        {
            this.SetFields(name, hashCode, (ToonClass)@class, (ToonFlags)gender, level, GameAccountManager.GetAccountByPersistentID((ulong)accountId), timePlayed, goldAmount);
        }

        public Toon(string name, int hashCode, int classId, ToonFlags flags, byte level, GameAccount account) // Toon with **newly generated** persistent ID
            : base(StringHashHelper.HashIdentity(name + "#" + hashCode.ToString("D3")))
        {
            this.SetFields(name, hashCode, GetClassByID(classId), flags, level, account, 0, 0);
        }

        public int ClassID
        {
            get
            {
                switch (this.Class)
                {
                    case ToonClass.EnumBarbarian:
                        return (int)ToonClass.HeroBarbarian;
                    case ToonClass.EnumDemonHunter:
                        return unchecked((int)ToonClass.HeroDemonHunter);
                    case ToonClass.EnumMonk:
                        return (int)ToonClass.HeroMonk;
                    case ToonClass.EnumWitchDoctor:
                        return (int)ToonClass.HeroWitchDoctor;
                    case ToonClass.EnumWizard:
                        return (int)ToonClass.HeroWizard;
                    default:
                        return (int)ToonClass.HeroNone;
                }
            }
        }

        public int VoiceClassID // Used for Conversations
        {
            get
            {
                switch (this.Class)
                {
                    case ToonClass.EnumDemonHunter:
                        return (int)ToonClass.VoiceDemonHunter;
                    case ToonClass.EnumBarbarian:
                        return (int)ToonClass.VoiceBarbarian;
                    case ToonClass.EnumWizard:
                        return (int)ToonClass.VoiceWizard;
                    case ToonClass.EnumWitchDoctor:
                        return (int)ToonClass.VoiceWitchDoctor;
                    case ToonClass.EnumMonk:
                        return (int)ToonClass.VoiceMonk;
                    default:
                        return (int)ToonClass.VoiceNone;
                }
            }
        }

        public int Gender
        {
            get
            {
                return (int)(this.Flags & ToonFlags.Female); // 0x00 for male, so we can just return the AND operation
            }
        }

        private void SetFields(string name, int hashCode, ToonClass @class, ToonFlags flags, byte level, GameAccount owner, uint timePlayed, int goldAmount)
        {
            //this.BnetEntityID = bnet.protocol.EntityId.CreateBuilder().SetHigh((ulong)EntityIdHelper.HighIdType.ToonId + this.PersistentID).SetLow(this.PersistentID).Build();
            this.D3EntityID = D3.OnlineService.EntityId.CreateBuilder().SetIdHigh((ulong)EntityIdHelper.HighIdType.ToonId).SetIdLow(this.PersistentID).Build();

            this.Name = name;
            this.HashCode = hashCode;
            this.Class = @class;
            this.Flags = flags;
            this.Level = level;
            this.GameAccount = owner;
            this.TimePlayed = timePlayed;
            this.GoldAmount = goldAmount;

            var visualItems = new[]
            {                                
                D3.Hero.VisualItem.CreateBuilder().SetEffectLevel(0).Build(), // Head
                D3.Hero.VisualItem.CreateBuilder().SetEffectLevel(0).Build(), // Chest
                D3.Hero.VisualItem.CreateBuilder().SetEffectLevel(0).Build(), // Feet
                D3.Hero.VisualItem.CreateBuilder().SetEffectLevel(0).Build(), // Hands
                D3.Hero.VisualItem.CreateBuilder().SetEffectLevel(0).Build(), // Weapon (1)
                D3.Hero.VisualItem.CreateBuilder().SetEffectLevel(0).Build(), // Weapon (2)
                D3.Hero.VisualItem.CreateBuilder().SetEffectLevel(0).Build(), // Shoulders
                D3.Hero.VisualItem.CreateBuilder().SetEffectLevel(0).Build(), // Legs
            };

            this.Equipment = D3.Hero.VisualEquipment.CreateBuilder().AddRangeVisualItem(visualItems).Build();

        }

        public void LevelUp()
        {
            this.Level++;
            this.GameAccount.ChangedFields.SetIntPresenceFieldValue(this.HeroLevelField);
        }

        #region Notifications

        //hero class generated
        //D3,Hero,1,0 -> D3.Hero.GbidClass: Hero Class
        //D3,Hero,2,0 -> D3.Hero.Level: Hero's current level
        //D3,Hero,3,0 -> D3.Hero.VisualEquipment: VisualEquipment
        //D3,Hero,4,0 -> D3.Hero.PlayerFlags: Hero's flags
        //D3,Hero,5,0 -> ?D3.Hero.NameText: Hero's Name
        //D3,Hero,6,0 -> Unk Int64 (0)
        //D3,Hero,7,0 -> Unk Int64 (0)

        public override List<bnet.protocol.presence.FieldOperation> GetSubscriptionNotifications()
        {
            var operationList = new List<bnet.protocol.presence.FieldOperation>();
            operationList.Add(this.HeroClassField.GetFieldOperation());
            operationList.Add(this.HeroLevelField.GetFieldOperation());
            operationList.Add(this.HeroVisualEquipmentField.GetFieldOperation());
            operationList.Add(this.HeroFlagsField.GetFieldOperation());
            operationList.Add(this.HeroNameField.GetFieldOperation());
            operationList.Add(this.Field6.GetFieldOperation());
            operationList.Add(this.Field7.GetFieldOperation());

            return operationList;
        }




        #endregion

        private static ToonClass GetClassByID(int classId)
        {
            switch (classId)
            {
                case (int)ToonClass.HeroBarbarian:
                    return ToonClass.EnumBarbarian;
                case unchecked((int)ToonClass.HeroDemonHunter):
                    return ToonClass.EnumDemonHunter;
                case (int)ToonClass.HeroMonk:
                    return ToonClass.EnumMonk;
                case (int)ToonClass.HeroWitchDoctor:
                    return ToonClass.EnumWitchDoctor;
                case (int)ToonClass.HeroWizard:
                    return ToonClass.EnumWizard;
                default:
                    return ToonClass.EnumBarbarian;
            }
        }

        public override string ToString()
        {
            return String.Format("{{ Toon: {0} [lowId: {1}] }}", this.Name, this.D3EntityID.IdLow);
        }

        public void SaveToDB()
        {
            try
            {
                // save character
                if (ExistsInDB())
                {
                    var query =
                        string.Format(
                            "UPDATE toons SET name='{0}', hashCode={1}, class={2}, gender={3}, level={4}, accountId={5}, timePlayed={6}, goldAmount={7} WHERE id={8}",
                            this.Name, this.HashCode, (byte)this.Class, (byte)this.Gender, this.Level, this.GameAccount.PersistentID, this.TimePlayed, this.GoldAmount, this.PersistentID);

                    var cmd = new SQLiteCommand(query, DBManager.Connection);
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    var query =
                        string.Format(
                            "INSERT INTO toons (id, name,  hashCode, class, gender, level, timePlayed, goldAmount, accountId) VALUES({0},'{1}',{2},{3},{4},{5},{6},{7},{8})",
                            this.PersistentID, this.Name, this.HashCode, (byte)this.Class, (byte)this.Gender, this.Level, this.TimePlayed, this.GoldAmount, this.GameAccount.PersistentID);

                    var cmd = new SQLiteCommand(query, DBManager.Connection);
                    cmd.ExecuteNonQuery();
                }

                //save main gear
                for (int slot = 0; slot < 8; slot++ )
                {
                    var visualItem = this.HeroVisualEquipmentField.Value.GetVisualItem(slot);
                    //item_identity_id = gbid
                    if (VisualItemExistsInDb(slot))
                    {
                        var itemQuery = string.Format("UPDATE inventory SET item_entity_id={0} WHERE toon_id={1} and inventory_slot={2}", unchecked((uint)visualItem.Gbid), unchecked((uint)this.PersistentID), slot);
                        var itemCmd = new SQLiteCommand(itemQuery, DBManager.Connection);
                        itemCmd.ExecuteNonQuery();
                    }
                    else
                    {
                        var itemQuery = string.Format("INSERT INTO inventory (toon_id, inventory_loc_x, inventory_loc_y, inventory_slot, item_entity_id) VALUES({0},{1},{2},{3},{4})", unchecked((uint)this.PersistentID), 0, 0, slot, unchecked((uint)visualItem.Gbid));
                        var itemCmd = new SQLiteCommand(itemQuery, DBManager.Connection);
                        itemCmd.ExecuteNonQuery();
                    }


                    //save other inventory

                    //save stash

                    //save gold
                 }
            }
            catch (Exception e)
            {
                Logger.ErrorException(e, "Toon.SaveToDB()");
            }
        }

        public bool DeleteFromDB()
        {
            try
            {
                // Remove from DB
                if (!ExistsInDB()) return false;

                var query = string.Format("DELETE FROM toons WHERE id={0}", this.PersistentID);
                var cmd = new SQLiteCommand(query, DBManager.Connection);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                Logger.ErrorException(e, "Toon.DeleteFromDB()");
                return false;
            }
        }

        private bool ExistsInDB()
        {
            var query = string.Format("SELECT id FROM toons WHERE id={0}", this.PersistentID);

            var cmd = new SQLiteCommand(query, DBManager.Connection);
            var reader = cmd.ExecuteReader();
            return reader.HasRows;
        }


        private bool VisualItemExistsInDb(int slot)
        {
            var query = string.Format("SELECT toon_id FROM inventory WHERE toon_id = {0} AND inventory_slot = {1}", this.PersistentID, slot);
            var cmd = new SQLiteCommand(query, DBManager.Connection);
            var reader = cmd.ExecuteReader();
            return reader.HasRows;
        }
    }

    public enum ToonClass: uint
    {
        HeroNone         = 0x00000000,
        HeroBarbarian    = 0x4FB91EE2,
        HeroMonk         = 0x003DAC15,
        HeroDemonHunter  = 0xC88B9649,
        HeroWitchDoctor  = 0x0343C22A,
        HeroWizard       = 0x1D4681B1,
        VoiceNone        = 0,
        VoiceDemonHunter = 0,
        VoiceBarbarian   = 1,
        VoiceWizard      = 2,
        VoiceWitchDoctor = 3,
        VoiceMonk        = 4,
        EnumNone         = 0,
        EnumBarbarian    = 0,
        EnumMonk         = 1,
        EnumDemonHunter  = 2,
        EnumWitchDoctor  = 3,
        EnumWizard       = 4
    }

    [Flags]
    public enum ToonFlags: uint
    {
        Male     = 0x00000000,
        Female   = 0x00000002,
        // TODO: These two need to be figured out still.. /plash
        Unknown1 = 0x00000020,
        Unknown2 = 0x00000040,
        Unknown3 = 0x00080000,
        Unknown4 = 0x02000000,
        AllUnknowns = Unknown1 | Unknown2 | Unknown3 | Unknown4
    }

}
