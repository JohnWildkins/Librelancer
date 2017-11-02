﻿/* The contents of this file are subject to the Mozilla Public License
 * Version 1.1 (the "License"); you may not use this file except in
 * compliance with the License. You may obtain a copy of the License at
 * http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS IS"
 * basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
 * License for the specific language governing rights and limitations
 * under the License.
 * 
 * 
 * The Initial Developer of the Original Code is Callum McGing (mailto:callum.mcging@gmail.com).
 * Portions created by the Initial Developer are Copyright (C) 2013-2016
 * the Initial Developer. All Rights Reserved.
 */
using System;
using System.Linq;
using System.Collections.Generic;
namespace LibreLancer
{
	public class UIManager : IDisposable
	{
		public IDrawable MenuButton;
		public FreelancerGame Game;
		public Color4 TextColor = new Color4 (160, 196, 210, 255);
		public List<UIElement> Elements = new List<UIElement> ();
		public event Action<string> Clicked;
		Font buttonFont;
		float currentSize = -2f;
		public UIManager (FreelancerGame game)
		{
			Game = game;
		}
		public Font GetButtonFont(float sz)
		{
			if (currentSize != sz) {
				if (buttonFont != null)
					buttonFont.Dispose ();
				currentSize = sz;
				buttonFont = Font.FromSystemFont (Game.Renderer2D, "Agency FB", currentSize, FontStyles.Bold);
			}
			return buttonFont;
		}
		public void Draw()
        {
			if (MenuButton != null) MenuButton.Update (IdentityCamera.Instance, TimeSpan.Zero, TimeSpan.FromSeconds(Game.TotalTime));
            Game.RenderState.DepthEnabled = false;
			foreach (var e in Elements)
				if (e.Visible) e.DrawBase ();
			Game.Renderer2D.Start (Game.Width, Game.Height);
			foreach (var e in Elements)
				if (e.Visible) e.DrawText ();
			Game.Renderer2D.Finish ();
		}
		bool startedAllAnim = false;
		public void FlyInAll(double duration, double spacing)
		{
			startedAllAnim = true;
			double currentspacing = 0;
			foreach (var elem in Elements)
			{
				elem.Animation = new FlyInLeft(
					elem.UIPosition,
					currentspacing,
					duration
				);
				elem.Animation.Begin();
				currentspacing += spacing;
			}
		}

		public void FlyOutAll(double duration, double spacing)
		{
			startedAllAnim = true;
			double currentspacing = 0;
			foreach (var elem in ((IEnumerable<UIElement>)Elements).Reverse())
			{
				elem.Animation = new FlyOutLeft(
					elem.UIPosition,
					currentspacing,
					duration
				);
				elem.Animation.Begin();
				currentspacing += spacing;
			}
		}

		public void OnClick(string tag)
		{
			if (Clicked != null)
				Clicked (tag);
		}

		public void WaitAnimationsComplete()
		{
			startedAllAnim = true;
		}

		public void Update(TimeSpan delta)
		{
			if (MenuButton != null) MenuButton.Update (IdentityCamera.Instance, delta, TimeSpan.FromSeconds(Game.TotalTime));
			bool animating = false;
			foreach (var elem in Elements) {
				if (elem.Visible) elem.Update(delta);
				if (elem.Animation != null && elem.Animation.Running) animating = true;
			}
			if (startedAllAnim && !animating) {
				startedAllAnim = false;
				OnAnimationComplete();
			}
		}

		public event Action AnimationComplete;

		void OnAnimationComplete()
		{
			if (AnimationComplete != null)
				AnimationComplete();
		}

		public Vector2 ScreenToPixel (float screenx, float screeny)
		{
			return IdentityCamera.Instance.ScreenToPixel(screenx, screeny);
		}

		public void Dispose()
		{
			buttonFont.Dispose();
		}
	}
}

