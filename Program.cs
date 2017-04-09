using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;

namespace The_Horizon_Leona
{
    class Program
    {
        private static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        private static Menu StartMenu, ComboMenu, DrawingsMenu, ActivatorMenu;

        public static Spell.Active _Q;
        public static Spell.Active _W;
        public static Spell.Skillshot _E;
        public static Spell.Skillshot _R;
        public static Spell.Targeted _Ignite;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;

        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (!_Player.ChampionName.Contains("Leona"))
            {
                return;
            }
            Chat.Print("The Horizon Leona - Loaded,");
            Chat.Print("For best experience left-click target.");

            _Q = new Spell.Active(SpellSlot.Q, 175);
            _W = new Spell.Active(SpellSlot.W, 275);
            _E = new Spell.Skillshot(SpellSlot.E, 750, SkillShotType.Linear, 250, 2000, 70);
            _E.AllowedCollisionCount = int.MaxValue;
            _R = new Spell.Skillshot(SpellSlot.R, 1200, SkillShotType.Circular, castDelay: 250, spellWidth: 200);
            _R.AllowedCollisionCount = int.MaxValue;
            _Ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);

            StartMenu = MainMenu.AddMenu("The Horizon Leona", "The Horizon Leona");
            ComboMenu = StartMenu.AddSubMenu("Combo", "Combo");
            DrawingsMenu = StartMenu.AddSubMenu("Drawings", "Drawings");
            ActivatorMenu = StartMenu.AddSubMenu("Activator", "Activator");


            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.AddLabel("Tick for enable/disable spells in Combo");
            ComboMenu.Add("UseQ", new CheckBox("Use [Q]"));
            ComboMenu.Add("UseW", new CheckBox("Use [W]"));
            ComboMenu.Add("UseE", new CheckBox("Use [E]"));
            ComboMenu.Add("UseR", new CheckBox("Use [R]"));

            DrawingsMenu.AddGroupLabel("Drawing Settings");
            DrawingsMenu.AddLabel("Tick for enable/disable Draw Spell Range");
            DrawingsMenu.Add("DQ", new CheckBox("- Draw [Q] range"));
            DrawingsMenu.AddSeparator(0);
            DrawingsMenu.Add("DW", new CheckBox("- Draw [W] range"));
            DrawingsMenu.AddSeparator(0);
            DrawingsMenu.Add("DE", new CheckBox("- Draw [E] range"));
            DrawingsMenu.AddSeparator(0);
            DrawingsMenu.Add("DR", new CheckBox("- Draw [R] range"));


            ActivatorMenu.AddGroupLabel("Activator Settings");
            ActivatorMenu.AddLabel("Use Summoner Spell");
            ActivatorMenu.Add("IGNI", new CheckBox("- Use Ignite if enemy is killable"));
            ActivatorMenu.AddSeparator(0);
            ActivatorMenu.AddSeparator(1);



            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;

        }

        private static void Game_OnTick(EventArgs args)
        {
            switch (Orbwalker.ActiveModesFlags)
            {
                case Orbwalker.ActiveModes.Combo:
                    Combo();
                    break;
                case Orbwalker.ActiveModes.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();

            }
            Activator();
        }


        private static void Drawing_OnDraw(EventArgs args)
        {
            var target = TargetSelector.GetTarget(_Q.Range, DamageType.Physical);
            if (DrawingsMenu["DQ"].Cast<CheckBox>().CurrentValue && _Q.IsLearned)
            {
                Circle.Draw(_Q.IsReady() ? Color.White : Color.Red, _Q.Range, _Player);
            }
            if (DrawingsMenu["DW"].Cast<CheckBox>().CurrentValue && _W.IsLearned)
            {
                Circle.Draw(_W.IsReady() ? Color.White : Color.Red, _W.Range, _Player);
            }
            if (DrawingsMenu["DE"].Cast<CheckBox>().CurrentValue && _E.IsLearned)
            {
                Circle.Draw(_E.IsReady() ? Color.White : Color.Red, _E.Range, _Player);
            }
            if (DrawingsMenu["DR"].Cast<CheckBox>().CurrentValue && _R.IsLearned)
            {
                Circle.Draw(_R.IsReady() ? Color.White : Color.Red, _R.Range, _Player);
            }

        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(_E.Range, DamageType.Magical);

            if (target == null)
               return;
            if (ComboMenu["UseQ"].Cast<CheckBox>().CurrentValue)
            {
                if (target.IsInRange(_Player, _Q.Range) && _Q.IsReady() && _Player.Distance(target) > 125)

                {
                    _Q.Cast();
                }



            }
            if (ComboMenu["UseW"].Cast<CheckBox>().CurrentValue)
            {
                if (target.IsInRange(_Player, _W.Range) && _W.IsReady())
                {

                    _W.Cast();



                }
            }
            if (ComboMenu["UseE"].Cast<CheckBox>().CurrentValue)
            {
                if (target.IsInRange(_Player, _E.Range) && _E.IsReady())
                    if (_E.GetPrediction(target).HitChance >= HitChance.Impossible)
                    {

                        _E.Cast(target);


                    }
            }
            if (ComboMenu["UseR"].Cast<CheckBox>().CurrentValue)
            {
                if (_E.GetPrediction(target).HitChance >= HitChance.High)
                    if (target.IsInRange(_Player, _R.Range))
                    {

                        _R.Cast(target);



                    }
            }





        }

        public static void Activator()
        {
            var target = TargetSelector.GetTarget(_Ignite.Range, DamageType.True);
            if (_Ignite != null && ActivatorMenu["IGNI"].Cast<CheckBox>().CurrentValue && _Ignite.IsReady())
            {
                if (target.Health + target.AttackShield <
                    _Player.GetSummonerSpellDamage(target, DamageLibrary.SummonerSpells.Ignite))
                {
                    _Ignite.Cast(target);
                }
            }
        }


    }
}
