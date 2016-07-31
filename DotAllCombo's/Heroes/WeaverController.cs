﻿using System.Threading.Tasks;
using SharpDX;

namespace DotaAllCombo.Heroes
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Ensage;
	using Ensage.Common;
	using Ensage.Common.Extensions;
	using Ensage.Common.Menu;

	using Service;
	using Service.Debug;

	internal class WeaverController : Variables, IHeroController
	{
		private Ability Q, W, R;

		private Item urn, orchid, ethereal, dagon, halberd, blink, mjollnir, abyssal, mom, Shiva, mail, bkb, satanic, medall;
		
		private readonly Menu ult = new Menu("AutoAbility", "Ultimate on an ally.");

		public void OnLoadEvent()
		{
			AssemblyExtensions.InitAssembly("VickTheRock", "0.1b");

			Print.LogMessage.Success("The threads of fate are mine to weave.");

			Menu.AddItem(new MenuItem("enabled", "Enabled").SetValue(true));
			Menu.AddItem(new MenuItem("orbwalk", "orbwalk").SetValue(true));
			Menu.AddItem(new MenuItem("keyBind", "Combo key").SetValue(new KeyBind('D', KeyBindType.Press)));

			Menu.AddItem(
				new MenuItem("Skills", "Skills").SetValue(new AbilityToggler(new Dictionary<string, bool>
				{
					{"weaver_shukuchi", true},
				    {"weaver_the_swarm", true},
				    {"weaver_time_lapse", true}
				})));
			Menu.AddItem(
				new MenuItem("Items", "Items:").SetValue(new AbilityToggler(new Dictionary<string, bool>
				{
					{"item_mask_of_madness", true},
					{"item_heavens_halberd", true},
					{"item_orchid", true},
					{"item_bloodthorn", true},
					{"item_blink", true},
					{"item_mjollnir", true},
					{"item_urn_of_shadows", true},
					{"item_ethereal_blade", true},
					{"item_abyssal_blade", true},
					{"item_shivas_guard", true},
					{"item_blade_mail", true},
					{"item_black_king_bar", true},
					{"item_satanic", true},
					{"item_medallion_of_courage", true},
					{"item_solar_crest", true}
				})));
			Menu.AddItem(new MenuItem("Heelm", "Min targets to BladeMail").SetValue(new Slider(2, 1, 5)));
			Menu.AddItem(new MenuItem("Heel", "Min targets to BKB").SetValue(new Slider(2, 1, 5)));
			Menu.AddItem(new MenuItem("MomentDownHealth", "Min Health Down To Ult").SetValue(new Slider(450, 200, 2000)));

			ult.AddItem(new MenuItem("ult", "Use ult in Ally").SetValue(true));
			ult.AddItem(new MenuItem("MomentAllyDownHealth", "Min Ally Health Down To Ult").SetValue(new Slider(750, 300, 2000)))
				.SetTooltip("You need have AghanimS!");
			Menu.AddSubMenu(ult);

		}

		public void Combo()
		{
			if(!Menu.Item("enabled").GetValue<bool>())return;

			Active = Game.IsKeyDown(Menu.Item("keyBind").GetValue<KeyBind>().Key);

			Q = me.Spellbook.SpellQ;
			R = me.Spellbook.SpellR;
			W = me.Spellbook.SpellW;

			mom = me.FindItem("item_mask_of_madness");
			urn = me.FindItem("item_urn_of_shadows");
			dagon = me.Inventory.Items.FirstOrDefault(x => x.Name.Contains("item_dagon"));
			ethereal = me.FindItem("item_ethereal_blade");
			halberd = me.FindItem("item_heavens_halberd");
			mjollnir = me.FindItem("item_mjollnir");
			blink = me.FindItem("item_blink");
			orchid = me.FindItem("item_orchid") ?? me.FindItem("item_bloodthorn");
			abyssal = me.FindItem("item_abyssal_blade");
			mail = me.FindItem("item_blade_mail");
			bkb = me.FindItem("item_black_king_bar");
			satanic = me.FindItem("item_satanic");
			medall = me.FindItem("item_medallion_of_courage") ?? me.FindItem("item_solar_crest");
			Shiva = me.FindItem("item_shivas_guard");
			var v =
				ObjectManager.GetEntities<Hero>()
					.Where(x => x.Team != me.Team && x.IsAlive && x.IsVisible && !x.IsIllusion && !x.IsMagicImmune())
					.ToList();
		    e = me.ClosestToMouseTarget(2400);
			if (e == null) return;
			
			if (Active && me.IsInvisible())
			{
				if (me.Distance2D(e) <= 150 && Utils.SleepCheck("Attack") && me.NetworkActivity != NetworkActivity.Attack)
				{
					me.Attack(e);
					Utils.Sleep(150, "Attack");
				}
				else if (me.Distance2D(e) <= 2400 && me.Distance2D(e) >= 130 && me.NetworkActivity!=NetworkActivity.Attack && Utils.SleepCheck("Move"))
				{
					me.Move(e.Position);
					Utils.Sleep(150, "Move");
				}
			}
			else if (Active && me.Distance2D(e) <= 1400 && e.IsAlive && !me.IsInvisible())
			{
				if (me.Distance2D(e) >= 400)
				{
					if (
						W != null && W.CanBeCasted() && me.Distance2D(e) <= 1100
						&& me.CanAttack()
						&& Menu.Item("Skills").GetValue<AbilityToggler>().IsEnabled(W.Name)
						&& Utils.SleepCheck("W")
						)
					{
						W.UseAbility();
						Utils.Sleep(100, "W");
					}
				}
				if (W!=null && W.IsInAbilityPhase || me.HasModifier("modifier_weaver_shukuchi"))return;

				if (Menu.Item("orbwalk").GetValue<bool>())
				{
					Orbwalking.Orbwalk(e, 0, 1600, true, true);
				}
				if (
					blink != null
					&& me.CanCast()
					&& blink.CanBeCasted()
					&& me.Distance2D(e) < 1190
					&& me.Distance2D(e) > me.AttackRange - 50
					&& Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(blink.Name)
					&& Utils.SleepCheck("blink")
					)
				{
					blink.UseAbility(e.Position);
					Utils.Sleep(250, "blink");
				}
				if (
					Q != null && Q.CanBeCasted() && me.Distance2D(e) <= me.AttackRange + 300
					&& me.CanAttack()
					&& Menu.Item("Skills").GetValue<AbilityToggler>().IsEnabled(Q.Name)
					&& Utils.SleepCheck("Q")
					)
				{
					Q.UseAbility(e.Position);
					Utils.Sleep(100, "Q");
				}
				if ( // MOM
				mom != null
				&& mom.CanBeCasted()
				&& me.CanCast()
				&& Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(mom.Name)
				&& Utils.SleepCheck("mom")
				&& me.Distance2D(e) <= 700
				)
				{
					mom.UseAbility();
					Utils.Sleep(250, "mom");
				}
				if ( // Mjollnir
					mjollnir != null
					&& mjollnir.CanBeCasted()
					&& me.CanCast()
					&& !e.IsMagicImmune()
					&& Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(mjollnir.Name)
					&& Utils.SleepCheck("mjollnir")
					&& me.Distance2D(e) <= 900
					)
				{
					mjollnir.UseAbility(me);
					Utils.Sleep(250, "mjollnir");
				} // Mjollnir Item end
				if ( // Medall
					medall != null
					&& medall.CanBeCasted()
					&& Utils.SleepCheck("Medall")
					&& Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(medall.Name)
					&& me.Distance2D(e) <= 700
					)
				{
					medall.UseAbility(e);
					Utils.Sleep(250, "Medall");
				} // Medall Item end
				if ( // orchid
					orchid != null
					&& orchid.CanBeCasted()
					&& me.CanCast()
					&& !e.IsLinkensProtected()
					&& !e.IsMagicImmune()
					&& me.Distance2D(e) <= me.AttackRange + 40
					&& Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(orchid.Name)
					&& Utils.SleepCheck("orchid")
					)
				{
					orchid.UseAbility(e);
					Utils.Sleep(250, "orchid");
				} // orchid Item end

				if (Shiva != null && Shiva.CanBeCasted() && me.Distance2D(e) <= 600
					&& Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(Shiva.Name)
					&& !e.IsMagicImmune() && Utils.SleepCheck("Shiva"))
				{
					Shiva.UseAbility();
					Utils.Sleep(100, "Shiva");
				}

				if (ethereal != null && ethereal.CanBeCasted()
					&& me.Distance2D(e) <= 700 && me.Distance2D(e) <= 400
					&& Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(ethereal.Name) &&
					Utils.SleepCheck("ethereal"))
				{
					ethereal.UseAbility(e);
					Utils.Sleep(100, "ethereal");
				}

				if (dagon != null
					&& dagon.CanBeCasted()
					&& me.Distance2D(e) <= 500
					&& Utils.SleepCheck("dagon"))
				{
					dagon.UseAbility(e);
					Utils.Sleep(100, "dagon");
				}
				if ( // Abyssal Blade
					abyssal != null
					&& abyssal.CanBeCasted()
					&& me.CanCast()
					&& !e.IsStunned()
					&& !e.IsHexed()
					&& Utils.SleepCheck("abyssal")
					&& Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(abyssal.Name)
					&& me.Distance2D(e) <= 400
					)
				{
					abyssal.UseAbility(e);
					Utils.Sleep(250, "abyssal");
				} // Abyssal Item end
				if (urn != null && urn.CanBeCasted() && urn.CurrentCharges > 0 && me.Distance2D(e) <= 400
					&& Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(urn.Name) && Utils.SleepCheck("urn"))
				{
					urn.UseAbility(e);
					Utils.Sleep(240, "urn");
				}
				if ( // Hellbard
					halberd != null
					&& halberd.CanBeCasted()
					&& me.CanCast()
					&& !e.IsMagicImmune()
					&& (e.NetworkActivity == NetworkActivity.Attack
						|| e.NetworkActivity == NetworkActivity.Crit
						|| e.NetworkActivity == NetworkActivity.Attack2)
					&& Utils.SleepCheck("halberd")
					&& me.Distance2D(e) <= 700
					&& Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(halberd.Name)
					)
				{
					halberd.UseAbility(e);
					Utils.Sleep(250, "halberd");
				}
				if ( // Satanic 
					satanic != null &&
					me.Health <= (me.MaximumHealth * 0.3) &&
					satanic.CanBeCasted() &&
					me.Distance2D(e) <= me.AttackRange + 50
					&& Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(satanic.Name)
					&& Utils.SleepCheck("satanic")
					)
				{
					satanic.UseAbility();
					Utils.Sleep(240, "satanic");
				} // Satanic Item end
				if (mail != null && mail.CanBeCasted() && (v.Count(x => x.Distance2D(me) <= 650) >=
														   (Menu.Item("Heelm").GetValue<Slider>().Value)) &&
					Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(mail.Name) && Utils.SleepCheck("mail"))
				{
					mail.UseAbility();
					Utils.Sleep(100, "mail");
				}
				if (bkb != null && bkb.CanBeCasted() && (v.Count(x => x.Distance2D(me) <= 650) >=
														 (Menu.Item("Heel").GetValue<Slider>().Value)) &&
					Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(bkb.Name) && Utils.SleepCheck("bkb"))
				{
					bkb.UseAbility();
					Utils.Sleep(100, "bkb");
				}
			}
			OnTimedEvent();
		}

		
		public void OnCloseEvent()
		{
			
		}
		
		private void OnTimedEvent()
		{
			if(Game.IsPaused || R == null)return;
			if (R != null)
			{
				if (Menu.Item("Skills").GetValue<AbilityToggler>().IsEnabled(R.Name))
				{
					float now = me.Health;
					Task.Delay(4000).ContinueWith(_ =>
					{
						float back4 = me.Health;
						if ((now - back4)>= Menu.Item("MomentDownHealth").GetValue<Slider>().Value )
						{
							if (R.CanBeCasted() && Utils.SleepCheck("R"))
							{
								if (!me.AghanimState())
									R.UseAbility();
								else
									R.UseAbility(me);
								Utils.Sleep(250, "R");
							}
						}

					});

					var ally = ObjectManager.GetEntities<Hero>()
										  .Where(x => x.Team == me.Team && x.IsAlive && x.IsVisible && !x.IsIllusion && !x.Equals(me)).ToList();
					if (Menu.Item("ult").GetValue<bool>() && me.AghanimState())
					{
						foreach (var a in ally)
						{
							float allyHealth = a.Health;
							Task.Delay(4000).ContinueWith(_ =>
							{
								float backAlly = a.Health;
								if ((allyHealth - backAlly) >= Menu.Item("MomentAllyDownHealth").GetValue<Slider>().Value && R.CanBeCasted() && me.Distance2D(a) <= 1000 + me.HullRadius + 24)
								{
									Console.WriteLine(allyHealth - backAlly);
									if (Utils.SleepCheck("ally"))
									{
										R.UseAbility(a);
										Utils.Sleep(250, "ally");
									}
								}
							});
						}
					}
				}
			}
		}
	}
}