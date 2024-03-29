using System;
using System.Collections.Generic;
using Server.Engines.Craft;
using Server.Factions;
using Server.Mobiles;
using Server.Network;
using Server.Spells;
using Server.Spells.Bushido;
using Server.Spells.Necromancy;
using Server.Spells.Ninjitsu;
using Server.Spells.Spellweaving;

namespace Server.Items
{
    public interface ISlayer
    {
        SlayerName Slayer { get; set; }
        SlayerName Slayer2 { get; set; }
    }

    public abstract class BaseWeapon : Item, IWeapon, IFactionItem, ICraftable, ISlayer, IDurability
    {
        private string m_EngravedText;
		
        [CommandProperty(AccessLevel.GameMaster)]
        public string EngravedText
        {
            get
            {
                return this.m_EngravedText;
            }
            set
            {
                this.m_EngravedText = value;
                this.InvalidateProperties();
            }
        }

        #region Factions
        private FactionItem m_FactionState;

        public FactionItem FactionItemState
        {
            get
            {
                return this.m_FactionState;
            }
            set
            {
                this.m_FactionState = value;

                if (this.m_FactionState == null)
                    this.Hue = CraftResources.GetHue(this.Resource);

                this.LootType = (this.m_FactionState == null ? LootType.Regular : LootType.Blessed);
            }
        }
        #endregion

        /* Weapon internals work differently now (Mar 13 2003)
        * 
        * The attributes defined below default to -1.
        * If the value is -1, the corresponding virtual 'Aos/Old' property is used.
        * If not, the attribute value itself is used. Here's the list:
        *  - MinDamage
        *  - MaxDamage
        *  - Speed
        *  - HitSound
        *  - MissSound
        *  - StrRequirement, DexRequirement, IntRequirement
        *  - WeaponType
        *  - WeaponAnimation
        *  - MaxRange
        */

        #region Var declarations

        // Instance values. These values are unique to each weapon.
        private WeaponDamageLevel m_DamageLevel;
        private WeaponAccuracyLevel m_AccuracyLevel;
        private WeaponDurabilityLevel m_DurabilityLevel;
        private WeaponQuality m_Quality;
        private Mobile m_Crafter;
        private Poison m_Poison;
        private int m_PoisonCharges;
        private bool m_Identified;
        private int m_Hits;
        private int m_MaxHits;
        private SlayerName m_Slayer;
        private SlayerName m_Slayer2;
        private SkillMod m_SkillMod, m_MageMod;
        private CraftResource m_Resource;
        private bool m_PlayerConstructed;

        private bool m_Cursed; // Is this weapon cursed via Curse Weapon necromancer spell? Temporary; not serialized.
        private bool m_Consecrated; // Is this weapon blessed via Consecrate Weapon paladin ability? Temporary; not serialized.

        private AosAttributes m_AosAttributes;
        private AosWeaponAttributes m_AosWeaponAttributes;
        private AosSkillBonuses m_AosSkillBonuses;
        private AosElementAttributes m_AosElementDamages;

        // Overridable values. These values are provided to override the defaults which get defined in the individual weapon scripts.
        private int m_StrReq, m_DexReq, m_IntReq;
        private int m_MinDamage, m_MaxDamage;
        private int m_HitSound, m_MissSound;
        private float m_Speed;
        private int m_MaxRange;
        private SkillName m_Skill;
        private WeaponType m_Type;
        private WeaponAnimation m_Animation;
        #endregion

        #region Virtual Properties
        public virtual WeaponAbility PrimaryAbility
        {
            get
            {
                return null;
            }
        }
        public virtual WeaponAbility SecondaryAbility
        {
            get
            {
                return null;
            }
        }

        public virtual int DefMaxRange
        {
            get
            {
                return 1;
            }
        }
        public virtual int DefHitSound
        {
            get
            {
                return 0;
            }
        }
        public virtual int DefMissSound
        {
            get
            {
                return 0;
            }
        }
        public virtual SkillName DefSkill
        {
            get
            {
                return SkillName.Swords;
            }
        }
        public virtual WeaponType DefType
        {
            get
            {
                return WeaponType.Slashing;
            }
        }
        public virtual WeaponAnimation DefAnimation
        {
            get
            {
                return WeaponAnimation.Slash1H;
            }
        }

        public virtual int AosStrengthReq
        {
            get
            {
                return 0;
            }
        }
        public virtual int AosDexterityReq
        {
            get
            {
                return 0;
            }
        }
        public virtual int AosIntelligenceReq
        {
            get
            {
                return 0;
            }
        }
        public virtual int AosMinDamage
        {
            get
            {
                return 0;
            }
        }
        public virtual int AosMaxDamage
        {
            get
            {
                return 0;
            }
        }
        public virtual int AosSpeed
        {
            get
            {
                return 0;
            }
        }
        public virtual float MlSpeed
        {
            get
            {
                return 0.0f;
            }
        }
        public virtual int AosMaxRange
        {
            get
            {
                return this.DefMaxRange;
            }
        }
        public virtual int AosHitSound
        {
            get
            {
                return this.DefHitSound;
            }
        }
        public virtual int AosMissSound
        {
            get
            {
                return this.DefMissSound;
            }
        }
        public virtual SkillName AosSkill
        {
            get
            {
                return this.DefSkill;
            }
        }
        public virtual WeaponType AosType
        {
            get
            {
                return this.DefType;
            }
        }
        public virtual WeaponAnimation AosAnimation
        {
            get
            {
                return this.DefAnimation;
            }
        }

        public virtual int OldStrengthReq
        {
            get
            {
                return 0;
            }
        }
        public virtual int OldDexterityReq
        {
            get
            {
                return 0;
            }
        }
        public virtual int OldIntelligenceReq
        {
            get
            {
                return 0;
            }
        }
        public virtual int OldMinDamage
        {
            get
            {
                return 0;
            }
        }
        public virtual int OldMaxDamage
        {
            get
            {
                return 0;
            }
        }
        public virtual int OldSpeed
        {
            get
            {
                return 0;
            }
        }
        public virtual int OldMaxRange
        {
            get
            {
                return this.DefMaxRange;
            }
        }
        public virtual int OldHitSound
        {
            get
            {
                return this.DefHitSound;
            }
        }
        public virtual int OldMissSound
        {
            get
            {
                return this.DefMissSound;
            }
        }
        public virtual SkillName OldSkill
        {
            get
            {
                return this.DefSkill;
            }
        }
        public virtual WeaponType OldType
        {
            get
            {
                return this.DefType;
            }
        }
        public virtual WeaponAnimation OldAnimation
        {
            get
            {
                return this.DefAnimation;
            }
        }

        public virtual int InitMinHits
        {
            get
            {
                return 0;
            }
        }
        public virtual int InitMaxHits
        {
            get
            {
                return 0;
            }
        }

        public virtual bool CanFortify
        {
            get
            {
                return true;
            }
        }

        public override int PhysicalResistance
        {
            get
            {
                return this.m_AosWeaponAttributes.ResistPhysicalBonus;
            }
        }
        public override int FireResistance
        {
            get
            {
                return this.m_AosWeaponAttributes.ResistFireBonus;
            }
        }
        public override int ColdResistance
        {
            get
            {
                return this.m_AosWeaponAttributes.ResistColdBonus;
            }
        }
        public override int PoisonResistance
        {
            get
            {
                return this.m_AosWeaponAttributes.ResistPoisonBonus;
            }
        }
        public override int EnergyResistance
        {
            get
            {
                return this.m_AosWeaponAttributes.ResistEnergyBonus;
            }
        }

        public virtual SkillName AccuracySkill
        {
            get
            {
                return SkillName.Tactics;
            }
        }
        #endregion

        #region Getters & Setters
        [CommandProperty(AccessLevel.GameMaster)]
        public AosAttributes Attributes
        {
            get
            {
                return this.m_AosAttributes;
            }
            set
            {
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public AosWeaponAttributes WeaponAttributes
        {
            get
            {
                return this.m_AosWeaponAttributes;
            }
            set
            {
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public AosSkillBonuses SkillBonuses
        {
            get
            {
                return this.m_AosSkillBonuses;
            }
            set
            {
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public AosElementAttributes AosElementDamages
        {
            get
            {
                return this.m_AosElementDamages;
            }
            set
            {
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Cursed
        {
            get
            {
                return this.m_Cursed;
            }
            set
            {
                this.m_Cursed = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Consecrated
        {
            get
            {
                return this.m_Consecrated;
            }
            set
            {
                this.m_Consecrated = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Identified
        {
            get
            {
                return this.m_Identified;
            }
            set
            {
                this.m_Identified = value;
                this.InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitPoints
        {
            get
            {
                return this.m_Hits;
            }
            set
            {
                if (this.m_Hits == value)
                    return;

                if (value > this.m_MaxHits)
                    value = this.m_MaxHits;

                this.m_Hits = value;

                this.InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MaxHitPoints
        {
            get
            {
                return this.m_MaxHits;
            }
            set
            {
                this.m_MaxHits = value;
                this.InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int PoisonCharges
        {
            get
            {
                return this.m_PoisonCharges;
            }
            set
            {
                this.m_PoisonCharges = value;
                this.InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Poison Poison
        {
            get
            {
                return this.m_Poison;
            }
            set
            {
                this.m_Poison = value;
                this.InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public WeaponQuality Quality
        {
            get
            {
                return this.m_Quality;
            }
            set
            {
                this.UnscaleDurability();
                this.m_Quality = value;
                this.ScaleDurability();
                this.InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Mobile Crafter
        {
            get
            {
                return this.m_Crafter;
            }
            set
            {
                this.m_Crafter = value;
                this.InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public SlayerName Slayer
        {
            get
            {
                return this.m_Slayer;
            }
            set
            {
                this.m_Slayer = value;
                this.InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public SlayerName Slayer2
        {
            get
            {
                return this.m_Slayer2;
            }
            set
            {
                this.m_Slayer2 = value;
                this.InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public CraftResource Resource
        {
            get
            {
                return this.m_Resource;
            }
            set
            {
                this.UnscaleDurability();
                this.m_Resource = value;
                this.Hue = CraftResources.GetHue(this.m_Resource);
                this.InvalidateProperties();
                this.ScaleDurability();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public WeaponDamageLevel DamageLevel
        {
            get
            {
                return this.m_DamageLevel;
            }
            set
            {
                this.m_DamageLevel = value;
                this.InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public WeaponDurabilityLevel DurabilityLevel
        {
            get
            {
                return this.m_DurabilityLevel;
            }
            set
            {
                this.UnscaleDurability();
                this.m_DurabilityLevel = value;
                this.InvalidateProperties();
                this.ScaleDurability();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool PlayerConstructed
        {
            get
            {
                return this.m_PlayerConstructed;
            }
            set
            {
                this.m_PlayerConstructed = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MaxRange
        {
            get
            {
                return (this.m_MaxRange == -1 ? Core.AOS ? this.AosMaxRange : this.OldMaxRange : this.m_MaxRange);
            }
            set
            {
                this.m_MaxRange = value;
                this.InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public WeaponAnimation Animation
        {
            get
            {
                return (this.m_Animation == (WeaponAnimation)(-1) ? Core.AOS ? this.AosAnimation : this.OldAnimation : this.m_Animation);
            }
            set
            {
                this.m_Animation = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public WeaponType Type
        {
            get
            {
                return (this.m_Type == (WeaponType)(-1) ? Core.AOS ? this.AosType : this.OldType : this.m_Type);
            }
            set
            {
                this.m_Type = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public SkillName Skill
        {
            get
            {
                return (this.m_Skill == (SkillName)(-1) ? Core.AOS ? this.AosSkill : this.OldSkill : this.m_Skill);
            }
            set
            {
                this.m_Skill = value;
                this.InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int HitSound
        {
            get
            {
                return (this.m_HitSound == -1 ? Core.AOS ? this.AosHitSound : this.OldHitSound : this.m_HitSound);
            }
            set
            {
                this.m_HitSound = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MissSound
        {
            get
            {
                return (this.m_MissSound == -1 ? Core.AOS ? this.AosMissSound : this.OldMissSound : this.m_MissSound);
            }
            set
            {
                this.m_MissSound = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MinDamage
        {
            get
            {
                return (this.m_MinDamage == -1 ? Core.AOS ? this.AosMinDamage : this.OldMinDamage : this.m_MinDamage);
            }
            set
            {
                this.m_MinDamage = value;
                this.InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MaxDamage
        {
            get
            {
                return (this.m_MaxDamage == -1 ? Core.AOS ? this.AosMaxDamage : this.OldMaxDamage : this.m_MaxDamage);
            }
            set
            {
                this.m_MaxDamage = value;
                this.InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public float Speed
        {
            get
            {
                if (this.m_Speed != -1)
                    return this.m_Speed;

                if (Core.ML)
                    return this.MlSpeed;
                else if (Core.AOS)
                    return this.AosSpeed;

                return this.OldSpeed;
            }
            set
            {
                this.m_Speed = value;
                this.InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int StrRequirement
        {
            get
            {
                return (this.m_StrReq == -1 ? Core.AOS ? this.AosStrengthReq : this.OldStrengthReq : this.m_StrReq);
            }
            set
            {
                this.m_StrReq = value;
                this.InvalidateProperties();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int DexRequirement
        {
            get
            {
                return (this.m_DexReq == -1 ? Core.AOS ? this.AosDexterityReq : this.OldDexterityReq : this.m_DexReq);
            }
            set
            {
                this.m_DexReq = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int IntRequirement
        {
            get
            {
                return (this.m_IntReq == -1 ? Core.AOS ? this.AosIntelligenceReq : this.OldIntelligenceReq : this.m_IntReq);
            }
            set
            {
                this.m_IntReq = value;
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public WeaponAccuracyLevel AccuracyLevel
        {
            get
            {
                return this.m_AccuracyLevel;
            }
            set
            {
                if (this.m_AccuracyLevel != value)
                {
                    this.m_AccuracyLevel = value;

                    if (this.UseSkillMod)
                    {
                        if (this.m_AccuracyLevel == WeaponAccuracyLevel.Regular)
                        {
                            if (this.m_SkillMod != null)
                                this.m_SkillMod.Remove();

                            this.m_SkillMod = null;
                        }
                        else if (this.m_SkillMod == null && this.Parent is Mobile)
                        {
                            this.m_SkillMod = new DefaultSkillMod(this.AccuracySkill, true, (int)this.m_AccuracyLevel * 5);
                            ((Mobile)this.Parent).AddSkillMod(this.m_SkillMod);
                        }
                        else if (this.m_SkillMod != null)
                        {
                            this.m_SkillMod.Value = (int)this.m_AccuracyLevel * 5;
                        }
                    }

                    this.InvalidateProperties();
                }
            }
        }

        #endregion

        public override void OnAfterDuped(Item newItem)
        {
            BaseWeapon weap = newItem as BaseWeapon;

            if (weap == null)
                return;

            weap.m_AosAttributes = new AosAttributes(newItem, this.m_AosAttributes);
            weap.m_AosElementDamages = new AosElementAttributes(newItem, this.m_AosElementDamages);
            weap.m_AosSkillBonuses = new AosSkillBonuses(newItem, this.m_AosSkillBonuses);
            weap.m_AosWeaponAttributes = new AosWeaponAttributes(newItem, this.m_AosWeaponAttributes);
        }

        public virtual void UnscaleDurability()
        {
            int scale = 100 + this.GetDurabilityBonus();

            this.m_Hits = ((this.m_Hits * 100) + (scale - 1)) / scale;
            this.m_MaxHits = ((this.m_MaxHits * 100) + (scale - 1)) / scale;
            this.InvalidateProperties();
        }

        public virtual void ScaleDurability()
        {
            int scale = 100 + this.GetDurabilityBonus();

            this.m_Hits = ((this.m_Hits * scale) + 99) / 100;
            this.m_MaxHits = ((this.m_MaxHits * scale) + 99) / 100;
            this.InvalidateProperties();
        }

        public int GetDurabilityBonus()
        {
            int bonus = 0;

            if (this.m_Quality == WeaponQuality.Exceptional)
                bonus += 20;

            switch ( this.m_DurabilityLevel )
            {
                case WeaponDurabilityLevel.Durable:
                    bonus += 20;
                    break;
                case WeaponDurabilityLevel.Substantial:
                    bonus += 50;
                    break;
                case WeaponDurabilityLevel.Massive:
                    bonus += 70;
                    break;
                case WeaponDurabilityLevel.Fortified:
                    bonus += 100;
                    break;
                case WeaponDurabilityLevel.Indestructible:
                    bonus += 120;
                    break;
            }

            if (Core.AOS)
            {
                bonus += this.m_AosWeaponAttributes.DurabilityBonus;

                CraftResourceInfo resInfo = CraftResources.GetInfo(this.m_Resource);
                CraftAttributeInfo attrInfo = null;

                if (resInfo != null)
                    attrInfo = resInfo.AttributeInfo;

                if (attrInfo != null)
                    bonus += attrInfo.WeaponDurability;
            }

            return bonus;
        }

        public int GetLowerStatReq()
        {
            if (!Core.AOS)
                return 0;

            int v = this.m_AosWeaponAttributes.LowerStatReq;

            CraftResourceInfo info = CraftResources.GetInfo(this.m_Resource);

            if (info != null)
            {
                CraftAttributeInfo attrInfo = info.AttributeInfo;

                if (attrInfo != null)
                    v += attrInfo.WeaponLowerRequirements;
            }

            if (v > 100)
                v = 100;

            return v;
        }

        public static void BlockEquip(Mobile m, TimeSpan duration)
        {
            if (m.BeginAction(typeof(BaseWeapon)))
                new ResetEquipTimer(m, duration).Start();
        }

        private class ResetEquipTimer : Timer
        {
            private readonly Mobile m_Mobile;

            public ResetEquipTimer(Mobile m, TimeSpan duration) : base(duration)
            {
                this.m_Mobile = m;
            }

            protected override void OnTick()
            {
                this.m_Mobile.EndAction(typeof(BaseWeapon));
            }
        }

        public override bool CheckConflictingLayer(Mobile m, Item item, Layer layer)
        {
            if (base.CheckConflictingLayer(m, item, layer))
                return true;

            if (this.Layer == Layer.TwoHanded && layer == Layer.OneHanded)
            {
                m.SendLocalizedMessage(500214); // You already have something in both hands.
                return true;
            }
            else if (this.Layer == Layer.OneHanded && layer == Layer.TwoHanded && !(item is BaseShield) && !(item is BaseEquipableLight))
            {
                m.SendLocalizedMessage(500215); // You can only wield one weapon at a time.
                return true;
            }

            return false;
        }

        public override bool AllowSecureTrade(Mobile from, Mobile to, Mobile newOwner, bool accepted)
        {
            if (!Ethics.Ethic.CheckTrade(from, to, newOwner, this))
                return false;

            return base.AllowSecureTrade(from, to, newOwner, accepted);
        }

        public virtual Race RequiredRace
        {
            get
            {
                return null;
            }
        }//On OSI, there are no weapons with race requirements, this is for custom stuff

        public override bool CanEquip(Mobile from)
        {
            if (!Ethics.Ethic.CheckEquip(from, this))
                return false;

            if (this.RequiredRace != null && from.Race != this.RequiredRace)
            {
                if (this.RequiredRace == Race.Elf)
                    from.SendLocalizedMessage(1072203); // Only Elves may use this.
                else
                    from.SendMessage("Only {0} may use this.", this.RequiredRace.PluralName);

                return false;
            }
            else if (from.Dex < this.DexRequirement)
            {
                from.SendMessage("You are not nimble enough to equip that.");
                return false;
            }
            else if (from.Str < AOS.Scale(this.StrRequirement, 100 - this.GetLowerStatReq()))
            {
                from.SendLocalizedMessage(500213); // You are not strong enough to equip that.
                return false;
            }
            else if (from.Int < this.IntRequirement)
            {
                from.SendMessage("You are not smart enough to equip that.");
                return false;
            }
            else if (!from.CanBeginAction(typeof(BaseWeapon)))
            {
                return false;
            }
            else
            {
                return base.CanEquip(from);
            }
        }

        public virtual bool UseSkillMod
        {
            get
            {
                return !Core.AOS;
            }
        }

        public override bool OnEquip(Mobile from)
        {
            int strBonus = this.m_AosAttributes.BonusStr;
            int dexBonus = this.m_AosAttributes.BonusDex;
            int intBonus = this.m_AosAttributes.BonusInt;

            if ((strBonus != 0 || dexBonus != 0 || intBonus != 0))
            {
                Mobile m = from;

                string modName = this.Serial.ToString();

                if (strBonus != 0)
                    m.AddStatMod(new StatMod(StatType.Str, modName + "Str", strBonus, TimeSpan.Zero));

                if (dexBonus != 0)
                    m.AddStatMod(new StatMod(StatType.Dex, modName + "Dex", dexBonus, TimeSpan.Zero));

                if (intBonus != 0)
                    m.AddStatMod(new StatMod(StatType.Int, modName + "Int", intBonus, TimeSpan.Zero));
            }

            from.NextCombatTime = DateTime.Now + this.GetDelay(from);

            if (this.UseSkillMod && this.m_AccuracyLevel != WeaponAccuracyLevel.Regular)
            {
                if (this.m_SkillMod != null)
                    this.m_SkillMod.Remove();

                this.m_SkillMod = new DefaultSkillMod(this.AccuracySkill, true, (int)this.m_AccuracyLevel * 5);
                from.AddSkillMod(this.m_SkillMod);
            }

            if (Core.AOS && this.m_AosWeaponAttributes.MageWeapon != 0 && this.m_AosWeaponAttributes.MageWeapon != 30)
            {
                if (this.m_MageMod != null)
                    this.m_MageMod.Remove();

                this.m_MageMod = new DefaultSkillMod(SkillName.Magery, true, -30 + this.m_AosWeaponAttributes.MageWeapon);
                from.AddSkillMod(this.m_MageMod);
            }

            return true;
        }

        public override void OnAdded(object parent)
        {
            base.OnAdded(parent);

            if (parent is Mobile)
            {
                Mobile from = (Mobile)parent;

                if (Core.AOS)
                    this.m_AosSkillBonuses.AddTo(from);

                from.CheckStatTimers();
                from.Delta(MobileDelta.WeaponDamage);
            }
        }

        public override void OnRemoved(object parent)
        {
            if (parent is Mobile)
            {
                Mobile m = (Mobile)parent;
                BaseWeapon weapon = m.Weapon as BaseWeapon;

                string modName = this.Serial.ToString();

                m.RemoveStatMod(modName + "Str");
                m.RemoveStatMod(modName + "Dex");
                m.RemoveStatMod(modName + "Int");

                if (weapon != null)
                    m.NextCombatTime = DateTime.Now + weapon.GetDelay(m);

                if (this.UseSkillMod && this.m_SkillMod != null)
                {
                    this.m_SkillMod.Remove();
                    this.m_SkillMod = null;
                }

                if (this.m_MageMod != null)
                {
                    this.m_MageMod.Remove();
                    this.m_MageMod = null;
                }

                if (Core.AOS)
                    this.m_AosSkillBonuses.Remove();

                ImmolatingWeaponSpell.StopImmolating(this);

                m.CheckStatTimers();

                m.Delta(MobileDelta.WeaponDamage);
            }
        }

        public virtual SkillName GetUsedSkill(Mobile m, bool checkSkillAttrs)
        {
            SkillName sk;

            if (checkSkillAttrs && this.m_AosWeaponAttributes.UseBestSkill != 0)
            {
                double swrd = m.Skills[SkillName.Swords].Value;
                double fenc = m.Skills[SkillName.Fencing].Value;
                double mcng = m.Skills[SkillName.Macing].Value;
                double val;

                sk = SkillName.Swords;
                val = swrd;

                if (fenc > val)
                {
                    sk = SkillName.Fencing;
                    val = fenc;
                }
                if (mcng > val)
                {
                    sk = SkillName.Macing;
                    val = mcng;
                }
            }
            else if (this.m_AosWeaponAttributes.MageWeapon != 0)
            {
                if (m.Skills[SkillName.Magery].Value > m.Skills[this.Skill].Value)
                    sk = SkillName.Magery;
                else
                    sk = this.Skill;
            }
            else
            {
                sk = this.Skill;

                if (sk != SkillName.Wrestling && !m.Player && !m.Body.IsHuman && m.Skills[SkillName.Wrestling].Value > m.Skills[sk].Value)
                    sk = SkillName.Wrestling;
            }

            return sk;
        }

        public virtual double GetAttackSkillValue(Mobile attacker, Mobile defender)
        {
            return attacker.Skills[this.GetUsedSkill(attacker, true)].Value;
        }

        public virtual double GetDefendSkillValue(Mobile attacker, Mobile defender)
        {
            return defender.Skills[this.GetUsedSkill(defender, true)].Value;
        }

        private static bool CheckAnimal(Mobile m, Type type)
        {
            return AnimalForm.UnderTransformation(m, type);
        }

        public virtual bool CheckHit(Mobile attacker, Mobile defender)
        {
            BaseWeapon atkWeapon = attacker.Weapon as BaseWeapon;
            BaseWeapon defWeapon = defender.Weapon as BaseWeapon;

            Skill atkSkill = attacker.Skills[atkWeapon.Skill];
            Skill defSkill = defender.Skills[defWeapon.Skill];

            double atkValue = atkWeapon.GetAttackSkillValue(attacker, defender);
            double defValue = defWeapon.GetDefendSkillValue(attacker, defender);

            double ourValue, theirValue;

            int bonus = this.GetHitChanceBonus();

            if (Core.AOS)
            {
                if (atkValue <= -20.0)
                    atkValue = -19.9;

                if (defValue <= -20.0)
                    defValue = -19.9;

                bonus += AosAttributes.GetValue(attacker, AosAttribute.AttackChance);

                if (Spells.Chivalry.DivineFurySpell.UnderEffect(attacker))
                    bonus += 10; // attacker gets 10% bonus when they're under divine fury

                if (CheckAnimal(attacker, typeof(GreyWolf)) || CheckAnimal(attacker, typeof(BakeKitsune)))
                    bonus += 20; // attacker gets 20% bonus when under Wolf or Bake Kitsune form

                if (HitLower.IsUnderAttackEffect(attacker))
                    bonus -= 25; // Under Hit Lower Attack effect -> 25% malus

                WeaponAbility ability = WeaponAbility.GetCurrentAbility(attacker);

                if (ability != null)
                    bonus += ability.AccuracyBonus;

                SpecialMove move = SpecialMove.GetCurrentMove(attacker);

                if (move != null)
                    bonus += move.GetAccuracyBonus(attacker);

                // Max Hit Chance Increase = 45%
                if (bonus > 45)
                    bonus = 45;

                ourValue = (atkValue + 20.0) * (100 + bonus);

                bonus = AosAttributes.GetValue(defender, AosAttribute.DefendChance);

                if (Spells.Chivalry.DivineFurySpell.UnderEffect(defender))
                    bonus -= 20; // defender loses 20% bonus when they're under divine fury

                if (HitLower.IsUnderDefenseEffect(defender))
                    bonus -= 25; // Under Hit Lower Defense effect -> 25% malus
					
                int blockBonus = 0;

                if (Block.GetBonus(defender, ref blockBonus))
                    bonus += blockBonus;

                int surpriseMalus = 0;

                if (SurpriseAttack.GetMalus(defender, ref surpriseMalus))
                    bonus -= surpriseMalus;

                int discordanceEffect = 0;

                // Defender loses -0/-28% if under the effect of Discordance.
                if (SkillHandlers.Discordance.GetEffect(attacker, ref discordanceEffect))
                    bonus -= discordanceEffect;

                // Defense Chance Increase = 45%
                if (bonus > 45)
                    bonus = 45;

                theirValue = (defValue + 20.0) * (100 + bonus);

                bonus = 0;
            }
            else
            {
                if (atkValue <= -50.0)
                    atkValue = -49.9;

                if (defValue <= -50.0)
                    defValue = -49.9;

                ourValue = (atkValue + 50.0);
                theirValue = (defValue + 50.0);
            }

            double chance = ourValue / (theirValue * 2.0);

            chance *= 1.0 + ((double)bonus / 100);

            if (Core.AOS && chance < 0.02)
                chance = 0.02;

            return attacker.CheckSkill(atkSkill.SkillName, chance);
        }

        public virtual TimeSpan GetDelay(Mobile m)
        {
            double speed = this.Speed;

            if (speed == 0)
                return TimeSpan.FromHours(1.0);

            double delayInSeconds;

            if (Core.SE)
            {
                /*
                * This is likely true for Core.AOS as well... both guides report the same
                * formula, and both are wrong.
                * The old formula left in for AOS for legacy & because we aren't quite 100%
                * Sure that AOS has THIS formula
                */
                int bonus = AosAttributes.GetValue(m, AosAttribute.WeaponSpeed);

                if (Spells.Chivalry.DivineFurySpell.UnderEffect(m))
                    bonus += 10;

                // Bonus granted by successful use of Honorable Execution.
                bonus += HonorableExecution.GetSwingBonus(m);

                if (DualWield.Registry.Contains(m))
                    bonus += ((DualWield.DualWieldTimer)DualWield.Registry[m]).BonusSwingSpeed;

                if (Feint.Registry.Contains(m))
                    bonus -= ((Feint.FeintTimer)Feint.Registry[m]).SwingSpeedReduction;

                TransformContext context = TransformationSpellHelper.GetContext(m);

                if (context != null && context.Spell is ReaperFormSpell)
                    bonus += ((ReaperFormSpell)context.Spell).SwingSpeedBonus;

                int discordanceEffect = 0;

                // Discordance gives a malus of -0/-28% to swing speed.
                if (SkillHandlers.Discordance.GetEffect(m, ref discordanceEffect))
                    bonus -= discordanceEffect;

                if (EssenceOfWindSpell.IsDebuffed(m))
                    bonus -= EssenceOfWindSpell.GetSSIMalus(m);

                if (bonus > 60)
                    bonus = 60;
				
                double ticks;

                if (Core.ML)
                {
                    int stamTicks = m.Stam / 30;

                    ticks = speed * 4;
                    ticks = Math.Floor((ticks - stamTicks) * (100.0 / (100 + bonus)));
                }
                else
                {
                    speed = Math.Floor(speed * (bonus + 100.0) / 100.0);

                    if (speed <= 0)
                        speed = 1;

                    ticks = Math.Floor((80000.0 / ((m.Stam + 100) * speed)) - 2);
                }
				
                // Swing speed currently capped at one swing every 1.25 seconds (5 ticks).
                if (ticks < 5)
                    ticks = 5;

                delayInSeconds = ticks * 0.25;
            }
            else if (Core.AOS)
            {
                int v = (m.Stam + 100) * (int)speed;

                int bonus = AosAttributes.GetValue(m, AosAttribute.WeaponSpeed);

                if (Spells.Chivalry.DivineFurySpell.UnderEffect(m))
                    bonus += 10;

                int discordanceEffect = 0;

                // Discordance gives a malus of -0/-28% to swing speed.
                if (SkillHandlers.Discordance.GetEffect(m, ref discordanceEffect))
                    bonus -= discordanceEffect;

                v += AOS.Scale(v, bonus);

                if (v <= 0)
                    v = 1;

                delayInSeconds = Math.Floor(40000.0 / v) * 0.5;

                // Maximum swing rate capped at one swing per second 
                // OSI dev said that it has and is supposed to be 1.25
                if (delayInSeconds < 1.25)
                    delayInSeconds = 1.25;
            }
            else
            {
                int v = (m.Stam + 100) * (int)speed;

                if (v <= 0)
                    v = 1;

                delayInSeconds = 15000.0 / v;
            }

            return TimeSpan.FromSeconds(delayInSeconds);
        }

        public virtual void OnBeforeSwing(Mobile attacker, Mobile defender)
        {
            WeaponAbility a = WeaponAbility.GetCurrentAbility(attacker);

            if (a != null && !a.OnBeforeSwing(attacker, defender))
                WeaponAbility.ClearCurrentAbility(attacker);

            SpecialMove move = SpecialMove.GetCurrentMove(attacker);

            if (move != null && !move.OnBeforeSwing(attacker, defender))
                SpecialMove.ClearCurrentMove(attacker);
        }

        public virtual TimeSpan OnSwing(Mobile attacker, Mobile defender)
        {
            return this.OnSwing(attacker, defender, 1.0);
        }

        public virtual TimeSpan OnSwing(Mobile attacker, Mobile defender, double damageBonus)
        {
            bool canSwing = true;

            if (Core.AOS)
            {
                canSwing = (!attacker.Paralyzed && !attacker.Frozen);

                if (canSwing)
                {
                    Spell sp = attacker.Spell as Spell;

                    canSwing = (sp == null || !sp.IsCasting || !sp.BlocksMovement);
                }

                if (canSwing)
                {
                    PlayerMobile p = attacker as PlayerMobile;

                    canSwing = (p == null || p.PeacedUntil <= DateTime.Now);
                }
            }

            #region Dueling
            if (attacker is PlayerMobile)
            {
                PlayerMobile pm = (PlayerMobile)attacker;

                if (pm.DuelContext != null && !pm.DuelContext.CheckItemEquip(attacker, this))
                    canSwing = false;
            }
            #endregion

            if (canSwing && attacker.HarmfulCheck(defender))
            {
                attacker.DisruptiveAction();

                if (attacker.NetState != null)
                    attacker.Send(new Swing(0, attacker, defender));

                if (attacker is BaseCreature)
                {
                    BaseCreature bc = (BaseCreature)attacker;
                    WeaponAbility ab = bc.GetWeaponAbility();

                    if (ab != null)
                    {
                        if (bc.WeaponAbilityChance > Utility.RandomDouble())
                            WeaponAbility.SetCurrentAbility(bc, ab);
                        else
                            WeaponAbility.ClearCurrentAbility(bc);
                    }
                }

                if (this.CheckHit(attacker, defender))
                    this.OnHit(attacker, defender, damageBonus);
                else
                    this.OnMiss(attacker, defender);
            }

            return this.GetDelay(attacker);
        }

        #region Sounds
        public virtual int GetHitAttackSound(Mobile attacker, Mobile defender)
        {
            int sound = attacker.GetAttackSound();

            if (sound == -1)
                sound = this.HitSound;

            return sound;
        }

        public virtual int GetHitDefendSound(Mobile attacker, Mobile defender)
        {
            return defender.GetHurtSound();
        }

        public virtual int GetMissAttackSound(Mobile attacker, Mobile defender)
        {
            if (attacker.GetAttackSound() == -1)
                return this.MissSound;
            else
                return -1;
        }

        public virtual int GetMissDefendSound(Mobile attacker, Mobile defender)
        {
            return -1;
        }

        #endregion

        public static bool CheckParry(Mobile defender)
        {
            if (defender == null)
                return false;

            BaseShield shield = defender.FindItemOnLayer(Layer.TwoHanded) as BaseShield;

            double parry = defender.Skills[SkillName.Parry].Value;
            double bushidoNonRacial = defender.Skills[SkillName.Bushido].NonRacialValue;
            double bushido = defender.Skills[SkillName.Bushido].Value;

            if (shield != null)
            {
                double chance = (parry - bushidoNonRacial) / 400.0;	// As per OSI, no negitive effect from the Racial stuffs, ie, 120 parry and '0' bushido with humans

                if (chance < 0) // chance shouldn't go below 0
                    chance = 0;				

                // Parry/Bushido over 100 grants a 5% bonus.
                if (parry >= 100.0 || bushido >= 100.0)
                    chance += 0.05;

                // Evasion grants a variable bonus post ML. 50% prior.
                if (Evasion.IsEvading(defender))
                    chance *= Evasion.GetParryScalar(defender);

                // Low dexterity lowers the chance.
                if (defender.Dex < 80)
                    chance = chance * (20 + defender.Dex) / 100;

                return defender.CheckSkill(SkillName.Parry, chance);
            }
            else if (!(defender.Weapon is Fists) && !(defender.Weapon is BaseRanged))
            {
                BaseWeapon weapon = defender.Weapon as BaseWeapon;

                double divisor = (weapon.Layer == Layer.OneHanded) ? 48000.0 : 41140.0;

                double chance = (parry * bushido) / divisor;

                double aosChance = parry / 800.0;

                // Parry or Bushido over 100 grant a 5% bonus.
                if (parry >= 100.0)
                {
                    chance += 0.05;
                    aosChance += 0.05;
                }
                else if (bushido >= 100.0)
                {
                    chance += 0.05;
                }

                // Evasion grants a variable bonus post ML. 50% prior.
                if (Evasion.IsEvading(defender))
                    chance *= Evasion.GetParryScalar(defender);

                // Low dexterity lowers the chance.
                if (defender.Dex < 80)
                    chance = chance * (20 + defender.Dex) / 100;

                if (chance > aosChance)
                    return defender.CheckSkill(SkillName.Parry, chance);
                else
                    return (aosChance > Utility.RandomDouble()); // Only skillcheck if wielding a shield & there's no effect from Bushido
            }

            return false;
        }

        public virtual int AbsorbDamageAOS(Mobile attacker, Mobile defender, int damage)
        {
            bool blocked = false;

            if (defender.Player || defender.Body.IsHuman)
            {
                blocked = CheckParry(defender);

                if (blocked)
                {
                    defender.FixedEffect(0x37B9, 10, 16);
                    damage = 0;

                    // Successful block removes the Honorable Execution penalty.
                    HonorableExecution.RemovePenalty(defender);

                    if (CounterAttack.IsCountering(defender))
                    {
                        BaseWeapon weapon = defender.Weapon as BaseWeapon;

                        if (weapon != null)
                        {
                            defender.FixedParticles(0x3779, 1, 15, 0x158B, 0x0, 0x3, EffectLayer.Waist);
                            weapon.OnSwing(defender, attacker);
                        }

                        CounterAttack.StopCountering(defender);
                    }

                    if (Confidence.IsConfident(defender))
                    {
                        defender.SendLocalizedMessage(1063117); // Your confidence reassures you as you successfully block your opponent's blow.

                        double bushido = defender.Skills.Bushido.Value;

                        defender.Hits += Utility.RandomMinMax(1, (int)(bushido / 12));
                        defender.Stam += Utility.RandomMinMax(1, (int)(bushido / 5));
                    }

                    BaseShield shield = defender.FindItemOnLayer(Layer.TwoHanded) as BaseShield;

                    if (shield != null)
                    {
                        shield.OnHit(this, damage);
                    }
                }
            }

            if (!blocked)
            {
                double positionChance = Utility.RandomDouble();

                Item armorItem;

                if (positionChance < 0.07)
                    armorItem = defender.NeckArmor;
                else if (positionChance < 0.14)
                    armorItem = defender.HandArmor;
                else if (positionChance < 0.28)
                    armorItem = defender.ArmsArmor;
                else if (positionChance < 0.43)
                    armorItem = defender.HeadArmor;
                else if (positionChance < 0.65)
                    armorItem = defender.LegsArmor;
                else
                    armorItem = defender.ChestArmor;

                IWearableDurability armor = armorItem as IWearableDurability;

                if (armor != null)
                    armor.OnHit(this, damage); // call OnHit to lose durability
            }

            return damage;
        }

        public virtual int AbsorbDamage(Mobile attacker, Mobile defender, int damage)
        {
            if (Core.AOS)
                return this.AbsorbDamageAOS(attacker, defender, damage);

            double chance = Utility.RandomDouble();

            Item armorItem;

            if (chance < 0.07)
                armorItem = defender.NeckArmor;
            else if (chance < 0.14)
                armorItem = defender.HandArmor;
            else if (chance < 0.28)
                armorItem = defender.ArmsArmor;
            else if (chance < 0.43)
                armorItem = defender.HeadArmor;
            else if (chance < 0.65)
                armorItem = defender.LegsArmor;
            else
                armorItem = defender.ChestArmor;

            IWearableDurability armor = armorItem as IWearableDurability;

            if (armor != null)
                damage = armor.OnHit(this, damage);

            BaseShield shield = defender.FindItemOnLayer(Layer.TwoHanded) as BaseShield;
            if (shield != null)
                damage = shield.OnHit(this, damage);

            int virtualArmor = defender.VirtualArmor + defender.VirtualArmorMod;

            if (virtualArmor > 0)
            {
                double scalar;

                if (chance < 0.14)
                    scalar = 0.07;
                else if (chance < 0.28)
                    scalar = 0.14;
                else if (chance < 0.43)
                    scalar = 0.15;
                else if (chance < 0.65)
                    scalar = 0.22;
                else
                    scalar = 0.35;

                int from = (int)(virtualArmor * scalar) / 2;
                int to = (int)(virtualArmor * scalar);

                damage -= Utility.Random(from, (to - from) + 1);
            }

            return damage;
        }

        public virtual int GetPackInstinctBonus(Mobile attacker, Mobile defender)
        {
            if (attacker.Player || defender.Player)
                return 0;

            BaseCreature bc = attacker as BaseCreature;

            if (bc == null || bc.PackInstinct == PackInstinct.None || (!bc.Controlled && !bc.Summoned))
                return 0;

            Mobile master = bc.ControlMaster;

            if (master == null)
                master = bc.SummonMaster;

            if (master == null)
                return 0;

            int inPack = 1;

            foreach (Mobile m in defender.GetMobilesInRange(1))
            {
                if (m != attacker && m is BaseCreature)
                {
                    BaseCreature tc = (BaseCreature)m;

                    if ((tc.PackInstinct & bc.PackInstinct) == 0 || (!tc.Controlled && !tc.Summoned))
                        continue;

                    Mobile theirMaster = tc.ControlMaster;

                    if (theirMaster == null)
                        theirMaster = tc.SummonMaster;

                    if (master == theirMaster && tc.Combatant == defender)
                        ++inPack;
                }
            }

            if (inPack >= 5)
                return 100;
            else if (inPack >= 4)
                return 75;
            else if (inPack >= 3)
                return 50;
            else if (inPack >= 2)
                return 25;

            return 0;
        }

        private static bool m_InDoubleStrike;

        public static bool InDoubleStrike
        {
            get
            {
                return m_InDoubleStrike;
            }
            set
            {
                m_InDoubleStrike = value;
            }
        }

        public void OnHit(Mobile attacker, Mobile defender)
        {
            this.OnHit(attacker, defender, 1.0);
        }

        public virtual void OnHit(Mobile attacker, Mobile defender, double damageBonus)
        {
            if (MirrorImage.HasClone(defender) && (defender.Skills.Ninjitsu.Value / 150.0) > Utility.RandomDouble())
            {
                Clone bc;

                foreach (Mobile m in defender.GetMobilesInRange(4))
                {
                    bc = m as Clone;

                    if (bc != null && bc.Summoned && bc.SummonMaster == defender)
                    {
                        attacker.SendLocalizedMessage(1063141); // Your attack has been diverted to a nearby mirror image of your target!
                        defender.SendLocalizedMessage(1063140); // You manage to divert the attack onto one of your nearby mirror images.

                        /*
                        * TODO: What happens if the Clone parries a blow?
                        * And what about if the attacker is using Honorable Execution
                        * and kills it?
                        */

                        defender = m;
                        break;
                    }
                }
            }

            this.PlaySwingAnimation(attacker);
            this.PlayHurtAnimation(defender);

            attacker.PlaySound(this.GetHitAttackSound(attacker, defender));
            defender.PlaySound(this.GetHitDefendSound(attacker, defender));

            int damage = this.ComputeDamage(attacker, defender);

            #region Damage Multipliers
            /*
            * The following damage bonuses multiply damage by a factor.
            * Capped at x3 (300%).
            */
            int percentageBonus = 0;

            WeaponAbility a = WeaponAbility.GetCurrentAbility(attacker);
            SpecialMove move = SpecialMove.GetCurrentMove(attacker);

            if (a != null)
            {
                percentageBonus += (int)(a.DamageScalar * 100) - 100;
            }

            if (move != null)
            {
                percentageBonus += (int)(move.GetDamageScalar(attacker, defender) * 100) - 100;
            }

            percentageBonus += (int)(damageBonus * 100) - 100;

            CheckSlayerResult cs = this.CheckSlayers(attacker, defender);

            if (cs != CheckSlayerResult.None)
            {
                if (cs == CheckSlayerResult.Slayer)
                    defender.FixedEffect(0x37B9, 10, 5);

                percentageBonus += 100;
            }

            if (!attacker.Player)
            {
                if (defender is PlayerMobile)
                {
                    PlayerMobile pm = (PlayerMobile)defender;

                    if (pm.EnemyOfOneType != null && pm.EnemyOfOneType != attacker.GetType())
                    {
                        percentageBonus += 100;
                    }
                }
            }
            else if (!defender.Player)
            {
                if (attacker is PlayerMobile)
                {
                    PlayerMobile pm = (PlayerMobile)attacker;

                    if (pm.WaitingForEnemy)
                    {
                        pm.EnemyOfOneType = defender.GetType();
                        pm.WaitingForEnemy = false;
                    }

                    if (pm.EnemyOfOneType == defender.GetType())
                    {
                        defender.FixedEffect(0x37B9, 10, 5, 1160, 0);

                        percentageBonus += 50;
                    }
                }
            }

            int packInstinctBonus = this.GetPackInstinctBonus(attacker, defender);

            if (packInstinctBonus != 0)
            {
                percentageBonus += packInstinctBonus;
            }

            if (m_InDoubleStrike)
            {
                percentageBonus -= 10;
            }

            TransformContext context = TransformationSpellHelper.GetContext(defender);

            if ((this.m_Slayer == SlayerName.Silver || this.m_Slayer2 == SlayerName.Silver) && context != null && context.Spell is NecromancerSpell && context.Type != typeof(HorrificBeastSpell))
            {
                // Every necromancer transformation other than horrific beast takes an additional 25% damage
                percentageBonus += 25;
            }

            if (attacker is PlayerMobile && !(Core.ML && defender is PlayerMobile))
            {
                PlayerMobile pmAttacker = (PlayerMobile)attacker;

                if (pmAttacker.HonorActive && pmAttacker.InRange(defender, 1))
                {
                    percentageBonus += 25;
                }

                if (pmAttacker.SentHonorContext != null && pmAttacker.SentHonorContext.Target == defender)
                {
                    percentageBonus += pmAttacker.SentHonorContext.PerfectionDamageBonus;
                }
            }

            BaseTalisman talisman = attacker.Talisman as BaseTalisman;

            if (talisman != null && talisman.Killer != null)
                percentageBonus += talisman.Killer.DamageBonus(defender);

            percentageBonus = Math.Min(percentageBonus, 300);

            damage = AOS.Scale(damage, 100 + percentageBonus);
            #endregion

            if (attacker is BaseCreature)
                ((BaseCreature)attacker).AlterMeleeDamageTo(defender, ref damage);

            if (defender is BaseCreature)
                ((BaseCreature)defender).AlterMeleeDamageFrom(attacker, ref damage);

            damage = this.AbsorbDamage(attacker, defender, damage);

            if (!Core.AOS && damage < 1)
                damage = 1;
            else if (Core.AOS && damage == 0) // parried
            {
                if (a != null && a.Validate(attacker) /*&& a.CheckMana( attacker, true )*/) // Parried special moves have no mana cost 
                {
                    a = null;
                    WeaponAbility.ClearCurrentAbility(attacker);

                    attacker.SendLocalizedMessage(1061140); // Your attack was parried!
                }
            }

            this.AddBlood(attacker, defender, damage);

            int phys, fire, cold, pois, nrgy, chaos, direct;

            this.GetDamageTypes(attacker, out phys, out fire, out cold, out pois, out nrgy, out chaos, out direct);

            if (Core.ML && this is BaseRanged)
            {
                BaseQuiver quiver = attacker.FindItemOnLayer(Layer.Cloak) as BaseQuiver;

                if (quiver != null)
                    quiver.AlterBowDamage(ref phys, ref fire, ref cold, ref pois, ref nrgy, ref chaos, ref direct);
            }

            if (this.m_Consecrated)
            {
                phys = defender.PhysicalResistance;
                fire = defender.FireResistance;
                cold = defender.ColdResistance;
                pois = defender.PoisonResistance;
                nrgy = defender.EnergyResistance;

                int low = phys, type = 0;

                if (fire < low)
                {
                    low = fire;
                    type = 1;
                }
                if (cold < low)
                {
                    low = cold;
                    type = 2;
                }
                if (pois < low)
                {
                    low = pois;
                    type = 3;
                }
                if (nrgy < low)
                {
                    low = nrgy;
                    type = 4;
                }

                phys = fire = cold = pois = nrgy = chaos = direct = 0;

                if (type == 0)
                    phys = 100;
                else if (type == 1)
                    fire = 100;
                else if (type == 2)
                    cold = 100;
                else if (type == 3)
                    pois = 100;
                else if (type == 4)
                    nrgy = 100;
            }

            // TODO: Scale damage, alongside the leech effects below, to weapon speed.
            if (ImmolatingWeaponSpell.IsImmolating(this) && damage > 0)
                ImmolatingWeaponSpell.DoEffect(this, defender);

            int damageGiven = damage;

            if (a != null && !a.OnBeforeDamage(attacker, defender))
            {
                WeaponAbility.ClearCurrentAbility(attacker);
                a = null;
            }

            if (move != null && !move.OnBeforeDamage(attacker, defender))
            {
                SpecialMove.ClearCurrentMove(attacker);
                move = null;
            }

            bool ignoreArmor = (a is ArmorIgnore || (move != null && move.IgnoreArmor(attacker)));

            damageGiven = AOS.Damage(defender, attacker, damage, ignoreArmor, phys, fire, cold, pois, nrgy, chaos, direct, false, this is BaseRanged, false);

            double propertyBonus = (move == null) ? 1.0 : move.GetPropertyBonus(attacker);

            if (Core.AOS)
            {
                int lifeLeech = 0;
                int stamLeech = 0;
                int manaLeech = 0;
                int wraithLeech = 0;

                if ((int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitLeechHits) * propertyBonus) > Utility.Random(100))
                    lifeLeech += 30; // HitLeechHits% chance to leech 30% of damage as hit points

                if ((int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitLeechStam) * propertyBonus) > Utility.Random(100))
                    stamLeech += 100; // HitLeechStam% chance to leech 100% of damage as stamina

                if ((int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitLeechMana) * propertyBonus) > Utility.Random(100))
                    manaLeech += 40; // HitLeechMana% chance to leech 40% of damage as mana

                if (this.m_Cursed)
                    lifeLeech += 50; // Additional 50% life leech for cursed weapons (necro spell)

                context = TransformationSpellHelper.GetContext(attacker);

                if (context != null && context.Type == typeof(VampiricEmbraceSpell))
                    lifeLeech += 20; // Vampiric embrace gives an additional 20% life leech

                if (context != null && context.Type == typeof(WraithFormSpell))
                {
                    wraithLeech = (5 + (int)((15 * attacker.Skills.SpiritSpeak.Value) / 100)); // Wraith form gives an additional 5-20% mana leech

                    // Mana leeched by the Wraith Form spell is actually stolen, not just leeched.
                    defender.Mana -= AOS.Scale(damageGiven, wraithLeech);

                    manaLeech += wraithLeech;
                }

                if (lifeLeech != 0)
                    attacker.Hits += AOS.Scale(damageGiven, lifeLeech);

                if (stamLeech != 0)
                    attacker.Stam += AOS.Scale(damageGiven, stamLeech);

                if (manaLeech != 0)
                    attacker.Mana += AOS.Scale(damageGiven, manaLeech);

                if (lifeLeech != 0 || stamLeech != 0 || manaLeech != 0)
                    attacker.PlaySound(0x44D);
            }

            if (this.m_MaxHits > 0 && ((this.MaxRange <= 1 && (defender is Slime || defender is ToxicElemental)) || Utility.Random(25) == 0)) // Stratics says 50% chance, seems more like 4%..
            {
                if (this.MaxRange <= 1 && (defender is Slime || defender is ToxicElemental))
                    attacker.LocalOverheadMessage(MessageType.Regular, 0x3B2, 500263); // *Acid blood scars your weapon!*

                if (Core.AOS && this.m_AosWeaponAttributes.SelfRepair > Utility.Random(10))
                {
                    this.HitPoints += 2;
                }
                else
                {
                    if (this.m_Hits > 0)
                    {
                        --this.HitPoints;
                    }
                    else if (this.m_MaxHits > 1)
                    {
                        --this.MaxHitPoints;

                        if (this.Parent is Mobile)
                            ((Mobile)this.Parent).LocalOverheadMessage(MessageType.Regular, 0x3B2, 1061121); // Your equipment is severely damaged.
                    }
                    else
                    {
                        this.Delete();
                    }
                }
            }

            if (attacker is VampireBatFamiliar)
            {
                BaseCreature bc = (BaseCreature)attacker;
                Mobile caster = bc.ControlMaster;

                if (caster == null)
                    caster = bc.SummonMaster;

                if (caster != null && caster.Map == bc.Map && caster.InRange(bc, 2))
                    caster.Hits += damage;
                else
                    bc.Hits += damage;
            }

            if (Core.AOS)
            {
                int physChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitPhysicalArea) * propertyBonus);
                int fireChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitFireArea) * propertyBonus);
                int coldChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitColdArea) * propertyBonus);
                int poisChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitPoisonArea) * propertyBonus);
                int nrgyChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitEnergyArea) * propertyBonus);

                if (physChance != 0 && physChance > Utility.Random(100))
                    this.DoAreaAttack(attacker, defender, 0x10E, 50, 100, 0, 0, 0, 0);

                if (fireChance != 0 && fireChance > Utility.Random(100))
                    this.DoAreaAttack(attacker, defender, 0x11D, 1160, 0, 100, 0, 0, 0);

                if (coldChance != 0 && coldChance > Utility.Random(100))
                    this.DoAreaAttack(attacker, defender, 0x0FC, 2100, 0, 0, 100, 0, 0);

                if (poisChance != 0 && poisChance > Utility.Random(100))
                    this.DoAreaAttack(attacker, defender, 0x205, 1166, 0, 0, 0, 100, 0);

                if (nrgyChance != 0 && nrgyChance > Utility.Random(100))
                    this.DoAreaAttack(attacker, defender, 0x1F1, 120, 0, 0, 0, 0, 100);

                int maChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitMagicArrow) * propertyBonus);
                int harmChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitHarm) * propertyBonus);
                int fireballChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitFireball) * propertyBonus);
                int lightningChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitLightning) * propertyBonus);
                int dispelChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitDispel) * propertyBonus);

                if (maChance != 0 && maChance > Utility.Random(100))
                    this.DoMagicArrow(attacker, defender);

                if (harmChance != 0 && harmChance > Utility.Random(100))
                    this.DoHarm(attacker, defender);

                if (fireballChance != 0 && fireballChance > Utility.Random(100))
                    this.DoFireball(attacker, defender);

                if (lightningChance != 0 && lightningChance > Utility.Random(100))
                    this.DoLightning(attacker, defender);

                if (dispelChance != 0 && dispelChance > Utility.Random(100))
                    this.DoDispel(attacker, defender);

                int laChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitLowerAttack) * propertyBonus);
                int ldChance = (int)(AosWeaponAttributes.GetValue(attacker, AosWeaponAttribute.HitLowerDefend) * propertyBonus);

                if (laChance != 0 && laChance > Utility.Random(100))
                    this.DoLowerAttack(attacker, defender);

                if (ldChance != 0 && ldChance > Utility.Random(100))
                    this.DoLowerDefense(attacker, defender);
            }

            if (attacker is BaseCreature)
                ((BaseCreature)attacker).OnGaveMeleeAttack(defender);

            if (defender is BaseCreature)
                ((BaseCreature)defender).OnGotMeleeAttack(attacker);

            if (a != null)
                a.OnHit(attacker, defender, damage);

            if (move != null)
                move.OnHit(attacker, defender, damage);

            if (defender is IHonorTarget && ((IHonorTarget)defender).ReceivedHonorContext != null)
                ((IHonorTarget)defender).ReceivedHonorContext.OnTargetHit(attacker);

            if (!(this is BaseRanged))
            {
                if (AnimalForm.UnderTransformation(attacker, typeof(GiantSerpent)))
                    defender.ApplyPoison(attacker, Poison.Lesser);

                if (AnimalForm.UnderTransformation(defender, typeof(BullFrog)))
                    attacker.ApplyPoison(defender, Poison.Regular);
            }
        }

        public virtual double GetAosDamage(Mobile attacker, int bonus, int dice, int sides)
        {
            int damage = Utility.Dice(dice, sides, bonus) * 100;
            int damageBonus = 0;

            // Inscription bonus
            int inscribeSkill = attacker.Skills[SkillName.Inscribe].Fixed;

            damageBonus += inscribeSkill / 200;

            if (inscribeSkill >= 1000)
                damageBonus += 5;

            if (attacker.Player)
            {
                // Int bonus
                damageBonus += (attacker.Int / 10);

                // SDI bonus
                damageBonus += AosAttributes.GetValue(attacker, AosAttribute.SpellDamage);

                TransformContext context = TransformationSpellHelper.GetContext(attacker);

                if (context != null && context.Spell is ReaperFormSpell)
                    damageBonus += ((ReaperFormSpell)context.Spell).SpellDamageBonus;
            }

            damage = AOS.Scale(damage, 100 + damageBonus);

            return damage / 100;
        }

        #region Do<AoSEffect>
        public virtual void DoMagicArrow(Mobile attacker, Mobile defender)
        {
            if (!attacker.CanBeHarmful(defender, false))
                return;

            attacker.DoHarmful(defender);

            double damage = this.GetAosDamage(attacker, 10, 1, 4);

            attacker.MovingParticles(defender, 0x36E4, 5, 0, false, true, 3006, 4006, 0);
            attacker.PlaySound(0x1E5);

            SpellHelper.Damage(TimeSpan.FromSeconds(1.0), defender, attacker, damage, 0, 100, 0, 0, 0);
        }

        public virtual void DoHarm(Mobile attacker, Mobile defender)
        {
            if (!attacker.CanBeHarmful(defender, false))
                return;

            attacker.DoHarmful(defender);

            double damage = this.GetAosDamage(attacker, 17, 1, 5);

            if (!defender.InRange(attacker, 2))
                damage *= 0.25; // 1/4 damage at > 2 tile range
            else if (!defender.InRange(attacker, 1))
                damage *= 0.50; // 1/2 damage at 2 tile range

            defender.FixedParticles(0x374A, 10, 30, 5013, 1153, 2, EffectLayer.Waist);
            defender.PlaySound(0x0FC);

            SpellHelper.Damage(TimeSpan.Zero, defender, attacker, damage, 0, 0, 100, 0, 0);
        }

        public virtual void DoFireball(Mobile attacker, Mobile defender)
        {
            if (!attacker.CanBeHarmful(defender, false))
                return;

            attacker.DoHarmful(defender);

            double damage = this.GetAosDamage(attacker, 19, 1, 5);

            attacker.MovingParticles(defender, 0x36D4, 7, 0, false, true, 9502, 4019, 0x160);
            attacker.PlaySound(0x15E);

            SpellHelper.Damage(TimeSpan.FromSeconds(1.0), defender, attacker, damage, 0, 100, 0, 0, 0);
        }

        public virtual void DoLightning(Mobile attacker, Mobile defender)
        {
            if (!attacker.CanBeHarmful(defender, false))
                return;

            attacker.DoHarmful(defender);

            double damage = this.GetAosDamage(attacker, 23, 1, 4);

            defender.BoltEffect(0);

            SpellHelper.Damage(TimeSpan.Zero, defender, attacker, damage, 0, 0, 0, 0, 100);
        }

        public virtual void DoDispel(Mobile attacker, Mobile defender)
        {
            bool dispellable = false;

            if (defender is BaseCreature)
                dispellable = ((BaseCreature)defender).Summoned && !((BaseCreature)defender).IsAnimatedDead;

            if (!dispellable)
                return;

            if (!attacker.CanBeHarmful(defender, false))
                return;

            attacker.DoHarmful(defender);

            Spells.MagerySpell sp = new Spells.Sixth.DispelSpell(attacker, null);

            if (sp.CheckResisted(defender))
            {
                defender.FixedEffect(0x3779, 10, 20);
            }
            else
            {
                Effects.SendLocationParticles(EffectItem.Create(defender.Location, defender.Map, EffectItem.DefaultDuration), 0x3728, 8, 20, 5042);
                Effects.PlaySound(defender, defender.Map, 0x201);

                defender.Delete();
            }
        }

        public virtual void DoLowerAttack(Mobile from, Mobile defender)
        {
            if (HitLower.ApplyAttack(defender))
            {
                defender.PlaySound(0x28E);
                Effects.SendTargetEffect(defender, 0x37BE, 1, 4, 0xA, 3);
            }
        }

        public virtual void DoLowerDefense(Mobile from, Mobile defender)
        {
            if (HitLower.ApplyDefense(defender))
            {
                defender.PlaySound(0x28E);
                Effects.SendTargetEffect(defender, 0x37BE, 1, 4, 0x23, 3);
            }
        }

        public virtual void DoAreaAttack(Mobile from, Mobile defender, int sound, int hue, int phys, int fire, int cold, int pois, int nrgy)
        {
            Map map = from.Map;

            if (map == null)
                return;

            List<Mobile> list = new List<Mobile>();

            foreach (Mobile m in from.GetMobilesInRange(10))
            {
                if (from != m && defender != m && SpellHelper.ValidIndirectTarget(from, m) && from.CanBeHarmful(m, false) && (!Core.ML || from.InLOS(m)))
                    list.Add(m);
            }

            if (list.Count == 0)
                return;

            Effects.PlaySound(from.Location, map, sound);

            // TODO: What is the damage calculation?

            for (int i = 0; i < list.Count; ++i)
            {
                Mobile m = list[i];

                double scalar = (11 - from.GetDistanceToSqrt(m)) / 10;

                if (scalar > 1.0)
                    scalar = 1.0;
                else if (scalar < 0.0)
                    continue;

                from.DoHarmful(m, true);
                m.FixedEffect(0x3779, 1, 15, hue, 0);
                AOS.Damage(m, from, (int)(this.GetBaseDamage(from) * scalar), phys, fire, cold, pois, nrgy);
            }
        }

        #endregion

        public virtual CheckSlayerResult CheckSlayers(Mobile attacker, Mobile defender)
        {
            BaseWeapon atkWeapon = attacker.Weapon as BaseWeapon;
            SlayerEntry atkSlayer = SlayerGroup.GetEntryByName(atkWeapon.Slayer);
            SlayerEntry atkSlayer2 = SlayerGroup.GetEntryByName(atkWeapon.Slayer2);

            if (atkSlayer != null && atkSlayer.Slays(defender) || atkSlayer2 != null && atkSlayer2.Slays(defender))
                return CheckSlayerResult.Slayer;

            BaseTalisman talisman = attacker.Talisman as BaseTalisman;

            if (talisman != null && TalismanSlayer.Slays(talisman.Slayer, defender))
                return CheckSlayerResult.Slayer;

            if (!Core.SE)
            {
                ISlayer defISlayer = Spellbook.FindEquippedSpellbook(defender);

                if (defISlayer == null)
                    defISlayer = defender.Weapon as ISlayer;

                if (defISlayer != null)
                {
                    SlayerEntry defSlayer = SlayerGroup.GetEntryByName(defISlayer.Slayer);
                    SlayerEntry defSlayer2 = SlayerGroup.GetEntryByName(defISlayer.Slayer2);

                    if (defSlayer != null && defSlayer.Group.OppositionSuperSlays(attacker) || defSlayer2 != null && defSlayer2.Group.OppositionSuperSlays(attacker))
                        return CheckSlayerResult.Opposition;
                }
            }

            return CheckSlayerResult.None;
        }

        public virtual void AddBlood(Mobile attacker, Mobile defender, int damage)
        {
            if (damage > 0)
            {
                new Blood().MoveToWorld(defender.Location, defender.Map);

                int extraBlood = (Core.SE ? Utility.RandomMinMax(3, 4) : Utility.RandomMinMax(0, 1));

                for (int i = 0; i < extraBlood; i++)
                {
                    new Blood().MoveToWorld(new Point3D(
                        defender.X + Utility.RandomMinMax(-1, 1),
                        defender.Y + Utility.RandomMinMax(-1, 1),
                        defender.Z), defender.Map);
                }
            }
        }

        public virtual void GetDamageTypes(Mobile wielder, out int phys, out int fire, out int cold, out int pois, out int nrgy, out int chaos, out int direct)
        {
            if (wielder is BaseCreature)
            {
                BaseCreature bc = (BaseCreature)wielder;

                phys = bc.PhysicalDamage;
                fire = bc.FireDamage;
                cold = bc.ColdDamage;
                pois = bc.PoisonDamage;
                nrgy = bc.EnergyDamage;
                chaos = bc.ChaosDamage;
                direct = bc.DirectDamage;
            }
            else
            {
                fire = this.m_AosElementDamages.Fire;
                cold = this.m_AosElementDamages.Cold;
                pois = this.m_AosElementDamages.Poison;
                nrgy = this.m_AosElementDamages.Energy;
                chaos = this.m_AosElementDamages.Chaos;
                direct = this.m_AosElementDamages.Direct;

                phys = 100 - fire - cold - pois - nrgy - chaos - direct;

                CraftResourceInfo resInfo = CraftResources.GetInfo(this.m_Resource);

                if (resInfo != null)
                {
                    CraftAttributeInfo attrInfo = resInfo.AttributeInfo;

                    if (attrInfo != null)
                    {
                        int left = phys;

                        left = this.ApplyCraftAttributeElementDamage(attrInfo.WeaponColdDamage, ref cold, left);
                        left = this.ApplyCraftAttributeElementDamage(attrInfo.WeaponEnergyDamage, ref nrgy, left);
                        left = this.ApplyCraftAttributeElementDamage(attrInfo.WeaponFireDamage, ref fire, left);
                        left = this.ApplyCraftAttributeElementDamage(attrInfo.WeaponPoisonDamage, ref pois, left);
                        left = this.ApplyCraftAttributeElementDamage(attrInfo.WeaponChaosDamage, ref chaos, left);
                        left = this.ApplyCraftAttributeElementDamage(attrInfo.WeaponDirectDamage, ref direct, left);

                        phys = left;
                    }
                }
            }
        }

        private int ApplyCraftAttributeElementDamage(int attrDamage, ref int element, int totalRemaining)
        {
            if (totalRemaining <= 0)
                return 0;

            if (attrDamage <= 0)
                return totalRemaining;

            int appliedDamage = attrDamage;

            if ((appliedDamage + element) > 100)
                appliedDamage = 100 - element;

            if (appliedDamage > totalRemaining)
                appliedDamage = totalRemaining;

            element += appliedDamage;

            return totalRemaining - appliedDamage;
        }

        public virtual void OnMiss(Mobile attacker, Mobile defender)
        {
            this.PlaySwingAnimation(attacker);
            attacker.PlaySound(this.GetMissAttackSound(attacker, defender));
            defender.PlaySound(this.GetMissDefendSound(attacker, defender));

            WeaponAbility ability = WeaponAbility.GetCurrentAbility(attacker);

            if (ability != null)
                ability.OnMiss(attacker, defender);

            SpecialMove move = SpecialMove.GetCurrentMove(attacker);

            if (move != null)
                move.OnMiss(attacker, defender);

            if (defender is IHonorTarget && ((IHonorTarget)defender).ReceivedHonorContext != null)
                ((IHonorTarget)defender).ReceivedHonorContext.OnTargetMissed(attacker);
        }

        public virtual void GetBaseDamageRange(Mobile attacker, out int min, out int max)
        {
            if (attacker is BaseCreature)
            {
                BaseCreature c = (BaseCreature)attacker;

                if (c.DamageMin >= 0)
                {
                    min = c.DamageMin;
                    max = c.DamageMax;
                    return;
                }

                if (this is Fists && !attacker.Body.IsHuman)
                {
                    min = attacker.Str / 28;
                    max = attacker.Str / 28;
                    return;
                }
            }

            min = this.MinDamage;
            max = this.MaxDamage;
        }

        public virtual double GetBaseDamage(Mobile attacker)
        {
            int min, max;

            this.GetBaseDamageRange(attacker, out min, out max);

            return Utility.RandomMinMax(min, max);
        }

        public virtual double GetBonus(double value, double scalar, double threshold, double offset)
        {
            double bonus = value * scalar;

            if (value >= threshold)
                bonus += offset;

            return bonus / 100;
        }

        public virtual int GetHitChanceBonus()
        {
            if (!Core.AOS)
                return 0;

            int bonus = 0;

            switch ( this.m_AccuracyLevel )
            {
                case WeaponAccuracyLevel.Accurate:
                    bonus += 02;
                    break;
                case WeaponAccuracyLevel.Surpassingly:
                    bonus += 04;
                    break;
                case WeaponAccuracyLevel.Eminently:
                    bonus += 06;
                    break;
                case WeaponAccuracyLevel.Exceedingly:
                    bonus += 08;
                    break;
                case WeaponAccuracyLevel.Supremely:
                    bonus += 10;
                    break;
            }

            return bonus;
        }

        public virtual int GetDamageBonus()
        {
            int bonus = this.VirtualDamageBonus;

            switch ( this.m_Quality )
            {
                case WeaponQuality.Low:
                    bonus -= 20;
                    break;
                case WeaponQuality.Exceptional:
                    bonus += 20;
                    break;
            }

            switch ( this.m_DamageLevel )
            {
                case WeaponDamageLevel.Ruin:
                    bonus += 15;
                    break;
                case WeaponDamageLevel.Might:
                    bonus += 20;
                    break;
                case WeaponDamageLevel.Force:
                    bonus += 25;
                    break;
                case WeaponDamageLevel.Power:
                    bonus += 30;
                    break;
                case WeaponDamageLevel.Vanq:
                    bonus += 35;
                    break;
            }

            return bonus;
        }

        public virtual void GetStatusDamage(Mobile from, out int min, out int max)
        {
            int baseMin, baseMax;

            this.GetBaseDamageRange(from, out baseMin, out baseMax);

            if (Core.AOS)
            {
                min = Math.Max((int)this.ScaleDamageAOS(from, baseMin, false), 1);
                max = Math.Max((int)this.ScaleDamageAOS(from, baseMax, false), 1);
            }
            else
            {
                min = Math.Max((int)this.ScaleDamageOld(from, baseMin, false), 1);
                max = Math.Max((int)this.ScaleDamageOld(from, baseMax, false), 1);
            }
        }

        public virtual double ScaleDamageAOS(Mobile attacker, double damage, bool checkSkills)
        {
            if (checkSkills)
            {
                attacker.CheckSkill(SkillName.Tactics, 0.0, attacker.Skills[SkillName.Tactics].Cap); // Passively check tactics for gain
                attacker.CheckSkill(SkillName.Anatomy, 0.0, attacker.Skills[SkillName.Anatomy].Cap); // Passively check Anatomy for gain

                if (this.Type == WeaponType.Axe)
                    attacker.CheckSkill(SkillName.Lumberjacking, 0.0, 100.0); // Passively check Lumberjacking for gain
            }

            #region Physical bonuses
            /*
            * These are the bonuses given by the physical characteristics of the mobile.
            * No caps apply.
            */
            double strengthBonus = this.GetBonus(attacker.Str, 0.300, 100.0, 5.00);
            double anatomyBonus = this.GetBonus(attacker.Skills[SkillName.Anatomy].Value, 0.500, 100.0, 5.00);
            double tacticsBonus = this.GetBonus(attacker.Skills[SkillName.Tactics].Value, 0.625, 100.0, 6.25);
            double lumberBonus = this.GetBonus(attacker.Skills[SkillName.Lumberjacking].Value, 0.200, 100.0, 10.00);

            if (this.Type != WeaponType.Axe)
                lumberBonus = 0.0;
            #endregion

            #region Modifiers
            /*
            * The following are damage modifiers whose effect shows on the status bar.
            * Capped at 100% total.
            */
            int damageBonus = AosAttributes.GetValue(attacker, AosAttribute.WeaponDamage);

            // Horrific Beast transformation gives a +25% bonus to damage.
            if (TransformationSpellHelper.UnderTransformation(attacker, typeof(HorrificBeastSpell)))
                damageBonus += 25;

            // Divine Fury gives a +10% bonus to damage.
            if (Spells.Chivalry.DivineFurySpell.UnderEffect(attacker))
                damageBonus += 10;

            int defenseMasteryMalus = 0;

            // Defense Mastery gives a -50%/-80% malus to damage.
            if (Server.Items.DefenseMastery.GetMalus(attacker, ref defenseMasteryMalus))
                damageBonus -= defenseMasteryMalus;

            int discordanceEffect = 0;

            // Discordance gives a -2%/-48% malus to damage.
            if (SkillHandlers.Discordance.GetEffect(attacker, ref discordanceEffect))
                damageBonus -= discordanceEffect * 2;

            if (damageBonus > 100)
                damageBonus = 100;
            #endregion

            double totalBonus = strengthBonus + anatomyBonus + tacticsBonus + lumberBonus + ((double)(this.GetDamageBonus() + damageBonus) / 100.0);

            return damage + (int)(damage * totalBonus);
        }

        public virtual int VirtualDamageBonus
        {
            get
            {
                return 0;
            }
        }

        public virtual int ComputeDamageAOS(Mobile attacker, Mobile defender)
        {
            return (int)this.ScaleDamageAOS(attacker, this.GetBaseDamage(attacker), true);
        }

        public virtual double ScaleDamageOld(Mobile attacker, double damage, bool checkSkills)
        {
            if (checkSkills)
            {
                attacker.CheckSkill(SkillName.Tactics, 0.0, attacker.Skills[SkillName.Tactics].Cap); // Passively check tactics for gain
                attacker.CheckSkill(SkillName.Anatomy, 0.0, attacker.Skills[SkillName.Anatomy].Cap); // Passively check Anatomy for gain

                if (this.Type == WeaponType.Axe)
                    attacker.CheckSkill(SkillName.Lumberjacking, 0.0, 100.0); // Passively check Lumberjacking for gain
            }

            /* Compute tactics modifier
            * :   0.0 = 50% loss
            * :  50.0 = unchanged
            * : 100.0 = 50% bonus
            */
            double tacticsBonus = (attacker.Skills[SkillName.Tactics].Value - 50.0) / 100.0;

            /* Compute strength modifier
            * : 1% bonus for every 5 strength
            */
            double strBonus = (attacker.Str / 5.0) / 100.0;

            /* Compute anatomy modifier
            * : 1% bonus for every 5 points of anatomy
            * : +10% bonus at Grandmaster or higher
            */
            double anatomyValue = attacker.Skills[SkillName.Anatomy].Value;
            double anatomyBonus = (anatomyValue / 5.0) / 100.0;

            if (anatomyValue >= 100.0)
                anatomyBonus += 0.1;

            /* Compute lumberjacking bonus
            * : 1% bonus for every 5 points of lumberjacking
            * : +10% bonus at Grandmaster or higher
            */
            double lumberBonus;

            if (this.Type == WeaponType.Axe)
            {
                double lumberValue = attacker.Skills[SkillName.Lumberjacking].Value;

                lumberBonus = (lumberValue / 5.0) / 100.0;

                if (lumberValue >= 100.0)
                    lumberBonus += 0.1;
            }
            else
            {
                lumberBonus = 0.0;
            }

            // New quality bonus:
            double qualityBonus = ((int)this.m_Quality - 1) * 0.2;

            // Apply bonuses
            damage += (damage * tacticsBonus) + (damage * strBonus) + (damage * anatomyBonus) + (damage * lumberBonus) + (damage * qualityBonus) + ((damage * this.VirtualDamageBonus) / 100);

            // Old quality bonus:
            #if false
			/* Apply quality offset
			 * : Low         : -4
			 * : Regular     :  0
			 * : Exceptional : +4
			 */
			damage += ((int)m_Quality - 1) * 4.0;
            #endif

            /* Apply damage level offset
            * : Regular : 0
            * : Ruin    : 1
            * : Might   : 3
            * : Force   : 5
            * : Power   : 7
            * : Vanq    : 9
            */
            if (this.m_DamageLevel != WeaponDamageLevel.Regular)
                damage += (2.0 * (int)this.m_DamageLevel) - 1.0;

            // Halve the computed damage and return
            damage /= 2.0;

            return this.ScaleDamageByDurability((int)damage);
        }

        public virtual int ScaleDamageByDurability(int damage)
        {
            int scale = 100;

            if (this.m_MaxHits > 0 && this.m_Hits < this.m_MaxHits)
                scale = 50 + ((50 * this.m_Hits) / this.m_MaxHits);

            return AOS.Scale(damage, scale);
        }

        public virtual int ComputeDamage(Mobile attacker, Mobile defender)
        {
            if (Core.AOS)
                return this.ComputeDamageAOS(attacker, defender);

            return (int)this.ScaleDamageOld(attacker, this.GetBaseDamage(attacker), true);
        }

        public virtual void PlayHurtAnimation(Mobile from)
        {
            int action;
            int frames;

            switch ( from.Body.Type )
            {
                case BodyType.Sea:
                case BodyType.Animal:
                    {
                        action = 7;
                        frames = 5;
                        break;
                    }
                case BodyType.Monster:
                    {
                        action = 10;
                        frames = 4;
                        break;
                    }
                case BodyType.Human:
                    {
                        action = 20;
                        frames = 5;
                        break;
                    }
                default:
                    return;
            }

            if (from.Mounted)
                return;

            from.Animate(action, frames, 1, true, false, 0);
        }

        public virtual void PlaySwingAnimation(Mobile from)
        {
            int action;

            switch ( from.Body.Type )
            {
                case BodyType.Sea:
                case BodyType.Animal:
                    {
                        action = Utility.Random(5, 2);
                        break;
                    }
                case BodyType.Monster:
                    {
                        switch ( this.Animation )
                        {
                            default:
                            case WeaponAnimation.Wrestle:
                            case WeaponAnimation.Bash1H:
                            case WeaponAnimation.Pierce1H:
                            case WeaponAnimation.Slash1H:
                            case WeaponAnimation.Bash2H:
                            case WeaponAnimation.Pierce2H:
                            case WeaponAnimation.Slash2H:
                                action = Utility.Random(4, 3);
                                break;
                            case WeaponAnimation.ShootBow:
                                return; // 7
                            case WeaponAnimation.ShootXBow:
                                return; // 8
                        }

                        break;
                    }
                case BodyType.Human:
                    {
                        if (!from.Mounted)
                        {
                            action = (int)this.Animation;
                        }
                        else
                        {
                            switch ( this.Animation )
                            {
                                default:
                                case WeaponAnimation.Wrestle:
                                case WeaponAnimation.Bash1H:
                                case WeaponAnimation.Pierce1H:
                                case WeaponAnimation.Slash1H:
                                    action = 26;
                                    break;
                                case WeaponAnimation.Bash2H:
                                case WeaponAnimation.Pierce2H:
                                case WeaponAnimation.Slash2H:
                                    action = 29;
                                    break;
                                case WeaponAnimation.ShootBow:
                                    action = 27;
                                    break;
                                case WeaponAnimation.ShootXBow:
                                    action = 28;
                                    break;
                            }
                        }

                        break;
                    }
                default:
                    return;
            }

            from.Animate(action, 7, 1, true, false, 0);
        }

        #region Serialization/Deserialization
        private static void SetSaveFlag(ref SaveFlag flags, SaveFlag toSet, bool setIf)
        {
            if (setIf)
                flags |= toSet;
        }

        private static bool GetSaveFlag(SaveFlag flags, SaveFlag toGet)
        {
            return ((flags & toGet) != 0);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)9); // version

            SaveFlag flags = SaveFlag.None;

            SetSaveFlag(ref flags, SaveFlag.DamageLevel, this.m_DamageLevel != WeaponDamageLevel.Regular);
            SetSaveFlag(ref flags, SaveFlag.AccuracyLevel, this.m_AccuracyLevel != WeaponAccuracyLevel.Regular);
            SetSaveFlag(ref flags, SaveFlag.DurabilityLevel, this.m_DurabilityLevel != WeaponDurabilityLevel.Regular);
            SetSaveFlag(ref flags, SaveFlag.Quality, this.m_Quality != WeaponQuality.Regular);
            SetSaveFlag(ref flags, SaveFlag.Hits, this.m_Hits != 0);
            SetSaveFlag(ref flags, SaveFlag.MaxHits, this.m_MaxHits != 0);
            SetSaveFlag(ref flags, SaveFlag.Slayer, this.m_Slayer != SlayerName.None);
            SetSaveFlag(ref flags, SaveFlag.Poison, this.m_Poison != null);
            SetSaveFlag(ref flags, SaveFlag.PoisonCharges, this.m_PoisonCharges != 0);
            SetSaveFlag(ref flags, SaveFlag.Crafter, this.m_Crafter != null);
            SetSaveFlag(ref flags, SaveFlag.Identified, this.m_Identified != false);
            SetSaveFlag(ref flags, SaveFlag.StrReq, this.m_StrReq != -1);
            SetSaveFlag(ref flags, SaveFlag.DexReq, this.m_DexReq != -1);
            SetSaveFlag(ref flags, SaveFlag.IntReq, this.m_IntReq != -1);
            SetSaveFlag(ref flags, SaveFlag.MinDamage, this.m_MinDamage != -1);
            SetSaveFlag(ref flags, SaveFlag.MaxDamage, this.m_MaxDamage != -1);
            SetSaveFlag(ref flags, SaveFlag.HitSound, this.m_HitSound != -1);
            SetSaveFlag(ref flags, SaveFlag.MissSound, this.m_MissSound != -1);
            SetSaveFlag(ref flags, SaveFlag.Speed, this.m_Speed != -1);
            SetSaveFlag(ref flags, SaveFlag.MaxRange, this.m_MaxRange != -1);
            SetSaveFlag(ref flags, SaveFlag.Skill, this.m_Skill != (SkillName)(-1));
            SetSaveFlag(ref flags, SaveFlag.Type, this.m_Type != (WeaponType)(-1));
            SetSaveFlag(ref flags, SaveFlag.Animation, this.m_Animation != (WeaponAnimation)(-1));
            SetSaveFlag(ref flags, SaveFlag.Resource, this.m_Resource != CraftResource.Iron);
            SetSaveFlag(ref flags, SaveFlag.xAttributes, !this.m_AosAttributes.IsEmpty);
            SetSaveFlag(ref flags, SaveFlag.xWeaponAttributes, !this.m_AosWeaponAttributes.IsEmpty);
            SetSaveFlag(ref flags, SaveFlag.PlayerConstructed, this.m_PlayerConstructed);
            SetSaveFlag(ref flags, SaveFlag.SkillBonuses, !this.m_AosSkillBonuses.IsEmpty);
            SetSaveFlag(ref flags, SaveFlag.Slayer2, this.m_Slayer2 != SlayerName.None);
            SetSaveFlag(ref flags, SaveFlag.ElementalDamages, !this.m_AosElementDamages.IsEmpty);
            SetSaveFlag(ref flags, SaveFlag.EngravedText, !String.IsNullOrEmpty(this.m_EngravedText));

            writer.Write((int)flags);

            if (GetSaveFlag(flags, SaveFlag.DamageLevel))
                writer.Write((int)this.m_DamageLevel);

            if (GetSaveFlag(flags, SaveFlag.AccuracyLevel))
                writer.Write((int)this.m_AccuracyLevel);

            if (GetSaveFlag(flags, SaveFlag.DurabilityLevel))
                writer.Write((int)this.m_DurabilityLevel);

            if (GetSaveFlag(flags, SaveFlag.Quality))
                writer.Write((int)this.m_Quality);

            if (GetSaveFlag(flags, SaveFlag.Hits))
                writer.Write((int)this.m_Hits);

            if (GetSaveFlag(flags, SaveFlag.MaxHits))
                writer.Write((int)this.m_MaxHits);

            if (GetSaveFlag(flags, SaveFlag.Slayer))
                writer.Write((int)this.m_Slayer);

            if (GetSaveFlag(flags, SaveFlag.Poison))
                Poison.Serialize(this.m_Poison, writer);

            if (GetSaveFlag(flags, SaveFlag.PoisonCharges))
                writer.Write((int)this.m_PoisonCharges);

            if (GetSaveFlag(flags, SaveFlag.Crafter))
                writer.Write((Mobile)this.m_Crafter);

            if (GetSaveFlag(flags, SaveFlag.StrReq))
                writer.Write((int)this.m_StrReq);

            if (GetSaveFlag(flags, SaveFlag.DexReq))
                writer.Write((int)this.m_DexReq);

            if (GetSaveFlag(flags, SaveFlag.IntReq))
                writer.Write((int)this.m_IntReq);

            if (GetSaveFlag(flags, SaveFlag.MinDamage))
                writer.Write((int)this.m_MinDamage);

            if (GetSaveFlag(flags, SaveFlag.MaxDamage))
                writer.Write((int)this.m_MaxDamage);

            if (GetSaveFlag(flags, SaveFlag.HitSound))
                writer.Write((int)this.m_HitSound);

            if (GetSaveFlag(flags, SaveFlag.MissSound))
                writer.Write((int)this.m_MissSound);

            if (GetSaveFlag(flags, SaveFlag.Speed))
                writer.Write((float)this.m_Speed);

            if (GetSaveFlag(flags, SaveFlag.MaxRange))
                writer.Write((int)this.m_MaxRange);

            if (GetSaveFlag(flags, SaveFlag.Skill))
                writer.Write((int)this.m_Skill);

            if (GetSaveFlag(flags, SaveFlag.Type))
                writer.Write((int)this.m_Type);

            if (GetSaveFlag(flags, SaveFlag.Animation))
                writer.Write((int)this.m_Animation);

            if (GetSaveFlag(flags, SaveFlag.Resource))
                writer.Write((int)this.m_Resource);

            if (GetSaveFlag(flags, SaveFlag.xAttributes))
                this.m_AosAttributes.Serialize(writer);

            if (GetSaveFlag(flags, SaveFlag.xWeaponAttributes))
                this.m_AosWeaponAttributes.Serialize(writer);

            if (GetSaveFlag(flags, SaveFlag.SkillBonuses))
                this.m_AosSkillBonuses.Serialize(writer);

            if (GetSaveFlag(flags, SaveFlag.Slayer2))
                writer.Write((int)this.m_Slayer2);

            if (GetSaveFlag(flags, SaveFlag.ElementalDamages))
                this.m_AosElementDamages.Serialize(writer);

            if (GetSaveFlag(flags, SaveFlag.EngravedText))
                writer.Write((string)this.m_EngravedText);
        }

        [Flags]
        private enum SaveFlag
        {
            None = 0x00000000,
            DamageLevel = 0x00000001,
            AccuracyLevel = 0x00000002,
            DurabilityLevel = 0x00000004,
            Quality = 0x00000008,
            Hits = 0x00000010,
            MaxHits = 0x00000020,
            Slayer = 0x00000040,
            Poison = 0x00000080,
            PoisonCharges = 0x00000100,
            Crafter = 0x00000200,
            Identified = 0x00000400,
            StrReq = 0x00000800,
            DexReq = 0x00001000,
            IntReq = 0x00002000,
            MinDamage = 0x00004000,
            MaxDamage = 0x00008000,
            HitSound = 0x00010000,
            MissSound = 0x00020000,
            Speed = 0x00040000,
            MaxRange = 0x00080000,
            Skill = 0x00100000,
            Type = 0x00200000,
            Animation = 0x00400000,
            Resource = 0x00800000,
            xAttributes = 0x01000000,
            xWeaponAttributes = 0x02000000,
            PlayerConstructed = 0x04000000,
            SkillBonuses = 0x08000000,
            Slayer2 = 0x10000000,
            ElementalDamages = 0x20000000,
            EngravedText = 0x40000000
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            switch ( version )
            {
                case 9:
                case 8:
                case 7:
                case 6:
                case 5:
                    {
                        SaveFlag flags = (SaveFlag)reader.ReadInt();

                        if (GetSaveFlag(flags, SaveFlag.DamageLevel))
                        {
                            this.m_DamageLevel = (WeaponDamageLevel)reader.ReadInt();

                            if (this.m_DamageLevel > WeaponDamageLevel.Vanq)
                                this.m_DamageLevel = WeaponDamageLevel.Ruin;
                        }

                        if (GetSaveFlag(flags, SaveFlag.AccuracyLevel))
                        {
                            this.m_AccuracyLevel = (WeaponAccuracyLevel)reader.ReadInt();

                            if (this.m_AccuracyLevel > WeaponAccuracyLevel.Supremely)
                                this.m_AccuracyLevel = WeaponAccuracyLevel.Accurate;
                        }

                        if (GetSaveFlag(flags, SaveFlag.DurabilityLevel))
                        {
                            this.m_DurabilityLevel = (WeaponDurabilityLevel)reader.ReadInt();

                            if (this.m_DurabilityLevel > WeaponDurabilityLevel.Indestructible)
                                this.m_DurabilityLevel = WeaponDurabilityLevel.Durable;
                        }

                        if (GetSaveFlag(flags, SaveFlag.Quality))
                            this.m_Quality = (WeaponQuality)reader.ReadInt();
                        else
                            this.m_Quality = WeaponQuality.Regular;

                        if (GetSaveFlag(flags, SaveFlag.Hits))
                            this.m_Hits = reader.ReadInt();

                        if (GetSaveFlag(flags, SaveFlag.MaxHits))
                            this.m_MaxHits = reader.ReadInt();

                        if (GetSaveFlag(flags, SaveFlag.Slayer))
                            this.m_Slayer = (SlayerName)reader.ReadInt();

                        if (GetSaveFlag(flags, SaveFlag.Poison))
                            this.m_Poison = Poison.Deserialize(reader);

                        if (GetSaveFlag(flags, SaveFlag.PoisonCharges))
                            this.m_PoisonCharges = reader.ReadInt();

                        if (GetSaveFlag(flags, SaveFlag.Crafter))
                            this.m_Crafter = reader.ReadMobile();

                        if (GetSaveFlag(flags, SaveFlag.Identified))
                            this.m_Identified = (version >= 6 || reader.ReadBool());

                        if (GetSaveFlag(flags, SaveFlag.StrReq))
                            this.m_StrReq = reader.ReadInt();
                        else
                            this.m_StrReq = -1;

                        if (GetSaveFlag(flags, SaveFlag.DexReq))
                            this.m_DexReq = reader.ReadInt();
                        else
                            this.m_DexReq = -1;

                        if (GetSaveFlag(flags, SaveFlag.IntReq))
                            this.m_IntReq = reader.ReadInt();
                        else
                            this.m_IntReq = -1;

                        if (GetSaveFlag(flags, SaveFlag.MinDamage))
                            this.m_MinDamage = reader.ReadInt();
                        else
                            this.m_MinDamage = -1;

                        if (GetSaveFlag(flags, SaveFlag.MaxDamage))
                            this.m_MaxDamage = reader.ReadInt();
                        else
                            this.m_MaxDamage = -1;

                        if (GetSaveFlag(flags, SaveFlag.HitSound))
                            this.m_HitSound = reader.ReadInt();
                        else
                            this.m_HitSound = -1;

                        if (GetSaveFlag(flags, SaveFlag.MissSound))
                            this.m_MissSound = reader.ReadInt();
                        else
                            this.m_MissSound = -1;

                        if (GetSaveFlag(flags, SaveFlag.Speed))
                        {
                            if (version < 9)
                                this.m_Speed = reader.ReadInt();
                            else
                                this.m_Speed = reader.ReadFloat();
                        }
                        else
                            this.m_Speed = -1;

                        if (GetSaveFlag(flags, SaveFlag.MaxRange))
                            this.m_MaxRange = reader.ReadInt();
                        else
                            this.m_MaxRange = -1;

                        if (GetSaveFlag(flags, SaveFlag.Skill))
                            this.m_Skill = (SkillName)reader.ReadInt();
                        else
                            this.m_Skill = (SkillName)(-1);

                        if (GetSaveFlag(flags, SaveFlag.Type))
                            this.m_Type = (WeaponType)reader.ReadInt();
                        else
                            this.m_Type = (WeaponType)(-1);

                        if (GetSaveFlag(flags, SaveFlag.Animation))
                            this.m_Animation = (WeaponAnimation)reader.ReadInt();
                        else
                            this.m_Animation = (WeaponAnimation)(-1);

                        if (GetSaveFlag(flags, SaveFlag.Resource))
                            this.m_Resource = (CraftResource)reader.ReadInt();
                        else
                            this.m_Resource = CraftResource.Iron;

                        if (GetSaveFlag(flags, SaveFlag.xAttributes))
                            this.m_AosAttributes = new AosAttributes(this, reader);
                        else
                            this.m_AosAttributes = new AosAttributes(this);

                        if (GetSaveFlag(flags, SaveFlag.xWeaponAttributes))
                            this.m_AosWeaponAttributes = new AosWeaponAttributes(this, reader);
                        else
                            this.m_AosWeaponAttributes = new AosWeaponAttributes(this);

                        if (this.UseSkillMod && this.m_AccuracyLevel != WeaponAccuracyLevel.Regular && this.Parent is Mobile)
                        {
                            this.m_SkillMod = new DefaultSkillMod(this.AccuracySkill, true, (int)this.m_AccuracyLevel * 5);
                            ((Mobile)this.Parent).AddSkillMod(this.m_SkillMod);
                        }

                        if (version < 7 && this.m_AosWeaponAttributes.MageWeapon != 0)
                            this.m_AosWeaponAttributes.MageWeapon = 30 - this.m_AosWeaponAttributes.MageWeapon;

                        if (Core.AOS && this.m_AosWeaponAttributes.MageWeapon != 0 && this.m_AosWeaponAttributes.MageWeapon != 30 && this.Parent is Mobile)
                        {
                            this.m_MageMod = new DefaultSkillMod(SkillName.Magery, true, -30 + this.m_AosWeaponAttributes.MageWeapon);
                            ((Mobile)this.Parent).AddSkillMod(this.m_MageMod);
                        }

                        if (GetSaveFlag(flags, SaveFlag.PlayerConstructed))
                            this.m_PlayerConstructed = true;

                        if (GetSaveFlag(flags, SaveFlag.SkillBonuses))
                            this.m_AosSkillBonuses = new AosSkillBonuses(this, reader);
                        else
                            this.m_AosSkillBonuses = new AosSkillBonuses(this);

                        if (GetSaveFlag(flags, SaveFlag.Slayer2))
                            this.m_Slayer2 = (SlayerName)reader.ReadInt();

                        if (GetSaveFlag(flags, SaveFlag.ElementalDamages))
                            this.m_AosElementDamages = new AosElementAttributes(this, reader);
                        else
                            this.m_AosElementDamages = new AosElementAttributes(this);

                        if (GetSaveFlag(flags, SaveFlag.EngravedText))
                            this.m_EngravedText = reader.ReadString();

                        break;
                    }
                case 4:
                    {
                        this.m_Slayer = (SlayerName)reader.ReadInt();

                        goto case 3;
                    }
                case 3:
                    {
                        this.m_StrReq = reader.ReadInt();
                        this.m_DexReq = reader.ReadInt();
                        this.m_IntReq = reader.ReadInt();

                        goto case 2;
                    }
                case 2:
                    {
                        this.m_Identified = reader.ReadBool();

                        goto case 1;
                    }
                case 1:
                    {
                        this.m_MaxRange = reader.ReadInt();

                        goto case 0;
                    }
                case 0:
                    {
                        if (version == 0)
                            this.m_MaxRange = 1; // default

                        if (version < 5)
                        {
                            this.m_Resource = CraftResource.Iron;
                            this.m_AosAttributes = new AosAttributes(this);
                            this.m_AosWeaponAttributes = new AosWeaponAttributes(this);
                            this.m_AosElementDamages = new AosElementAttributes(this);
                            this.m_AosSkillBonuses = new AosSkillBonuses(this);
                        }

                        this.m_MinDamage = reader.ReadInt();
                        this.m_MaxDamage = reader.ReadInt();

                        this.m_Speed = reader.ReadInt();

                        this.m_HitSound = reader.ReadInt();
                        this.m_MissSound = reader.ReadInt();

                        this.m_Skill = (SkillName)reader.ReadInt();
                        this.m_Type = (WeaponType)reader.ReadInt();
                        this.m_Animation = (WeaponAnimation)reader.ReadInt();
                        this.m_DamageLevel = (WeaponDamageLevel)reader.ReadInt();
                        this.m_AccuracyLevel = (WeaponAccuracyLevel)reader.ReadInt();
                        this.m_DurabilityLevel = (WeaponDurabilityLevel)reader.ReadInt();
                        this.m_Quality = (WeaponQuality)reader.ReadInt();

                        this.m_Crafter = reader.ReadMobile();

                        this.m_Poison = Poison.Deserialize(reader);
                        this.m_PoisonCharges = reader.ReadInt();

                        if (this.m_StrReq == this.OldStrengthReq)
                            this.m_StrReq = -1;

                        if (this.m_DexReq == this.OldDexterityReq)
                            this.m_DexReq = -1;

                        if (this.m_IntReq == this.OldIntelligenceReq)
                            this.m_IntReq = -1;

                        if (this.m_MinDamage == this.OldMinDamage)
                            this.m_MinDamage = -1;

                        if (this.m_MaxDamage == this.OldMaxDamage)
                            this.m_MaxDamage = -1;

                        if (this.m_HitSound == this.OldHitSound)
                            this.m_HitSound = -1;

                        if (this.m_MissSound == this.OldMissSound)
                            this.m_MissSound = -1;

                        if (this.m_Speed == this.OldSpeed)
                            this.m_Speed = -1;

                        if (this.m_MaxRange == this.OldMaxRange)
                            this.m_MaxRange = -1;

                        if (this.m_Skill == this.OldSkill)
                            this.m_Skill = (SkillName)(-1);

                        if (this.m_Type == this.OldType)
                            this.m_Type = (WeaponType)(-1);

                        if (this.m_Animation == this.OldAnimation)
                            this.m_Animation = (WeaponAnimation)(-1);

                        if (this.UseSkillMod && this.m_AccuracyLevel != WeaponAccuracyLevel.Regular && this.Parent is Mobile)
                        {
                            this.m_SkillMod = new DefaultSkillMod(this.AccuracySkill, true, (int)this.m_AccuracyLevel * 5);
                            ((Mobile)this.Parent).AddSkillMod(this.m_SkillMod);
                        }

                        break;
                    }
            }

            if (Core.AOS && this.Parent is Mobile)
                this.m_AosSkillBonuses.AddTo((Mobile)this.Parent);

            int strBonus = this.m_AosAttributes.BonusStr;
            int dexBonus = this.m_AosAttributes.BonusDex;
            int intBonus = this.m_AosAttributes.BonusInt;

            if (this.Parent is Mobile && (strBonus != 0 || dexBonus != 0 || intBonus != 0))
            {
                Mobile m = (Mobile)this.Parent;

                string modName = this.Serial.ToString();

                if (strBonus != 0)
                    m.AddStatMod(new StatMod(StatType.Str, modName + "Str", strBonus, TimeSpan.Zero));

                if (dexBonus != 0)
                    m.AddStatMod(new StatMod(StatType.Dex, modName + "Dex", dexBonus, TimeSpan.Zero));

                if (intBonus != 0)
                    m.AddStatMod(new StatMod(StatType.Int, modName + "Int", intBonus, TimeSpan.Zero));
            }

            if (this.Parent is Mobile)
                ((Mobile)this.Parent).CheckStatTimers();

            if (this.m_Hits <= 0 && this.m_MaxHits <= 0)
            {
                this.m_Hits = this.m_MaxHits = Utility.RandomMinMax(this.InitMinHits, this.InitMaxHits);
            }

            if (version < 6)
                this.m_PlayerConstructed = true; // we don't know, so, assume it's crafted
        }

        #endregion

        public BaseWeapon(int itemID) : base(itemID)
        {
            this.Layer = (Layer)this.ItemData.Quality;

            this.m_Quality = WeaponQuality.Regular;
            this.m_StrReq = -1;
            this.m_DexReq = -1;
            this.m_IntReq = -1;
            this.m_MinDamage = -1;
            this.m_MaxDamage = -1;
            this.m_HitSound = -1;
            this.m_MissSound = -1;
            this.m_Speed = -1;
            this.m_MaxRange = -1;
            this.m_Skill = (SkillName)(-1);
            this.m_Type = (WeaponType)(-1);
            this.m_Animation = (WeaponAnimation)(-1);

            this.m_Hits = this.m_MaxHits = Utility.RandomMinMax(this.InitMinHits, this.InitMaxHits);

            this.m_Resource = CraftResource.Iron;

            this.m_AosAttributes = new AosAttributes(this);
            this.m_AosWeaponAttributes = new AosWeaponAttributes(this);
            this.m_AosSkillBonuses = new AosSkillBonuses(this);
            this.m_AosElementDamages = new AosElementAttributes(this);
        }

        public BaseWeapon(Serial serial) : base(serial)
        {
        }

        private string GetNameString()
        {
            string name = this.Name;

            if (name == null)
                name = String.Format("#{0}", this.LabelNumber);

            return name;
        }

        [Hue, CommandProperty(AccessLevel.GameMaster)]
        public override int Hue
        {
            get
            {
                return base.Hue;
            }
            set
            {
                base.Hue = value;
                this.InvalidateProperties();
            }
        }

        public int GetElementalDamageHue()
        {
            int phys, fire, cold, pois, nrgy, chaos, direct;
            this.GetDamageTypes(null, out phys, out fire, out cold, out pois, out nrgy, out chaos, out direct);
            //Order is Cold, Energy, Fire, Poison, Physical left

            int currentMax = 50;
            int hue = 0;

            if (pois >= currentMax)
            {
                hue = 1267 + (pois - 50) / 10;
                currentMax = pois;
            }

            if (fire >= currentMax)
            {
                hue = 1255 + (fire - 50) / 10;
                currentMax = fire;
            }

            if (nrgy >= currentMax)
            {
                hue = 1273 + (nrgy - 50) / 10;
                currentMax = nrgy;
            }

            if (cold >= currentMax)
            {
                hue = 1261 + (cold - 50) / 10;
                currentMax = cold;
            }

            return hue;
        }

        public override void AddNameProperty(ObjectPropertyList list)
        {
            int oreType;

            switch ( this.m_Resource )
            {
                case CraftResource.DullCopper:
                    oreType = 1053108;
                    break; // dull copper
                case CraftResource.ShadowIron:
                    oreType = 1053107;
                    break; // shadow iron
                case CraftResource.Copper:
                    oreType = 1053106;
                    break; // copper
                case CraftResource.Bronze:
                    oreType = 1053105;
                    break; // bronze
                case CraftResource.Gold:
                    oreType = 1053104;
                    break; // golden
                case CraftResource.Agapite:
                    oreType = 1053103;
                    break; // agapite
                case CraftResource.Verite:
                    oreType = 1053102;
                    break; // verite
                case CraftResource.Valorite:
                    oreType = 1053101;
                    break; // valorite
                case CraftResource.SpinedLeather:
                    oreType = 1061118;
                    break; // spined
                case CraftResource.HornedLeather:
                    oreType = 1061117;
                    break; // horned
                case CraftResource.BarbedLeather:
                    oreType = 1061116;
                    break; // barbed
                case CraftResource.RedScales:
                    oreType = 1060814;
                    break; // red
                case CraftResource.YellowScales:
                    oreType = 1060818;
                    break; // yellow
                case CraftResource.BlackScales:
                    oreType = 1060820;
                    break; // black
                case CraftResource.GreenScales:
                    oreType = 1060819;
                    break; // green
                case CraftResource.WhiteScales:
                    oreType = 1060821;
                    break; // white
                case CraftResource.BlueScales:
                    oreType = 1060815;
                    break; // blue
                default:
                    oreType = 0;
                    break;
            }

            if (oreType != 0)
                list.Add(1053099, "#{0}\t{1}", oreType, this.GetNameString()); // ~1_oretype~ ~2_armortype~
            else if (this.Name == null)
                list.Add(this.LabelNumber);
            else
                list.Add(this.Name);
				
            /*
            * Want to move this to the engraving tool, let the non-harmful 
            * formatting show, and remove CLILOCs embedded: more like OSI
            * did with the books that had markup, etc.
            * 
            * This will have a negative effect on a few event things imgame 
            * as is.
            * 
            * If we cant find a more OSI-ish way to clean it up, we can 
            * easily put this back, and use it in the deserialize
            * method and engraving tool, to make it perm cleaned up.
            */

            if (!String.IsNullOrEmpty(this.m_EngravedText))
                list.Add(1062613, this.m_EngravedText);
            /* list.Add( 1062613, Utility.FixHtml( m_EngravedText ) ); */
        }

        public override bool AllowEquipedCast(Mobile from)
        {
            if (base.AllowEquipedCast(from))
                return true;

            return (this.m_AosAttributes.SpellChanneling != 0);
        }

        public virtual int ArtifactRarity
        {
            get
            {
                return 0;
            }
        }

        public virtual int GetLuckBonus()
        {
            CraftResourceInfo resInfo = CraftResources.GetInfo(this.m_Resource);

            if (resInfo == null)
                return 0;

            CraftAttributeInfo attrInfo = resInfo.AttributeInfo;

            if (attrInfo == null)
                return 0;

            return attrInfo.WeaponLuck;
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            if (this.m_Crafter != null)
                list.Add(1050043, this.m_Crafter.Name); // crafted by ~1_NAME~

            #region Factions
            if (this.m_FactionState != null)
                list.Add(1041350); // faction item
            #endregion

            if (this.m_AosSkillBonuses != null)
                this.m_AosSkillBonuses.GetProperties(list);

            if (this.m_Quality == WeaponQuality.Exceptional)
                list.Add(1060636); // exceptional

            if (this.RequiredRace == Race.Elf)
                list.Add(1075086); // Elves Only

            if (this.ArtifactRarity > 0)
                list.Add(1061078, this.ArtifactRarity.ToString()); // artifact rarity ~1_val~

            if (this is IUsesRemaining && ((IUsesRemaining)this).ShowUsesRemaining)
                list.Add(1060584, ((IUsesRemaining)this).UsesRemaining.ToString()); // uses remaining: ~1_val~

            if (this.m_Poison != null && this.m_PoisonCharges > 0)
                list.Add(1062412 + this.m_Poison.Level, this.m_PoisonCharges.ToString());

            if (this.m_Slayer != SlayerName.None)
            {
                SlayerEntry entry = SlayerGroup.GetEntryByName(this.m_Slayer);
                if (entry != null)
                    list.Add(entry.Title);
            }

            if (this.m_Slayer2 != SlayerName.None)
            {
                SlayerEntry entry = SlayerGroup.GetEntryByName(this.m_Slayer2);
                if (entry != null)
                    list.Add(entry.Title);
            }

            base.AddResistanceProperties(list);

            int prop;

            if (Core.ML && this is BaseRanged && ((BaseRanged)this).Balanced)
                list.Add(1072792); // Balanced

            if ((prop = this.m_AosWeaponAttributes.UseBestSkill) != 0)
                list.Add(1060400); // use best weapon skill

            if ((prop = (this.GetDamageBonus() + this.m_AosAttributes.WeaponDamage)) != 0)
                list.Add(1060401, prop.ToString()); // damage increase ~1_val~%

            if ((prop = this.m_AosAttributes.DefendChance) != 0)
                list.Add(1060408, prop.ToString()); // defense chance increase ~1_val~%

            if ((prop = this.m_AosAttributes.EnhancePotions) != 0)
                list.Add(1060411, prop.ToString()); // enhance potions ~1_val~%

            if ((prop = this.m_AosAttributes.CastRecovery) != 0)
                list.Add(1060412, prop.ToString()); // faster cast recovery ~1_val~

            if ((prop = this.m_AosAttributes.CastSpeed) != 0)
                list.Add(1060413, prop.ToString()); // faster casting ~1_val~

            if ((prop = (this.GetHitChanceBonus() + this.m_AosAttributes.AttackChance)) != 0)
                list.Add(1060415, prop.ToString()); // hit chance increase ~1_val~%

            if ((prop = this.m_AosWeaponAttributes.HitColdArea) != 0)
                list.Add(1060416, prop.ToString()); // hit cold area ~1_val~%

            if ((prop = this.m_AosWeaponAttributes.HitDispel) != 0)
                list.Add(1060417, prop.ToString()); // hit dispel ~1_val~%

            if ((prop = this.m_AosWeaponAttributes.HitEnergyArea) != 0)
                list.Add(1060418, prop.ToString()); // hit energy area ~1_val~%

            if ((prop = this.m_AosWeaponAttributes.HitFireArea) != 0)
                list.Add(1060419, prop.ToString()); // hit fire area ~1_val~%

            if ((prop = this.m_AosWeaponAttributes.HitFireball) != 0)
                list.Add(1060420, prop.ToString()); // hit fireball ~1_val~%

            if ((prop = this.m_AosWeaponAttributes.HitHarm) != 0)
                list.Add(1060421, prop.ToString()); // hit harm ~1_val~%

            if ((prop = this.m_AosWeaponAttributes.HitLeechHits) != 0)
                list.Add(1060422, prop.ToString()); // hit life leech ~1_val~%

            if ((prop = this.m_AosWeaponAttributes.HitLightning) != 0)
                list.Add(1060423, prop.ToString()); // hit lightning ~1_val~%

            if ((prop = this.m_AosWeaponAttributes.HitLowerAttack) != 0)
                list.Add(1060424, prop.ToString()); // hit lower attack ~1_val~%

            if ((prop = this.m_AosWeaponAttributes.HitLowerDefend) != 0)
                list.Add(1060425, prop.ToString()); // hit lower defense ~1_val~%

            if ((prop = this.m_AosWeaponAttributes.HitMagicArrow) != 0)
                list.Add(1060426, prop.ToString()); // hit magic arrow ~1_val~%

            if ((prop = this.m_AosWeaponAttributes.HitLeechMana) != 0)
                list.Add(1060427, prop.ToString()); // hit mana leech ~1_val~%

            if ((prop = this.m_AosWeaponAttributes.HitPhysicalArea) != 0)
                list.Add(1060428, prop.ToString()); // hit physical area ~1_val~%

            if ((prop = this.m_AosWeaponAttributes.HitPoisonArea) != 0)
                list.Add(1060429, prop.ToString()); // hit poison area ~1_val~%

            if ((prop = this.m_AosWeaponAttributes.HitLeechStam) != 0)
                list.Add(1060430, prop.ToString()); // hit stamina leech ~1_val~%

            if (ImmolatingWeaponSpell.IsImmolating(this))
                list.Add(1111917); // Immolated

            if (Core.ML && this is BaseRanged && (prop = ((BaseRanged)this).Velocity) != 0)
                list.Add(1072793, prop.ToString()); // Velocity ~1_val~%

            if ((prop = this.m_AosAttributes.BonusDex) != 0)
                list.Add(1060409, prop.ToString()); // dexterity bonus ~1_val~

            if ((prop = this.m_AosAttributes.BonusHits) != 0)
                list.Add(1060431, prop.ToString()); // hit point increase ~1_val~

            if ((prop = this.m_AosAttributes.BonusInt) != 0)
                list.Add(1060432, prop.ToString()); // intelligence bonus ~1_val~

            if ((prop = this.m_AosAttributes.LowerManaCost) != 0)
                list.Add(1060433, prop.ToString()); // lower mana cost ~1_val~%

            if ((prop = this.m_AosAttributes.LowerRegCost) != 0)
                list.Add(1060434, prop.ToString()); // lower reagent cost ~1_val~%

            if ((prop = this.GetLowerStatReq()) != 0)
                list.Add(1060435, prop.ToString()); // lower requirements ~1_val~%

            if ((prop = (this.GetLuckBonus() + this.m_AosAttributes.Luck)) != 0)
                list.Add(1060436, prop.ToString()); // luck ~1_val~

            if ((prop = this.m_AosWeaponAttributes.MageWeapon) != 0)
                list.Add(1060438, (30 - prop).ToString()); // mage weapon -~1_val~ skill

            if ((prop = this.m_AosAttributes.BonusMana) != 0)
                list.Add(1060439, prop.ToString()); // mana increase ~1_val~

            if ((prop = this.m_AosAttributes.RegenMana) != 0)
                list.Add(1060440, prop.ToString()); // mana regeneration ~1_val~

            if ((prop = this.m_AosAttributes.NightSight) != 0)
                list.Add(1060441); // night sight

            if ((prop = this.m_AosAttributes.ReflectPhysical) != 0)
                list.Add(1060442, prop.ToString()); // reflect physical damage ~1_val~%

            if ((prop = this.m_AosAttributes.RegenStam) != 0)
                list.Add(1060443, prop.ToString()); // stamina regeneration ~1_val~

            if ((prop = this.m_AosAttributes.RegenHits) != 0)
                list.Add(1060444, prop.ToString()); // hit point regeneration ~1_val~

            if ((prop = this.m_AosWeaponAttributes.SelfRepair) != 0)
                list.Add(1060450, prop.ToString()); // self repair ~1_val~

            if ((prop = this.m_AosAttributes.SpellChanneling) != 0)
                list.Add(1060482); // spell channeling

            if ((prop = this.m_AosAttributes.SpellDamage) != 0)
                list.Add(1060483, prop.ToString()); // spell damage increase ~1_val~%

            if ((prop = this.m_AosAttributes.BonusStam) != 0)
                list.Add(1060484, prop.ToString()); // stamina increase ~1_val~

            if ((prop = this.m_AosAttributes.BonusStr) != 0)
                list.Add(1060485, prop.ToString()); // strength bonus ~1_val~

            if ((prop = this.m_AosAttributes.WeaponSpeed) != 0)
                list.Add(1060486, prop.ToString()); // swing speed increase ~1_val~%

            if (Core.ML && (prop = this.m_AosAttributes.IncreasedKarmaLoss) != 0)
                list.Add(1075210, prop.ToString()); // Increased Karma Loss ~1val~%

            int phys, fire, cold, pois, nrgy, chaos, direct;

            this.GetDamageTypes(null, out phys, out fire, out cold, out pois, out nrgy, out chaos, out direct);

            if (phys != 0)
                list.Add(1060403, phys.ToString()); // physical damage ~1_val~%

            if (fire != 0)
                list.Add(1060405, fire.ToString()); // fire damage ~1_val~%

            if (cold != 0)
                list.Add(1060404, cold.ToString()); // cold damage ~1_val~%

            if (pois != 0)
                list.Add(1060406, pois.ToString()); // poison damage ~1_val~%

            if (nrgy != 0)
                list.Add(1060407, nrgy.ToString()); // energy damage ~1_val

            if (Core.ML && chaos != 0)
                list.Add(1072846, chaos.ToString()); // chaos damage ~1_val~%

            if (Core.ML && direct != 0)
                list.Add(1079978, direct.ToString()); // Direct Damage: ~1_PERCENT~%

            list.Add(1061168, "{0}\t{1}", this.MinDamage.ToString(), this.MaxDamage.ToString()); // weapon damage ~1_val~ - ~2_val~

            if (Core.ML)
                list.Add(1061167, String.Format("{0}s", this.Speed)); // weapon speed ~1_val~
            else
                list.Add(1061167, this.Speed.ToString());

            if (this.MaxRange > 1)
                list.Add(1061169, this.MaxRange.ToString()); // range ~1_val~

            int strReq = AOS.Scale(this.StrRequirement, 100 - this.GetLowerStatReq());

            if (strReq > 0)
                list.Add(1061170, strReq.ToString()); // strength requirement ~1_val~

            if (this.Layer == Layer.TwoHanded)
                list.Add(1061171); // two-handed weapon
            else
                list.Add(1061824); // one-handed weapon

            if (Core.SE || this.m_AosWeaponAttributes.UseBestSkill == 0)
            {
                switch ( this.Skill )
                {
                    case SkillName.Swords:
                        list.Add(1061172);
                        break; // skill required: swordsmanship
                    case SkillName.Macing:
                        list.Add(1061173);
                        break; // skill required: mace fighting
                    case SkillName.Fencing:
                        list.Add(1061174);
                        break; // skill required: fencing
                    case SkillName.Archery:
                        list.Add(1061175);
                        break; // skill required: archery
                }
            }

            if (this.m_Hits >= 0 && this.m_MaxHits > 0)
                list.Add(1060639, "{0}\t{1}", this.m_Hits, this.m_MaxHits); // durability ~1_val~ / ~2_val~
        }

        public override void OnSingleClick(Mobile from)
        {
            List<EquipInfoAttribute> attrs = new List<EquipInfoAttribute>();

            if (this.DisplayLootType)
            {
                if (this.LootType == LootType.Blessed)
                    attrs.Add(new EquipInfoAttribute(1038021)); // blessed
                else if (this.LootType == LootType.Cursed)
                    attrs.Add(new EquipInfoAttribute(1049643)); // cursed
            }

            #region Factions
            if (this.m_FactionState != null)
                attrs.Add(new EquipInfoAttribute(1041350)); // faction item
            #endregion

            if (this.m_Quality == WeaponQuality.Exceptional)
                attrs.Add(new EquipInfoAttribute(1018305 - (int)this.m_Quality));

            if (this.m_Identified || from.AccessLevel >= AccessLevel.GameMaster)
            {
                if (this.m_Slayer != SlayerName.None)
                {
                    SlayerEntry entry = SlayerGroup.GetEntryByName(this.m_Slayer);
                    if (entry != null)
                        attrs.Add(new EquipInfoAttribute(entry.Title));
                }

                if (this.m_Slayer2 != SlayerName.None)
                {
                    SlayerEntry entry = SlayerGroup.GetEntryByName(this.m_Slayer2);
                    if (entry != null)
                        attrs.Add(new EquipInfoAttribute(entry.Title));
                }

                if (this.m_DurabilityLevel != WeaponDurabilityLevel.Regular)
                    attrs.Add(new EquipInfoAttribute(1038000 + (int)this.m_DurabilityLevel));

                if (this.m_DamageLevel != WeaponDamageLevel.Regular)
                    attrs.Add(new EquipInfoAttribute(1038015 + (int)this.m_DamageLevel));

                if (this.m_AccuracyLevel != WeaponAccuracyLevel.Regular)
                    attrs.Add(new EquipInfoAttribute(1038010 + (int)this.m_AccuracyLevel));
            }
            else if (this.m_Slayer != SlayerName.None || this.m_Slayer2 != SlayerName.None || this.m_DurabilityLevel != WeaponDurabilityLevel.Regular || this.m_DamageLevel != WeaponDamageLevel.Regular || this.m_AccuracyLevel != WeaponAccuracyLevel.Regular)
                attrs.Add(new EquipInfoAttribute(1038000)); // Unidentified

            if (this.m_Poison != null && this.m_PoisonCharges > 0)
                attrs.Add(new EquipInfoAttribute(1017383, this.m_PoisonCharges));

            int number;

            if (this.Name == null)
            {
                number = this.LabelNumber;
            }
            else
            {
                this.LabelTo(from, this.Name);
                number = 1041000;
            }

            if (attrs.Count == 0 && this.Crafter == null && this.Name != null)
                return;

            EquipmentInfo eqInfo = new EquipmentInfo(number, this.m_Crafter, false, attrs.ToArray());

            from.Send(new DisplayEquipmentInfo(this, eqInfo));
        }

        private static BaseWeapon m_Fists; // This value holds the default--fist--weapon

        public static BaseWeapon Fists
        {
            get
            {
                return m_Fists;
            }
            set
            {
                m_Fists = value;
            }
        }

        #region ICraftable Members

        public int OnCraft(int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, BaseTool tool, CraftItem craftItem, int resHue)
        {
            this.Quality = (WeaponQuality)quality;

            if (makersMark)
                this.Crafter = from;

            this.PlayerConstructed = true;

            Type resourceType = typeRes;

            if (resourceType == null)
                resourceType = craftItem.Resources.GetAt(0).ItemType;

            if (Core.AOS)
            {
                this.Resource = CraftResources.GetFromType(resourceType);

                CraftContext context = craftSystem.GetContext(from);

                if (context != null && context.DoNotColor)
                    this.Hue = 0;

                if (tool is BaseRunicTool)
                    ((BaseRunicTool)tool).ApplyAttributesTo(this);

                if (this.Quality == WeaponQuality.Exceptional)
                {
                    if (this.Attributes.WeaponDamage > 35)
                        this.Attributes.WeaponDamage -= 20;
                    else
                        this.Attributes.WeaponDamage = 15;

                    if (Core.ML)
                    {
                        this.Attributes.WeaponDamage += (int)(from.Skills.ArmsLore.Value / 20);

                        if (this.Attributes.WeaponDamage > 50)
                            this.Attributes.WeaponDamage = 50;

                        from.CheckSkill(SkillName.ArmsLore, 0, 100);
                    }
                }
            }
            else if (tool is BaseRunicTool)
            {
                CraftResource thisResource = CraftResources.GetFromType(resourceType);

                if (thisResource == ((BaseRunicTool)tool).Resource)
                {
                    this.Resource = thisResource;

                    CraftContext context = craftSystem.GetContext(from);

                    if (context != null && context.DoNotColor)
                        this.Hue = 0;

                    switch ( thisResource )
                    {
                        case CraftResource.DullCopper:
                            {
                                this.Identified = true;
                                this.DurabilityLevel = WeaponDurabilityLevel.Durable;
                                this.AccuracyLevel = WeaponAccuracyLevel.Accurate;
                                break;
                            }
                        case CraftResource.ShadowIron:
                            {
                                this.Identified = true;
                                this.DurabilityLevel = WeaponDurabilityLevel.Durable;
                                this.DamageLevel = WeaponDamageLevel.Ruin;
                                break;
                            }
                        case CraftResource.Copper:
                            {
                                this.Identified = true;
                                this.DurabilityLevel = WeaponDurabilityLevel.Fortified;
                                this.DamageLevel = WeaponDamageLevel.Ruin;
                                this.AccuracyLevel = WeaponAccuracyLevel.Surpassingly;
                                break;
                            }
                        case CraftResource.Bronze:
                            {
                                this.Identified = true;
                                this.DurabilityLevel = WeaponDurabilityLevel.Fortified;
                                this.DamageLevel = WeaponDamageLevel.Might;
                                this.AccuracyLevel = WeaponAccuracyLevel.Surpassingly;
                                break;
                            }
                        case CraftResource.Gold:
                            {
                                this.Identified = true;
                                this.DurabilityLevel = WeaponDurabilityLevel.Indestructible;
                                this.DamageLevel = WeaponDamageLevel.Force;
                                this.AccuracyLevel = WeaponAccuracyLevel.Eminently;
                                break;
                            }
                        case CraftResource.Agapite:
                            {
                                this.Identified = true;
                                this.DurabilityLevel = WeaponDurabilityLevel.Indestructible;
                                this.DamageLevel = WeaponDamageLevel.Power;
                                this.AccuracyLevel = WeaponAccuracyLevel.Eminently;
                                break;
                            }
                        case CraftResource.Verite:
                            {
                                this.Identified = true;
                                this.DurabilityLevel = WeaponDurabilityLevel.Indestructible;
                                this.DamageLevel = WeaponDamageLevel.Power;
                                this.AccuracyLevel = WeaponAccuracyLevel.Exceedingly;
                                break;
                            }
                        case CraftResource.Valorite:
                            {
                                this.Identified = true;
                                this.DurabilityLevel = WeaponDurabilityLevel.Indestructible;
                                this.DamageLevel = WeaponDamageLevel.Vanq;
                                this.AccuracyLevel = WeaponAccuracyLevel.Supremely;
                                break;
                            }
                    }
                }
            }

            return quality;
        }
        #endregion
    }

    public enum CheckSlayerResult
    {
        None,
        Slayer,
        Opposition
    }
}